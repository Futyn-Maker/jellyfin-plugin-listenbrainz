using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;

/// <summary>
/// JSPF track payload.
/// </summary>
public class PlaylistTrackPayload
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistTrackPayload"/> class.
    /// </summary>
    public PlaylistTrackPayload()
    {
        Identifier = new List<string>();
    }

    /// <summary>
    /// Gets or sets track identifiers (MusicBrainz recording URLs).
    /// </summary>
    [JsonProperty("identifier")]
    [SuppressMessage("Warning", "CA2227", Justification = "Serialization DTO")]
    public IList<string> Identifier { get; set; }

    /// <summary>
    /// Create a track payload from a recording MBID.
    /// </summary>
    /// <param name="recordingMbid">Recording MBID.</param>
    /// <returns>Track payload.</returns>
    public static PlaylistTrackPayload FromRecordingMbid(string recordingMbid) => new()
    {
        Identifier = new List<string> { "https://musicbrainz.org/recording/" + recordingMbid },
    };
}
