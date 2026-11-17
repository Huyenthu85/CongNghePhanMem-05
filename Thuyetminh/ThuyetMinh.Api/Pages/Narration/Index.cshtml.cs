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
    public long? PublishedVersionId { get; set; }
    public Dictionary<string, string> Descriptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class IndexModel : PageModel
{
    private const string CookieName = "tm_visitor";
    private readonly IAnonymousVisitorService _visitorService;
    private readonly IListeningBalanceRepository _balanceRepository;
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public bool RequireGps => _configuration.GetValue("Listening:RequireGps", true) || !_environment.IsDevelopment();
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

    private static string PreviewText(string value)
    {
        const int limit = 220;

        var text = string.Join(
            " ",
            value.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries
            )
        );

        var starts = System.Globalization.StringInfo
            .ParseCombiningCharacters(text);

        if (starts.Length <= limit)
            return text;

        int end = starts[limit];
        int space = text.LastIndexOf(' ', end - 1);

        if (space > end * 0.8)
            end = space;

        return text[..end].TrimEnd() + "…";
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
                ActivationRadiusMeters = x.ActivationRadiusMeters,
                PublishedVersionId = x.PublishedVersionId
            })
            .ToListAsync();
        var published = from shop in _db.Shops.AsNoTracking()
                        join sub in _db.ShopSubmissions.AsNoTracking()
                            on shop.PublishedVersionId equals (long?)sub.Id
                        where shop.PublishStatus == PublishStatus.Approved
                           && sub.Status == SubmissionStatus.Approved
                           && sub.ShopId == shop.Id
                        select new
                        {
                            ShopId = shop.Id,
                            SubmissionId = sub.Id,
                            sub.Description
                        };

        var approvedById = ApprovedShops.ToDictionary(x => x.Id);

        foreach (var item in await published.ToListAsync())
        {
            if (approvedById.TryGetValue(item.ShopId, out var shop)
                && shop.PublishedVersionId == item.SubmissionId
                && !string.IsNullOrWhiteSpace(item.Description))
            {
                shop.Descriptions["vi"] = item.Description.Trim();
            }
        }

        var translations = await (
            from item in published
            join t in _db.Translations.AsNoTracking()
                on item.SubmissionId equals t.ShopSubmissionId
            join l in _db.Languages.AsNoTracking()
                on t.LanguageId equals l.Id
            where t.Status == "Success" && l.IsActive
            select new
            {
                item.ShopId,
                item.SubmissionId,
                l.Code,
                t.TranslatedText
            }
        ).ToListAsync();

        foreach (var item in translations)
        {
            var code = item.Code.ToLowerInvariant();

            if (code is not ("vi" or "en" or "zh" or "km"))
                continue;

            if (approvedById.TryGetValue(item.ShopId, out var shop)
                && shop.PublishedVersionId == item.SubmissionId
                && !string.IsNullOrWhiteSpace(item.TranslatedText))
            {
                shop.Descriptions.TryAdd(
                    code,
                    item.TranslatedText.Trim()
                );
            }
        }

        ShopId = ApprovedShops.FirstOrDefault()?.Id ?? 0;

        // Preserve the original direct-local development fallback.
        if (ShopId == 0 && _environment.IsDevelopment() &&
            ThuyetMinh.Api.Services.DemoNetwork.IsDirectLocalRequest(HttpContext))
            ShopId = await _db.Shops.AsNoTracking().Select(x => x.Id).FirstOrDefaultAsync();

        return Page();
    }
}
