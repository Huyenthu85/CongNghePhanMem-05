using System.Security.Claims;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Middleware;

public class OnlineUserMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, OnlineUserService onlineUsers,
        IAnonymousVisitorService visitors)
    {
        var path = context.Request.Path;
        if (!path.StartsWithSegments("/css") && !path.StartsWithSegments("/js") &&
            !path.StartsWithSegments("/images") && !path.StartsWithSegments("/lib") &&
            !path.StartsWithSegments("/audio") && !path.StartsWithSegments("/favicon.ico") &&
            !path.StartsWithSegments("/swagger") && !path.StartsWithSegments("/_framework"))
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var agent = context.Request.Headers["User-Agent"].ToString();
            if (context.User.Identity?.IsAuthenticated == true &&
                long.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            {
                onlineUsers.Track($"account-{id}", context.User.Identity.Name ?? "Tài khoản",
                    context.User.IsInRole("Admin") ? "Admin" : "Owner", ip, agent);
            }
            else if (context.Request.Cookies.TryGetValue("tm_visitor", out var token))
            {
                var visitor = await visitors.FindExistingAsync(token);
                if (visitor != null)
                    onlineUsers.Track($"visitor-{visitor.Id}", $"Khách-{visitor.Id}", "Guest", ip, agent);
            }
        }
        await next(context);
    }
}
