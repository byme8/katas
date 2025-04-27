using TinyUrl.Models;
using TinyUrl.Services;

namespace TinyUrl.Handlers;

public static class UrlsHandler
{
    public static void MapAddUrl(this WebApplication app)
    {
        app.MapPost("/api/urls", async (
            IHttpContextAccessor httpContextAccessor,
            CreateUrlRequest request,
            TinyUrlService service) =>
        {
            var httpRequest = httpContextAccessor.HttpContext?.Request;
            if (httpRequest == null)
            {
                return Results.BadRequest("Invalid host");
            }

            var host = $"{httpRequest.Scheme}://{httpRequest.Host}";
            var redirectUrl = await service.AddUrl(host, request.Url);
            var response = new CreateUrlResponse
            {
                Url = redirectUrl
            };

            return Results.Ok(response);
        });
    }
}