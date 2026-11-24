using System.Globalization;
using System.Text;
using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Resources;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// Create playlist request (POST playlist/create).
/// </summary>
public class CreatePlaylistRequest : IListenBrainzRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistRequest"/> class.
    /// </summary>
    /// <param name="playlist">Playlist payload. Title and public flag are required by the server.</param>
    public CreatePlaylistRequest(PlaylistPayload playlist)
    {
        BaseUrl = General.BaseUrl;
        Playlist = playlist;
    }

    /// <inheritdoc />
    [JsonIgnore]
    public string? ApiToken { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public string BaseUrl { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public Dictionary<string, string> QueryDict { get; } = new();

    /// <inheritdoc />
    [JsonIgnore]
    public string Endpoint => Endpoints.PlaylistCreate;

    /// <summary>
    /// Gets playlist payload.
    /// </summary>
    [JsonProperty("playlist")]
    public PlaylistPayload Playlist { get; }
}
