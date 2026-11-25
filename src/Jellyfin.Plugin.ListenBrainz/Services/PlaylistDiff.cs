namespace Jellyfin.Plugin.ListenBrainz.Services;

/// <summary>
/// Computes a minimal edit script that turns one ordered list of recording MBIDs into another,
/// expressed with the operations offered by the ListenBrainz playlist API: deleting a contiguous
/// range of items and inserting a contiguous run of items at an offset.
/// </summary>
public static class PlaylistDiff
{
    /// <summary>
    /// Upper bound of LCS matrix cells (after trimming the common prefix/suffix). Above this the
    /// caller should simply rebuild the playlist.
    /// </summary>
    public const long MaxLcsCells = 16_000_000;

    /// <summary>
    /// Compute an edit script. Deletions are ordered from the highest index to the lowest and must be applied first;
    /// insertions are ordered by ascending offset and must be applied afterwards, in order.
    /// </summary>
    /// <param name="current">Current list.</param>
    /// <param name="desired">Desired list.</param>
    /// <returns>Edit script, or null if the lists are too large to diff (rebuild instead).</returns>
    public static PlaylistEditScript? Compute(IReadOnlyList<string> current, IReadOnlyList<string> desired)
    {
        int prefix = 0;
        while (prefix < current.Count && prefix < desired.Count &&
               string.Equals(current[prefix], desired[prefix], StringComparison.OrdinalIgnoreCase))
        {
            prefix++;
        }

        int suffix = 0;
        while (suffix < current.Count - prefix && suffix < desired.Count - prefix &&
               string.Equals(current[current.Count - 1 - suffix], desired[desired.Count - 1 - suffix], StringComparison.OrdinalIgnoreCase))
        {
            suffix++;
        }

        int n = current.Count - prefix - suffix;
        int m = desired.Count - prefix - suffix;

        // keepCurrent[i] / keepDesired[j]: element is part of the LCS (indices relative to the middle part)
        var keepCurrent = new bool[n];
        var keepDesired = new bool[m];
        if (n > 0 && m > 0)
        {
            if (!MarkLcsOfUniqueLists(current, desired, prefix, n, m, keepCurrent, keepDesired))
            {
                if ((long)(n + 1) * (m + 1) > MaxLcsCells)
                {
                    return null;
                }

                MarkLcsByDynamicProgramming(current, desired, prefix, n, m, keepCurrent, keepDesired);
            }
        }

        var deletions = new List<PlaylistDeletion>();
        int run = -1;
        for (int i = 0; i <= n; i++)
        {
            bool deleted = i < n && !keepCurrent[i];
            if (deleted && run < 0)
            {
                run = i;
            }
            else if (!deleted && run >= 0)
            {
                deletions.Add(new PlaylistDeletion(prefix + run, i - run));
                run = -1;
            }
        }

        deletions.Reverse();

        var insertions = new List<PlaylistInsertion>();
        run = -1;
        for (int j = 0; j <= m; j++)
        {
            bool inserted = j < m && !keepDesired[j];
            if (inserted && run < 0)
            {
                run = j;
            }
            else if (!inserted && run >= 0)
            {
                var items = new List<string>(j - run);
                for (int k = run; k < j; k++)
                {
                    items.Add(desired[prefix + k]);
                }

                insertions.Add(new PlaylistInsertion(prefix + run, items));
                run = -1;
            }
        }

        return new PlaylistEditScript(deletions, insertions);
    }

    /// <summary>
    /// Count the items of <paramref name="current"/> that <paramref name="desired"/> no longer contains,
    /// taking duplicates into account. Items that only move are not counted.
    /// </summary>
    /// <param name="current">Current list.</param>
    /// <param name="desired">Desired list.</param>
    /// <returns>Number of items that would disappear.</returns>
    public static int CountDropped(IReadOnlyList<string> current, IReadOnlyList<string> desired)
    {
        var remaining = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in desired)
        {
            remaining[item] = remaining.GetValueOrDefault(item) + 1;
        }

