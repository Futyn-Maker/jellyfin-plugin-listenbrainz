export interface LibraryConfig {
    Id: string;
    IsAllowed: boolean;
    IsLibraryExportIncluded: boolean;
}

export interface MediaLibrary {
    Id: string;
    Name: string;
    IsMusicLibrary: boolean;
}

export interface PluginConfiguration {
    BackupPath: string;
    ConfigVersion: number;
    IsAllPlaylistsSyncEnabled: boolean;
    IsAlternativeModeEnabled: boolean;
    IsImmediateFavoriteSyncEnabled: boolean;
    IsMusicBrainzEnabled: boolean;
    LibraryConfigs: LibraryConfig[];
    ListenBrainzApiUrl: string;
    MbidDelimiters: string;
    MusicBrainzApiUrl: string;
    PlaylistRematchIntervalDays: number;
    UserConfigs: PluginUserConfig[];
}

// Track order of an exported playlist. Must match the names of the ExportTrackOrder enum.
export type ExportTrackOrder =
    | "SourceOrder"
    | "ArtistAlbumTrack"
    | "DateAddedNewestFirst"
    | "DateAddedOldestFirst"
    | "Title";

// Plugin user configuration.
// The keys must match the fields of UserConfig class.
export interface PluginUserConfig {
    ApiToken: string;
    IsBackupEnabled: boolean;
    IsFavoritesSyncEnabled: boolean;
    IsListenSubmitEnabled: boolean;
    IsPlaylistsSyncEnabled: boolean;
    IsStrictModeEnabled: boolean;
    IsTopDiscoveriesSyncEnabled: boolean;
    IsTopMissedRecordingsSyncEnabled: boolean;
    IsWeeklyExplorationSyncEnabled: boolean;
    IsWeeklyJamsSyncEnabled: boolean;
    IsGeneratedPlaylistsSyncEnabled: boolean;
    JellyfinUserId: string;
    KeepPlaylistsAfterRotation: boolean;
    IsPlaylistExportEnabled: boolean;
    IsLibraryExportEnabled: boolean;
    LibraryExportTitle: string;
    LibraryExportDescription: string;
    IsPlaylistExportPublic: boolean;
    IsPlaylistExportCleanupEnabled: boolean;
    LibraryExportOrder: ExportTrackOrder;
    PlaylistExportOrder: ExportTrackOrder;
    UserName: string;
}

export interface TokenValidationResult {
    IsValid: boolean;
    Reason: string;
    UserName: string;
}
