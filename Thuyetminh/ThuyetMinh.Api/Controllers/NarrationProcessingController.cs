
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
[Route("api/admin/narration-processing")]
[Authorize(Roles = "Admin")]
public class NarrationProcessingController : ControllerBase
{
    private readonly NarrationProcessingService _service;

    public NarrationProcessingController(NarrationProcessingService service)
    {
        _service = service;
    }

    [HttpPost("{submissionId:long}/translations")]
    public async Task<IActionResult> ProcessTranslations(
        long submissionId,
        CancellationToken cancellationToken)
    {
        if (submissionId <= 0)
            return BadRequest(new { error = "ID bai gui khong hop le." });

        try
        {
            var result = await _service.ProcessAsync(
                submissionId, cancellationToken);

            return Ok(new
            {
                shopSubmissionId = submissionId,
                languages = result
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
