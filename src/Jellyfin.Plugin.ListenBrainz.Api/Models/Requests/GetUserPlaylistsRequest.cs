using System.Globalization;
using System.Text;
using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Resources;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// Get playlists created by a user (GET user/{name}/playlists). Private playlists are included when the token belongs to the user.
/// </summary>
public class GetUserPlaylistsRequest : IListenBrainzRequest
{
    private readonly string _userName;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPlaylistsRequest"/> class.
    /// </summary>
    /// <param name="userName">ListenBrainz user name.</param>
    /// <param name="count">Number of playlists to fetch.</param>
    /// <param name="offset">Offset.</param>
    public GetUserPlaylistsRequest(string userName, int count = Limits.DefaultItemsPerGet, int offset = 0)
    {
        _userName = userName;
        BaseUrl = General.BaseUrl;
        QueryDict = new Dictionary<string, string>
        {
            { "count", count.ToString(NumberFormatInfo.InvariantInfo) },
            { "offset", offset.ToString(NumberFormatInfo.InvariantInfo) },
        };
    }

    /// <inheritdoc />
    [JsonIgnore]
    public string? ApiToken { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public string BaseUrl { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public Dictionary<string, string> QueryDict { get; }

    /// <inheritdoc />
    [JsonIgnore]
    public string Endpoint => string.Format(CultureInfo.InvariantCulture, CompositeFormat.Parse(Endpoints.UserPlaylists), _userName);
}
