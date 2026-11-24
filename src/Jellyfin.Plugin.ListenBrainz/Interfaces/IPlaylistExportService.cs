namespace Jellyfin.Plugin.ListenBrainz.Interfaces;

/// <summary>
/// Exports (mirrors) Jellyfin playlists and, optionally, the whole music library to ListenBrainz playlists.
/// </summary>
public interface IPlaylistExportService
{
    /// <summary>
    /// Request an export run. Requests are debounced, so bursts of library events (e.g. during a scan)
    /// result in a single run shortly after the burst ends.
    /// </summary>
    /// <param name="reason">Reason, for logging.</param>
    public void RequestExport(string reason);

    /// <summary>
    /// Export playlists of all configured users now.
    /// </summary>
    /// <param name="isScheduledRun">Scheduled/manual run: verify every playlist against ListenBrainz and
    /// apply large removals that event-triggered runs hold back.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task ExportAllAsync(bool isScheduledRun, CancellationToken cancellationToken);
}
