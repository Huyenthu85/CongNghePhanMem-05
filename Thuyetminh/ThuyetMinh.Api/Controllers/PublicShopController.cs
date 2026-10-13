using Microsoft.AspNetCore.Mvc;
using ThuyetMinh.Business.Dtos;
using ThuyetMinh.Business.Services;

namespace ThuyetMinh.Api.Controllers;

[ApiController]
[Route("api/public/shops")]
public class PublicShopController : ControllerBase
{
    private readonly IShopService _shops;
    public PublicShopController(IShopService shops) => _shops = shops;

    [HttpGet("{id:long}/content")]
    public async Task<ActionResult<ShopContentResponse>> GetContent(long id)
    {
        var content = await _shops.GetPublishedContentAsync(id);
        return content is null ? NotFound() : Ok(content);
    }
}