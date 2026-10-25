using PKForge.Domain;

namespace PKForge.App;

/// <summary>Reads and writes save documents by file path. A document id on Windows is the full path.</summary>
public sealed class WindowsFileAccess : ISaveFileAccess
{
    public async ValueTask<ReadOnlyMemory<byte>> ReadAsync(string documentId, CancellationToken cancellationToken = default) =>
        await File.ReadAllBytesAsync(documentId, cancellationToken).ConfigureAwait(false);

    public async ValueTask WriteAtomicallyAsync(string documentId, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        // Write beside the target, then swap: a crash mid-write never leaves a half-written save.
        var temp = documentId + ".pkforge-tmp";
        await using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            await output.FlushAsync(cancellationToken).ConfigureAwait(false);
            output.Flush(flushToDisk: true);
        }
        if (File.Exists(documentId))
            File.Replace(temp, documentId, destinationBackupFileName: null);
        else
            File.Move(temp, documentId);
    }
}
