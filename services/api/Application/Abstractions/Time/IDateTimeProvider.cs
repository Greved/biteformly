namespace BiteForm.Application.Abstractions.Time;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

