using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CutBookApi.DTOs;
using CutBookApi.Services;

namespace CutBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ---- Shop Owner Endpoints ----

    /// <summary>Create shop (ShopOwner only). Gets 1 month free trial automatically.</summary>
    [HttpPost]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopDto dto)
    {
        try
        {
            var result = await _shopService.CreateShopAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update shop details</summary>
    [HttpPut]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> UpdateShop([FromBody] UpdateShopDto dto)
    {
        try
        {
            var result = await _shopService.UpdateShopAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Go Live — provide lat/long to make shop visible to customers</summary>
    [HttpPost("go-live")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> GoLive([FromBody] GoLiveDto dto)
    {
        try
        {
            var result = await _shopService.GoLiveAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Go Offline — hide shop from nearby search</summary>
    [HttpPost("go-offline")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> GoOffline()
    {
        try
        {
            await _shopService.GoOfflineAsync(GetUserId());
            return Ok(new { message = "Shop is now offline." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Get my shop details</summary>
    [HttpGet("my-shop")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> GetMyShop()
    {
        try
        {
            var result = await _shopService.GetMyShopAsync(GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Add a service to your shop</summary>
    [HttpPost("services")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> AddService([FromBody] CreateServiceDto dto)
    {
        try
        {
            var result = await _shopService.AddServiceAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Remove a service from your shop</summary>
    [HttpDelete("services/{serviceId}")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> DeleteService(int serviceId)
    {
        try
        {
            await _shopService.DeleteServiceAsync(GetUserId(), serviceId);
            return Ok(new { message = "Service removed." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ---- Customer Endpoints ----

    /// <summary>Search nearby shops by lat/long and optional radius</summary>
    [HttpPost("search-nearby")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> SearchNearby([FromBody] SearchShopDto dto)
    {
        var result = await _shopService.SearchNearbyShopsAsync(dto);
        return Ok(result);
    }
}
