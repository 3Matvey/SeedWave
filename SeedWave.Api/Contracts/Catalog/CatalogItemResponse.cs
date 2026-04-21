namespace SeedWave.Api.Contracts.Catalog
{
    public record CatalogItemResponse
    (
        int SequenceIndex,
        string Title,
        string Artist,
        string AlbumTitle,
        string Genre,
        int Likes
    );
}
