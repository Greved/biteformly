using BiteForm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiteForm.Infrastructure.Persistence.Configurations;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.ToTable("form_fields");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Key).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Label).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Type).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Order).IsRequired();
        builder.HasIndex(x => new { x.FormId, x.Key }).IsUnique();
    }
}

