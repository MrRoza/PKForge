namespace PKForge.App.Services;

/// <summary>
/// The Bad Apple easter egg's song. One instance per playback; disposing it stops the song.
/// Platforms without one give no song, and the picture runs on its own clock.
/// </summary>
public interface IEasterEggSong : IDisposable
{
    /// <summary>Loads and starts the song; false when there is none. <paramref name="ended"/> fires when it finishes.</summary>
    Task<bool> StartAsync(Action ended);

    /// <summary>Where the song is, in milliseconds: the picture follows it.</summary>
    long PositionMs { get; }
}

/// <summary>No song: the easter egg plays silently.</summary>
public sealed class NoEasterEggSong : IEasterEggSong
{
    public Task<bool> StartAsync(Action ended) => Task.FromResult(false);
    public long PositionMs => 0;
    public void Dispose() { }
}
