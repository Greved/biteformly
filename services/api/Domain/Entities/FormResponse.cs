namespace BiteForm.Domain.Entities;

public class FormResponse
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid FieldId { get; set; }
    public string? Value { get; set; }

    public FormSubmission? Submission { get; set; }
    public FormField? Field { get; set; }
}

