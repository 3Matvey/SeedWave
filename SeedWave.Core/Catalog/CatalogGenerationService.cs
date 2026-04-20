using SeedWave.Core.Regions;

namespace SeedWave.Core.Catalog
{
    /// <summary>
    /// Generates a deterministic catalog page for the specified request parameters.
    /// </summary>
    public class CatalogGenerationService(
        ILocaleProfileProvider localeProfileProvider,
        SongGenerationService songGenerationService)
    {
        public async Task<CatalogPage> GenerateAsync(CatalogGenerationRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            ValidateRequest(request);

            var localeProfile = await localeProfileProvider.GetAsync(request.Region, cancellationToken);
            var songs = GenerateSongs(request, localeProfile);

            return CreateCatalogPage(request, songs);
        }

        private List<SongRecord> GenerateSongs(CatalogGenerationRequest request, LocaleProfile localeProfile)
        {
            var songs = new List<SongRecord>(request.PageSize);

            for (int offset = 0; offset < request.PageSize; offset++)
            {
                var sequenceIndex = CalculateSequenceIndex(request.Page, request.PageSize, offset);
                var identity = CreateSongIdentity(request, sequenceIndex);
                var song = songGenerationService.Generate(identity, request.LikesAverage, localeProfile);

                songs.Add(song);
            }

            return songs;
        }

        private static int CalculateSequenceIndex(int page, int pageSize, int offset)
        {
            return ((page - 1) * pageSize) + offset + 1;
        }

        private static SongIdentity CreateSongIdentity(CatalogGenerationRequest request, int sequenceIndex)
        {
            return new SongIdentity
            (
                request.Region,
                request.Seed,
                request.Page,
                sequenceIndex
            );
        }

        private static CatalogPage CreateCatalogPage(CatalogGenerationRequest request,IReadOnlyList<SongRecord> songs)
        {
            return new CatalogPage
            (
                request.Region,
                request.Seed,
                request.LikesAverage,
                request.Page,
                request.PageSize,
                songs
            );
        }

        private static void ValidateRequest(CatalogGenerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Region))
                throw new ArgumentException("Region is required.", nameof(request));

            if (request.Page <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.Page), "Page must be greater than zero.");

            if (request.PageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(request.PageSize), "Page size must be greater than zero.");

            if (request.LikesAverage < 0 || request.LikesAverage > 10)
                throw new ArgumentOutOfRangeException(
                    nameof(request.LikesAverage),
                    "Average likes must be between 0 and 10.");
        }   
    }
}