namespace SeedWave.Core.Generation
{
    public sealed record GeneratedAudio(
        byte[] Content,
        string ContentType,
        string FileName);
}