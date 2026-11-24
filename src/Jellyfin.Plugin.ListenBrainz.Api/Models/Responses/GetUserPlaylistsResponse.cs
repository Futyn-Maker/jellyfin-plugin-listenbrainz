using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Responses;

/// <summary>
/// Playlists created by a user.
/// </summary>
public class GetUserPlaylistsResponse : IListenBrainzResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPlaylistsResponse"/> class.
    /// </summary>
    public GetUserPlaylistsResponse()
    {
        WrappedPlaylists = new List<WrappedPlaylist>();
    }

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsOk { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsNotOk => !IsOk;

    /// <summary>
    /// Gets or sets number of playlists in this response.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets offset of this response.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Gets or sets total number of playlists of the user.
    /// </summary>
    public int PlaylistCount { get; set; }

    /// <summary>
    /// Gets playlists.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Playlist> Playlists => WrappedPlaylists.Select(wp => wp.Playlist);

    [JsonProperty("playlists")]
    private IEnumerable<WrappedPlaylist> WrappedPlaylists { get; set; }
}
