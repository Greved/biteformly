using BiteForm.Application.Abstractions.Time;

namespace BiteForm.Infrastructure;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

