using PKForge.Domain;

namespace PKForge.App;

/// <summary>Reads and writes files directly inside a picked folder (bank archives, bulk moves).</summary>
public sealed class WindowsFolderFileAccess : IFolderFileAccess
{
    public ValueTask<IReadOnlyList<PickedDocument>> ListFilesAsync(string treeId, CancellationToken cancellationToken = default) =>
        new(Directory.EnumerateFiles(treeId)
            .Select(path => new PickedDocument(path, Path.GetFileName(path)))
            .ToList());

    public async ValueTask<ReadOnlyMemory<byte>> ReadFileAsync(string documentId, CancellationToken cancellationToken = default) =>
        await File.ReadAllBytesAsync(documentId, cancellationToken).ConfigureAwait(false);

    public async ValueTask WriteFileAsync(string treeId, string fileName, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        await using var output = new FileStream(Path.Combine(treeId, fileName), FileMode.Create, FileAccess.Write);
        await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    }
}
