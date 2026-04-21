namespace SeedWave.Api.Contracts.Catalog
{
    public record CatalogPageResponse
    (
        string Region,
        ulong Seed,
        double LikesAverage,
        int Page,
        int PageSize,
        IReadOnlyList<CatalogItemResponse> Songs
    );
}
