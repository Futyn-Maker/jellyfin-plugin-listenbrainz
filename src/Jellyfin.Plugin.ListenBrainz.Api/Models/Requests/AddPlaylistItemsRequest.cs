using System.Globalization;
using System.Text;
using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Resources;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// Add items to a playlist (POST playlist/{id}/item/add[/{offset}]).
/// </summary>
public class AddPlaylistItemsRequest : IListenBrainzRequest
{
    private readonly string _playlistId;
    private readonly int? _offset;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistItemsRequest"/> class.
    /// </summary>
    /// <param name="playlistId">Playlist MBID.</param>
    /// <param name="recordingMbids">Recording MBIDs to add (at most <see cref="Limits.MaxRecordingsPerPlaylistAdd"/>).</param>
    /// <param name="offset">Insert position. Appends to the end if null.</param>
    public AddPlaylistItemsRequest(string playlistId, IEnumerable<string> recordingMbids, int? offset = null)
    {
        _playlistId = playlistId;
        _offset = offset;
        BaseUrl = General.BaseUrl;
        Playlist = new PlaylistPayload().WithRecordings(recordingMbids);
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
    public string Endpoint => _offset is null
        ? string.Format(CultureInfo.InvariantCulture, CompositeFormat.Parse(Endpoints.PlaylistItemAdd), _playlistId)
        : string.Format(CultureInfo.InvariantCulture, CompositeFormat.Parse(Endpoints.PlaylistItemAddAtOffset), _playlistId, _offset.Value);

    /// <summary>
    /// Gets playlist payload containing the tracks to add.
    /// </summary>
    [JsonProperty("playlist")]
    public PlaylistPayload Playlist { get; }
}
