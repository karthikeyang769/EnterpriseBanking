using System;
using System.Threading.Tasks;
using MassTransit;
using SharedEvents;
using NotificationService.Domain.Entities;
using NotificationService.Persistence;

namespace NotificationService.Consumers;

public class CustomerRegisteredNotificationConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly NotificationDbContext _context;

    public CustomerRegisteredNotificationConsumer(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<CustomerRegisteredEvent> context)
    {
        var message = context.Message;
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            CustomerId = message.CustomerId,
            Target = message.Email,
            NotificationType = "Email",
            Title = "Welcome to Enterprise Smart Banking!",
            Message = $"Hello {message.FirstName} {message.LastName},\n\nYour account has been successfully registered. Thank you for choosing us!",
            SentAt = DateTime.UtcNow
        };

        await _context.NotificationLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}

public class MoneyTransferredNotificationConsumer : IConsumer<MoneyTransferredEvent>
{
    private readonly NotificationDbContext _context;

    public MoneyTransferredNotificationConsumer(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<MoneyTransferredEvent> context)
    {
        var message = context.Message;
        
        var logSender = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Target = message.SourceAccountNumber,
            NotificationType = "SMS",
            Title = "Account Debited Alert",
            Message = $"Your account {message.SourceAccountNumber} has been debited by {message.Amount:C} for transfer to {message.TargetAccountNumber}. Ref: {message.ReferenceNumber}",
            SentAt = DateTime.UtcNow
        };

        var logReceiver = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Target = message.TargetAccountNumber,
            NotificationType = "SMS",
            Title = "Account Credited Alert",
            Message = $"Your account {message.TargetAccountNumber} has been credited by {message.Amount:C} from {message.SourceAccountNumber}. Ref: {message.ReferenceNumber}",
            SentAt = DateTime.UtcNow
        };

        await _context.NotificationLogs.AddRangeAsync(logSender, logReceiver);
        await _context.SaveChangesAsync();
    }
}

public class DepositCompletedNotificationConsumer : IConsumer<DepositCompletedEvent>
{
    private readonly NotificationDbContext _context;

    public DepositCompletedNotificationConsumer(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<DepositCompletedEvent> context)
    {
        var message = context.Message;
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Target = message.AccountNumber,
            NotificationType = "SMS",
            Title = "Deposit Credited Alert",
            Message = $"Your account {message.AccountNumber} has been credited by {message.Amount:C} via Cash Deposit. Ref: {message.ReferenceNumber}",
            SentAt = DateTime.UtcNow
        };

        await _context.NotificationLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
