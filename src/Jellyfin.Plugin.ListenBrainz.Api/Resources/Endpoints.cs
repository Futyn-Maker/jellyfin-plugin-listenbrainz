namespace Jellyfin.Plugin.ListenBrainz.Api.Resources;

/// <summary>
/// ListenBrainz API endpoints.
/// </summary>
public static class Endpoints
{
    /// <summary>
    /// Endpoint for submitting listens.
    /// </summary>
    public const string SubmitListens = "submit-listens";

    /// <summary>
    /// Endpoint for token validation.
    /// </summary>
    public const string ValidateToken = "validate-token";

    /// <summary>
    /// Feedback endpoint base.
    /// </summary>
    private const string FeedbackEndpointBase = "feedback";

    /// <summary>
    /// Endpoint for recording feedback.
    /// </summary>
    public const string RecordingFeedback = FeedbackEndpointBase + "/recording-feedback";

    /// <summary>
    /// Endpoint for user feedback.
    /// </summary>
    public const string UserFeedback = FeedbackEndpointBase + "/user/{0}/get-feedback";

    /// <summary>
    /// User endpoint base.
    /// </summary>
    private const string UserEndpointBase = "user";

    /// <summary>
    /// Endpoint for user listens.
    /// </summary>
    public const string ListensEndpoint = UserEndpointBase + "/{0}/listens";

    /// <summary>
    /// Edpoint for user collaborator playlists.
    /// </summary>
    public const string CreatedForPlaylists = UserEndpointBase + "/{0}/playlists/createdfor";

    /// <summary>
    /// Endpoint for playlist details.
    /// </summary>
    public const string PlaylistDetails = "playlist/{0}";

    /// <summary>
    /// Endpoint for playlists created by a user.
    /// </summary>
    public const string UserPlaylists = UserEndpointBase + "/{0}/playlists";

    /// <summary>
    /// Endpoint for creating a playlist.
    /// </summary>
    public const string PlaylistCreate = "playlist/create";

    /// <summary>
    /// Endpoint for editing playlist metadata.
    /// </summary>
    public const string PlaylistEdit = "playlist/edit/{0}";

    /// <summary>
    /// Endpoint for deleting a playlist.
    /// </summary>
    public const string PlaylistDelete = "playlist/{0}/delete";

    /// <summary>
    /// Endpoint for appending items to a playlist.
    /// </summary>
    public const string PlaylistItemAdd = "playlist/{0}/item/add";

    /// <summary>
    /// Endpoint for inserting items into a playlist at a specific offset.
    /// </summary>
    public const string PlaylistItemAddAtOffset = "playlist/{0}/item/add/{1}";

    /// <summary>
    /// Endpoint for deleting items from a playlist.
    /// </summary>
    public const string PlaylistItemDelete = "playlist/{0}/item/delete";
}
