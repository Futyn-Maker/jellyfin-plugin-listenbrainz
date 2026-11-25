import { defaultMbidDelimiters, defaultPlaylistRematchIntervalDays, userDefaults } from "./constants";
import { getLibraryExportCheckboxName, getUniqueLibraryName } from "./utils";
import { ExportTrackOrder, MediaLibrary, PluginConfiguration, PluginUserConfig } from "./types";

// ── User Config ──

function getUserConfigFormElements(view: HTMLElement) {
    return {
        apiToken: view.querySelector("#ListenBrainzApiToken") as HTMLInputElement,
        backup: view.querySelector("#IsUserBackupEnabled") as HTMLInputElement,
        favoritesSync: view.querySelector("#IsFavoritesSyncEnabled") as HTMLInputElement,
        listenSubmit: view.querySelector("#IsListenSubmitEnabled") as HTMLInputElement,
        playlistsSync: view.querySelector("#IsPlaylistsSyncEnabled") as HTMLInputElement,
        strictMode: view.querySelector("#IsStrictModeEnabled") as HTMLInputElement,
        topDiscoveriesSync: view.querySelector("#IsTopDiscoveriesSyncEnabled") as HTMLInputElement,
        topMissedRecordingsSync: view.querySelector("#IsTopMissedRecordingsSyncEnabled") as HTMLInputElement,
        weeklyExplorationSync: view.querySelector("#IsWeeklyExplorationSyncEnabled") as HTMLInputElement,
        weeklyJamsSync: view.querySelector("#IsWeeklyJamsSyncEnabled") as HTMLInputElement,
        generatedPlaylistsSync: view.querySelector("#IsGeneratedPlaylistsSyncEnabled") as HTMLInputElement,
        playlistsRetention: view.querySelector("#KeepPlaylistsAfterRotation") as HTMLInputElement,
        playlistExport: view.querySelector("#IsPlaylistExportEnabled") as HTMLInputElement,
        libraryExport: view.querySelector("#IsLibraryExportEnabled") as HTMLInputElement,
        libraryExportTitle: view.querySelector("#LibraryExportTitle") as HTMLInputElement,
        libraryExportDescription: view.querySelector("#LibraryExportDescription") as HTMLTextAreaElement,
        playlistExportPublic: view.querySelector("#IsPlaylistExportPublic") as HTMLInputElement,
        playlistExportCleanup: view.querySelector("#IsPlaylistExportCleanupEnabled") as HTMLInputElement,
        libraryExportOrder: view.querySelector("#LibraryExportOrder") as HTMLSelectElement,
        playlistExportOrder: view.querySelector("#PlaylistExportOrder") as HTMLSelectElement,
        userDropdown: view.querySelector("#JellyfinUser") as HTMLSelectElement,
    };
}

export function fillUserConfigForm(view: HTMLElement, userConfig: PluginUserConfig): void {
    const elements = getUserConfigFormElements(view);
    elements.apiToken.value = atob(userConfig.ApiToken);
    elements.backup.checked = userConfig.IsBackupEnabled;
    elements.favoritesSync.checked = userConfig.IsFavoritesSyncEnabled;
    elements.listenSubmit.checked = userConfig.IsListenSubmitEnabled;
    elements.playlistsSync.checked = userConfig.IsPlaylistsSyncEnabled;
    elements.strictMode.checked = userConfig.IsStrictModeEnabled;
    elements.topDiscoveriesSync.checked = userConfig.IsTopDiscoveriesSyncEnabled;
    elements.topMissedRecordingsSync.checked = userConfig.IsTopMissedRecordingsSyncEnabled;
    elements.weeklyExplorationSync.checked = userConfig.IsWeeklyExplorationSyncEnabled;
    elements.weeklyJamsSync.checked = userConfig.IsWeeklyJamsSyncEnabled;
    elements.generatedPlaylistsSync.checked = userConfig.IsGeneratedPlaylistsSyncEnabled;
    elements.playlistsRetention.checked = userConfig.KeepPlaylistsAfterRotation;
    elements.playlistExport.checked = userConfig.IsPlaylistExportEnabled;
    elements.libraryExport.checked = userConfig.IsLibraryExportEnabled;
    elements.libraryExportTitle.value = userConfig.LibraryExportTitle;
    elements.libraryExportDescription.value = userConfig.LibraryExportDescription;
    elements.playlistExportPublic.checked = userConfig.IsPlaylistExportPublic;
    elements.playlistExportCleanup.checked = userConfig.IsPlaylistExportCleanupEnabled;
    selectOrder(elements.libraryExportOrder, userConfig.LibraryExportOrder, userDefaults.LibraryExportOrder);
    selectOrder(elements.playlistExportOrder, userConfig.PlaylistExportOrder, userDefaults.PlaylistExportOrder);
}

