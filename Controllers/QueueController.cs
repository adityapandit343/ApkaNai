using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CutBook.API.DTOs;
using CutBook.API.Services;

namespace CutBook.API.Controllers;

[ApiController]
[Route("api/shops/{shopId}/queue")]
public class QueueController : ControllerBase
{
    private readonly QueueService _queueService;

    public QueueController(QueueService queueService)
    {
        _queueService = queueService;
    }

    // GET api/shops/1/queue — live queue status (public, no login needed)
    [HttpGet]
    public async Task<IActionResult> GetStatus(int shopId)
    {
        var status = await _queueService.GetQueueStatusAsync(shopId);
        return Ok(status);
    }

    // POST api/shops/1/queue/join — customer join karta hai (no login needed)
    [HttpPost("join")]
    public async Task<IActionResult> Join(int shopId, [FromBody] JoinQueueDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            return BadRequest(new { message = "Naam dena zaroori hai" });

        var result = await _queueService.JoinQueueAsync(shopId, dto);
        if (result == null)
            return NotFound(new { message = "Shop nahi mili ya band hai" });

        return Ok(result);
    }

    // POST api/shops/1/queue/next — barber next bulata hai (login required)
    [Authorize]
    [HttpPost("next")]
    public async Task<IActionResult> CallNext(int shopId)
    {
        var result = await _queueService.CallNextAsync(shopId);
        if (result == null)
            return Ok(new { message = "Queue empty hai" });

        return Ok(result);
    }

    // PUT api/shops/1/queue/5/serving
    [Authorize]
    [HttpPut("{entryId}/serving")]
    public async Task<IActionResult> MarkServing(int entryId)
    {
        var ok = await _queueService.MarkServingAsync(entryId);
        return ok ? Ok(new { message = "Serving mark ho gaya" }) : NotFound();
    }

    // PUT api/shops/1/queue/5/done
    [Authorize]
    [HttpPut("{entryId}/done")]
    public async Task<IActionResult> MarkDone(int entryId)
    {
        var ok = await _queueService.MarkDoneAsync(entryId);
        return ok ? Ok(new { message = "Done mark ho gaya" }) : NotFound();
    }

    // PUT api/shops/1/queue/5/noshow
    [Authorize]
    [HttpPut("{entryId}/noshow")]
    public async Task<IActionResult> MarkNoShow(int entryId)
    {
        var ok = await _queueService.MarkNoShowAsync(entryId);
        return ok ? Ok(new { message = "No show mark ho gaya" }) : NotFound();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(int shopId)
    {
        var summary = await _queueService.GetQueueSummaryAsync(shopId);

        if (summary == null)
            return NotFound(new { message = "Shop nahi mili" });

        return Ok(summary);
    }

}
