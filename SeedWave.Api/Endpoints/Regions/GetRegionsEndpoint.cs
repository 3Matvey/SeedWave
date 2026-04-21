using SeedWave.Api.Contracts.Regions;
using SeedWave.Core.Regions;

namespace SeedWave.Api.Endpoints.Regions
{
    public static class GetRegionsEndpoint
    {
        public static IEndpointRouteBuilder MapGetRegionsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/regions", HandleAsync)
                .WithName("GetRegions")
                .WithSummary("Returns all supported generation regions.");

            return endpoints;
        }

        private static async Task<IResult> HandleAsync(ILocaleProfileProvider localeProfileProvider, CancellationToken cancellationToken)
        {
            var regions = await localeProfileProvider.GetSupportedRegionsAsync(cancellationToken);
            var response = MapResponse(regions);

            return Results.Ok(response);
        }

        private static IReadOnlyList<RegionResponse> MapResponse(IReadOnlyList<string> regions)
        {
            return [.. regions.Select(region => new RegionResponse(region))];
        }
    }
}