// Selects the configured order, or the default if the select has no such option.
function selectOrder(select: HTMLSelectElement, order: ExportTrackOrder, fallback: ExportTrackOrder): void {
    select.value = order;
    if (select.value !== order) {
        select.value = fallback;
    }
}

export function getUserConfigFormData(view: HTMLElement): PluginUserConfig {
    const elements = getUserConfigFormElements(view);
    return {
        ApiToken: btoa(elements.apiToken.value.trim()),
        IsBackupEnabled: elements.backup.checked,
        IsFavoritesSyncEnabled: elements.favoritesSync.checked,
        IsListenSubmitEnabled: elements.listenSubmit.checked,
        IsPlaylistsSyncEnabled: elements.playlistsSync.checked,
        IsStrictModeEnabled: elements.strictMode.checked,
        IsTopDiscoveriesSyncEnabled: elements.topDiscoveriesSync.checked,
        IsTopMissedRecordingsSyncEnabled: elements.topMissedRecordingsSync.checked,
        IsWeeklyExplorationSyncEnabled: elements.weeklyExplorationSync.checked,
        IsWeeklyJamsSyncEnabled: elements.weeklyJamsSync.checked,
        IsGeneratedPlaylistsSyncEnabled: elements.generatedPlaylistsSync.checked,
        JellyfinUserId: elements.userDropdown.value,
        KeepPlaylistsAfterRotation: elements.playlistsRetention.checked,
        IsPlaylistExportEnabled: elements.playlistExport.checked,
        IsLibraryExportEnabled: elements.libraryExport.checked,
        LibraryExportTitle: elements.libraryExportTitle.value.trim(),
        LibraryExportDescription: elements.libraryExportDescription.value.trim(),
        IsPlaylistExportPublic: elements.playlistExportPublic.checked,
        IsPlaylistExportCleanupEnabled: elements.playlistExportCleanup.checked,
        LibraryExportOrder: elements.libraryExportOrder.value as ExportTrackOrder,
        PlaylistExportOrder: elements.playlistExportOrder.value as ExportTrackOrder,
        UserName: "",
    };
}

// ── General Config ──

function getGeneralConfigFormElements(view: HTMLElement) {
    return {
        allPlaylistsEnabled: view.querySelector("#IsAllPlaylistsSyncEnabled") as HTMLInputElement,
        altModeEnabled: view.querySelector("#IsAlternativeModeEnabled") as HTMLInputElement,
        mbidDelimiters: view.querySelector("#MbidDelimiters") as HTMLInputElement,
        immediateFavorites: view.querySelector("#IsImmediateFavoriteSyncEnabled") as HTMLInputElement,
        listenBrainzUrl: view.querySelector("#ListenBrainzApiUrl") as HTMLInputElement,
        playlistRematchInterval: view.querySelector("#PlaylistRematchIntervalDays") as HTMLInputElement,
    };
}

export function fillGeneralConfigForm(view: HTMLElement, pluginConfig: PluginConfiguration): void {
    const elements = getGeneralConfigFormElements(view);
    elements.allPlaylistsEnabled.checked = pluginConfig.IsAllPlaylistsSyncEnabled;
    elements.altModeEnabled.checked = pluginConfig.IsAlternativeModeEnabled;
    elements.mbidDelimiters.value =
        pluginConfig.MbidDelimiters === defaultMbidDelimiters ? "" : pluginConfig.MbidDelimiters;
    elements.immediateFavorites.checked = pluginConfig.IsImmediateFavoriteSyncEnabled;
    elements.listenBrainzUrl.value = pluginConfig.ListenBrainzApiUrl;
    elements.playlistRematchInterval.value = String(pluginConfig.PlaylistRematchIntervalDays);
}

export function getGeneralConfigFormData(
    view: HTMLElement,
): Pick<
    PluginConfiguration,
    | "ListenBrainzApiUrl"
    | "IsAlternativeModeEnabled"
    | "IsImmediateFavoriteSyncEnabled"
    | "IsAllPlaylistsSyncEnabled"
    | "MbidDelimiters"
    | "PlaylistRematchIntervalDays"
> {
    const elements = getGeneralConfigFormElements(view);
    const rematchInterval = parseInt(elements.playlistRematchInterval.value, 10);
    return {
        MbidDelimiters: elements.mbidDelimiters.value,
        IsAllPlaylistsSyncEnabled: elements.allPlaylistsEnabled.checked,
        IsAlternativeModeEnabled: elements.altModeEnabled.checked,
        IsImmediateFavoriteSyncEnabled: elements.immediateFavorites.checked,
        ListenBrainzApiUrl: elements.listenBrainzUrl.value,
        PlaylistRematchIntervalDays: isNaN(rematchInterval) ? defaultPlaylistRematchIntervalDays : rematchInterval,
    };
}

