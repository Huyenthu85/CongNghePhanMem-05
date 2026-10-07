using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
[Route("api/admin/submissions")]
[Authorize(Roles = "Admin")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _review;
    public ReviewController(IReviewService review) => _review = review;

    private long CurrentUserId =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("pending")]
    public async Task<ActionResult<List<SubmissionResponse>>> Pending()
    {
        var list = await _review.ListPendingAsync();
        return Ok(list.Select(s => new SubmissionResponse(
            s.Id, s.ShopId, s.Status, s.RejectionReason,
            s.CreatedAt, s.ReviewedAt)));
    }

    [HttpPost("{id:long}/review")]
    public async Task<ActionResult<SubmissionResponse>> Review(
        long id, [FromBody] ReviewRequest req)
    {
        var sub = await _review.ReviewAsync(id, CurrentUserId, req);
        return Ok(new SubmissionResponse(
            sub.Id, sub.ShopId, sub.Status, sub.RejectionReason,
            sub.CreatedAt, sub.ReviewedAt));
    }
}