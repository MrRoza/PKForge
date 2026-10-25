using PKForge.App.Services;
using MediaPlayer = Android.Media.MediaPlayer;

namespace PKForge.App;

/// <summary>The easter egg's song through Android's MediaPlayer.</summary>
public sealed class AndroidEasterEggSong : IEasterEggSong
{
    private MediaPlayer? _song;
    private bool _disposed;

    public long PositionMs => _song?.CurrentPosition ?? 0;

    /// <summary>The song plays from a copy in the cache: MediaPlayer cannot read the APK's compressed assets.</summary>
    public async Task<bool> StartAsync(Action ended)
    {
        try
        {
            var path = Path.Combine(FileSystem.CacheDirectory, "badapple.m4a");
            if (!File.Exists(path))
            {
                await using var asset = await FileSystem.OpenAppPackageFileAsync("badapple/audio.m4a");
                var partial = path + ".part";
                await using (var file = File.Create(partial)) await asset.CopyToAsync(file);
                File.Move(partial, path, overwrite: true);
            }
            if (_disposed) return false;
            var song = new MediaPlayer();
            song.SetDataSource(path);
            song.Prepare();
            song.Completion += (_, _) => MainThread.BeginInvokeOnMainThread(ended);
            if (_disposed) { song.Release(); return false; }
            _song = song;
            song.Start();
            return true;
        }
        catch (Exception error) when (error is IOException or Java.Lang.Exception)
        {
            AppLog.Warn("about", $"The easter egg plays without its song: {error.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _song?.Stop();
        _song?.Release();
        _song = null;
    }
}
