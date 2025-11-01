using BiteForm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiteForm.Infrastructure.Persistence.Configurations;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("form_submissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.SubmittedAtUtc).IsRequired();
        builder.Property(x => x.SubmittedBy).HasMaxLength(256);

        builder.HasMany(x => x.Responses)
               .WithOne(x => x.Submission!)
               .HasForeignKey(x => x.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

