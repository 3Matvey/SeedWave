using SeedWave.Core.Regions;

namespace SeedWave.Core.Generation
{
    public class GenreGenerator
    {
        public string Generate(LocaleProfile localeProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(localeProfile);

            ValidateGenres(localeProfile.Genres);

            var random = new Random(seed);

            return SelectGenre(localeProfile.Genres, random);
        }

        private static void ValidateGenres(IReadOnlyList<string> genres)
        {
            if (genres.Count == 0)
                throw new InvalidOperationException("Genre catalog cannot be empty.");
        }

        private static string SelectGenre(IReadOnlyList<string> genres, Random random)
        {
            return genres[random.Next(genres.Count)];
        }
    }
}