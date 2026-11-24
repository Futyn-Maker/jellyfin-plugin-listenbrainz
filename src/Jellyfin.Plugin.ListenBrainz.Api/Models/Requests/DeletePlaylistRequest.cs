using System.Globalization;
using System.Text;
using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Resources;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// Delete playlist request (POST playlist/{id}/delete).
/// </summary>
public class DeletePlaylistRequest : IListenBrainzRequest
{
    private readonly string _playlistId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistRequest"/> class.
    /// </summary>
    /// <param name="playlistId">Playlist MBID.</param>
    public DeletePlaylistRequest(string playlistId)
    {
        _playlistId = playlistId;
        BaseUrl = General.BaseUrl;
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
    public string Endpoint => string.Format(CultureInfo.InvariantCulture, CompositeFormat.Parse(Endpoints.PlaylistDelete), _playlistId);
}
