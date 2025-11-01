using BiteForm.Api.Endpoints;
using FluentValidation;

namespace BiteForm.Api.Validators;

public sealed class CreateSubmissionRequestValidator : AbstractValidator<SubmissionsEndpoints.CreateSubmissionRequest>
{
    public CreateSubmissionRequestValidator()
    {
        RuleFor(x => x.SubmittedBy).MaximumLength(256);
        RuleForEach(x => x.Responses).SetValidator(new CreateResponseItemValidator());
    }
}

public sealed class UpdateSubmissionRequestValidator : AbstractValidator<SubmissionsEndpoints.UpdateSubmissionRequest>
{
    public UpdateSubmissionRequestValidator()
    {
        RuleFor(x => x.SubmittedBy).MaximumLength(256);
    }
}

public sealed class CreateResponseItemValidator : AbstractValidator<SubmissionsEndpoints.CreateResponseItem>
{
    public CreateResponseItemValidator()
    {
        RuleFor(x => x.FieldId).NotEmpty();
        RuleFor(x => x.Value).MaximumLength(4000);
    }
}

public sealed class CreateResponseRequestValidator : AbstractValidator<SubmissionsEndpoints.CreateResponseRequest>
{
    public CreateResponseRequestValidator()
    {
        RuleFor(x => x.FieldId).NotEmpty();
        RuleFor(x => x.Value).MaximumLength(4000);
    }
}

public sealed class UpdateResponseRequestValidator : AbstractValidator<SubmissionsEndpoints.UpdateResponseRequest>
{
    public UpdateResponseRequestValidator()
    {
        RuleFor(x => x.Value).MaximumLength(4000);
    }
}

