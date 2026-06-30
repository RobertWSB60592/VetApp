using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetMed.Api.Services;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly IHealthService _health;

    public HealthController(IHealthService health) => _health = health;

    private int OwnerId => int.Parse(User.FindFirstValue("sub")!);

    [HttpGet("vaccinations/pet/{petId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<VaccinationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVaccinationsByPet(int petId, CancellationToken ct)
    {
        var items = await _health.GetVaccinationsByPetAsync(petId, OwnerId, ct);
        return Ok(items);
    }

    [HttpGet("prescriptions/pet/{petId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<PrescriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrescriptionsByPet(int petId, CancellationToken ct)
    {
        var items = await _health.GetPrescriptionsByPetAsync(petId, OwnerId, ct);
        return Ok(items);
    }

    [HttpGet("vaccinations/upcoming")]
    [ProducesResponseType(typeof(IReadOnlyList<VaccinationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingVaccinations([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var items = await _health.GetUpcomingVaccinationsAsync(OwnerId, days, ct);
        return Ok(items);
    }
}
