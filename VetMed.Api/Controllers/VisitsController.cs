using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetMed.Api.Services;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/visits")]
public sealed class VisitsController : ControllerBase
{
    private readonly IVisitService _visits;
    private readonly IValidator<CreateVisitDto> _createValidator;

    public VisitsController(IVisitService visits, IValidator<CreateVisitDto> createValidator)
    {
        _visits = visits;
        _createValidator = createValidator;
    }

    private int OwnerId => int.Parse(User.FindFirstValue("sub")!);

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VisitDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var visits = await _visits.GetByOwnerAsync(OwnerId, ct);
        return Ok(visits);
    }

    [HttpGet("pet/{petId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<VisitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPet(int petId, CancellationToken ct)
    {
        var visits = await _visits.GetByPetAsync(petId, OwnerId, ct);
        return Ok(visits);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VisitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateVisitDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem();
        }

        var created = await _visits.CreateAsync(OwnerId, dto, ct);
        if (created is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Pet or doctor not found, or pet does not belong to the authenticated owner.");
        }

        return CreatedAtAction(nameof(GetAll), created);
    }
}
