using System.Diagnostics.CodeAnalysis;

namespace Jellyfin.Plugin.ListenBrainz.Dtos;

/// <summary>
/// Persisted state of the Jellyfin -> ListenBrainz playlist export.
/// </summary>
public class PlaylistExportState
{
    /// <summary>
    /// Gets or sets exported playlists per Jellyfin user, keyed by export key
    /// ("library" for the whole-library playlist, "playlist:{id}" for Jellyfin playlists).
    /// </summary>
    [SuppressMessage("Warning", "CA2227", Justification = "Needed for deserialization")]
    public Dictionary<Guid, Dictionary<string, PlaylistExportEntry>> Users { get; set; } = new();
}
