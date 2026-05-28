using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VetMed.Infrastructure.Data;
using VetMed.Infrastructure.Repositories;

namespace VetMed.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connectionString, npgsql =>
                npgsql.EnableRetryOnFailure(maxRetryCount: 5)));

        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();

        return services;
    }
}
