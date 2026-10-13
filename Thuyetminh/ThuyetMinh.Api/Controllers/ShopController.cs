using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
[Route("api/owner/shops")]
[Authorize(Roles = "Owner")]
public class ShopController : ControllerBase
{
    private readonly IShopService _shops;
    public ShopController(IShopService shops) => _shops = shops;

    private long CurrentUserId =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ShopResponse>>> MyShops()
    {
        var shops = await _shops.ListMyShopsAsync(CurrentUserId);
        return Ok(shops.Select(s => new ShopResponse(
            s.Id, s.Name, s.Address, s.Latitude, s.Longitude,
            s.ActivationRadiusMeters, s.PublishStatus, s.PublishedVersionId)));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ShopResponse>> GetMyShop(long id)
    {
        var s = await _shops.GetMyShopAsync(id, CurrentUserId);
        return Ok(new ShopResponse(
            s.Id, s.Name, s.Address, s.Latitude, s.Longitude,
            s.ActivationRadiusMeters, s.PublishStatus, s.PublishedVersionId));
    }

    [HttpPost("{id:long}/submissions")]
    public async Task<ActionResult<SubmissionResponse>> Submit(
        long id, [FromBody] ShopSubmissionRequest req)
    {
        var sub = await _shops.CreateSubmissionAsync(id, CurrentUserId, req);
        return Ok(new SubmissionResponse(
            sub.Id, sub.ShopId, sub.Status, sub.RejectionReason,
            sub.CreatedAt, sub.ReviewedAt));
    }

    [HttpGet("{id:long}/submissions")]
    public async Task<ActionResult<List<SubmissionResponse>>> MySubmissions(long id)
    {
        var list = await _shops.ListMySubmissionsAsync(id, CurrentUserId);
        return Ok(list.Select(s => new SubmissionResponse(
            s.Id, s.ShopId, s.Status, s.RejectionReason,
            s.CreatedAt, s.ReviewedAt)));
    }
}