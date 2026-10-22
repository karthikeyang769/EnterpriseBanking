using System;

namespace NotificationService.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Target { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty; // Email, SMS
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
