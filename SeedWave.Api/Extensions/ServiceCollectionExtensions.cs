using SeedWave.Core.AudioComposition;
using SeedWave.Core.Catalog;
using SeedWave.Core.Generation;
using SeedWave.Core.Regions;
using SeedWave.Infrastructure.Audio;
using SeedWave.Infrastructure.Covers;
using SeedWave.Infrastructure.Localization;

namespace SeedWave.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddSeedWaveServices(IWebHostEnvironment environment)
            {
                var localesPath = Path.Combine(environment.ContentRootPath, "assets", "locales");

                services.AddSingleton<ILocaleProfileProvider>(_ =>
                    new JsonLocaleProfileProvider(localesPath));

                services.AddSingleton<ISeedDeriver, SeedDeriver>();
                services.AddSingleton<ReviewTextGenerator>();
                services.AddSingleton<SongTitleGenerator>();
                services.AddSingleton<ArtistNameGenerator>();
                services.AddSingleton<AlbumTitleGenerator>();
                services.AddSingleton<GenreGenerator>();
                services.AddSingleton<LikesGenerator>();

                services.AddSingleton<AudioProfileGenerator>();
                services.AddSingleton<CoverProfileBuilder>();
                services.AddSingleton<CompositionPlanBuilder>();

                services.AddSingleton<WavEncoder>();
                var soundFontPath = Path.Combine(
                    environment.ContentRootPath,
                    "assets",
                    "soundfonts",
                    "default.sf2");

                services.AddSingleton(new AudioSettings
                            {
                    SoundFontPath = soundFontPath
                });

                services.AddSingleton<IAudioRenderer, PreviewAudioRenderer>();
                services.AddSingleton<IMidiRenderer, MidiFileRenderer>();
                services.AddSingleton<ICoverRenderer, CoverSvgRenderer>();

                services.AddSingleton<SongGenerationService>();
                services.AddSingleton<CatalogGenerationService>();

                return services;
            }
        }
    }
}