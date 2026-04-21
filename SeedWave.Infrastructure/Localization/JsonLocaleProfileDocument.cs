namespace SeedWave.Infrastructure.Localization
{
    public class JsonLocaleProfileDocument
    {
        public string Region { get; init; } = string.Empty;

        public JsonArtistLexiconDocument Artists { get; init; } = new();

        public JsonSongLexiconDocument Songs { get; init; } = new();

        public JsonAlbumLexiconDocument Albums { get; init; } = new();

        public List<string> Genres { get; init; } = [];
    }
}