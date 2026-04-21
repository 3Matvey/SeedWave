using SeedWave.Api.Contracts.Catalog;
using SeedWave.Core.Catalog;

namespace SeedWave.Api.Endpoints.Catalog
{
    public static class GetCatalogEndpoint
    {
        public static IEndpointRouteBuilder MapGetCatalogEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/catalog", HandleAsync)
                .WithName("GetCatalog")
                .WithSummary("Generates a deterministic catalog page.");

            return endpoints;
        }

        private static async Task<IResult> HandleAsync(
            string region,
            ulong seed,
            double likesAverage,
            int page,
            int pageSize,
            CatalogGenerationService catalogGenerationService,
            CancellationToken cancellationToken)
        {
            var request = new CatalogGenerationRequest
            (
                region,
                seed,
                likesAverage,
                page,
                pageSize
            );

            var catalogPage = await catalogGenerationService.GenerateAsync(request, cancellationToken);
            var response = MapResponse(catalogPage);

            return Results.Ok(response);
        }

        private static CatalogPageResponse MapResponse(CatalogPage catalogPage)
        {
            var songs = catalogPage.Songs
                .Select(song => new CatalogItemResponse
                (
                    song.Identity.SequenceIndex,
                    song.Core.Title,
                    song.Core.Artist,
                    song.Core.AlbumTitle,
                    song.Core.Genre,
                    song.Likes
                ))
                .ToList();

            return new CatalogPageResponse
            (
                catalogPage.Region,
                catalogPage.Seed,
                catalogPage.LikesAverage,
                catalogPage.Page,
                catalogPage.PageSize,
                songs
            );
        }
    }
}
