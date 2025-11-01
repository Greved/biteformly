using BiteForm.Api.Endpoints;
using BiteForm.Api.Validators;
using NUnit.Framework;

namespace BiteForm.Tests.Validation;

[TestFixture]
public class SubmissionsValidationTests
{
    [Test]
    public void CreateSubmission_InvalidItem_ReturnsErrors()
    {
        var v = new CreateSubmissionRequestValidator();
        var longEmail = new string('e', 300);
        var req = new SubmissionsEndpoints.CreateSubmissionRequest(
            longEmail,
            new List<SubmissionsEndpoints.CreateResponseItem>
            {
                new(Guid.Empty, new string('v', 4100))
            }
        );
        var result = v.Validate(req);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName.Contains("SubmittedBy")));
        Assert.That(result.Errors.Any(e => e.PropertyName.Contains("Responses[0].FieldId")));
        Assert.That(result.Errors.Any(e => e.PropertyName.Contains("Responses[0].Value")));
    }

    [Test]
    public void CreateSubmission_Valid_Ok()
    {
        var v = new CreateSubmissionRequestValidator();
        var req = new SubmissionsEndpoints.CreateSubmissionRequest(
            "user@example.com",
            new List<SubmissionsEndpoints.CreateResponseItem>
            {
                new(Guid.NewGuid(), "hello")
            }
        );
        var result = v.Validate(req);
        Assert.That(result.IsValid, Is.True, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Test]
    public void Response_Update_InvalidValue_ReturnsError()
    {
        var v = new UpdateResponseRequestValidator();
        var req = new SubmissionsEndpoints.UpdateResponseRequest(new string('x', 5000));
        var result = v.Validate(req);
        Assert.That(result.IsValid, Is.False);
    }
}

