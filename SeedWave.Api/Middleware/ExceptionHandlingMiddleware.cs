using SeedWave.Api.Contracts.Common;
using System.Text.Json;

namespace SeedWave.Api.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (ArgumentException exception)
            {
                await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, exception.Message);
            }
            catch (FileNotFoundException exception)
            {
                await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                await WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError, exception.Message);
            }
            catch (Exception)
            {
                await WriteErrorResponseAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }
        private static async Task WriteErrorResponseAsync(HttpContext context, int statusCode, string message)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse(statusCode, message);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
