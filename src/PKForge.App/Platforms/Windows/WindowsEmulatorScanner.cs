using PKForge.Domain;

namespace PKForge.App;

/// <summary>Walks a picked folder on disk looking for parseable Pokémon saves. Ids are full paths.</summary>
public sealed class WindowsEmulatorScanner(ISaveEngine engine) : EmulatorScanner(engine)
{
    protected override string RootDocId(string treeId) => treeId;

    protected override string? DocumentId(string treeId, string docId) => docId;

    protected override Stream? OpenRead(string documentId) => File.OpenRead(documentId);

    /// <summary>The app binary's build time: changes with every new build, stable after.</summary>
    protected override long InstallEpoch { get; } =
        File.GetLastWriteTimeUtc(typeof(WindowsEmulatorScanner).Assembly.Location).Ticks;

    protected override List<ChildDocument> ListChildren(string treeId, string parentDocId)
    {
        try
        {
            return new DirectoryInfo(parentDocId).EnumerateFileSystemInfos()
                .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden))
                .Select(x => new ChildDocument(x.FullName, x.Name, x is DirectoryInfo,
                    x is FileInfo ? new DateTimeOffset(x.LastWriteTimeUtc) : null))
                .ToList();
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return []; // an unreadable folder is simply skipped
        }
    }
}
