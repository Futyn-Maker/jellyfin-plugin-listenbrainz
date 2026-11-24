using Jellyfin.Plugin.ListenBrainz.Interfaces;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ListenBrainz.Tasks;

/// <summary>
/// Export (mirror) Jellyfin playlists and the whole library to ListenBrainz playlists.
/// Changes are also exported automatically shortly after they happen; this task is the periodic
/// full verification and can be run manually.
/// </summary>
public class ExportPlaylistsTask : IScheduledTask
{
    private readonly ILogger _logger;
    private readonly IPlaylistExportService _exportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportPlaylistsTask"/> class.
    /// </summary>
    /// <param name="loggerFactory">Logger factory.</param>
    /// <param name="exportService">Playlist export service.</param>
    public ExportPlaylistsTask(ILoggerFactory loggerFactory, IPlaylistExportService exportService)
    {
        _logger = loggerFactory.CreateLogger($"{Plugin.LoggerCategory}.ExportPlaylistsTask");
        _exportService = exportService;
    }

    /// <inheritdoc />
    public string Name => "Export playlists to ListenBrainz";

    /// <inheritdoc />
    public string Key => "ExportPlaylistsToListenBrainz";

    /// <inheritdoc />
    public string Description => "Mirror Jellyfin playlists (and optionally the whole music library) to ListenBrainz playlists.";

    /// <inheritdoc />
    public string Category => "ListenBrainz";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting playlist export to ListenBrainz");
        await _exportService.ExportAllAsync(isScheduledRun: true, cancellationToken);
        progress.Report(100);
        _logger.LogInformation("Playlist export to ListenBrainz finished");
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() =>
    [
        new TaskTriggerInfo { Type = TaskTriggerInfoType.StartupTrigger },
        new TaskTriggerInfo { Type = TaskTriggerInfoType.IntervalTrigger, IntervalTicks = TimeSpan.FromHours(6).Ticks },
    ];
}
