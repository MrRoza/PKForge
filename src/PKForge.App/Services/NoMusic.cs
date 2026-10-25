using PKForge.Domain;

namespace PKForge.App.Services;

/// <summary>
/// The music player for platforms without one: an empty library that never plays. The Music
/// menu still opens and says why, through <see cref="LastError"/>.
/// </summary>
public sealed class NoMusic : IMusicPlayer
{
    public IReadOnlyList<MusicTrack> Library => [];
    public bool IsPlaying => false;
    public bool IsOn => false;
    public int? CurrentIndex => null;
    public MusicOrder Order => MusicOrder.InOrder;
    public MusicPlace Place { get; private set; } = MusicPlace.Menus;
    public bool Autostart => false;
    public string? LastError => "Music is not available on this platform yet.";

    public int Add(IReadOnlyList<PickedDocument> documents) => 0;
    public void Remove(int index) { }
    public void Clear() { }

    public void Play() { }
    public void Pause() { }
    public void Skip() { }
    public void SetOrder(MusicOrder order) { }
    public void SetAutostart(bool on) { }

    public MusicSource SourceFor(MusicPlace place) => MusicSource.Default;
    public void SetSource(MusicPlace place, MusicSource source) { }
    public void EnterPlace(MusicPlace place) => Place = place;

    public void MaybeAutostart() { }
    public void PauseForBackground() { }
    public void ResumeFromBackground() { }
}
