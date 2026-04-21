using SeedWave.Core.Regions;
using System.Text.Json;

namespace SeedWave.Infrastructure.Localization
{
    public class JsonLocaleProfileProvider : ILocaleProfileProvider
    {
        private readonly string _localesDirectoryPath;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonLocaleProfileProvider(string localesDirectoryPath)
        {
            _localesDirectoryPath = localesDirectoryPath;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<LocaleProfile> GetAsync(string region, CancellationToken cancellationToken)
        {
            ValidateRegion(region);

            var filePath = BuildLocaleFilePath(region);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Locale profile '{region}' was not found.", filePath);

            var document = await LoadDocumentAsync(filePath, cancellationToken);

            return MapToLocaleProfile(document);
        }

        public async Task<IReadOnlyList<string>> GetSupportedRegionsAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_localesDirectoryPath))
                return [];

            string[] filePaths = Directory.GetFiles(_localesDirectoryPath, "*.json");

            var regions = new List<string>(filePaths.Length);

            foreach (var filePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                regions.Add(Path.GetFileNameWithoutExtension(filePath));
            }

            return regions;
        }

        private string BuildLocaleFilePath(string region)
        {
            return Path.Combine(_localesDirectoryPath, $"{region}.json");
        }

        private async Task<JsonLocaleProfileDocument> LoadDocumentAsync(string filePath, CancellationToken cancellationToken)
        {
            await using var stream = File.OpenRead(filePath);

            var document = await JsonSerializer.DeserializeAsync<JsonLocaleProfileDocument>
            (
                stream,
                _jsonSerializerOptions,
                cancellationToken
            );

            return document
                ?? throw new InvalidOperationException($"Locale profile file '{filePath}' is empty or invalid.");
        }

        private static LocaleProfile MapToLocaleProfile(JsonLocaleProfileDocument document)
        {
            return new LocaleProfile
            (
                document.Region,
                MapArtists(document.Artists),
                MapSongs(document.Songs),
                MapAlbums(document.Albums),
                MapReviews(document.Reviews),
                document.Genres
            );
        }
        private static ReviewLexicon MapReviews(JsonReviewLexiconDocument reviews)
        {
            return new ReviewLexicon
            (
                reviews.Openings,
                reviews.MoodDescriptors,
                reviews.ArrangementDescriptors,
                reviews.Closings
            );
        }

        private static ArtistLexicon MapArtists(JsonArtistLexiconDocument artists)
        {
            return new ArtistLexicon
            (
                artists.FirstNames,
                artists.LastNames,
                artists.BandPrefixes,
                artists.BandNouns
            );
        }

        private static SongLexicon MapSongs(JsonSongLexiconDocument songs)
        {
            return new SongLexicon
            (
                songs.Adjectives,
                songs.Nouns
            );
        }

        private static AlbumLexicon MapAlbums(JsonAlbumLexiconDocument albums)
        {
            return new AlbumLexicon
            (
                albums.Adjectives,
                albums.Nouns,
                albums.SingleLiteral
            );
        }

        private static void ValidateRegion(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentException("Region is required.", nameof(region));
        }
    }
}
