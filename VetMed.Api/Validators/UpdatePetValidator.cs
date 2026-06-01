using FluentValidation;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Validators;

public sealed class UpdatePetValidator : AbstractValidator<UpdatePetDto>
{
    public UpdatePetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Species).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Sex).IsInEnum();
        RuleFor(x => x.WeightKg)
            .GreaterThan(0)
            .When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Born)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Born date cannot be in the future.");
        RuleFor(x => x.Breed).MaximumLength(100).When(x => x.Breed is not null);
        RuleFor(x => x.MicrochipNumber).MaximumLength(40).When(x => x.MicrochipNumber is not null);
        RuleFor(x => x.Color).MaximumLength(60).When(x => x.Color is not null);
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
    }
}
