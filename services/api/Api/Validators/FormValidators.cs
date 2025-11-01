using BiteForm.Api.Endpoints;
using FluentValidation;

namespace BiteForm.Api.Validators;

public sealed class CreateFormRequestValidator : AbstractValidator<FormsEndpoints.CreateFormRequest>
{
    public CreateFormRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class UpdateFormRequestValidator : AbstractValidator<FormsEndpoints.UpdateFormRequest>
{
    public UpdateFormRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public sealed class CreateFieldRequestValidator : AbstractValidator<FormsEndpoints.CreateFieldRequest>
{
    public CreateFieldRequestValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateFieldRequestValidator : AbstractValidator<FormsEndpoints.UpdateFieldRequest>
{
    public UpdateFieldRequestValidator()
    {
        When(x => x.Key is not null, () => RuleFor(x => x.Key!).NotEmpty().MaximumLength(128));
        When(x => x.Label is not null, () => RuleFor(x => x.Label!).NotEmpty().MaximumLength(256));
        When(x => x.Type is not null, () => RuleFor(x => x.Type!).NotEmpty().MaximumLength(64));
        When(x => x.Order.HasValue, () => RuleFor(x => x.Order!.Value).GreaterThanOrEqualTo(0));
    }
}

