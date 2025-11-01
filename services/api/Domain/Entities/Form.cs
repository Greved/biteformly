namespace BiteForm.Domain.Entities;

public class Form
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public List<FormField> Fields { get; set; } = new();
    public List<FormSubmission> Submissions { get; set; } = new();
}

