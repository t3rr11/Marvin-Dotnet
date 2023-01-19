using Marvin.ClanQueueServer.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Marvin.ClanQueueServer.Attributes;

public class HeaderApiKeyAuthAttribute : ActionFilterAttribute
{
    private const string HeaderValue = "bWFydmluMjAyMg==";
    private const string HeaderName = "X-Marvin-API-Key";

    public HeaderApiKeyAuthAttribute()
    {
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var headerValue))
        {
            context.Result = new OkObjectResult(new ApiResponse<int>()
            {
                Data = 0,
                Message = "Missing header.",
                Status = "error",
                StatusCode = ApiResponseCodes.Unauthorized
            });
            return;
        }

        if (headerValue.Count != 1)
        {
            context.Result = new OkObjectResult(new ApiResponse<int>()
            {
                Data = 0,
                Message = "Wrong header values.",
                Status = "error",
                StatusCode = ApiResponseCodes.Unauthorized
            });
            return;
        }

        var key = headerValue.First();

        if (key != HeaderValue)
        {
            context.Result = new OkObjectResult(new ApiResponse<int>()
            {
                Data = 0,
                Message = "Wrong API key.",
                Status = "error",
                StatusCode = ApiResponseCodes.Unauthorized
            });
            return;
        }

        base.OnActionExecuting(context);
    }
}