using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Models;
using Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;
using Jellyfin.Plugin.ListenBrainz.Api.Resources;
using Jellyfin.Plugin.ListenBrainz.Configuration;
using Jellyfin.Plugin.ListenBrainz.Dtos;
using Jellyfin.Plugin.ListenBrainz.Exceptions;
using Jellyfin.Plugin.ListenBrainz.Extensions;
using Jellyfin.Plugin.ListenBrainz.Interfaces;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using IJellyfinPlaylistManager = MediaBrowser.Controller.Playlists.IPlaylistManager;
using JellyfinPlaylist = MediaBrowser.Controller.Playlists.Playlist;

namespace Jellyfin.Plugin.ListenBrainz.Services;

/// <summary>
/// Default implementation of <see cref="IPlaylistExportService"/>.
/// <para>
/// For every configured user the service builds the desired state (ordered recording MBIDs of each owned
/// Jellyfin playlist and, optionally, of the whole library), compares it with the ListenBrainz copies and
/// applies a minimal set of item deletions/insertions. The mapping Jellyfin source -> ListenBrainz playlist
/// is persisted, so playlists are updated in place (stable ListenBrainz links).
/// </para>
/// </summary>
public sealed class DefaultPlaylistExportService : IPlaylistExportService, IDisposable
{
    /// <summary>
    /// Export key of the whole-library playlist.
    /// </summary>
    public const string LibraryKey = "library";

    /// <summary>
    /// Prefix of export keys of Jellyfin playlists.
    /// </summary>
    public const string PlaylistKeyPrefix = "playlist:";

    private const int MassRemovalMinTracks = 20;
    private const double MassRemovalRatio = 0.5;

    private static readonly TimeSpan DebounceDelay = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan MaxDebounceDelay = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan VerifyInterval = TimeSpan.FromHours(12);
    private static readonly TimeSpan MissingGracePeriod = TimeSpan.FromMinutes(15);

    private readonly ILogger _logger;
    private readonly IPluginConfigService _config;
    private readonly IListenBrainzApiClient _api;
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly IJellyfinPlaylistManager _playlistManager;
    private readonly IPersistentJsonService<PlaylistExportState> _stateStorage;
    private readonly SemaphoreSlim _runLock = new(1, 1);
    private readonly object _timerLock = new();
    private readonly Timer _timer;
    private DateTime? _firstPendingRequestUtc;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultPlaylistExportService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="config">Plugin configuration service.</param>
    /// <param name="api">ListenBrainz API client.</param>
    /// <param name="libraryManager">Library manager.</param>
    /// <param name="userManager">User manager.</param>
    /// <param name="playlistManager">Jellyfin playlist manager.</param>
    /// <param name="stateStorage">Persistent state storage.</param>
    public DefaultPlaylistExportService(
        ILogger logger,
        IPluginConfigService config,
        IListenBrainzApiClient api,
        ILibraryManager libraryManager,
        IUserManager userManager,
        IJellyfinPlaylistManager playlistManager,
        IPersistentJsonService<PlaylistExportState> stateStorage)
    {
        _logger = logger;
        _config = config;
        _api = api;
        _libraryManager = libraryManager;
        _userManager = userManager;
        _playlistManager = playlistManager;
        _stateStorage = stateStorage;
        _timer = new Timer(OnDebounceElapsed, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    /// <inheritdoc />
    public void RequestExport(string reason)
    {
        if (_isDisposed || !_config.UserConfigs.Any(IsExportConfigured))
        {
            return;
        }

        lock (_timerLock)
        {
            var now = DateTime.UtcNow;
            _firstPendingRequestUtc ??= now;
            var due = DebounceDelay;
            var untilMax = _firstPendingRequestUtc.Value + MaxDebounceDelay - now;
            if (untilMax < due)
            {
                due = untilMax < TimeSpan.Zero ? TimeSpan.Zero : untilMax;
            }

            _timer.Change(due, Timeout.InfiniteTimeSpan);
        }

        _logger.LogDebug("Playlist export requested: {Reason}", reason);
    }

    /// <inheritdoc />
    public async Task ExportAllAsync(bool isScheduledRun, CancellationToken cancellationToken)
    {
        var userConfigs = _config.UserConfigs.Where(IsExportConfigured).ToList();
        if (userConfigs.Count == 0)
        {
            _logger.LogDebug("No user has playlist export enabled");
            return;
        }

        await _runLock.WaitAsync(cancellationToken);
        try
        {
            var state = await LoadStateAsync(cancellationToken);
            foreach (var userConfig in userConfigs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await ExportUserAsync(userConfig, state, isScheduledRun, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Playlist export for Jellyfin user {UserId} failed", userConfig.JellyfinUserId);
                }
                finally
                {
                    await _stateStorage.SaveAsync(state, cancellationToken: CancellationToken.None);
                }
            }
        }
        finally
        {
            _runLock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _timer.Dispose();
        _runLock.Dispose();
    }

    private static bool IsExportConfigured(UserConfig config) =>
        (config.IsPlaylistExportEnabled || config.IsLibraryExportEnabled) &&
        !string.IsNullOrEmpty(config.ApiToken) &&
        !string.IsNullOrEmpty(config.UserName);

    private void OnDebounceElapsed(object? state)
    {
        lock (_timerLock)
        {
            _firstPendingRequestUtc = null;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await ExportAllAsync(isScheduledRun: false, CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Event-triggered playlist export failed");
            }
        });
    }

    private async Task<PlaylistExportState> LoadStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _stateStorage.ReadAsync(cancellationToken: cancellationToken);
        }
        catch (ServiceException e) when (e.InnerException is FileNotFoundException or DirectoryNotFoundException)
        {
            return new PlaylistExportState();
        }
        catch (ServiceException e)
        {
            _logger.LogWarning(e, "Could not read playlist export state, starting with an empty state");
            return new PlaylistExportState();
        }
    }

