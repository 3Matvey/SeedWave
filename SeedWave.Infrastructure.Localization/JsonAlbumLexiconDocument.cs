namespace SeedWave.Infrastructure.Localization
{
    public class JsonAlbumLexiconDocument
    {
        public List<string> Adjectives { get; init; } = [];

        public List<string> Nouns { get; init; } = [];

        public string SingleLiteral { get; init; } = string.Empty;
    }
}
