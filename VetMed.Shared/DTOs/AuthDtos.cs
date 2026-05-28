namespace VetMed.Shared.DTOs;

public record RegisterDto(string FullName, string Email, string Password);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(string Token, DateTime ExpiresAt, OwnerDto Owner);

public record OwnerDto(int Id, string FullName, string Email, string? Phone, int PetCount);
