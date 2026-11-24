namespace Jellyfin.Plugin.ListenBrainz.Services;

/// <summary>
/// Edit script produced by <see cref="PlaylistDiff"/>.
/// </summary>
/// <param name="Deletions">Deletions, highest index first.</param>
/// <param name="Insertions">Insertions, lowest offset first.</param>
public sealed record PlaylistEditScript(IReadOnlyList<PlaylistDeletion> Deletions, IReadOnlyList<PlaylistInsertion> Insertions)
{
    /// <summary>
    /// Gets number of deleted items.
    /// </summary>
    public int DeletedCount => Deletions.Sum(d => d.Count);

    /// <summary>
    /// Gets number of inserted items.
    /// </summary>
    public int InsertedCount => Insertions.Sum(i => i.Items.Count);

    /// <summary>
    /// Number of API calls needed to apply this script.
    /// </summary>
    /// <param name="maxPerAdd">Maximum number of items per add request.</param>
    /// <returns>Number of calls.</returns>
    public int CallCount(int maxPerAdd) =>
        Deletions.Count + Insertions.Sum(i => (i.Items.Count + maxPerAdd - 1) / maxPerAdd);
}
