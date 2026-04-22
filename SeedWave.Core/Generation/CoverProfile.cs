namespace SeedWave.Core.Generation
{
    /// <summary>
    /// Describes the deterministic visual profile used to render a song cover.
    /// </summary>
    public record CoverProfile
    (
        CoverLayoutKind Layout,
        string BackgroundColorHex,
        string AccentColorHex,
        string TextColorHex,
        int ShapeSeed,
        bool UseGrainOverlay
    );
}
