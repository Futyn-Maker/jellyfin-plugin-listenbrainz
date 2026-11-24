using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// JSPF playlist payload used by playlist write endpoints (create, edit, item/add).
/// Unset (null) properties are omitted from the serialized JSON.
/// </summary>
public class PlaylistPayload
{
    /// <summary>
    /// JSPF extension key for ListenBrainz playlist metadata.
    /// </summary>
    public const string JspfPlaylistExtensionKey = "https://musicbrainz.org/doc/jspf#playlist";

    /// <summary>
    /// Gets or sets playlist title.
    /// </summary>
    [JsonProperty("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets playlist annotation (description). ListenBrainz accepts basic HTML.
    /// </summary>
    [JsonProperty("annotation")]
    public string? Annotation { get; set; }

    /// <summary>
    /// Gets or sets JSPF extension data.
    /// </summary>
    [JsonProperty("extension")]
    [SuppressMessage("Warning", "CA2227", Justification = "Serialization DTO")]
    public Dictionary<string, Dictionary<string, object>>? Extension { get; set; }

    /// <summary>
    /// Gets or sets playlist tracks.
    /// </summary>
    [JsonProperty("track")]
    [SuppressMessage("Warning", "CA2227", Justification = "Serialization DTO")]
    public IList<PlaylistTrackPayload>? Track { get; set; }

    /// <summary>
    /// Set the "public" flag of the playlist.
    /// </summary>
    /// <param name="isPublic">Whether the playlist should be public.</param>
    /// <returns>This instance.</returns>
    public PlaylistPayload WithPublic(bool isPublic)
    {
        Extension ??= new Dictionary<string, Dictionary<string, object>>();
        if (!Extension.TryGetValue(JspfPlaylistExtensionKey, out var ext))
        {
            ext = new Dictionary<string, object>();
            Extension[JspfPlaylistExtensionKey] = ext;
        }

        ext["public"] = isPublic;
        return this;
    }

    /// <summary>
    /// Set playlist tracks from recording MBIDs.
    /// </summary>
    /// <param name="recordingMbids">Recording MBIDs.</param>
    /// <returns>This instance.</returns>
    public PlaylistPayload WithRecordings(IEnumerable<string> recordingMbids)
    {
        Track = recordingMbids.Select(PlaylistTrackPayload.FromRecordingMbid).ToList();
        return this;
    }
}
