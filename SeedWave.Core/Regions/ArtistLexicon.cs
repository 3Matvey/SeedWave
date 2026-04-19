namespace SeedWave.Core.Regions
{
    /// <summary>
    /// Contains locale-specific data used to generate artist names.
    /// </summary>
    public record ArtistLexicon
    (
        IReadOnlyList<string> FirstNames,
        IReadOnlyList<string> LastNames,
        IReadOnlyList<string> BandPrefixes,
        IReadOnlyList<string> BandNouns
    );
}