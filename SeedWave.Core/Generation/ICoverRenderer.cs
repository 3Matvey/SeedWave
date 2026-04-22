namespace SeedWave.Core.Generation
{
    public interface ICoverRenderer
    {
        GeneratedCover Render(
            CoverProfile coverProfile,
            AudioProfile audioProfile,
            int seed,
            string title,
            string artist,
            string fileNameWithoutExtension);
    }
}
