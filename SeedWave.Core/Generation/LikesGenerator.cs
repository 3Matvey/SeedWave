namespace SeedWave.Core.Generation
{
    public class LikesGenerator
    {
        public int Generate(double averageLikes, int seed)
        {
            ValidateAverageLikes(averageLikes);

            var wholePart = (int)Math.Floor(averageLikes);
            var fractionalPart = averageLikes - wholePart;

            if (fractionalPart == 0)
                return wholePart;

            var random = new Random(seed);

            return ApplyFractionalRounding(wholePart, fractionalPart, random);
        }

        private static void ValidateAverageLikes(double averageLikes)
        {
            if (averageLikes < 0 || averageLikes > 10)
                throw new ArgumentOutOfRangeException(
                    nameof(averageLikes),
                    "Average likes must be between 0 and 10.");
        }

        private static int ApplyFractionalRounding(int wholePart, double fractionalPart, Random random)
        {
            return random.NextDouble() < fractionalPart
                ? wholePart + 1
                : wholePart;
        }
    }
}