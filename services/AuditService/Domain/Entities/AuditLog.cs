using System;

namespace AuditService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
}
