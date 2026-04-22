using SeedWave.Core.Catalog;
using SeedWave.Core.Generation;
using SeedWave.Core.Regions;

namespace SeedWave.Api.Endpoints.Catalog
{
    public static class GetSongCoverEndpoint
    {
        public static IEndpointRouteBuilder MapGetSongCoverEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/catalog/{sequenceIndex:int}/cover", HandleAsync)
                .WithName("GetSongCover")
                .WithSummary("Returns a generated SVG cover for the specified song.");

            return endpoints;
        }

        private static async Task<IResult> HandleAsync(
            int sequenceIndex,
            string region,
            ulong seed,
            int page,
            double likesAverage,
            ILocaleProfileProvider localeProfileProvider,
            ISeedDeriver seedDeriver,
            SongGenerationService songGenerationService,
            AudioProfileGenerator audioProfileGenerator,
            CoverProfileBuilder coverProfileBuilder,
            ICoverRenderer coverRenderer,
            CancellationToken cancellationToken)
        {
            Validate(sequenceIndex, page);

            var locale = await GetLocale(localeProfileProvider, region, cancellationToken);
            var identity = CreateIdentity(region, seed, page, sequenceIndex);
            var song = songGenerationService.Generate(identity, likesAverage, locale);

            var (audioProfile, coverProfile, coverSeed) = BuildProfiles(
                identity,
                seedDeriver,
                audioProfileGenerator,
                coverProfileBuilder);

            var fileName = BuildFileName(song, sequenceIndex);

            var cover = coverRenderer.Render(
                coverProfile,
                audioProfile,
                coverSeed,
                song.Core.Title,
                song.Core.Artist,
                fileName);

            return Results.File(cover.Content, cover.ContentType, cover.FileName);
        }

        private static void Validate(int sequenceIndex, int page)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequenceIndex);

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        }

        private static Task<LocaleProfile> GetLocale(
            ILocaleProfileProvider provider,
            string region,
            CancellationToken ct)
        {
            return provider.GetAsync(region, ct);
        }

        private static SongIdentity CreateIdentity(
            string region,
            ulong seed,
            int page,
            int sequenceIndex)
        {
            return new SongIdentity(region, seed, page, sequenceIndex);
        }

        private static (AudioProfile, CoverProfile, int) BuildProfiles(
            SongIdentity identity,
            ISeedDeriver seedDeriver,
            AudioProfileGenerator audioProfileGenerator,
            CoverProfileBuilder coverProfileBuilder)
        {
            var audioSeed = seedDeriver.Derive(identity, SeedPurpose.Audio);
            var coverSeed = seedDeriver.Derive(identity, SeedPurpose.Cover);

            var audioProfile = audioProfileGenerator.Generate(audioSeed);
            var coverProfile = coverProfileBuilder.Build(audioProfile, coverSeed);

            return (audioProfile, coverProfile, coverSeed);
        }

        private static string BuildFileName(SongRecord song, int index)
        {
            var slug = Slugify(song.Core.Title);

            return string.IsNullOrWhiteSpace(slug)
                ? $"song-{index}"
                : $"{slug}-{index}";
        }

        private static string Slugify(string value)
        {
            var chars = value
                .Trim()
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray();

            var raw = new string(chars);

            while (raw.Contains("--", StringComparison.Ordinal))
                raw = raw.Replace("--", "-", StringComparison.Ordinal);

            return raw.Trim('-');
        }
    }
}