namespace SeedWave.Core.Generation
{
    public sealed class AudioProfileGenerator
    {
        private static readonly AudioProfileTemplate[] Templates =
        [
            new(
                Name: "LoFiMinor",
                TempoMin: 74,
                TempoMax: 92,
                AllowedKeys:
                [
                    MusicalKey.C,
                    MusicalKey.D,
                    MusicalKey.E,
                    MusicalKey.F,
                    MusicalKey.G,
                    MusicalKey.A
                ],
                AllowedModes: [ScaleMode.Minor],
                AllowedBars: [8],
                LeadOctaves: [4],
                BassOffsetFromLead: 2),

            new(
                Name: "LoFiMajor",
                TempoMin: 78,
                TempoMax: 96,
                AllowedKeys:
                [
                    MusicalKey.C,
                    MusicalKey.D,
                    MusicalKey.F,
                    MusicalKey.G,
                    MusicalKey.A
                ],
                AllowedModes: [ScaleMode.Major],
                AllowedBars: [8],
                LeadOctaves: [4],
                BassOffsetFromLead: 2),

            new(
                Name: "SynthwaveMinor",
                TempoMin: 96,
                TempoMax: 116,
                AllowedKeys:
                [
                    MusicalKey.A,
                    MusicalKey.B,
                    MusicalKey.C,
                    MusicalKey.D,
                    MusicalKey.E,
                    MusicalKey.FSharp,
                    MusicalKey.G
                ],
                AllowedModes: [ScaleMode.Minor],
                AllowedBars: [8],
                LeadOctaves: [5],
                BassOffsetFromLead: 2),

            new(
                Name: "SynthwaveMajor",
                TempoMin: 100,
                TempoMax: 118,
                AllowedKeys:
                [
                    MusicalKey.C,
                    MusicalKey.D,
                    MusicalKey.E,
                    MusicalKey.G,
                    MusicalKey.A
                ],
                AllowedModes: [ScaleMode.Major],
                AllowedBars: [8],
                LeadOctaves: [5],
                BassOffsetFromLead: 2),

            new(
                Name: "DreamyMinor",
                TempoMin: 68,
                TempoMax: 84,
                AllowedKeys:
                [
                    MusicalKey.D,
                    MusicalKey.E,
                    MusicalKey.F,
                    MusicalKey.G,
                    MusicalKey.A
                ],
                AllowedModes: [ScaleMode.Minor],
                AllowedBars: [4, 8],
                LeadOctaves: [4, 5],
                BassOffsetFromLead: 2),

            new(
                Name: "DreamyMajor",
                TempoMin: 70,
                TempoMax: 88,
                AllowedKeys:
                [
                    MusicalKey.C,
                    MusicalKey.D,
                    MusicalKey.F,
                    MusicalKey.G,
                    MusicalKey.A
                ],
                AllowedModes: [ScaleMode.Major],
                AllowedBars: [4, 8],
                LeadOctaves: [4, 5],
                BassOffsetFromLead: 2)
        ];

        public AudioProfile Generate(int seed)
        {
            var random = new Random(seed);
            var template = PickTemplate(random);

            var tempoBpm = random.Next(template.TempoMin, template.TempoMax + 1);
            var key = Pick(random, template.AllowedKeys);
            var mode = Pick(random, template.AllowedModes);
            var bars = Pick(random, template.AllowedBars);
            var leadOctave = Pick(random, template.LeadOctaves);
            var bassOctave = Math.Max(2, leadOctave - template.BassOffsetFromLead);

            return new AudioProfile(
                TempoBpm: tempoBpm,
                Key: key,
                Mode: mode,
                Bars: bars,
                LeadOctave: leadOctave,
                BassOctave: bassOctave);
        }

        private static AudioProfileTemplate PickTemplate(Random random)
        {
            var roll = random.Next(100);

            return roll switch
            {
                < 28 => Templates[0], // LoFiMinor
                < 40 => Templates[1], // LoFiMajor
                < 66 => Templates[2], // SynthwaveMinor
                < 76 => Templates[3], // SynthwaveMajor
                < 90 => Templates[4], // DreamyMinor
                _ => Templates[5]     // DreamyMajor
            };
        }

        private static T Pick<T>(Random random, IReadOnlyList<T> values)
        {
            return values[random.Next(values.Count)];
        }

        private sealed record AudioProfileTemplate(
            string Name,
            int TempoMin,
            int TempoMax,
            IReadOnlyList<MusicalKey> AllowedKeys,
            IReadOnlyList<ScaleMode> AllowedModes,
            IReadOnlyList<int> AllowedBars,
            IReadOnlyList<int> LeadOctaves,
            int BassOffsetFromLead);
    }
}