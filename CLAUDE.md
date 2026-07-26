# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

This is a freshly scaffolded ASP.NET Core Web API project (`dotnet new webapi` with controllers, no auth, HTTPS enabled). It currently contains only the default template code (`WeatherForecastController`) — no domain models, database, or hospital-management business logic have been added yet. This is not a git repository.

## Commands

Run all commands from the repository root (`HospitalManagement.sln`) or from the `HospitalManagement/` project directory.

```bash
dotnet restore
dotnet build
dotnet run --project HospitalManagement
```

- Dev server runs at `http://localhost:5104` (and `https://localhost:7240` under the `https` launch profile), opening Swagger UI at `/swagger` automatically.
- There is no test project yet; `dotnet test` has nothing to run until one is added.
- `HospitalManagement/HospitalManagement.http` has a sample REST Client request against the running dev server.

## Architecture

- Single project (`HospitalManagement/HospitalManagement.csproj`), target framework `net8.0`, nullable reference types and implicit usings enabled.
- Standard ASP.NET Core Web API pipeline in `Program.cs`: controllers + Swagger/Swashbuckle registered, Swagger UI only enabled in `Development`, `UseHttpsRedirection` → `UseAuthorization` → `MapControllers`.
- Controllers live under `HospitalManagement/Controllers/` using attribute routing (`[Route("[controller]")]`).
- As real functionality is added (patients, appointments, staff, etc.), expect this to grow into the conventional ASP.NET Core layout: `Controllers/`, `Models/`, `Services/`, and eventually a data-access layer (e.g. `Data/` with EF Core) — none of that exists yet, so don't assume it when navigating the codebase.
