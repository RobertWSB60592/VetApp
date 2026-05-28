using VetMed.Shared.DTOs;

namespace VetMed.App.Services;

public interface IDoctorApiService
{
    Task<List<DoctorDto>> GetDoctorsAsync();
}
