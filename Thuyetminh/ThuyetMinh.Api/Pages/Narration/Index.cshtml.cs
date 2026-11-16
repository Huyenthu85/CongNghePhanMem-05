using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Api.Pages.Narration;

public sealed class ApprovedShopInfo
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ActivationRadiusMeters { get; set; }
}

public class IndexModel : PageModel
{
    private const string CookieName = "tm_visitor";
    private readonly IAnonymousVisitorService _visitorService;
    private readonly IListeningBalanceRepository _balanceRepository;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public bool RequireGps => _configuration.GetValue<bool>("Listening:RequireGps");
    public long ShopId { get; private set; }
    public long RemainingSeconds { get; private set; }
    public List<ApprovedShopInfo> ApprovedShops { get; private set; } = new();

    public IndexModel(IAnonymousVisitorService visitorService,
        IListeningBalanceRepository balanceRepository, AppDbContext db,
        IWebHostEnvironment environment, IConfiguration configuration)
    {
        _visitorService = visitorService;
        _balanceRepository = balanceRepository;
        _db = db;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        //Làm giao diện trước
        /*Request.Cookies.TryGetValue(CookieName, out string? token);
        var visitor = await _visitorService.FindExistingAsync(token);
        if (visitor == null) return Redirect("/payment");

        var balance = await _balanceRepository.GetOrCreateAsync(visitor.Id);
        RemainingSeconds = balance.RemainingSeconds;
        if (RemainingSeconds <= 0) return Redirect("/payment");*/

        Request.Cookies.TryGetValue(CookieName, out string? token);
        var visitor = await _visitorService.FindExistingAsync(token);
        bool localPreview = _environment.IsDevelopment()
            && ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext)
            && Request.Query["preview"] == "1";

        if (visitor == null)
        {
            if (!localPreview) return Redirect("/payment");
            RemainingSeconds = 0;
        }
        else
        {
            var balance = await _balanceRepository.GetOrCreateAsync(visitor.Id);
            RemainingSeconds = balance.RemainingSeconds;
            if (RemainingSeconds <= 0 && !localPreview)
                return Redirect("/payment");
        }

        ApprovedShops = await _db.Shops.AsNoTracking()
            .Where(x => x.PublishStatus == PublishStatus.Approved)
            .Select(x => new ApprovedShopInfo
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                ActivationRadiusMeters = x.ActivationRadiusMeters
            })
            .ToListAsync();
        ShopId = ApprovedShops.FirstOrDefault()?.Id ?? 0;

        // Preserve the original direct-local development fallback.
        if (ShopId == 0 && _environment.IsDevelopment() &&
            ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext))
            ShopId = await _db.Shops.AsNoTracking().Select(x => x.Id).FirstOrDefaultAsync();

        return Page();
    }
}
