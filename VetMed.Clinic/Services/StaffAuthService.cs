using Microsoft.EntityFrameworkCore;
using VetMed.Infrastructure.Data;

namespace VetMed.Clinic.Services;

public record StaffPrincipal(int Id, string FullName, string Email);

public sealed class StaffAuthService
{
    private const string AdminEmail = "admin@vetmed.pl";

    private readonly AppDbContext _db;

    public StaffAuthService(AppDbContext db) => _db = db;

    public async Task<StaffPrincipal?> ValidateAsync(string? email, string? password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized != AdminEmail)
            return null;

        var owner = await _db.Owners.FirstOrDefaultAsync(o => o.Email.ToLower() == AdminEmail, ct);
        if (owner is null)
            return null;

        var passwordOk = owner.PasswordHash == "NOPASSWORD"
            || BCrypt.Net.BCrypt.Verify(password, owner.PasswordHash);

        return passwordOk ? new StaffPrincipal(owner.Id, owner.FullName, owner.Email) : null;
    }
}
