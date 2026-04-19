using SeedWave.Core.Catalog;

namespace SeedWave.Core.Generation
{
    /// <summary>
    /// Derives deterministic seeds for individual generation aspects of a song.
    /// </summary>
    public interface ISeedDeriver
    {
        int Derive(SongIdentity identity, SeedPurpose purpose);
    }
}