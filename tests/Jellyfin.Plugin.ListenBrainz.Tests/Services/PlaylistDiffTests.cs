using Jellyfin.Plugin.ListenBrainz.Services;
using Xunit;

namespace Jellyfin.Plugin.ListenBrainz.Tests.Services;

public class PlaylistDiffTests
{
    private static List<string> L(string s) => s.Select(c => c.ToString()).ToList();

    [Theory]
    [InlineData("", "")]
    [InlineData("", "abc")]
    [InlineData("abc", "")]
    [InlineData("abc", "abc")]
    [InlineData("abcdef", "abxdef")]
    [InlineData("abcdef", "acdef")]
    [InlineData("abcdef", "fedcba")]
    [InlineData("aabbcc", "abcabc")]
    [InlineData("abc", "xyzabcxyz")]
    [InlineData("abcabc", "cba")]
    public void Compute_ApplyYieldsDesired(string current, string desired)
    {
        var script = PlaylistDiff.Compute(L(current), L(desired));
        Assert.NotNull(script);
        Assert.Equal(L(desired), PlaylistDiff.Apply(L(current), script));
    }

    [Fact]
    public void Compute_IdenticalLists_NoOperations()
    {
        var script = PlaylistDiff.Compute(L("abcdef"), L("abcdef"))!;
        Assert.Empty(script.Deletions);
        Assert.Empty(script.Insertions);
    }

    [Fact]
    public void Compute_SingleInsertion_IsOneCall()
    {
        var script = PlaylistDiff.Compute(L("abcdef"), L("abcXdef"))!;
        Assert.Empty(script.Deletions);
        var insertion = Assert.Single(script.Insertions);
        Assert.Equal(3, insertion.Offset);
        Assert.Equal(["X"], insertion.Items);
    }

    [Fact]
    public void Compute_ContiguousRemoval_IsOneCall()
    {
        var script = PlaylistDiff.Compute(L("abcdefgh"), L("abgh"))!;
        var deletion = Assert.Single(script.Deletions);
        Assert.Equal(new PlaylistDeletion(2, 4), deletion);
        Assert.Empty(script.Insertions);
    }

    [Fact]
    public void Compute_RandomizedLists_ApplyYieldsDesired()
    {
        var random = new Random(12345);
        for (int round = 0; round < 500; round++)
        {
            var alphabet = Enumerable.Range(0, random.Next(1, 30)).Select(i => $"m{i}").ToList();
            var current = Enumerable.Range(0, random.Next(0, 60)).Select(_ => alphabet[random.Next(alphabet.Count)]).ToList();
            var desired = Enumerable.Range(0, random.Next(0, 60)).Select(_ => alphabet[random.Next(alphabet.Count)]).ToList();
            var script = PlaylistDiff.Compute(current, desired);
            Assert.NotNull(script);
            Assert.Equal(desired, PlaylistDiff.Apply(current, script));
        }
    }

    [Fact]
    public void Compute_LargeListWithSmallChange_TrimsPrefixAndSuffix()
    {
        var current = Enumerable.Range(0, 20_000).Select(i => $"r{i}").ToList();
        var desired = current.ToList();
        desired.Insert(12_345, "new");
        desired.RemoveAt(500);
        var script = PlaylistDiff.Compute(current, desired);
        Assert.NotNull(script);
        Assert.Equal(desired, PlaylistDiff.Apply(current, script));
        Assert.Equal(1, script.DeletedCount);
        Assert.Equal(1, script.InsertedCount);
    }

    [Fact]
    public void Compute_HugeUniqueLists_UsesFastPath()
    {
        var random = new Random(7);
        var current = Enumerable.Range(0, 5_000).Select(i => $"r{i}").ToList();
        var desired = current.Where(_ => random.Next(10) > 0).OrderBy(_ => random.Next(100) == 0 ? random.Next() : 0).ToList();
        desired.InsertRange(1_000, Enumerable.Range(0, 300).Select(i => $"new{i}"));
        var script = PlaylistDiff.Compute(current, desired);
        Assert.NotNull(script);
        Assert.Equal(desired, PlaylistDiff.Apply(current, script));
    }

    [Fact]
    public void Compute_HugeListsWithDuplicates_ReturnsNull()
    {
        var current = Enumerable.Range(0, 5_000).Select(i => $"a{i % 2_000}").ToList();
        var desired = Enumerable.Range(0, 5_000).Select(i => $"b{i % 2_000}").ToList();
        Assert.Null(PlaylistDiff.Compute(current, desired));
    }
}
