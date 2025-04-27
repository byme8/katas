using TinyUrl.Services;

namespace TinyUrl.Handlers;

public static class RedirectMiddleware
{
    public static void MapTinyUrlRedirect(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var httpRequest = context.Request;
            var path = httpRequest.Path.Value;

            if (string.IsNullOrEmpty(path) || path.StartsWith("/api"))
            {
                await next.Invoke();
                return;
            }

            var service = context.RequestServices.GetRequiredService<TinyUrlService>();
            var redirect = await service.GetUrl(path);

            if (string.IsNullOrEmpty(redirect))
            {
                await next.Invoke();
                return;
            }

            context.Response.Redirect(redirect);
        });
    }
}