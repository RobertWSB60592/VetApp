using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetMed.Infrastructure.Repositories;
using VetMed.Shared.DTOs;

namespace VetMed.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/doctors")]
public sealed class DoctorsController : ControllerBase
{
    private readonly IDoctorRepository _doctors;

    public DoctorsController(IDoctorRepository doctors) => _doctors = doctors;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var doctors = await _doctors.GetAllAsync(ct);
        var dtos = doctors.Select(d => new DoctorDto(d.Id, d.FullName, d.Specialization, d.IsAvailable)).ToList();
        return Ok(dtos);
    }
}
