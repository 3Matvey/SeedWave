namespace SeedWave.Core.AudioComposition
{
    public record NoteEvent
    (
        TrackKind Track,
        int MidiNote,
        double StartBeat,
        double DurationBeats,
        double Velocity
    );
}
