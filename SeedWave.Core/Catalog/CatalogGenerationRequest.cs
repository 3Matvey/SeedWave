namespace SeedWave.Core.Catalog
{
    /// <summary>
    /// Describes the input parameters required to generate a catalog page.
    /// </summary>
    public record CatalogGenerationRequest
    (
        string Region,
        ulong Seed,
        double LikesAverage,
        int Page,
        int PageSize
    );
}