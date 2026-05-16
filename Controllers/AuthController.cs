using Microsoft.AspNetCore.Mvc;
using CutBookApi.DTOs;
using CutBookApi.Services;

namespace CutBookApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Register a new customer</summary>
    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterDto dto)
    {
        try
        {
            var result = await _auth.RegisterCustomerAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Register a new shop owner (starts 1-month free trial)</summary>
    [HttpPost("register/shopowner")]
    public async Task<IActionResult> RegisterShopOwner([FromBody] ShopOwnerRegisterDto dto)
    {
        try
        {
            var result = await _auth.RegisterShopOwnerAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Login for both customers and shop owners</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _auth.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
