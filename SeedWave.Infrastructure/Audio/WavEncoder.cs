using System.Text;

namespace SeedWave.Infrastructure.Audio
{
    /// <summary>
    /// Encodes floating-point audio samples into a 16-bit PCM WAV file.
    /// </summary>
    public class WavEncoder
    {
        private const short BitsPerSample = 16;
        private const short ChannelCount = 1;

        public byte[] Encode(float[] samples, int sampleRate)
        {
            ArgumentNullException.ThrowIfNull(samples);

            var dataSize = samples.Length * sizeof(short);
            var riffChunkSize = 36 + dataSize;
            var byteRate = sampleRate * ChannelCount * BitsPerSample / 8;
            var blockAlign = (short)(ChannelCount * BitsPerSample / 8);

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

            WriteRiffHeader(writer, riffChunkSize);
            WriteFormatChunk(writer, sampleRate, byteRate, blockAlign);
            WriteDataChunk(writer, samples);

            writer.Flush();

            return stream.ToArray();
        }

        private static void WriteRiffHeader(BinaryWriter writer, int riffChunkSize)
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(riffChunkSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        }

        private static void WriteFormatChunk(BinaryWriter writer, int sampleRate, int byteRate, short blockAlign)
        {
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(ChannelCount);
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