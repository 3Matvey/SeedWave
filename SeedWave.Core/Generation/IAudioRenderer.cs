using SeedWave.Core.AudioComposition;

namespace SeedWave.Core.Generation
{
    public interface IAudioRenderer
    {
        GeneratedAudio Render(CompositionPlan plan, string fileName);
    }
}