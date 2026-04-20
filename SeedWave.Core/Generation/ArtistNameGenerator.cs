using SeedWave.Core.Regions;

namespace SeedWave.Core.Generation
{
    public class ArtistNameGenerator
    {
        public string Generate(LocaleProfile localeProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(localeProfile);

            var artists = localeProfile.Artists;

            ValidateLexicon(artists);

            var random = new Random(seed);

            return random.NextDouble() < 0.45
                ? GenerateBandName(artists, random)
                : GeneratePersonalName(artists, random);
        }

        private static void ValidateLexicon(ArtistLexicon artists)
        {
            if (artists.FirstNames.Count == 0)
                throw new InvalidOperationException("Artist first-name lexicon cannot be empty.");

            if (artists.LastNames.Count == 0)
                throw new InvalidOperationException("Artist last-name lexicon cannot be empty.");

            if (artists.BandPrefixes.Count == 0)
                throw new InvalidOperationException("Artist band-prefix lexicon cannot be empty.");

            if (artists.BandNouns.Count == 0)
                throw new InvalidOperationException("Artist band-noun lexicon cannot be empty.");
        }

        private static string GenerateBandName(ArtistLexicon artists, Random random)
        {
            var prefix = artists.BandPrefixes[random.Next(artists.BandPrefixes.Count)];
            var noun = artists.BandNouns[random.Next(artists.BandNouns.Count)];

            return $"{prefix} {noun}";
        }

        private static string GeneratePersonalName(ArtistLexicon artists, Random random)
        {
            var firstName = artists.FirstNames[random.Next(artists.FirstNames.Count)];
            var lastName = artists.LastNames[random.Next(artists.LastNames.Count)];

            return $"{firstName} {lastName}";
        }
    }
}