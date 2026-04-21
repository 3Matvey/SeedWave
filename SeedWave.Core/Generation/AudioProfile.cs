namespace SeedWave.Core.Generation
{
    /// <summary>
    /// Describes the deterministic musical profile used to render a song preview.
    /// </summary>
    public record AudioProfile
    (
        int TempoBpm,
        MusicalKey Key,
        ScaleMode Mode,
        int Bars,
        int LeadOctave,
        int BassOctave
    );
}
