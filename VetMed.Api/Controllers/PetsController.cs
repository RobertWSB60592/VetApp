using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetMed.Api.Services;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pets")]
public sealed class PetsController : ControllerBase
{
    private readonly IPetService _pets;
    private readonly IValidator<CreatePetDto> _createValidator;

    public PetsController(IPetService pets, IValidator<CreatePetDto> createValidator)
    {
        _pets = pets;
        _createValidator = createValidator;
    }

    private int OwnerId => int.Parse(User.FindFirstValue("sub")!);

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var pets = await _pets.GetByOwnerAsync(OwnerId, ct);
        return Ok(pets);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var pet = await _pets.GetByIdAsync(id, OwnerId, ct);
        if (pet is null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Pet not found.");
        return Ok(pet);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePetDto dto, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem();
        }

        var created = await _pets.CreateAsync(OwnerId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetDto dto, CancellationToken ct)
    {
        var updated = await _pets.UpdateAsync(id, OwnerId, dto, ct);
        if (updated is null)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Pet not found.");
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _pets.DeleteAsync(id, OwnerId, ct);
        if (!deleted)
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Pet not found.");
        return NoContent();
    }
}
