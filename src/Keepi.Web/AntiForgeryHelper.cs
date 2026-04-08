using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;

namespace Keepi.Web;

public interface IAntiforgeryHelper
{
    Task<bool> IsValidRequest(HttpContext context, string apiBasePath);
}

internal sealed class AntiForgeryHelper(IAntiforgery antiforgery, ILogger<AntiForgeryHelper> logger)
    : IAntiforgeryHelper
{
    public async Task<bool> IsValidRequest(HttpContext context, string apiBasePath)
    {
        if (!context.Request.Path.StartsWithSegments(other: apiBasePath))
        {
            return true;
        }

        if (await antiforgery.IsRequestValidAsync(httpContext: context))
        {
            return true;
        }

        logger.LogWarning(
            "Received request {Url} that failed the anti forgery request validation",
            context.Request.GetDisplayUrl()
        );

        return false;
    }
}
