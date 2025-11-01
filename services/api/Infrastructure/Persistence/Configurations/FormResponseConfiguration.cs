using BiteForm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiteForm.Infrastructure.Persistence.Configurations;

public class FormResponseConfiguration : IEntityTypeConfiguration<FormResponse>
{
    public void Configure(EntityTypeBuilder<FormResponse> builder)
    {
        builder.ToTable("form_responses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Value).HasMaxLength(4000);

        builder.HasOne(x => x.Field)
               .WithMany()
               .HasForeignKey(x => x.FieldId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

