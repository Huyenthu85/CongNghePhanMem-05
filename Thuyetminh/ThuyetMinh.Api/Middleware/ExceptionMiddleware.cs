using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (AuthException e)               { await Write(ctx, 401, e.Message); }
        catch (UnauthorizedAccessException e) { await Write(ctx, 403, e.Message); }
        catch (KeyNotFoundException e)        { await Write(ctx, 404, e.Message); }
        catch (BusinessException e)           { await Write(ctx, 400, e.Message); }
    }

    private static Task Write(HttpContext ctx, int code, string msg)
    {
        ctx.Response.StatusCode = code;
        return ctx.Response.WriteAsJsonAsync(new { error = msg });
    }
}