    private async Task ExportUserAsync(UserConfig userConfig, PlaylistExportState state, bool isScheduledRun, CancellationToken cancellationToken)
    {
        var user = _userManager.GetUserById(userConfig.JellyfinUserId);
        if (user is null)
        {
            _logger.LogWarning("Jellyfin user {UserId} does not exist, skipping playlist export", userConfig.JellyfinUserId);
            return;
        }

        if (!state.Users.TryGetValue(userConfig.JellyfinUserId, out var entries))
        {
            entries = new Dictionary<string, PlaylistExportEntry>();
            state.Users[userConfig.JellyfinUserId] = entries;
        }

        var context = new ExportContext(userConfig, entries, isScheduledRun);
        var desired = new List<DesiredPlaylist>();
        if (userConfig.IsLibraryExportEnabled)
        {
            desired.Add(BuildLibraryPlaylist(user, userConfig));
        }

        if (userConfig.IsPlaylistExportEnabled)
        {
            desired.AddRange(BuildJellyfinPlaylists(user));
        }

        foreach (var playlist in desired)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await SyncPlaylistAsync(context, playlist, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exporting playlist '{Title}' to ListenBrainz failed, will retry on the next run", playlist.Title);
            }
        }

        if (userConfig.IsPlaylistExportEnabled)
        {
            await CleanUpDeletedPlaylistsAsync(context, desired, cancellationToken);
        }
    }

    private DesiredPlaylist BuildLibraryPlaylist(User user, UserConfig userConfig)
    {
        var musicLibraries = _libraryManager
            .GetUserRootFolder()
            .Children
            .OfType<CollectionFolder>()
            .Where(f => f.CollectionType == CollectionType.music)
            .ToList();

        // Libraries explicitly marked for the library playlist; if none are marked (e.g. a configuration
        // saved before this option existed), the libraries allowed for listen submission; if the library
        // configuration was never saved, all music libraries.
        var libraryConfigs = _config.LibraryConfigs;
        List<CollectionFolder> included;
        if (libraryConfigs.Count == 0)
        {
            included = musicLibraries;
        }
        else if (libraryConfigs.Any(lc => lc.IsLibraryExportIncluded))
        {
            included = musicLibraries.Where(lib => libraryConfigs.Any(lc => lc.Id == lib.Id && lc.IsLibraryExportIncluded)).ToList();
        }
        else
        {
            included = musicLibraries.Where(lib => libraryConfigs.Any(lc => lc.Id == lib.Id && lc.IsAllowed)).ToList();
        }

        var items = new List<Audio>();
        foreach (var library in included)
        {
            items.AddRange(library
                .GetItemList(new InternalItemsQuery(user)
                {
                    IncludeItemTypes = [BaseItemKind.Audio],
                    Recursive = true,
                    IsVirtualItem = false,
                })
                .OfType<Audio>());
        }

        items.Sort(CompareForLibraryOrder);
        var (mbids, skipped) = CollectRecordingMbids(items, distinct: true);
        _logger.LogDebug(
            "Library playlist: {Count} tracks from {Libraries} libraries ({Skipped} without recording MBID)",
            mbids.Count,
            included.Count,
            skipped.Count);

        return new DesiredPlaylist(
            LibraryKey,
            userConfig.LibraryExportTitle,
            userConfig.LibraryExportDescription,
            mbids,
            skipped);
    }

    private IEnumerable<DesiredPlaylist> BuildJellyfinPlaylists(User user)
    {
        var playlists = _playlistManager
            .GetPlaylists(user.Id)
            .Where(p => p.OwnerUserId.Equals(user.Id))
            .Where(p => p.PlaylistMediaType is MediaType.Audio or MediaType.Unknown)
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var playlist in playlists)
        {
            var audioItems = playlist.GetManageableItems().Select(t => t.Item2).OfType<Audio>().ToList();
            var (mbids, skipped) = CollectRecordingMbids(audioItems, distinct: false);
            var description = string.IsNullOrWhiteSpace(playlist.Overview)
                ? string.Empty
                : WebUtility.HtmlEncode(playlist.Overview.Trim());
            yield return new DesiredPlaylist(
                PlaylistKeyPrefix + playlist.Id.ToString("N", CultureInfo.InvariantCulture),
                playlist.Name,
                description,
                mbids,
                skipped);
        }
    }

    private static (List<string> Mbids, List<string> Skipped) CollectRecordingMbids(IEnumerable<Audio> items, bool distinct)
    {
        var mbids = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skipped = new List<string>();
        foreach (var item in items)
        {
            var mbid = item.GetRecordingMbid()?.Trim();
            if (string.IsNullOrEmpty(mbid) || !Guid.TryParse(mbid, out var guid))
            {
                skipped.Add(item.Path ?? item.Name);
                continue;
            }

            var normalized = guid.ToString("D", CultureInfo.InvariantCulture);
            if (distinct && !seen.Add(normalized))
            {
                continue;
            }

            mbids.Add(normalized);
        }

        return (mbids, skipped);
    }

    private static int CompareForLibraryOrder(Audio a, Audio b)
    {
        static string Artist(Audio x) => x.AlbumArtists.FirstOrDefault() ?? x.Artists.FirstOrDefault() ?? string.Empty;
        var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
        const CompareOptions Options = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        int c = compareInfo.Compare(Artist(a), Artist(b), Options);
        if (c != 0)
        {
            return c;
        }

        c = compareInfo.Compare(a.Album ?? string.Empty, b.Album ?? string.Empty, Options);
        if (c != 0)
        {
            return c;
        }

        c = (a.ParentIndexNumber ?? 0).CompareTo(b.ParentIndexNumber ?? 0);
        if (c != 0)
        {
            return c;
        }

        c = (a.IndexNumber ?? 0).CompareTo(b.IndexNumber ?? 0);
        if (c != 0)
        {
            return c;
        }

        c = compareInfo.Compare(a.SortName ?? a.Name ?? string.Empty, b.SortName ?? b.Name ?? string.Empty, Options);
        return c != 0 ? c : string.CompareOrdinal(a.Path, b.Path);
    }

    private async Task SyncPlaylistAsync(ExportContext context, DesiredPlaylist desired, CancellationToken cancellationToken)
    {
        var isPublic = context.UserConfig.IsPlaylistExportPublic;
        var hash = ComputeHash(desired, isPublic);
        var now = DateTime.UtcNow;
        context.Entries.TryGetValue(desired.Key, out var entry);

        if (desired.Skipped.Count > 0)
        {
            _logger.LogInformation(
                "Playlist '{Title}': {Count} track(s) have no MusicBrainz recording ID and are not exported: {Tracks}",
                desired.Title,
                desired.Skipped.Count,
                string.Join("; ", desired.Skipped.Take(20)));
        }

        if (entry is not null &&
            entry.ContentHash == hash &&
            !context.IsScheduledRun &&
            now - entry.LastVerifiedUtc < VerifyInterval)
        {
            return;
        }

        Playlist? remote = null;
        if (entry is not null)
        {
            remote = await GetRemotePlaylistAsync(context, entry.ListenBrainzPlaylistId, cancellationToken);
            if (remote is null)
            {
                _logger.LogInformation("ListenBrainz playlist {Id} ('{Title}') no longer exists, it will be recreated", entry.ListenBrainzPlaylistId, entry.Title);
            }
        }

        remote ??= await FindAdoptablePlaylistAsync(context, desired, cancellationToken);

        if (remote is null)
        {
            var newId = await CreatePlaylistAsync(context, desired, isPublic, cancellationToken);
            entry = new PlaylistExportEntry { ListenBrainzPlaylistId = newId };
            context.Entries[desired.Key] = entry;
            remote = await GetRemotePlaylistAsync(context, newId, cancellationToken)
                     ?? throw new ServiceException($"Newly created ListenBrainz playlist {newId} cannot be fetched");
        }
        else if (entry is null || entry.ListenBrainzPlaylistId != remote.PlaylistId)
        {
            entry = new PlaylistExportEntry { ListenBrainzPlaylistId = remote.PlaylistId };
            context.Entries[desired.Key] = entry;
        }

        var playlistId = entry.ListenBrainzPlaylistId;
        bool metadataChanged = entry.ContentHash != hash;
        if (remote.Title != desired.Title ||
            remote.JspfPlaylist.Public != isPublic ||
            (metadataChanged && (remote.Annotation ?? string.Empty).Trim() != desired.Annotation.Trim()))
        {
            await EditPlaylistAsync(context, playlistId, desired, isPublic, cancellationToken);
        }

        var current = remote.Tracks.Select(t => t.RecordingMbid ?? string.Empty).ToList();
        if (!current.SequenceEqual(desired.Mbids, StringComparer.OrdinalIgnoreCase))
        {
            bool applied = await ApplyTrackChangesAsync(context, playlistId, desired, current, cancellationToken);
            if (!applied)
            {
                return;
            }

            var verify = await GetRemotePlaylistAsync(context, playlistId, cancellationToken);
            var after = verify?.Tracks.Select(t => t.RecordingMbid ?? string.Empty).ToList() ?? new List<string>();
            if (!after.SequenceEqual(desired.Mbids, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Playlist '{Title}' differs from Jellyfin after an incremental update, rebuilding it", desired.Title);
                await RebuildTracksAsync(context, playlistId, after.Count, desired.Mbids, cancellationToken);
            }
        }

        entry.Title = desired.Title;
        entry.ContentHash = hash;
        entry.TrackCount = desired.Mbids.Count;
        entry.SkippedCount = desired.Skipped.Count;
        entry.LastSyncedUtc = now;
        entry.LastVerifiedUtc = now;
        entry.MissingSinceUtc = null;
        _logger.LogInformation(
            "Playlist '{Title}' is in sync with ListenBrainz playlist {Id} ({Count} tracks)",
            desired.Title,
            playlistId,
            desired.Mbids.Count);
    }

    private async Task<bool> ApplyTrackChangesAsync(
        ExportContext context,
        string playlistId,
        DesiredPlaylist desired,
        List<string> current,
        CancellationToken cancellationToken)
    {
        var script = PlaylistDiff.Compute(current, desired.Mbids);
        int rebuildCalls = (current.Count > 0 ? 1 : 0) + ((desired.Mbids.Count + Limits.MaxRecordingsPerPlaylistAdd - 1) / Limits.MaxRecordingsPerPlaylistAdd);
        int removed = script?.DeletedCount ?? current.Count;

        if (!context.IsScheduledRun && current.Count >= MassRemovalMinTracks && removed > current.Count * MassRemovalRatio)
        {
            _logger.LogWarning(
                "Playlist '{Title}': update would remove {Removed} of {Count} tracks; holding it back until the next scheduled export run",
                desired.Title,
                removed,
                current.Count);
            return false;
        }

        if (script is null || script.CallCount(Limits.MaxRecordingsPerPlaylistAdd) >= rebuildCalls)
        {
            await RebuildTracksAsync(context, playlistId, current.Count, desired.Mbids, cancellationToken);
            return true;
        }

        foreach (var deletion in script.Deletions)
        {
            await DeleteItemsAsync(context, playlistId, deletion.Index, deletion.Count, cancellationToken);
        }

        foreach (var insertion in script.Insertions)
        {
            for (int i = 0; i < insertion.Items.Count; i += Limits.MaxRecordingsPerPlaylistAdd)
            {
                var chunk = insertion.Items.Skip(i).Take(Limits.MaxRecordingsPerPlaylistAdd).ToList();
                await AddItemsAsync(context, playlistId, chunk, insertion.Offset + i, cancellationToken);
            }
        }

        _logger.LogInformation(
            "Playlist '{Title}': removed {Removed}, added {Added} track(s)",
            desired.Title,
            script.DeletedCount,
            script.InsertedCount);
        return true;
    }

    private async Task RebuildTracksAsync(ExportContext context, string playlistId, int currentCount, IReadOnlyList<string> mbids, CancellationToken cancellationToken)
    {
        if (currentCount > 0)
        {
            await DeleteItemsAsync(context, playlistId, 0, currentCount, cancellationToken);
        }

        for (int i = 0; i < mbids.Count; i += Limits.MaxRecordingsPerPlaylistAdd)
        {
            var chunk = mbids.Skip(i).Take(Limits.MaxRecordingsPerPlaylistAdd).ToList();
            await AddItemsAsync(context, playlistId, chunk, null, cancellationToken);
        }

        _logger.LogInformation("Rebuilt ListenBrainz playlist {Id} with {Count} tracks", playlistId, mbids.Count);
    }

    private async Task CleanUpDeletedPlaylistsAsync(ExportContext context, List<DesiredPlaylist> desired, CancellationToken cancellationToken)
    {
        var desiredKeys = desired.Select(d => d.Key).ToHashSet(StringComparer.Ordinal);
        var now = DateTime.UtcNow;
        foreach (var (key, entry) in context.Entries.Where(kv => kv.Key.StartsWith(PlaylistKeyPrefix, StringComparison.Ordinal)).ToList())
        {
            if (desiredKeys.Contains(key))
            {
                continue;
            }

            // Only treat a playlist as deleted when Jellyfin really does not know it anymore,
            // and only after a grace period (protects against transient states during startup/scans).
            var idText = key[PlaylistKeyPrefix.Length..];
            if (Guid.TryParse(idText, out var jellyfinId) && _libraryManager.GetItemById(jellyfinId) is JellyfinPlaylist)
            {
                continue;
            }

            entry.MissingSinceUtc ??= now;
            if (now - entry.MissingSinceUtc < MissingGracePeriod)
            {
                _logger.LogDebug("Jellyfin playlist for '{Title}' is missing, waiting before deleting its ListenBrainz copy", entry.Title);
                continue;
            }

            if (!context.UserConfig.IsPlaylistExportCleanupEnabled)
            {
                _logger.LogInformation("Jellyfin playlist '{Title}' was deleted; keeping ListenBrainz playlist {Id} (cleanup disabled)", entry.Title, entry.ListenBrainzPlaylistId);
                context.Entries.Remove(key);
                continue;
            }

            var request = new DeletePlaylistRequest(entry.ListenBrainzPlaylistId) { ApiToken = context.Token, BaseUrl = _config.ListenBrainzApiUrl };
            var response = await _api.DeletePlaylist(request, cancellationToken);
            if (response.IsOk || response.Code == 404)
            {
                _logger.LogInformation("Jellyfin playlist '{Title}' was deleted; deleted ListenBrainz playlist {Id}", entry.Title, entry.ListenBrainzPlaylistId);
                context.Entries.Remove(key);
            }
            else
            {
                _logger.LogWarning("Deleting ListenBrainz playlist {Id} failed: {Code} {Error}", entry.ListenBrainzPlaylistId, response.Code, response.Error);
            }
        }
    }

    private async Task<Playlist?> GetRemotePlaylistAsync(ExportContext context, string playlistId, CancellationToken cancellationToken)
    {
        var request = new GetPlaylistRequest(playlistId, fetchMetadata: false) { ApiToken = context.Token, BaseUrl = _config.ListenBrainzApiUrl };
        var response = await _api.GetPlaylist(request, cancellationToken);
        if (response.IsOk)
        {
            return response.Playlist;
        }

        if (response.Code == 404)
        {
            return null;
        }

        throw new ServiceException($"Fetching ListenBrainz playlist {playlistId} failed: {response.Code} {response.Error}");
    }

    private async Task<Playlist?> FindAdoptablePlaylistAsync(ExportContext context, DesiredPlaylist desired, CancellationToken cancellationToken)
    {
        // Recovers the mapping after a lost state file: reuse a playlist of this user with the same title
        // that is not mapped to another source, instead of creating a duplicate.
        if (context.RemotePlaylists is null)
        {
            context.RemotePlaylists = new List<Playlist>();
            int offset = 0;
            while (true)
            {
                var request = new GetUserPlaylistsRequest(context.UserConfig.UserName, 100, offset) { ApiToken = context.Token, BaseUrl = _config.ListenBrainzApiUrl };
                var response = await _api.GetUserPlaylists(request, cancellationToken);
                if (response.IsNotOk)
                {
                    throw new ServiceException("Listing ListenBrainz playlists failed");
                }

                var page = response.Playlists.ToList();
                context.RemotePlaylists.AddRange(page);
                offset += page.Count;
                if (page.Count == 0 || offset >= response.PlaylistCount)
                {
                    break;
                }
            }
        }

        var mappedIds = context.Entries.Values.Select(e => e.ListenBrainzPlaylistId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var candidate = context.RemotePlaylists.FirstOrDefault(p =>
            string.Equals(p.Title, desired.Title, StringComparison.Ordinal) && !mappedIds.Contains(p.PlaylistId));
        if (candidate is null)
        {
            return null;
        }

        _logger.LogInformation("Reusing existing ListenBrainz playlist {Id} with title '{Title}'", candidate.PlaylistId, desired.Title);
        return await GetRemotePlaylistAsync(context, candidate.PlaylistId, cancellationToken);
    }

    private async Task<string> CreatePlaylistAsync(ExportContext context, DesiredPlaylist desired, bool isPublic, CancellationToken cancellationToken)
    {
        var first = desired.Mbids.Take(Limits.MaxRecordingsPerPlaylistAdd).ToList();
        var payload = new PlaylistPayload { Title = desired.Title, Annotation = desired.Annotation }
            .WithPublic(isPublic)
            .WithRecordings(first);
        var request = new CreatePlaylistRequest(payload) { ApiToken = context.Token, BaseUrl = _config.ListenBrainzApiUrl };
        var response = await _api.CreatePlaylist(request, cancellationToken);
        if (response.IsNotOk || string.IsNullOrEmpty(response.PlaylistMbid))
        {
            throw new ServiceException($"Creating ListenBrainz playlist '{desired.Title}' failed: {response.Code} {response.Error}");
        }

        for (int i = first.Count; i < desired.Mbids.Count; i += Limits.MaxRecordingsPerPlaylistAdd)
        {
            var chunk = desired.Mbids.Skip(i).Take(Limits.MaxRecordingsPerPlaylistAdd).ToList();
            await AddItemsAsync(context, response.PlaylistMbid, chunk, null, cancellationToken);
        }

        _logger.LogInformation(
            "Created ListenBrainz playlist {Id} '{Title}' with {Count} tracks",
            response.PlaylistMbid,
            desired.Title,
            desired.Mbids.Count);
        return response.PlaylistMbid;
    }

    private async Task EditPlaylistAsync(ExportContext context, string playlistId, DesiredPlaylist desired, bool isPublic, CancellationToken cancellationToken)
    {
        var payload = new PlaylistPayload { Title = desired.Title, Annotation = desired.Annotation }.WithPublic(isPublic);
        var request = new EditPlaylistRequest(playlistId, payload) { ApiToken = context.Token, BaseUrl = _config.ListenBrainzApiUrl };
        var response = await _api.EditPlaylist(request, cancellationToken);
        if (response.IsNotOk)
        {
            throw new ServiceException($"Editing ListenBrainz playlist {playlistId} failed: {response.Code} {response.Error}");
        }
    }

    private async Task AddItemsAsync(ExportContext context, string playlistId, IReadOnlyList<string> mbids, int? offset, CancellationToken cancellationToken)
    {
        var request = new AddPlaylistItemsRequest(playlistId, mbids, offset) { ApiToken = context.Token, BaseUrl = _config.ListenBrainzApiUrl };
        var response = await _api.AddPlaylistItems(request, cancellationToken);
        if (response.IsNotOk)
        {
            throw new ServiceException($"Adding items to ListenBrainz playlist {playlistId} failed: {response.Code} {response.Error}");
        }
    }

    private async Task DeleteItemsAsync(ExportContext context, string playlistId, int index, int count, CancellationToken cancellationToken)
    {
        var request = new DeletePlaylistItemsRequest(playlistId, index, count) { ApiToken = context.Token, BaseUrl = _config.ListenBrainzApiUrl };
        var response = await _api.DeletePlaylistItems(request, cancellationToken);
        if (response.IsNotOk)
        {
            throw new ServiceException($"Deleting items from ListenBrainz playlist {playlistId} failed: {response.Code} {response.Error}");
        }
    }

    private static string ComputeHash(DesiredPlaylist desired, bool isPublic)
    {
        var builder = new StringBuilder();
        builder.Append(desired.Title).Append('\n')
            .Append(desired.Annotation).Append('\n')
            .Append(isPublic ? "public" : "private").Append('\n');
        foreach (var mbid in desired.Mbids)
        {
            builder.Append(mbid).Append(',');
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }

    private sealed record DesiredPlaylist(string Key, string Title, string Annotation, IReadOnlyList<string> Mbids, IReadOnlyList<string> Skipped);

    private sealed class ExportContext
    {
        public ExportContext(UserConfig userConfig, Dictionary<string, PlaylistExportEntry> entries, bool isScheduledRun)
        {
            UserConfig = userConfig;
            Entries = entries;
            IsScheduledRun = isScheduledRun;
            Token = userConfig.PlaintextApiToken;
        }

        public UserConfig UserConfig { get; }

        public Dictionary<string, PlaylistExportEntry> Entries { get; }

        public bool IsScheduledRun { get; }

        public string Token { get; }

        public List<Playlist>? RemotePlaylists { get; set; }
    }
}
