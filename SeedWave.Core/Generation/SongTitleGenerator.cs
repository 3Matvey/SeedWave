using SeedWave.Core.Regions;

namespace SeedWave.Core.Generation
{
    public class SongTitleGenerator
    {
        public string Generate(LocaleProfile localeProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(localeProfile);

            ValidateLexicon(localeProfile.Songs);

            var random = new Random(seed);

            return GenerateTitle(localeProfile.Songs, random);
        }

        private static void ValidateLexicon(SongLexicon songs)
        {
            if (songs.Adjectives.Count == 0)
                throw new InvalidOperationException("Song adjective lexicon cannot be empty.");

            if (songs.Nouns.Count == 0)
                throw new InvalidOperationException("Song noun lexicon cannot be empty.");
        }

        private static string GenerateTitle(SongLexicon songs, Random random)
        {
            var adjective = songs.Adjectives[random.Next(songs.Adjectives.Count)];
            var noun = songs.Nouns[random.Next(songs.Nouns.Count)];

            return $"{adjective} {noun}";
        }
    }
}