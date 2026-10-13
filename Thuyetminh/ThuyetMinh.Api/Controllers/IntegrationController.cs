using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
[Route("api/integration/shops")]
[Authorize(Roles = "Admin")]
public class IntegrationController : ControllerBase
{
    private readonly IReviewService _review;
    public IntegrationController(IReviewService review) => _review = review;

    [HttpPut("{id:long}/publish-status")]
    public async Task<IActionResult> UpdatePublishStatus(long id, [FromBody] PublishStatus status)
    {
        await _review.UpdatePublishStatusAsync(id, status);
        return NoContent();
    }
}