using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ReviewDetailModel : PageModel
{
    private readonly IReviewService _review;
    private readonly IShopSubmissionRepository _subs;

    public ReviewDetailModel(IReviewService review, IShopSubmissionRepository subs)
    {
        _review = review;
        _subs = subs;
    }

    public ShopSubmission Submission { get; set; } = null!;

    [BindProperty] public long SubmissionId { get; set; }
    [BindProperty] public string? RejectionReason { get; set; }
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var sub = await _subs.FindByIdAsync(id);
        if (sub is null) return NotFound();
        Submission = sub;
        SubmissionId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        var adminId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var req = new ReviewRequest
        {
            Approve = action == "approve",
            RejectionReason = RejectionReason
        };

        try
        {
            await _review.ReviewAsync(SubmissionId, adminId, req);
            return RedirectToPage("/Admin/ReviewList");
        }
        catch (BusinessException ex)
        {
            Message = ex.Message;
            var sub = await _subs.FindByIdAsync(SubmissionId);
            if (sub is null) return NotFound();
            Submission = sub;
            return Page();
        }
    }
}