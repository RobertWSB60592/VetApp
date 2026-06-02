using FluentValidation;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Validators;

public sealed class CreateVisitValidator : AbstractValidator<CreateVisitDto>
{
    public CreateVisitValidator()
    {
        RuleFor(x => x.ScheduledAt)
            .Must(d => d.ToUniversalTime().Date >= DateTime.UtcNow.Date)
            .WithMessage("Scheduled date must be in the future.");
        RuleFor(x => x.PetId).GreaterThan(0);
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
    }
}
