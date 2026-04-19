namespace SeedWave.Core.Catalog
{
    /// <summary>
    /// Represents a generated catalog record with identity, core metadata, and likes.
    /// </summary>
    public record SongRecord
    (
        SongIdentity Identity,
        SongCore Core,
        int Likes
    );
    //TODO review, cover profile и audio profile
}