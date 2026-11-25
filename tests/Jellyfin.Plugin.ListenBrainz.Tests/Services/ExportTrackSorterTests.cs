using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.ListenBrainz.Configuration;
using Jellyfin.Plugin.ListenBrainz.Services;
using MediaBrowser.Controller.Entities.Audio;
using Xunit;

namespace Jellyfin.Plugin.ListenBrainz.Tests.Services;

public class ExportTrackSorterTests
{
    private static Audio Track(string title, string artist, string album, int track, int addedDay) => new()
    {
        Name = title,
        SortName = title,
        Artists = [artist],
        Album = album,
        IndexNumber = track,
        DateCreated = new DateTime(2026, 1, addedDay, 12, 0, 0, DateTimeKind.Utc),
        Path = $"/music/{artist}/{album}/{track:00} {title}.flac",
    };

    // Source order: deliberately none of the sort orders.
    private static List<Audio> Tracks() =>
    [
        Track("Echo", "Beta", "Second", 2, 3),
        Track("Alpha", "Alpha", "First", 1, 1),
        Track("Delta", "Beta", "Second", 1, 3),
        Track("Charlie", "Alpha", "First", 2, 2),
    ];

    private static List<string> Titles(IEnumerable<Audio> items) => items.Select(i => i.Name).ToList();

    [Fact]
    public void Sort_SourceOrder_KeepsTheGivenOrder()
    {
        Assert.Equal(["Echo", "Alpha", "Delta", "Charlie"], Titles(ExportTrackSorter.Sort(Tracks(), ExportTrackOrder.SourceOrder)));
    }

    [Fact]
    public void Sort_ArtistAlbumTrack()
    {
        Assert.Equal(["Alpha", "Charlie", "Delta", "Echo"], Titles(ExportTrackSorter.Sort(Tracks(), ExportTrackOrder.ArtistAlbumTrack)));
    }

    [Fact]
    public void Sort_DateAddedNewestFirst_TiesInTrackOrder()
    {
        Assert.Equal(["Delta", "Echo", "Charlie", "Alpha"], Titles(ExportTrackSorter.Sort(Tracks(), ExportTrackOrder.DateAddedNewestFirst)));
    }

    [Fact]
    public void Sort_DateAddedOldestFirst_TiesInTrackOrder()
    {
        Assert.Equal(["Alpha", "Charlie", "Delta", "Echo"], Titles(ExportTrackSorter.Sort(Tracks(), ExportTrackOrder.DateAddedOldestFirst)));
    }

    [Fact]
    public void Sort_Title()
    {
        var tracks = Tracks();
        tracks.Add(Track("alpha", "Zulu", "Last", 1, 4));
        Assert.Equal(["Alpha", "alpha", "Charlie", "Delta", "Echo"], Titles(ExportTrackSorter.Sort(tracks, ExportTrackOrder.Title)));
    }

    [Fact]
    public void Sort_KeepsDuplicates()
    {
        var tracks = Tracks();
        tracks.Add(tracks[0]);
        Assert.Equal(5, ExportTrackSorter.Sort(tracks, ExportTrackOrder.DateAddedNewestFirst).Count);
    }
}
