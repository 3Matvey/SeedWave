using SeedWave.Core.AudioComposition;

namespace SeedWave.Core.Generation
{
    public interface IMidiRenderer
    {
        GeneratedMidi Render(CompositionPlan plan, string fileName);
    }
}
