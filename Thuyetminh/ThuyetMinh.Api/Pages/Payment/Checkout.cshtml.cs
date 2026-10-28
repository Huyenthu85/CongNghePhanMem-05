
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
namespace ThuyetMinh.Api.Pages.Payment
{
    public class CheckoutModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IAnonymousVisitorService _visitorService;
        private readonly AppDbContext _db;
        public CheckoutModel(IWebHostEnvironment environment, IAnonymousVisitorService visitorService, AppDbContext db)
        {
            _environment = environment;
            _visitorService = visitorService;
            _db = db;
        }
        public long AmountVnd { get; } = 20000;
        public long GrantedSeconds { get; } = 900;
        public string PublicQrHost => HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Demo:PublicHost"] ?? "";
                // NGROK-PUBLIC-QR-20261027: DEMO only. Never enable on a production deployment.
        public bool ShowDemoControls => _environment.IsDevelopment() &&
            ((User.IsInRole("Admin") && ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext)) ||
             (string.Equals(Request.Host.Host, HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Demo:PublicHost"], StringComparison.OrdinalIgnoreCase) &&
              HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<bool>("Demo:EnablePublicQr")));
        public async Task<IActionResult> OnGetAsync()
        {
            Response.Headers.CacheControl = "no-store";
            Request.Cookies.TryGetValue("tm_visitor", out string? token);
            var visitor = await _visitorService.FindExistingAsync(token);
            if (visitor != null)
            {
                long remainingSeconds = await _db.ListeningBalances
                    .AsNoTracking()
                    .Where(x => x.AnonymousVisitorId == visitor.Id)
                    .Select(x => x.RemainingSeconds)
                    .FirstOrDefaultAsync();
                bool adminDemo = Request.Query["demo"] == "1" && ShowDemoControls;
                if (remainingSeconds > 0 && !adminDemo)
                {
                    return Redirect("/Narration");
                }
            }
            return Page();
        }
    }
}
