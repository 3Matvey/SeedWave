namespace SeedWave.Api.Contracts.Common
{
    public record ErrorResponse
    (
        int StatusCode,
        string Message
    );
}
