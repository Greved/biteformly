using BiteForm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiteForm.Infrastructure.Persistence.Configurations;

public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("forms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasMany(x => x.Fields)
               .WithOne(x => x.Form!)
               .HasForeignKey(x => x.FormId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Submissions)
               .WithOne(x => x.Form!)
               .HasForeignKey(x => x.FormId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

