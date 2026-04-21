namespace SeedWave.Infrastructure.Audio
{
    public record RenderedAudio
    (
        byte[] Content,
        string ContentType,
        string FileName
    );
}
