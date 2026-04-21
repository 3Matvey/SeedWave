using System;
using System.Collections.Generic;
using System.Text;

namespace SeedWave.Core.Generation
{
    public class AudioProfileGenerator
    {
        public AudioProfile Generate(int seed)
        {
            var random = new Random(seed);

            var tempoBpm = GenerateTempo(random);
            var key = GenerateKey(random);
            var mode = GenerateMode(random);
            var bars = GenerateBars(random);
            var leadOctave = GenerateLeadOctave(random);
            var bassOctave = GenerateBassOctave(leadOctave);

            return new AudioProfile
            (
                tempoBpm,
                key,
                mode,
                bars,
                leadOctave,
                bassOctave
            );
        }

        private static int GenerateTempo(Random random)
        {
            return random.Next(92, 131);
        }

        private static MusicalKey GenerateKey(Random random)
        {
            return (MusicalKey)random.Next(0, 12);
        }

        private static ScaleMode GenerateMode(Random random)
        {
            return random.NextDouble() < 0.65 ? ScaleMode.Minor : ScaleMode.Major;
        }

        private static int GenerateBars(Random random)
        {
            return random.NextDouble() < 0.5 ? 4 : 8;
        }

        private static int GenerateLeadOctave(Random random)
        {
            return random.Next(4, 6);
        }

        private static int GenerateBassOctave(int leadOctave)
        {
            return Math.Max(2, leadOctave - 2);
        }
    }

}
