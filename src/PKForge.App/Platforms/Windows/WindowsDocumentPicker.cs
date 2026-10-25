using PKForge.Domain;

namespace PKForge.App;

/// <summary>The system file dialog; picked files are opened by their full path.</summary>
public sealed class WindowsDocumentPicker : IDocumentPicker
{
    public async ValueTask<PickedDocument?> PickSaveAsync(CancellationToken cancellationToken = default)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Open a Pokémon save" });
        return result is null ? null : new PickedDocument(result.FullPath, result.FileName);
    }

    public async ValueTask<IReadOnlyList<PickedDocument>> PickManyAsync(CancellationToken cancellationToken = default)
    {
        var results = await FilePicker.Default.PickMultipleAsync(new PickOptions());
        return results?.Where(x => x is not null).Select(x => new PickedDocument(x!.FullPath, x.FileName)).ToList() ?? [];
    }
}
