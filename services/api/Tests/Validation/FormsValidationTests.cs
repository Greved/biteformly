using BiteForm.Api.Endpoints;
using BiteForm.Api.Validators;
using NUnit.Framework;

namespace BiteForm.Tests.Validation;

[TestFixture]
public class FormsValidationTests
{
    [Test]
    public void CreateForm_Invalid_ReturnsErrors()
    {
        var v = new CreateFormRequestValidator();
        var req = new FormsEndpoints.CreateFormRequest("", "", new string('x', 2100));
        var result = v.Validate(req);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(req.TenantId)));
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(req.Name)));
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(req.Description)));
    }

    [Test]
    public void CreateForm_Valid_Ok()
    {
        var v = new CreateFormRequestValidator();
        var req = new FormsEndpoints.CreateFormRequest("tenant_1", "My Form", "desc");
        var result = v.Validate(req);
        Assert.That(result.IsValid, Is.True, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Test]
    public void CreateField_Invalid_ReturnsErrors()
    {
        var v = new CreateFieldRequestValidator();
        var req = new FormsEndpoints.CreateFieldRequest("", "", "", true, -1);
        var result = v.Validate(req);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(req.Key)));
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(req.Label)));
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(req.Type)));
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(req.Order)));
    }

    [Test]
    public void UpdateField_WithInvalidLengths_ReturnsErrors()
    {
        var v = new UpdateFieldRequestValidator();
        var longKey = new string('k', 200);
        var longLabel = new string('l', 300);
        var longType = new string('t', 200);
        var req = new FormsEndpoints.UpdateFieldRequest(longKey, longLabel, longType, null, -5);
        var result = v.Validate(req);
        Assert.That(result.IsValid, Is.False);
    }
}