        int dropped = 0;
        foreach (var item in current)
        {
            if (remaining.TryGetValue(item, out var count) && count > 0)
            {
                remaining[item] = count - 1;
            }
            else
            {
                dropped++;
            }
        }

        return dropped;
    }

    /// <summary>
    /// Apply an edit script to a list (used for verification and tests).
    /// </summary>
    /// <param name="current">Current list.</param>
    /// <param name="script">Edit script.</param>
    /// <returns>Resulting list.</returns>
    public static IReadOnlyList<string> Apply(IReadOnlyList<string> current, PlaylistEditScript script)
    {
        var list = current.ToList();
        foreach (var d in script.Deletions)
        {
            list.RemoveRange(d.Index, d.Count);
        }

        foreach (var ins in script.Insertions)
        {
            list.InsertRange(ins.Offset, ins.Items);
        }

        return list;
    }

    /// <summary>
    /// LCS of two lists without duplicates (e.g. the de-duplicated library playlist) in O(k log k):
    /// matched pairs (i, j) sorted by i form an LCS exactly when their j values increase, so the LCS is the
    /// longest increasing subsequence of the j values.
    /// </summary>
    private static bool MarkLcsOfUniqueLists(
        IReadOnlyList<string> current,
        IReadOnlyList<string> desired,
        int prefix,
        int n,
        int m,
        bool[] keepCurrent,
        bool[] keepDesired)
    {
        var desiredIndex = new Dictionary<string, int>(m, StringComparer.OrdinalIgnoreCase);
        for (int j = 0; j < m; j++)
        {
            if (!desiredIndex.TryAdd(desired[prefix + j], j))
            {
                return false;
            }
        }

        var seen = new HashSet<string>(n, StringComparer.OrdinalIgnoreCase);
        var pairs = new List<(int I, int J)>();
        for (int i = 0; i < n; i++)
        {
            var value = current[prefix + i];
            if (!seen.Add(value))
            {
                return false;
            }

            if (desiredIndex.TryGetValue(value, out var j))
            {
                pairs.Add((i, j));
            }
        }

        // Patience sorting: tails[k] = index (into pairs) of the smallest tail of an increasing run of length k + 1.
        var tails = new List<int>();
        var previous = new int[pairs.Count];
        for (int p = 0; p < pairs.Count; p++)
        {
            int lo = 0, hi = tails.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (pairs[tails[mid]].J < pairs[p].J)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            previous[p] = lo > 0 ? tails[lo - 1] : -1;
            if (lo == tails.Count)
            {
                tails.Add(p);
            }
            else
            {
                tails[lo] = p;
            }
        }

        for (int p = tails.Count > 0 ? tails[^1] : -1; p >= 0; p = previous[p])
        {
            keepCurrent[pairs[p].I] = true;
            keepDesired[pairs[p].J] = true;
        }

        return true;
    }

    private static void MarkLcsByDynamicProgramming(
        IReadOnlyList<string> current,
        IReadOnlyList<string> desired,
        int prefix,
        int n,
        int m,
        bool[] keepCurrent,
        bool[] keepDesired)
    {
        int width = m + 1;
        var lcs = new int[(n + 1) * width];
        for (int i = n - 1; i >= 0; i--)
        {
            for (int j = m - 1; j >= 0; j--)
            {
                lcs[(i * width) + j] = string.Equals(current[prefix + i], desired[prefix + j], StringComparison.OrdinalIgnoreCase)
                    ? lcs[((i + 1) * width) + j + 1] + 1
                    : Math.Max(lcs[((i + 1) * width) + j], lcs[(i * width) + j + 1]);
            }
        }

        int a = 0, b = 0;
        while (a < n && b < m)
        {
            if (string.Equals(current[prefix + a], desired[prefix + b], StringComparison.OrdinalIgnoreCase))
            {
                keepCurrent[a++] = true;
                keepDesired[b++] = true;
            }
            else if (lcs[((a + 1) * width) + b] >= lcs[(a * width) + b + 1])
            {
                a++;
            }
            else
            {
                b++;
            }
        }
    }
}
