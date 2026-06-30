FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["VetMed.Clinic/VetMed.Clinic.csproj", "VetMed.Clinic/"]
COPY ["VetMed.Infrastructure/VetMed.Infrastructure.csproj", "VetMed.Infrastructure/"]
COPY ["VetMed.Shared/VetMed.Shared.csproj", "VetMed.Shared/"]
RUN dotnet restore "VetMed.Clinic/VetMed.Clinic.csproj"
COPY . .
WORKDIR "/src/VetMed.Clinic"
RUN dotnet publish "VetMed.Clinic.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VetMed.Clinic.dll"]
