using Microsoft.EntityFrameworkCore;
using CutBookApi.Data;
using CutBookApi.DTOs;
using CutBookApi.Helpers;
using CutBookApi.Models;

namespace CutBookApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterDto dto);
    Task<AuthResponseDto> RegisterShopOwnerAsync(ShopOwnerRegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;

    public AuthService(AppDbContext db, JwtHelper jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Role = "Customer"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> RegisterShopOwnerAsync(ShopOwnerRegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Role = "ShopOwner"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled.");

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user) => new()
    {
        Token = _jwt.GenerateToken(user),
        Role = user.Role,
        UserId = user.Id,
        FullName = user.FullName,
        Email = user.Email
    };
}
