namespace SeedWave.Infrastructure.Audio
{
    public sealed class AudioSettings
    {
        public string SoundFontPath { get; init; } = string.Empty;

        public int SampleRate { get; init; } = 44_100;

        public int TailSeconds { get; init; } = 4;

        public float TargetPeak { get; init; } = 0.92f;

        public float SoftClipDrive { get; init; } = 1.35f;
    }
}