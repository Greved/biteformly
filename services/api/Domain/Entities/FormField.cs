namespace BiteForm.Domain.Entities;

public class FormField
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string Key { get; set; } = string.Empty; // machine key
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text"; // e.g., text, number, select, etc.
    public bool Required { get; set; }
    public int Order { get; set; }

    public Form? Form { get; set; }
}

