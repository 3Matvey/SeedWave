namespace SeedWave.Core.Regions
{
    /// <summary>
    /// Contains locale-specific data used to generate album titles and the single literal.
    /// </summary>
    public record AlbumLexicon
    (
        IReadOnlyList<string> Adjectives,
        IReadOnlyList<string> Nouns,
        string SingleLiteral
    );
}