using Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;
using Jellyfin.Plugin.ListenBrainz.Api.Models.Responses;

namespace Jellyfin.Plugin.ListenBrainz.Api.Interfaces;

/// <summary>
/// ListenBrainz API client.
/// </summary>
public interface IListenBrainzApiClient
{
    /// <summary>
    /// Validate provided token.
    /// </summary>
    /// <param name="request">Validate token request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Submit listens.
    /// </summary>
    /// <param name="request">Submit listens request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<SubmitListensResponse> SubmitListens(SubmitListensRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Submit a recording feedback.
    /// </summary>
    /// <param name="request">Recording feedback request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<RecordingFeedbackResponse> SubmitRecordingFeedback(RecordingFeedbackRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get listens of a specified user.
    /// </summary>
    /// <param name="request">User listens request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<GetUserListensResponse> GetUserListens(GetUserListensRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get listen(s) feedback of a specified user.
    /// </summary>
    /// <param name="request">User feedback request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<GetUserFeedbackResponse> GetUserFeedback(GetUserFeedbackRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get playlists that have been created for a specified user.
    /// </summary>
    /// <param name="request">'Created for' playlists request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<GetCreatedForPlaylistsResponse> GetCreatedForPlaylists(GetCreatedForPlaylistsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get detailed information for a specific playlist.
    /// </summary>
    /// <param name="request">Playlist details request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<GetPlaylistResponse> GetPlaylist(GetPlaylistRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get playlists created by a user.
    /// </summary>
    /// <param name="request">User playlists request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<GetUserPlaylistsResponse> GetUserPlaylists(GetUserPlaylistsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Create a playlist.
    /// </summary>
    /// <param name="request">Create playlist request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response with the new playlist MBID.</returns>
    public Task<PlaylistWriteResponse> CreatePlaylist(CreatePlaylistRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Edit playlist metadata (title, annotation, public flag).
    /// </summary>
    /// <param name="request">Edit playlist request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<PlaylistWriteResponse> EditPlaylist(EditPlaylistRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a playlist.
    /// </summary>
    /// <param name="request">Delete playlist request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<PlaylistWriteResponse> DeletePlaylist(DeletePlaylistRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Add (append or insert) items to a playlist.
    /// </summary>
    /// <param name="request">Add items request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<PlaylistWriteResponse> AddPlaylistItems(AddPlaylistItemsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete a range of items from a playlist.
    /// </summary>
    /// <param name="request">Delete items request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Request response.</returns>
    public Task<PlaylistWriteResponse> DeletePlaylistItems(DeletePlaylistItemsRequest request, CancellationToken cancellationToken);
}
