using BiteForm.Domain.Entities;
using BiteForm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BiteForm.Tests.Persistence;

[TestFixture]
public class AppDbContextTests
{
    [Test]
    public void Can_build_model_and_insert_entities_in_memory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase("biteform_tests"));

        using var sp = services.BuildServiceProvider();
        using var db = sp.GetRequiredService<AppDbContext>();

        var formId = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var responseId = Guid.NewGuid();

        var form = new Form
        {
            Id = formId,
            TenantId = "tenant_123",
            Name = "Contact",
            CreatedAtUtc = DateTime.UtcNow
        };

        var field = new FormField
        {
            Id = fieldId,
            FormId = formId,
            Key = "email",
            Label = "Email",
            Type = "text",
            Required = true,
            Order = 1
        };

        var submission = new FormSubmission
        {
            Id = submissionId,
            FormId = formId,
            TenantId = "tenant_123",
            SubmittedAtUtc = DateTime.UtcNow,
            SubmittedBy = "user@example.com"
        };

        var response = new FormResponse
        {
            Id = responseId,
            SubmissionId = submissionId,
            FieldId = fieldId,
            Value = "user@example.com"
        };

        db.Add(form);
        db.Add(field);
        db.Add(submission);
        db.Add(response);
        db.SaveChanges();

        Assert.That(db.Forms.Count(), Is.EqualTo(1));
        Assert.That(db.Fields.Count(), Is.EqualTo(1));
        Assert.That(db.Submissions.Count(), Is.EqualTo(1));
        Assert.That(db.Responses.Count(), Is.EqualTo(1));
    }
}

