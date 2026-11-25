namespace Jellyfin.Plugin.ListenBrainz.Configuration;

/// <summary>
/// Order of the tracks of a playlist exported to ListenBrainz.
/// </summary>
public enum ExportTrackOrder
{
    /// <summary>
    /// The order of the source: the item order of the Jellyfin playlist. The library playlist has no order
    /// of its own and uses <see cref="ArtistAlbumTrack"/>.
    /// </summary>
    SourceOrder = 0,

    /// <summary>
    /// By the first artist of each track, then album, disc and track number.
    /// </summary>
    ArtistAlbumTrack = 1,

    /// <summary>
    /// By the date the track was added to Jellyfin, newest first.
    /// </summary>
    DateAddedNewestFirst = 2,

    /// <summary>
    /// By the date the track was added to Jellyfin, oldest first.
    /// </summary>
    DateAddedOldestFirst = 3,

    /// <summary>
    /// By track title.
    /// </summary>
    Title = 4,
}
