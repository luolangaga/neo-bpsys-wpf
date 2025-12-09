using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using PluginMarket.Api.Data;

namespace PluginMarket.Api.Filters;

public class AdminApiKeyAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<AdminOptions>>().Value;
        var has = context.HttpContext.Request.Headers.TryGetValue("X-Admin-Key", out var values);
        if (!has || values.Count == 0 || string.IsNullOrWhiteSpace(options.ApiKey) || values[0] != options.ApiKey)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        await next();
    }
}
