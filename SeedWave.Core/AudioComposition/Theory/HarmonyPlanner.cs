namespace SeedWave.Core.AudioComposition.Theory
{
    public class HarmonyPlanner
    {
        private static readonly int[][] CommonProgressions =
        [
            [0, 5, 3, 4],
            [0, 3, 4, 3],
            [0, 4, 5, 3],
            [0, 2, 5, 4]
        ];

        public IReadOnlyList<HarmonicBar> Build(int bars, MusicTheoryContext theory, int seed)
        {
            ArgumentNullException.ThrowIfNull(theory);

            var random = new Random(seed);
            var baseProgression = CommonProgressions[random.Next(CommonProgressions.Length)];

            var result = new List<HarmonicBar>(bars);

            for (var bar = 0; bar < bars; bar++)
            {
                var degree = baseProgression[bar % baseProgression.Length];
                var chord = theory.GetTriadChord(degree);

                result.Add(new HarmonicBar(degree, chord));
            }

            if (result.Count > 0)
            {
                result[^1] = new HarmonicBar(0, theory.GetTriadChord(0));
            }

            return result;
        }
    }
}