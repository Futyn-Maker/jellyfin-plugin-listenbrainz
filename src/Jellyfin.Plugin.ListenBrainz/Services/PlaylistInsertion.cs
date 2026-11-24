namespace Jellyfin.Plugin.ListenBrainz.Services;

/// <summary>
/// Insertion of a contiguous run of playlist items.
/// </summary>
/// <param name="Offset">Insert position.</param>
/// <param name="Items">Recording MBIDs to insert.</param>
public sealed record PlaylistInsertion(int Offset, IReadOnlyList<string> Items);
