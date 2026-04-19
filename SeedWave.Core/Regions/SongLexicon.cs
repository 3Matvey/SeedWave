namespace SeedWave.Core.Regions
{
    /// <summary>
    /// Contains locale-specific data used to generate song titles.
    /// </summary>
    public record SongLexicon
    (
        IReadOnlyList<string> Adjectives,
        IReadOnlyList<string> Nouns
    );
}