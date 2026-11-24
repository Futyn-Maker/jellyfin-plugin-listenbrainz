using System.Globalization;
using System.Text;
using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Resources;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// Delete a contiguous range of items from a playlist (POST playlist/{id}/item/delete).
/// </summary>
public class DeletePlaylistItemsRequest : IListenBrainzRequest
{
    private readonly string _playlistId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistItemsRequest"/> class.
    /// </summary>
    /// <param name="playlistId">Playlist MBID.</param>
    /// <param name="index">Index of the first item to delete.</param>
    /// <param name="count">Number of items to delete.</param>
    public DeletePlaylistItemsRequest(string playlistId, int index, int count)
    {
        _playlistId = playlistId;
        BaseUrl = General.BaseUrl;
        Index = index;
        Count = count;
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
    public string Endpoint => string.Format(CultureInfo.InvariantCulture, CompositeFormat.Parse(Endpoints.PlaylistItemDelete), _playlistId);

    /// <summary>
    /// Gets index of the first item to delete.
    /// </summary>
    [JsonProperty("index")]
    public int Index { get; }

    /// <summary>
    /// Gets number of items to delete.
    /// </summary>
    [JsonProperty("count")]
    public int Count { get; }
}
