using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using VetMed.Api.Extensions;
using VetMed.Api.Middleware;
using VetMed.Infrastructure;
using VetMed.Infrastructure.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .WriteTo.Console());

    var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddInfrastructure(connStr);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddOpenApiWithJwt();
    builder.Services.AddControllers();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db, BCrypt.Net.BCrypt.HashPassword);
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
    app.Run($"http://0.0.0.0:{port}");
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
