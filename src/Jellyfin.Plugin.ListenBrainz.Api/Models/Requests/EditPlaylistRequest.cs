using System.Globalization;
using System.Text;
using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Resources;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// Edit playlist metadata request (POST playlist/edit/{id}).
/// </summary>
public class EditPlaylistRequest : IListenBrainzRequest
{
    private readonly string _playlistId;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditPlaylistRequest"/> class.
    /// </summary>
    /// <param name="playlistId">Playlist MBID.</param>
    /// <param name="playlist">Playlist payload with title, annotation and/or public flag.</param>
    public EditPlaylistRequest(string playlistId, PlaylistPayload playlist)
    {
        _playlistId = playlistId;
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
    public string Endpoint => string.Format(CultureInfo.InvariantCulture, CompositeFormat.Parse(Endpoints.PlaylistEdit), _playlistId);

    /// <summary>
    /// Gets playlist payload.
    /// </summary>
    [JsonProperty("playlist")]
    public PlaylistPayload Playlist { get; }
}
