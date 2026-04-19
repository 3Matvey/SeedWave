namespace SeedWave.Core.Catalog
{
    /// <summary>
    /// Contains the core song metadata that must remain stable when presentation data changes.
    /// </summary>
    public record SongCore
    (
        string Title,
        string Artist,
        string AlbumTitle,
        string Genre,
        bool IsSingle
    );
}