using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CutBookApi.DTOs;
using CutBookApi.Services;

namespace CutBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;

    public QueueController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ---- Customer Endpoints ----

    /// <summary>
    /// Customer sends a real-time haircut request to a shop.
    /// Shop owner gets a live notification via SignalR.
    /// </summary>
    [HttpPost("request")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateHaircutRequestDto dto)
    {
        try
        {
            var result = await _queueService.CreateRequestAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get my current active request status and token number</summary>
    [HttpGet("my-request")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetMyRequest()
    {
        try
        {
            var result = await _queueService.GetMyRequestAsync(GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ---- Shop Owner Endpoints ----

    /// <summary>Get all pending requests awaiting owner decision</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> GetPendingRequests()
    {
        try
        {
            var result = await _queueService.GetPendingRequestsAsync(GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Accept a customer request — adds to queue and sends token number to customer via SignalR.
    /// </summary>
    [HttpPost("accept/{requestId}")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> AcceptRequest(int requestId)
    {
        try
        {
            var result = await _queueService.AcceptRequestAsync(GetUserId(), requestId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Reject a customer request</summary>
    [HttpPost("reject/{requestId}")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> RejectRequest(int requestId)
    {
        try
        {
            var result = await _queueService.RejectRequestAsync(GetUserId(), requestId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>View the current live queue for the shop</summary>
    [HttpGet("live-queue")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> GetQueue()
    {
        try
        {
            var result = await _queueService.GetQueueAsync(GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Mark current customer as done and call the next customer.
    /// Next customer receives a real-time "YourTurn" notification.
    /// </summary>
    [HttpPost("next")]
    [Authorize(Roles = "ShopOwner")]
    public async Task<IActionResult> NextCustomer()
    {
        try
        {
            var result = await _queueService.NextCustomerAsync(GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
