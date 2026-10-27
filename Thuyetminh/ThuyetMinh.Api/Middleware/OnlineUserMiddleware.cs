using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Middleware
{
    public class OnlineUserMiddleware
    {
        private readonly RequestDelegate _next;

        public OnlineUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, OnlineUserService onlineUsers)
        {
            var path = context.Request.Path;

            // Bỏ qua static files
            if (!path.StartsWithSegments("/css") &&
                !path.StartsWithSegments("/js") &&
                !path.StartsWithSegments("/audio") &&
                !path.StartsWithSegments("/favicon") &&
                !path.StartsWithSegments("/_framework") &&
                !path.StartsWithSegments("/lib") &&
                !path.StartsWithSegments("/images"))
            {
                // Lấy hoặc tạo SessionId
                var sessionId = context.Request.Cookies["SessionId"];
                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToString("N");
                    context.Response.Cookies.Append("SessionId", sessionId, new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddHours(8)
                    });
                }

                // Kiểm tra xem người dùng có cookie "tm_visitor" không
                // Cookie này được tạo khi khách đã thanh toán / mua phút
                var visitorCookie = context.Request.Cookies["tm_visitor"];
                var hasPurchased = !string.IsNullOrEmpty(visitorCookie);

                // Xác định username + role
                var username = context.User?.Identity?.Name;
                var isAuthenticated = !string.IsNullOrEmpty(username);

                if (!isAuthenticated)
                {
                    if (hasPurchased)
                    {
                        // Khách đã thanh toán → đánh dấu online với mã visitor
                        username = $"Khách-{visitorCookie!.Substring(0, Math.Min(6, visitorCookie.Length))}";
                    }
                    else
                    {
                        // Khách vãng lai chưa thanh toán → KHÔNG ghi nhận online
                        // Vẫn cho qua middleware, không track
                        await _next(context);
                        return;
                    }
                }

                var role = context.User?.IsInRole("Admin") == true ? "Admin" :
                           context.User?.IsInRole("Owner") == true ? "Owner" : "Guest";

                // Xác định có đang ở trang nghe không
                var isListening = path.StartsWithSegments("/Listen") ||
                                  path.StartsWithSegments("/Narration");
                string? listeningShop = null;
                if (isListening)
                {
                    var placeParam = context.Request.Query["place"].ToString();
                    if (!string.IsNullOrEmpty(placeParam)) listeningShop = placeParam;
                }

                onlineUsers.Track(
                    sessionId,
                    username,
                    role,
                    context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    context.Request.Headers["User-Agent"].ToString(),
                    isListening,
                    listeningShop
                );
            }

            await _next(context);
        }
    }
}