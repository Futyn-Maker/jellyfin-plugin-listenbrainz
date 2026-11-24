using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Jellyfin.Plugin.ListenBrainz.Common.Extensions;

namespace Jellyfin.Plugin.ListenBrainz.Configuration;

/// <summary>
/// ListenBrainz user configuration.
/// </summary>
public class UserConfig
{
    /// <summary>
    /// Default title of the whole-library ListenBrainz playlist.
    /// </summary>
    public const string DefaultLibraryExportTitle = "Music library";

    private string _apiToken = string.Empty;
    private string _userName = string.Empty;
    private string _libraryExportTitle = DefaultLibraryExportTitle;
    private string _libraryExportDescription = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfig"/> class.
    /// </summary>
    public UserConfig()
    {
        IsListenSubmitEnabled = false;
        ApiToken = string.Empty;
        UserName = string.Empty;
        IsBackupEnabled = false;
        IsStrictModeEnabled = false;
        IsGeneratedPlaylistsSyncEnabled = false;
        IsWeeklyJamsSyncEnabled = true;
        IsWeeklyExplorationSyncEnabled = false;
        IsTopDiscoveriesSyncEnabled = true;
        IsTopMissedRecordingsSyncEnabled = false;
        KeepPlaylistsAfterRotation = false;
        IsPlaylistExportEnabled = false;
        IsLibraryExportEnabled = false;
        IsPlaylistExportPublic = true;
        IsPlaylistExportCleanupEnabled = true;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user's Jellyfin playlists are exported (mirrored) to ListenBrainz.
    /// </summary>
    public bool IsPlaylistExportEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the whole music library is exported as one ListenBrainz playlist.
    /// </summary>
    public bool IsLibraryExportEnabled { get; set; }

    /// <summary>
    /// Gets or sets title of the whole-library ListenBrainz playlist.
    /// </summary>
    public string LibraryExportTitle
    {
        get => _libraryExportTitle;
        set => _libraryExportTitle = string.IsNullOrWhiteSpace(value) ? DefaultLibraryExportTitle : value.SanitizeForXml();
    }

    /// <summary>
    /// Gets or sets description (annotation) of the whole-library ListenBrainz playlist.
    /// </summary>
    public string LibraryExportDescription
    {
        get => _libraryExportDescription;
        set => _libraryExportDescription = (value ?? string.Empty).SanitizeForXml();
    }

    /// <summary>
    /// Gets or sets a value indicating whether exported ListenBrainz playlists are public.
    /// </summary>
    public bool IsPlaylistExportPublic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the ListenBrainz copy is deleted when its Jellyfin playlist is deleted.
    /// </summary>
    public bool IsPlaylistExportCleanupEnabled { get; set; }

    /// <summary>
    /// Gets or sets Jellyfin user id.
    /// </summary>
    public Guid JellyfinUserId { get; set; }

    /// <summary>
    /// Gets or sets ListenBrainz API token.
    /// </summary>
    public string ApiToken
    {
        get => _apiToken;
        set => _apiToken = value.SanitizeForXml();
    }

    /// <summary>
    /// Gets or sets ListenBrainz API token in plaintext.
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public string PlaintextApiToken
    {
        get => Encoding.UTF8.GetString(Convert.FromBase64String(ApiToken));
        set => ApiToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    /// <summary>
    /// Gets or sets a value indicating whether ListenBrainz submission is enabled.
    /// </summary>
    public bool IsListenSubmitEnabled { get; set; }

    /// <summary>
    /// Gets a value indicating whether ListenBrainz submission is not enabled.
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public bool IsNotListenSubmitEnabled => !IsListenSubmitEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether ListenBrainz favorites sync is enabled.
    /// </summary>
    public bool IsFavoritesSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether ListenBrainz playlist sync is enabled.
    /// </summary>
    public bool IsPlaylistsSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether ListenBrainz generated playlist sync is enabled.
    /// </summary>
    public bool IsGeneratedPlaylistsSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Weekly Jams playlists should be synced.
    /// </summary>
    public bool IsWeeklyJamsSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Weekly Exploration playlists should be synced.
    /// </summary>
    public bool IsWeeklyExplorationSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Top Discoveries of the year playlists should be synced.
    /// </summary>
    public bool IsTopDiscoveriesSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Top Missed Recordings of the year playlists should be synced.
    /// </summary>
    public bool IsTopMissedRecordingsSyncEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether playlists should be kept after rotation.
    /// </summary>
    public bool KeepPlaylistsAfterRotation { get; set; }

    /// <summary>
    /// Gets or sets a ListenBrainz username.
    /// </summary>
    public string UserName
    {
        get => _userName;
        set => _userName = value.SanitizeForXml();
    }

    /// <summary>
    /// Gets or sets a value indicating whether listens should be backed up.
    /// </summary>
    public bool IsBackupEnabled { get; set; }

    /// <summary>
    /// Gets a value indicating whether listens backup is not enabled.
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public bool IsNotBackupEnabled => !IsBackupEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether strict mode is enabled for listen submission.
    /// </summary>
    public bool IsStrictModeEnabled { get; set; }
}
