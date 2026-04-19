namespace SeedWave.Core.Catalog
{
    /// <summary>
    /// Represents a generated catalog page containing a batch of songs for the current request.
    /// </summary>
    public record CatalogPage
    (
        string Region,
        ulong Seed,
        double LikesAverage,
        int Page,
        int PageSize,
        IReadOnlyList<SongRecord> Songs
    );
}