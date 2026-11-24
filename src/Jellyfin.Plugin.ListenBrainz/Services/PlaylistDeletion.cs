namespace Jellyfin.Plugin.ListenBrainz.Services;

/// <summary>
/// Deletion of a contiguous range of playlist items.
/// </summary>
/// <param name="Index">Index of the first item.</param>
/// <param name="Count">Number of items.</param>
public sealed record PlaylistDeletion(int Index, int Count);
