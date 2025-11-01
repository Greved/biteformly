namespace BiteForm.Domain.Entities;

public class FormSubmission
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public string? SubmittedBy { get; set; }

    public Form? Form { get; set; }
    public List<FormResponse> Responses { get; set; } = new();
}

