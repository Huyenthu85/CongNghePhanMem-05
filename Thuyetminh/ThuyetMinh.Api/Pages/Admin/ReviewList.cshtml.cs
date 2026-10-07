using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ReviewListModel : PageModel
{
    private readonly IReviewService _review;
    public ReviewListModel(IReviewService review) => _review = review;

    public List<ShopSubmission> Submissions { get; set; } = new();

    public async Task OnGetAsync()
    {
        Submissions = await _review.ListPendingAsync();
    }
}