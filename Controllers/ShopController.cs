using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CutBook.API.Data;
using CutBook.API.DTOs;
using CutBook.API.Models;

namespace CutBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopController : ControllerBase
{
    private readonly AppDbContext _db;

    public ShopController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/shop — apni shops dekho
    [HttpGet]
    public async Task<IActionResult> GetMyShops()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var shops = await _db.Shops
            .Include(s => s.Plan)
            .Include(s => s.QueueEntries)
            .Where(s => s.OwnerId == userId)
            .Select(s => new ShopResponseDto
            {
                Id           = s.Id,
                Name         = s.Name,
                Address      = s.Address,
                PhoneNumber  = s.PhoneNumber,
                IsActive     = s.IsActive,
                PlanName     = s.Plan != null ? s.Plan.Name : "Free",
                TotalWaiting = s.QueueEntries.Count(q => q.Status == QueueStatus.Waiting)
            })
            .ToListAsync();

        return Ok(shops);
    }

    // POST api/shop — nayi shop banao
    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var shop = new Shop
        {
            Name = dto.Name,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            AvgServiceTime = dto.AvgServiceTime <= 0 ? 15 : dto.AvgServiceTime,
            OwnerId = userId
        };


        _db.Shops.Add(shop);
        await _db.SaveChangesAsync();

        return Ok(new ShopResponseDto
        {
            Id          = shop.Id,
            Name        = shop.Name,
            Address     = shop.Address,
            PhoneNumber = shop.PhoneNumber,
            IsActive    = shop.IsActive,
            PlanName    = "Free"
        });
    }

    // PUT api/shop/1/toggle — shop band/chalu karo
    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleShop(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var shop   = await _db.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);

        if (shop == null) return NotFound();

        shop.IsActive = !shop.IsActive;
        await _db.SaveChangesAsync();

        return Ok(new { isActive = shop.IsActive });
    }

    // DELETE api/shop/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShop(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var shop   = await _db.Shops.FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == userId);

        if (shop == null) return NotFound();

        _db.Shops.Remove(shop);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Shop delete ho gayi" });
    }
    [AllowAnonymous]
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
    [FromQuery] double lat,
    [FromQuery] double lng,
    [FromQuery] double radiusKm = 10)
    {
        var today = DateTime.UtcNow.Date;

        var shops = await _db.Shops
            .Include(s => s.QueueEntries)
            .Where(s => s.Latitude != null && s.Longitude != null)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Address,
                phone = s.PhoneNumber,
                latitude = s.Latitude,
                longitude = s.Longitude,
                isOpen = s.IsActive,
                avgServiceTime = s.AvgServiceTime,
                distanceKm =
                    6371 * Math.Acos(
                        Math.Cos(lat * Math.PI / 180) *
                        Math.Cos((double)s.Latitude! * Math.PI / 180) *
                        Math.Cos(((double)s.Longitude! - lng) * Math.PI / 180) +
                        Math.Sin(lat * Math.PI / 180) *
                        Math.Sin((double)s.Latitude! * Math.PI / 180)
                    ),
                waitingCount = s.QueueEntries.Count(q =>
                    q.Status == QueueStatus.Waiting &&
                    q.JoinedAt.Date == today),
                servingToken = s.QueueEntries
                    .Where(q => q.Status == QueueStatus.Serving && q.JoinedAt.Date == today)
                    .Select(q => (int?)q.TokenNumber)
                    .FirstOrDefault()
            })
            .Where(s => s.distanceKm <= radiusKm)
            .OrderBy(s => s.distanceKm)
            .Take(50)
            .ToListAsync();

        return Ok(shops.Select(s => new
        {
            s.Id,
            s.Name,
            s.Address,
            s.phone,
            s.latitude,
            s.longitude,
            distanceKm = Math.Round(s.distanceKm, 2),
            s.isOpen,
            queueSummary = new
            {
                s.servingToken,
                s.waitingCount,
                estimatedWait = s.waitingCount * s.avgServiceTime,
                avgServiceTime = s.avgServiceTime
            }
        }));
    }

}
