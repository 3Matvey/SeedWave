namespace SeedWave.Core.Regions
{
    /// <summary>
    /// Provides locale profiles used during deterministic catalog generation.
    /// </summary>
    public interface ILocaleProfileProvider
    {
        Task<LocaleProfile> GetAsync(string region, CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetSupportedRegionsAsync(CancellationToken cancellationToken);
    }
}