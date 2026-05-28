using FluentValidation;
using VetMed.Shared.DTOs;
using VetMed.Shared.Enums;

namespace VetMed.Api.Validators;

public sealed class CreatePetValidator : AbstractValidator<CreatePetDto>
{
    public CreatePetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Species).IsInEnum();
        RuleFor(x => x.WeightKg)
            .GreaterThan(0)
            .When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Born)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Born date cannot be in the future.");
        RuleFor(x => x.Breed).MaximumLength(100).When(x => x.Breed is not null);
    }
}
