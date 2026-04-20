using SeedWave.Core.Regions;

namespace SeedWave.Core.Generation
{
    public class AlbumTitleGenerator
    {
        public AlbumGenerationResult Generate(LocaleProfile localeProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(localeProfile);

            ValidateLexicon(localeProfile.Albums);

            var random = new Random(seed);

            return random.NextDouble() < 0.30
                ? GenerateSingle(localeProfile.Albums)
                : GenerateAlbumTitle(localeProfile.Albums, random);
        }

        private static void ValidateLexicon(AlbumLexicon albums)
        {
            if (albums.Adjectives.Count == 0)
                throw new InvalidOperationException("Album adjective lexicon cannot be empty.");

            if (albums.Nouns.Count == 0)
                throw new InvalidOperationException("Album noun lexicon cannot be empty.");

            if (string.IsNullOrWhiteSpace(albums.SingleLiteral))
                throw new InvalidOperationException("Album single literal cannot be empty.");
        }

        private static AlbumGenerationResult GenerateSingle(AlbumLexicon albums)
        {
            return new AlbumGenerationResult
            (
                albums.SingleLiteral,
                true
            );
        }

        private static AlbumGenerationResult GenerateAlbumTitle(AlbumLexicon albums, Random random)
        {
            var adjective = albums.Adjectives[random.Next(albums.Adjectives.Count)];
            var noun = albums.Nouns[random.Next(albums.Nouns.Count)];

            return new AlbumGenerationResult
            (
                $"{adjective} {noun}",
                false
            );
        }
    }
}