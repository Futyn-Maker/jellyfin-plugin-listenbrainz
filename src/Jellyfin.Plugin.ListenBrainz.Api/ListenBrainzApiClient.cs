using Jellyfin.Plugin.ListenBrainz.Api.Interfaces;
using Jellyfin.Plugin.ListenBrainz.Api.Models.Requests;
using Jellyfin.Plugin.ListenBrainz.Api.Models.Responses;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ListenBrainz.Api;

/// <summary>
/// ListenBrainz API client.
/// </summary>
public class ListenBrainzApiClient : IListenBrainzApiClient
{
    private readonly IBaseApiClient _apiClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenBrainzApiClient"/> class.
    /// </summary>
    /// <param name="apiClient">Underlying base client.</param>
    /// <param name="logger">Logger instance.</param>
    public ListenBrainzApiClient(IBaseApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendGetRequest<ValidateTokenRequest, ValidateTokenResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SubmitListensResponse> SubmitListens(SubmitListensRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendPostRequest<SubmitListensRequest, SubmitListensResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RecordingFeedbackResponse> SubmitRecordingFeedback(RecordingFeedbackRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendPostRequest<RecordingFeedbackRequest, RecordingFeedbackResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetUserListensResponse> GetUserListens(GetUserListensRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendGetRequest<GetUserListensRequest, GetUserListensResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetUserFeedbackResponse> GetUserFeedback(GetUserFeedbackRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendGetRequest<GetUserFeedbackRequest, GetUserFeedbackResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetCreatedForPlaylistsResponse> GetCreatedForPlaylists(GetCreatedForPlaylistsRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendGetRequest<GetCreatedForPlaylistsRequest, GetCreatedForPlaylistsResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetPlaylistResponse> GetPlaylist(GetPlaylistRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendGetRequest<GetPlaylistRequest, GetPlaylistResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GetUserPlaylistsResponse> GetUserPlaylists(GetUserPlaylistsRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendGetRequest<GetUserPlaylistsRequest, GetUserPlaylistsResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistWriteResponse> CreatePlaylist(CreatePlaylistRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendPostRequest<CreatePlaylistRequest, PlaylistWriteResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistWriteResponse> EditPlaylist(EditPlaylistRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendPostRequest<EditPlaylistRequest, PlaylistWriteResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistWriteResponse> DeletePlaylist(DeletePlaylistRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendPostRequest<DeletePlaylistRequest, PlaylistWriteResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistWriteResponse> AddPlaylistItems(AddPlaylistItemsRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendPostRequest<AddPlaylistItemsRequest, PlaylistWriteResponse>(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistWriteResponse> DeletePlaylistItems(DeletePlaylistItemsRequest request, CancellationToken cancellationToken)
    {
        return await _apiClient.SendPostRequest<DeletePlaylistItemsRequest, PlaylistWriteResponse>(request, cancellationToken);
    }
}
