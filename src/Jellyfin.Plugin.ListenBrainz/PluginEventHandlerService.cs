using Jellyfin.Plugin.ListenBrainz.Handlers;
using Jellyfin.Plugin.ListenBrainz.Interfaces;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JellyfinPlaylist = MediaBrowser.Controller.Playlists.Playlist;

namespace Jellyfin.Plugin.ListenBrainz;

/// <summary>
/// Hosted service which subscribes the plugin event handlers to Jellyfin events for the lifetime of the host.
/// </summary>
public sealed class PluginEventHandlerService : IHostedService
{
    private readonly ILogger<PluginEventHandlerService> _logger;
    private readonly ISessionManager _sessionManager;
    private readonly IUserDataManager _userDataManager;
    private readonly PlaybackStartHandler _playbackStartHandler;
    private readonly PlaybackProgressHandler _playbackProgressHandler;
    private readonly PlaybackStopHandler _playbackStopHandler;
    private readonly UserDataSaveHandler _userDataSaveHandler;
    private readonly ILibraryManager _libraryManager;
    private readonly IPlaylistExportService _playlistExportService;
    private bool _isRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginEventHandlerService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="sessionManager">Session manager.</param>
    /// <param name="userDataManager">User data manager.</param>
    /// <param name="playbackStartHandler">Playback start handler.</param>
    /// <param name="playbackProgressHandler">Playback progress handler.</param>
    /// <param name="playbackStopHandler">Playback stop handler.</param>
    /// <param name="userDataSaveHandler">User data save handler.</param>
    /// <param name="libraryManager">Library manager.</param>
    /// <param name="playlistExportService">Playlist export service.</param>
    public PluginEventHandlerService(
        ILogger<PluginEventHandlerService> logger,
        ISessionManager sessionManager,
        IUserDataManager userDataManager,
        PlaybackStartHandler playbackStartHandler,
        PlaybackProgressHandler playbackProgressHandler,
        PlaybackStopHandler playbackStopHandler,
        UserDataSaveHandler userDataSaveHandler,
        ILibraryManager libraryManager,
        IPlaylistExportService playlistExportService)
    {
        _logger = logger;
        _sessionManager = sessionManager;
        _userDataManager = userDataManager;
        _playbackStartHandler = playbackStartHandler;
        _playbackProgressHandler = playbackProgressHandler;
        _playbackStopHandler = playbackStopHandler;
        _userDataSaveHandler = userDataSaveHandler;
        _libraryManager = libraryManager;
        _playlistExportService = playlistExportService;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_isRegistered)
        {
            _logger.LogDebug("Plugin event handlers are already registered");
            return Task.CompletedTask;
        }

        _sessionManager.PlaybackStart += _playbackStartHandler.HandleEvent;
        _sessionManager.PlaybackProgress += _playbackProgressHandler.HandleEvent;
        _sessionManager.PlaybackStopped += _playbackStopHandler.HandleEvent;
        _userDataManager.UserDataSaved += _userDataSaveHandler.HandleEvent;
        _libraryManager.ItemAdded += OnLibraryItemChanged;
        _libraryManager.ItemUpdated += OnLibraryItemChanged;
        _libraryManager.ItemRemoved += OnLibraryItemChanged;

        _isRegistered = true;
        _logger.LogDebug("Plugin event handlers have been registered");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_isRegistered)
        {
            _logger.LogDebug("Plugin event handlers are already unregistered");
            return Task.CompletedTask;
        }

        _sessionManager.PlaybackStart -= _playbackStartHandler.HandleEvent;
        _sessionManager.PlaybackProgress -= _playbackProgressHandler.HandleEvent;
        _sessionManager.PlaybackStopped -= _playbackStopHandler.HandleEvent;
        _userDataManager.UserDataSaved -= _userDataSaveHandler.HandleEvent;
        _libraryManager.ItemAdded -= OnLibraryItemChanged;
        _libraryManager.ItemUpdated -= OnLibraryItemChanged;
        _libraryManager.ItemRemoved -= OnLibraryItemChanged;

        _isRegistered = false;
        _logger.LogDebug("Plugin event handlers have been unregistered");
        return Task.CompletedTask;
    }

    private void OnLibraryItemChanged(object? sender, ItemChangeEventArgs e)
    {
        // Playlist edits (add/remove/move items) raise ItemUpdated for the playlist;
        // library changes raise events for the audio items.
        if (e.Item is JellyfinPlaylist or Audio)
        {
            _playlistExportService.RequestExport($"{e.Item.GetType().Name} '{e.Item.Name}' changed ({e.UpdateReason})");
        }
    }
}
