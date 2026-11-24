using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Responses;

/// <summary>
/// Response of playlist write endpoints (create, edit, delete, item add/delete).
/// </summary>
public class PlaylistWriteResponse : IListenBrainzResponse
{
    /// <inheritdoc />
    [JsonIgnore]
    public bool IsOk { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsNotOk => !IsOk;

    /// <summary>
    /// Gets or sets status reported by the server ("ok" on success).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets MBID of a newly created playlist (create endpoint only).
    /// </summary>
    public string? PlaylistMbid { get; set; }

    /// <summary>
    /// Gets or sets HTTP-like error code reported in the body of a failed request.
    /// </summary>
    public int? Code { get; set; }

    /// <summary>
    /// Gets or sets error message reported in the body of a failed request.
    /// </summary>
    public string? Error { get; set; }
}
