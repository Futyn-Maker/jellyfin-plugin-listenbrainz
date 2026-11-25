using System.Globalization;
using Jellyfin.Plugin.ListenBrainz.Configuration;
using MediaBrowser.Controller.Entities.Audio;

namespace Jellyfin.Plugin.ListenBrainz.Services;

/// <summary>
/// Puts the tracks of an exported playlist in the configured order.
/// </summary>
public static class ExportTrackSorter
{
    private const CompareOptions TextOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private static readonly CompareInfo TextCompareInfo = CultureInfo.InvariantCulture.CompareInfo;

    private static readonly Comparer<Audio> ArtistAlbumTrack = Comparer<Audio>.Create(CompareArtistAlbumTrack);

    private static readonly Comparer<string> Text = Comparer<string>.Create((a, b) => TextCompareInfo.Compare(a, b, TextOptions));

    /// <summary>
    /// Orders tracks. The sort is stable and fully deterministic: <see cref="ExportTrackOrder.SourceOrder"/> keeps
    /// the given order, and tracks that are equal under another order are ordered by artist, album and track number.
    /// </summary>
    /// <param name="items">Tracks.</param>
    /// <param name="order">Requested order.</param>
    /// <returns>Ordered tracks.</returns>
    public static IReadOnlyList<Audio> Sort(IEnumerable<Audio> items, ExportTrackOrder order) => order switch
    {
        ExportTrackOrder.ArtistAlbumTrack => items.OrderBy(x => x, ArtistAlbumTrack).ToList(),
        ExportTrackOrder.DateAddedNewestFirst => items.OrderByDescending(x => x.DateCreated).ThenBy(x => x, ArtistAlbumTrack).ToList(),
        ExportTrackOrder.DateAddedOldestFirst => items.OrderBy(x => x.DateCreated).ThenBy(x => x, ArtistAlbumTrack).ToList(),
        ExportTrackOrder.Title => items.OrderBy(Title, Text).ThenBy(x => x, ArtistAlbumTrack).ToList(),
        _ => items.ToList(),
    };

    private static string Title(Audio x) => x.SortName ?? x.Name ?? string.Empty;

    private static int CompareArtistAlbumTrack(Audio? a, Audio? b)
    {
        if (ReferenceEquals(a, b))
        {
            return 0;
        }

        if (a is null || b is null)
        {
            return a is null ? -1 : 1;
        }

        static string Artist(Audio x) => x.Artists.FirstOrDefault() ?? x.AlbumArtists.FirstOrDefault() ?? string.Empty;

        int c = TextCompareInfo.Compare(Artist(a), Artist(b), TextOptions);
        if (c != 0)
        {
            return c;
        }

        c = TextCompareInfo.Compare(a.Album ?? string.Empty, b.Album ?? string.Empty, TextOptions);
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

        c = TextCompareInfo.Compare(Title(a), Title(b), TextOptions);
        return c != 0 ? c : string.CompareOrdinal(a.Path, b.Path);
    }
}
