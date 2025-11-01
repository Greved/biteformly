# BiteForm API (services/api)

## Overview
Clean-ish .NET 9 Web API with `Domain/`, `Application/`, `Infrastructure/`, `Api/`. AI providers are vendor-agnostic and selected at runtime via configuration.

## Run

- Dev server: `dotnet run --project services/api/Api`
- Swagger (dev): `http://localhost:5000/swagger` (or shown in console)
- Health: `GET /health`
- Version: `GET /version`

## Configuration

- Base file: `services/api/Api/appsettings.json`
- Dev overrides: `services/api/Api/appsettings.Development.json`
- Env vars override JSON keys using `__` (e.g., `AI__Provider=Gemini`).

### Sections

- `Supabase`: `Url`, `ProjectId`, `JwtSecret`
- `PostHog`: `ApiKey`, `Host`
- `AI`:
  - `Provider`: `NoOp` | `OpenAI` | `Gemini`
  - `OpenAI`: `ApiKey`, `BaseUrl`, `Model`
  - `Gemini`: `ApiKey`, `Endpoint`, `Model`

Example (PowerShell):

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:AI__Provider = "Gemini"
$env:AI__Gemini__ApiKey = "<your-key>"
dotnet run --project services/api/Api
```

## Tests

- Run: `dotnet test services/api/Tests`
- Framework: NUnit + coverlet collector

