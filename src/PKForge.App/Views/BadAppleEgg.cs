using PKForge.App.Services;
using PKForge.Chrome;
using PKForge.Domain;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PKForge.App.Views;

/// <summary>
/// The About screen's easter egg: Bad Apple!! as text art made of Pokémon names, over the whole
/// screen, with its song. Nothing is loaded until it starts. The picture follows the song's
/// clock; B, a tap or leaving the app stops it, and the player's own music picks up where it was.
/// </summary>
public sealed class BadAppleEgg : IPadHandler
{
    private readonly TaskCompletionSource _done = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Grid _host;
    private readonly Grid _overlay;
    private readonly GamepadRouter? _router;
    private readonly SKCanvasView _canvas;
    private readonly FrameInvalidator _frame;
    private readonly BadAppleFrames _frames;
    private readonly BadApplePainter _painter;
    private readonly SKFont _font = new(BoxBrowserPage.PixelTypeface(), 20);
    private readonly IMusicPlayer? _music;
    private readonly IEasterEggSong? _song;
    private bool _hasSong;
    private readonly System.Diagnostics.Stopwatch _clock = new();
    private bool _closed;

    public static async Task PlayAsync(Grid host)
    {
        BadAppleEgg egg;
        try
        {
            egg = new BadAppleEgg(host, await FileSystem.OpenAppPackageFileAsync("badapple/frames.bin"));
        }
        catch (Exception error) when (error is IOException or InvalidDataException)
        {
            AppLog.Warn("about", $"The easter egg could not start: {error.Message}");
            return;
        }
        await egg._done.Task;
    }

    private BadAppleEgg(Grid host, Stream frames)
    {
        _host = host;
        _frames = new BadAppleFrames(frames);
        var services = IPlatformApplication.Current?.Services;
        var names = services?.GetService<IGameDataService>()?.SpeciesNames.Skip(1).Where(name => name.Length > 0) ?? [];
        _painter = new BadApplePainter(_font, "PKFORGE·" + string.Concat(names.Select(name => name.ToUpperInvariant() + "·")));

        // The player's music steps aside the way it does when the app leaves the screen.
        _music = services?.GetService<IMusicPlayer>();
        _music?.PauseForBackground();
        App.Suspended += Close;
        _song = services?.GetService<IEasterEggSong>();

        _canvas = new SKCanvasView { EnableTouchEvents = true };
        _canvas.PaintSurface += Paint;
        _canvas.Touch += (_, args) =>
        {
            args.Handled = true;
            if (args.ActionType == SKTouchAction.Released) Close();
        };
        _frame = new FrameInvalidator(_canvas);
        _overlay = new Grid { BackgroundColor = Colors.Transparent, Children = { _canvas } };
        Grid.SetRowSpan(_overlay, Math.Max(1, host.RowDefinitions.Count));
        Grid.SetColumnSpan(_overlay, Math.Max(1, host.ColumnDefinitions.Count));
        host.Children.Add(_overlay);
        _router = services?.GetService<GamepadRouter>();
        _router?.Push(this);
        _ = StartSongAsync();
    }

    private async Task StartSongAsync()
    {
        // No song: the picture still plays, on its own clock.
        _hasSong = _song is not null && await _song.StartAsync(Close);
        _clock.Start();
        _frame.Request();
    }

    public bool OnPadButton(PadButton button)
    {
        if (button == PadButton.B) Close();
        return true; // nothing reaches the About window beneath
    }

    private void Close()
    {
        if (_closed) return;
        _closed = true;
        App.Suspended -= Close;
        _router?.Remove(this);
        _host.Remove(_overlay);
        _song?.Dispose();
        _frames.Dispose();
        _font.Dispose();
        _music?.ResumeFromBackground();
        _done.TrySetResult();
    }

    private void Paint(object? sender, SKPaintSurfaceEventArgs args)
    {
        var canvas = args.Surface.Canvas;
        var theme = ColorTheme.Current;
        canvas.Clear(theme.Void);
        if (_closed || !_clock.IsRunning) return;
        var ms = _hasSong ? _song!.PositionMs : _clock.ElapsedMilliseconds;
        if (!_frames.SeekForward((int)(ms * _frames.Fps / 1000)))
        {
            // With the song, its end closes the egg; without it, the last frame does.
            if (!_hasSong) MainThread.BeginInvokeOnMainThread(Close);
            return;
        }
        _painter.Paint(canvas, new SKRect(0, 0, args.Info.Width, args.Info.Height), _frames, _font, theme);
        _frame.Request();
    }
}
