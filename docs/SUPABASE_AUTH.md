Supabase Auth (API)

Configuration keys (Api/appsettings.json):
- `Supabase.Url`
- `Supabase.ProjectId`
- `Supabase.JwtSecret` (required for API JWT validation)

The API validates Supabase JWTs with HS256 using `Supabase:JwtSecret`.

Test endpoint:
- `GET /api/v1/me` with header `Authorization: Bearer <access_token>`

Local setup (PowerShell):

```
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=biteform;Username=postgres;Password=postgres"
$env:Supabase__JwtSecret = "<your-supabase-jwt-secret>"
dotnet run --project services/api/Api
```

