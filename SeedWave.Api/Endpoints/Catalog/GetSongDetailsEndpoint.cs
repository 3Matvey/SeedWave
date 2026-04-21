using SeedWave.Api.Contracts.Catalog;
using SeedWave.Core.Catalog;
using SeedWave.Core.Regions;

namespace SeedWave.Api.Endpoints.Catalog
{
    public static class GetSongDetailsEndpoint
    {
        public static IEndpointRouteBuilder MapGetSongDetailsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/catalog/{sequenceIndex:int}", HandleAsync)
                .WithName("GetSongDetails")
                .WithSummary("Returns detailed information about a generated song.");

            return endpoints;
        }

        private static async Task<IResult> HandleAsync
        (
            int sequenceIndex,
            string region,
            ulong seed,
            double likesAverage,
            int page,
            ILocaleProfileProvider localeProfileProvider,
            SongGenerationService songGenerationService,
            CancellationToken cancellationToken
        )
        {
            if (sequenceIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(sequenceIndex), "Sequence index must be greater than zero.");

            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");

            var localeProfile = await localeProfileProvider.GetAsync(region, cancellationToken);

            var identity = new SongIdentity
            (
                region,
                seed,
                page,
                sequenceIndex
            );

            var song = songGenerationService.Generate(identity, likesAverage, localeProfile);
            var response = MapResponse(song);

            return Results.Ok(response);
        }

        private static SongDetailsResponse MapResponse(SongRecord song)
        {
            return new SongDetailsResponse
            (
                song.Identity.SequenceIndex,
                song.Identity.Region,
                song.Identity.UserSeed,
                song.Core.Title,
                song.Core.Artist,
                song.Core.AlbumTitle,
                song.Core.Genre,
                song.Likes,
                song.ReviewText
            );
        }
    }
}
