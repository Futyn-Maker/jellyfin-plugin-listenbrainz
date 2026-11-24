using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Responses;

/// <summary>
/// Playlist response.
/// </summary>
public class GetPlaylistResponse : IListenBrainzResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistResponse"/> class.
    /// </summary>
    public GetPlaylistResponse()
    {
        Playlist = new();
    }

    /// <inheritdoc />
    public bool IsOk { get; set; }

    /// <inheritdoc />
    public bool IsNotOk => !IsOk;

    /// <summary>
    /// Gets or sets playlist data.
    /// </summary>
    public Playlist Playlist { get; set; }

    /// <summary>
    /// Gets or sets error code reported in the body of a failed request (e.g. 404 for a deleted playlist).
    /// </summary>
    public int? Code { get; set; }

    /// <summary>
    /// Gets or sets error message reported in the body of a failed request.
    /// </summary>
    public string? Error { get; set; }
}
