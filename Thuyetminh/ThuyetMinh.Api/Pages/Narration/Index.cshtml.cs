using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;

namespace ThuyetMinh.Api.Pages.Narration;

public class IndexModel : PageModel
{
    private const string CookieName = "tm_visitor";
    private readonly IAnonymousVisitorService _visitorService;
    private readonly IListeningBalanceRepository _balanceRepository;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    public long ShopId { get; private set; }
    public long RemainingSeconds { get; private set; }

    public IndexModel(IAnonymousVisitorService visitorService, IListeningBalanceRepository balanceRepository,
        AppDbContext db, IWebHostEnvironment environment)
    {
        _visitorService = visitorService;
        _balanceRepository = balanceRepository;
        _db = db;
        _environment = environment;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Request.Cookies.TryGetValue(CookieName, out string? token);
        var visitor = await _visitorService.FindExistingAsync(token);
        if (visitor == null) return Redirect("/payment");
        var balance = await _balanceRepository.GetOrCreateAsync(visitor.Id);
        RemainingSeconds = balance.RemainingSeconds;
        if (RemainingSeconds <= 0) return Redirect("/payment");
        var approvedShop = await _db.Shops.AsNoTracking()
            .Where(x => x.PublishStatus == PublishStatus.Approved)
            .Select(x => x.Id).FirstOrDefaultAsync();
        ShopId = approvedShop;
        if (ShopId == 0 && _environment.IsDevelopment() &&
            ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext))
            ShopId = await _db.Shops.AsNoTracking().Select(x => x.Id).FirstOrDefaultAsync();
        return Page();
    }
}
