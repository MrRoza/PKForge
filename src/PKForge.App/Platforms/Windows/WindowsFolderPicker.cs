using PKForge.Domain;
using Windows.Storage.Pickers;

namespace PKForge.App;

/// <summary>The system folder dialog; a folder grant on Windows is just its full path.</summary>
public sealed class WindowsFolderPicker : IFolderPicker
{
    public ValueTask<PickedFolder?> PickFolderAsync(CancellationToken cancellationToken = default) =>
        new(MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            picker.FileTypeFilter.Add("*");
            // Unpackaged WinUI pickers must be parented to a window handle.
            var window = Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window
                ?? throw new InvalidOperationException("No app window is available for the folder picker.");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
            var folder = await picker.PickSingleFolderAsync();
            return folder is null ? null : new PickedFolder(folder.Path, folder.Name);
        }));
}
