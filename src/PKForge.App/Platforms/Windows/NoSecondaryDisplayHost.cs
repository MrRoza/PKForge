using PKForge.Domain;

namespace PKForge.App;

/// <summary>Desktop has no second screen surface: everything stays in the main window.</summary>
public sealed class NoSecondaryDisplayHost : ISecondaryDisplayHost
{
    public bool IsAvailable => false;
    public ValueTask ShowAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    public ValueTask DismissAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
