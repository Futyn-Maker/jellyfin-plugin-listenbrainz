namespace Jellyfin.Plugin.ListenBrainz.Dtos;

/// <summary>
/// One exported playlist: a Jellyfin source mirrored to a ListenBrainz playlist.
/// </summary>
public class PlaylistExportEntry
{
    /// <summary>
    /// Gets or sets the ListenBrainz playlist MBID.
    /// </summary>
    public string ListenBrainzPlaylistId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last exported title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets hash of the last successfully exported content (title, annotation, visibility, tracks).
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets number of exported tracks.
    /// </summary>
    public int TrackCount { get; set; }

    /// <summary>
    /// Gets or sets number of source tracks skipped because they have no MusicBrainz recording ID.
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// Gets or sets time of the last successful export.
    /// </summary>
    public DateTime LastSyncedUtc { get; set; }

    /// <summary>
    /// Gets or sets time the ListenBrainz copy was last compared with the source.
    /// </summary>
    public DateTime LastVerifiedUtc { get; set; }

    /// <summary>
    /// Gets or sets time since when the source Jellyfin playlist has been missing (null if present).
    /// </summary>
    public DateTime? MissingSinceUtc { get; set; }
}
