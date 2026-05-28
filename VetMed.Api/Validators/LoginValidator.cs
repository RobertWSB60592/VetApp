using FluentValidation;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Validators;

public sealed class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotNull();
    }
}
