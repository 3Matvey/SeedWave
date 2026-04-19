namespace SeedWave.Core.Catalog
{
    /// <summary>
    /// Uniquely identifies a generated song within a specific region, seed, and page context.
    /// </summary>
    public record SongIdentity
    (
        string Region,
        ulong UserSeed,
        int Page,           //TODO maybe delete later
        int SequenceIndex
    );
}