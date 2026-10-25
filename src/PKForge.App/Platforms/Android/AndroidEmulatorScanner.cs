using Android.Provider;
using PKForge.Domain;
using AndroidUri = Android.Net.Uri;

namespace PKForge.App;

/// <summary>Walks a granted SAF tree looking for parseable Pokémon saves.</summary>
public sealed class AndroidEmulatorScanner(ISaveEngine engine) : EmulatorScanner(engine)
{
    protected override string RootDocId(string treeId) =>
        DocumentsContract.GetTreeDocumentId(Parse(treeId))
            ?? throw new InvalidOperationException("The folder grant has no tree document id.");

    protected override string? DocumentId(string treeId, string docId) =>
        DocumentsContract.BuildDocumentUriUsingTree(Parse(treeId), docId)?.ToString();

    protected override Stream? OpenRead(string documentId) =>
        Platform.AppContext.ContentResolver?.OpenInputStream(AndroidUri.Parse(documentId)!);

    /// <summary>Android's last package update time: zero-once per install, stable after.</summary>
    private static long _installEpoch = -1;

    protected override long InstallEpoch
    {
        get
        {
            if (_installEpoch >= 0) return _installEpoch;
            try
            {
                var context = Platform.AppContext;
                _installEpoch = context.PackageManager!.GetPackageInfo(context.PackageName!, 0)?.LastUpdateTime ?? 0;
            }
            catch
            {
                _installEpoch = 0; // unbustable cache is still better than a crashing scan
            }
            return _installEpoch;
        }
    }

    protected override List<ChildDocument> ListChildren(string treeId, string parentDocId)
    {
        var results = new List<ChildDocument>();
        var childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(Parse(treeId), parentDocId);
        if (childrenUri is null) return results;

        string[] projection =
        [
            DocumentsContract.Document.ColumnDocumentId,
            DocumentsContract.Document.ColumnDisplayName,
            DocumentsContract.Document.ColumnMimeType,
            DocumentsContract.Document.ColumnLastModified,
        ];
        using var cursor = Platform.AppContext.ContentResolver?.Query(childrenUri, projection, null, null, null);
        if (cursor is null) return results;

        while (cursor.MoveToNext())
        {
            var docId = cursor.GetString(0);
            var name = cursor.GetString(1);
            if (docId is null || name is null) continue;
            var isDirectory = cursor.GetString(2) == DocumentsContract.Document.MimeTypeDir;
            var modifiedMs = cursor.GetLong(3);
            results.Add(new ChildDocument(docId, name, isDirectory,
                modifiedMs > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(modifiedMs) : null));
        }
        return results;
    }

    private static AndroidUri Parse(string treeId) =>
        AndroidUri.Parse(treeId) ?? throw new ArgumentException("Invalid tree URI.", nameof(treeId));
}
