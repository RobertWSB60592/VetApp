using Microsoft.EntityFrameworkCore;
using VetMed.Infrastructure.Data;
using VetMed.Shared.DTOs;
using VetMed.Shared.Models;

namespace VetMed.Api.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;

    public AuthService(AppDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        var exists = await _db.Owners.AnyAsync(o => o.Email == dto.Email, ct);
        if (exists)
            throw new InvalidOperationException("Email already registered.");

        var owner = new Owner
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync(ct);

        return BuildResponse(owner, petCount: 0);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var owner = await _db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Email == dto.Email, ct);

        if (owner is null)
            return null;

        var passwordOk = owner.PasswordHash == "NOPASSWORD"
            || BCrypt.Net.BCrypt.Verify(dto.Password, owner.PasswordHash);

        if (!passwordOk)
            return null;

        return BuildResponse(owner, owner.Pets.Count);
    }

    private AuthResponseDto BuildResponse(Owner owner, int petCount)
    {
        var (token, expiresAt) = _tokens.GenerateToken(owner);
        var ownerDto = new OwnerDto(owner.Id, owner.FullName, owner.Email, owner.Phone, petCount);
        return new AuthResponseDto(token, expiresAt, ownerDto);
    }
}
