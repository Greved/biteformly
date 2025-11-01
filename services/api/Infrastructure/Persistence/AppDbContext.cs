using BiteForm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BiteForm.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormField> Fields => Set<FormField>();
    public DbSet<FormSubmission> Submissions => Set<FormSubmission>();
    public DbSet<FormResponse> Responses => Set<FormResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

