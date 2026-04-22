using System.Text;

namespace SeedWave.Infrastructure.Audio
{
    /// <summary>
    /// Encodes floating-point audio samples into a 16-bit PCM WAV file.
    /// Supports both mono and interleaved multi-channel audio.
    /// </summary>
    public sealed class WavEncoder
    {
        private const short BitsPerSample = 16;

        public byte[] Encode(float[] monoSamples, int sampleRate)
        {
            ArgumentNullException.ThrowIfNull(monoSamples);
            return EncodeInterleaved(monoSamples, sampleRate, 1);
        }

        public byte[] EncodeInterleaved(float[] interleavedSamples, int sampleRate, short channelCount)
        {
            ArgumentNullException.ThrowIfNull(interleavedSamples);

            if (channelCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channelCount));
            }

            if (interleavedSamples.Length % channelCount != 0)
            {
                throw new ArgumentException(
                    "Interleaved sample count must be divisible by channel count.",
                    nameof(interleavedSamples));
            }

            var dataSize = interleavedSamples.Length * sizeof(short);
            var riffChunkSize = 36 + dataSize;
            var byteRate = sampleRate * channelCount * BitsPerSample / 8;
            var blockAlign = (short)(channelCount * BitsPerSample / 8);

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

            WriteRiffHeader(writer, riffChunkSize);
            WriteFormatChunk(writer, sampleRate, channelCount, byteRate, blockAlign);
            WriteDataChunk(writer, interleavedSamples);

            writer.Flush();
            return stream.ToArray();
        }

        private static void WriteRiffHeader(BinaryWriter writer, int riffChunkSize)
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(riffChunkSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        }

        private static void WriteFormatChunk(
            BinaryWriter writer,
            int sampleRate,
            short channelCount,
            int byteRate,
            short blockAlign)
        {
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channelCount);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(BitsPerSample);
        }

        private static void WriteDataChunk(BinaryWriter writer, float[] samples)
        {
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(samples.Length * sizeof(short));

            foreach (var sample in samples)
            {
                var clamped = Math.Clamp(sample, -1f, 1f);
                var pcm = (short)Math.Round(clamped * short.MaxValue);
                writer.Write(pcm);
            }
        }
    }
}