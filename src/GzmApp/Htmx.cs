namespace GzmApp;

public static class Htmx
{
    public const string ErrorMessage = "Coś poszło nie tak na serwerze. Spróbuj jeszcze raz.";

    // HTMX 4 sends "HX-Request: true" with every request it makes.
    public static bool IsHtmx(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Headers["HX-Request"] == "true";
    }

    // HTMX 4 swaps 4xx/5xx responses into the page: a server error in an HTMX request returns a short fragment,
    // not the whole error page.
    public static IApplicationBuilder UseHtmxErrorFragments(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (Exception ex) when (context.Request.IsHtmx() && !context.Response.HasStarted)
            {
                context.RequestServices.GetRequiredService<ILogger<HtmxLog>>().LogError(ex, "Błąd w żądaniu HTMX {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync($"<p class=\"alert alert-danger\" role=\"alert\">{ErrorMessage}</p>");
            }
        });
    }

    public sealed class HtmxLog;
}
