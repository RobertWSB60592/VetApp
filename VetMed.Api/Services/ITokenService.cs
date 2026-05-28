using VetMed.Shared.Models;

namespace VetMed.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(Owner owner);
}
