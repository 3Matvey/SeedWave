namespace SeedWave.Api.Contracts.Catalog
{
    public record SongDetailsResponse
    (
        int SequenceIndex,
        string Region,
        ulong Seed,
        string Title,
        string Artist,
        string AlbumTitle,
        string Genre,
        int Likes,
        string ReviewText
    );
}
