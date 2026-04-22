using SeedWave.Core.AudioComposition;
using SeedWave.Core.Catalog;
using SeedWave.Core.Generation;
using SeedWave.Infrastructure.Audio;

namespace SeedWave.Api.Endpoints.Catalog
{
    public static class GetSongPreviewEndpoint
    {
        public static IEndpointRouteBuilder MapGetSongPreviewEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/catalog/{sequenceIndex:int}/preview", HandleAsync)
                .WithName("GetSongPreview")
                .WithSummary("Returns a generated WAV preview for the specified song.");

            return endpoints;
        }

        private static IResult HandleAsync
        (
            int sequenceIndex,
            string region,
            ulong seed,
            int page,
            ISeedDeriver seedDeriver,
            AudioProfileGenerator audioProfileGenerator,
            CompositionPlanBuilder compositionPlanBuilder,
            IAudioRenderer previewAudioRenderer
        )
        {
            if (sequenceIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(sequenceIndex), "Sequence index must be greater than zero.");

            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");

            var identity = new SongIdentity
            (
                region,
                seed,
                page,
                sequenceIndex
            );

            var audioSeed = seedDeriver.Derive(identity, SeedPurpose.Audio);
            var audioProfile = audioProfileGenerator.Generate(audioSeed);
            var compositionPlan = compositionPlanBuilder.Build(audioProfile, audioSeed);
            var renderedAudio = previewAudioRenderer.Render(compositionPlan, $"song-{sequenceIndex}.wav");

            return Results.File(renderedAudio.Content, renderedAudio.ContentType, renderedAudio.FileName);
        }
    }
}