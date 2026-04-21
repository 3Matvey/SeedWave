namespace SeedWave.Core.Regions
{
    public record ReviewLexicon
    (
        IReadOnlyList<string> Openings,
        IReadOnlyList<string> MoodDescriptors,
        IReadOnlyList<string> ArrangementDescriptors,
        IReadOnlyList<string> Closings
    );
}
