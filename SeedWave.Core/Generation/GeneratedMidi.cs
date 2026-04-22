namespace SeedWave.Core.Generation
{
    public sealed record GeneratedMidi(
        byte[] Content,
        string ContentType,
        string FileName);
}
