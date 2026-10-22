using System;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using SharedEvents;
using AuditService.Domain.Entities;
using AuditService.Persistence;

namespace AuditService.Consumers;

public class CustomerRegisteredAuditConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly AuditDbContext _context;

    public CustomerRegisteredAuditConsumer(AuditDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<CustomerRegisteredEvent> context)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventName = nameof(CustomerRegisteredEvent),
            EventData = JsonSerializer.Serialize(context.Message),
            OccurredOn = context.Message.RegisteredAt
        };
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}

public class AccountCreatedAuditConsumer : IConsumer<AccountCreatedEvent>
{
    private readonly AuditDbContext _context;

    public AccountCreatedAuditConsumer(AuditDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<AccountCreatedEvent> context)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventName = nameof(AccountCreatedEvent),
            EventData = JsonSerializer.Serialize(context.Message),
            OccurredOn = context.Message.CreatedAt
        };
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}

public class MoneyTransferredAuditConsumer : IConsumer<MoneyTransferredEvent>
{
    private readonly AuditDbContext _context;

    public MoneyTransferredAuditConsumer(AuditDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<MoneyTransferredEvent> context)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventName = nameof(MoneyTransferredEvent),
            EventData = JsonSerializer.Serialize(context.Message),
            OccurredOn = context.Message.TransferredAt
        };
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
