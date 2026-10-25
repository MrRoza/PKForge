using PKForge.Domain;

namespace PKForge.App;

/// <summary>Releases ship an APK only: desktop builds come from source and are updated by hand.</summary>
public sealed class DesktopAppUpdateService : IAppUpdateService
{
    public bool CanInstallUpdates => false;

    public event Action<string>? InstallFailed { add { } remove { } }

    public Task<AppUpdateCheck> CheckAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new AppUpdateCheck(false, null, "Desktop builds are updated manually."));

    public bool ShouldPromptAutomatically(AvailableAppUpdate update) => false;

    public void DontRemindMe(AvailableAppUpdate update) { }

    public async Task<AppUpdateInstallResult> DownloadAndInstallAsync(
        AvailableAppUpdate update, Action<long, long> onProgress, CancellationToken cancellationToken)
    {
        await Launcher.OpenAsync(update.ReleaseUrl).ConfigureAwait(false);
        return AppUpdateInstallResult.ReleasePageOpened;
    }

    public void OpenInstallPermissionSettings() { }
}