// ── MusicBrainz Config ──

function getMusicBrainzConfigFormElements(view: HTMLElement) {
    return {
        musicBrainzEnabled: view.querySelector("#IsMusicBrainzEnabled") as HTMLInputElement,
        musicBrainzUrl: view.querySelector("#MusicBrainzApiUrl") as HTMLInputElement,
    };
}

export function fillMusicBrainzConfigForm(view: HTMLElement, pluginConfig: PluginConfiguration): void {
    const elements = getMusicBrainzConfigFormElements(view);
    elements.musicBrainzEnabled.checked = pluginConfig.IsMusicBrainzEnabled;
    elements.musicBrainzUrl.value = pluginConfig.MusicBrainzApiUrl;
}

export function getMusicBrainzConfigFormData(
    view: HTMLElement,
): Pick<PluginConfiguration, "IsMusicBrainzEnabled" | "MusicBrainzApiUrl"> {
    const elements = getMusicBrainzConfigFormElements(view);
    return {
        IsMusicBrainzEnabled: elements.musicBrainzEnabled.checked,
        MusicBrainzApiUrl: elements.musicBrainzUrl.value,
    };
}

// ── Backup Config ──

function getBackupConfigFormElements(view: HTMLElement) {
    return {
        backupPath: view.querySelector("#BackupPath") as HTMLInputElement,
    };
}

export function fillBackupConfigForm(view: HTMLElement, pluginConfig: PluginConfiguration): void {
    const elements = getBackupConfigFormElements(view);
    elements.backupPath.value = pluginConfig.BackupPath;
}

export function getBackupConfigFormData(view: HTMLElement): Pick<PluginConfiguration, "BackupPath"> {
    const elements = getBackupConfigFormElements(view);
    return {
        BackupPath: elements.backupPath.value,
    };
}

// ── Libraries Config ──

export function fillLibrariesConfigForm(
    view: HTMLElement,
    pluginConfig: PluginConfiguration,
    jellyfinLibraries: MediaLibrary[],
): void {
    fillLibraryExportForm(view, pluginConfig, jellyfinLibraries);
    if (pluginConfig.LibraryConfigs.length > 0) {
        pluginConfig.LibraryConfigs.map((lc) => {
            const checkboxId = getUniqueLibraryName(lc.Id);
            const checkbox = view.querySelector(`#${checkboxId}`) as HTMLInputElement;
            if (checkbox) {
                checkbox.checked = lc.IsAllowed;
            }
        });

        return;
    }

    jellyfinLibraries.forEach((library) => {
        const checkboxId = getUniqueLibraryName(library.Id);
        const checkbox = view.querySelector(`#${checkboxId}`) as HTMLInputElement;
        if (checkbox) {
            checkbox.checked = library.IsMusicLibrary;
        }
    });
}

// Mirrors the server-side selection: explicitly included libraries, otherwise the libraries
// allowed for listen submission, otherwise (nothing saved yet) all music libraries.
function fillLibraryExportForm(
    view: HTMLElement,
    pluginConfig: PluginConfiguration,
    jellyfinLibraries: MediaLibrary[],
): void {
    const configs = pluginConfig.LibraryConfigs;
    const anyIncluded = configs.some((lc) => lc.IsLibraryExportIncluded);
    jellyfinLibraries.forEach((library) => {
        const checkbox = view.querySelector(`#${getLibraryExportCheckboxName(library.Id)}`) as HTMLInputElement;
        if (!checkbox) {
            return;
        }

        const lc = configs.find((c) => c.Id === library.Id);
        if (configs.length === 0) {
            checkbox.checked = library.IsMusicLibrary;
        } else if (anyIncluded) {
            checkbox.checked = lc?.IsLibraryExportIncluded ?? false;
        } else {
            checkbox.checked = lc?.IsAllowed ?? false;
        }
    });
}

export function getLibrariesConfigFormData(view: HTMLElement): Pick<PluginConfiguration, "LibraryConfigs"> {
    const checkboxes = view.querySelectorAll<HTMLInputElement>("[name^=library_]");
    return {
        LibraryConfigs: [...checkboxes].map((box) => {
            const id = box.id.replace(/^library_/, "").replace(/_IsAllowed$/, "");
            const exportBox = view.querySelector(`#${getLibraryExportCheckboxName(id)}`) as HTMLInputElement | null;
            return {
                Id: id,
                IsAllowed: box.checked,
                IsLibraryExportIncluded: exportBox?.checked ?? false,
            };
        }),
    };
}
