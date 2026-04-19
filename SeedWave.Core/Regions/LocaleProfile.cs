namespace SeedWave.Core.Regions
{
    /// <summary>
    /// Represents the complete locale-specific dataset required to generate catalog content.
    /// </summary>
    public record LocaleProfile
    (
        string Region,
        ArtistLexicon Artists,
        SongLexicon Songs,
        AlbumLexicon Albums,
        IReadOnlyList<string> Genres
    );
}