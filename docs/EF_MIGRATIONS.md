EF Core Migrations (API)

Projects:
- Migration project: `services/api/Infrastructure`
- Startup project: `services/api/Api`

Design-time factory: `Infrastructure/Persistence/DesignTimeDbContextFactory.cs` loads `Api/appsettings*.json` and env vars.

Commands (run from repo root):

```
# Add a migration
dotnet ef migrations add <Name> -p services/api/Infrastructure -s services/api/Api -o Persistence/Migrations

# Update database
dotnet ef database update -p services/api/Infrastructure -s services/api/Api
```

Set `ConnectionStrings:Default` in `services/api/Api/appsettings.Development.json` or via environment variables before running.

Testing note

- Integration tests run with `UseInMemoryDb=true` in configuration. The Infrastructure DI selects the EF InMemory provider when this flag is set, avoiding mixed Npgsql/InMemory issues. Tests also set an empty `ConnectionStrings:Default` to ensure only the in-memory provider is used.
