using System;
using System.Threading.Tasks;
using MassTransit;
using Nest;
using SharedEvents;

namespace SearchService.Consumers;

public class AccountCreatedSearchConsumer : IConsumer<AccountCreatedEvent>
{
    private readonly IElasticClient _elasticClient;

    public AccountCreatedSearchConsumer(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task Consume(ConsumeContext<AccountCreatedEvent> context)
    {
        var message = context.Message;
        
        var document = new AccountSearchDoc
        {
            Id = message.AccountId.ToString(),
            CustomerId = message.CustomerId.ToString(),
            AccountNumber = message.AccountNumber,
            AccountType = message.AccountType,
            Balance = message.InitialBalance,
            CreatedAt = message.CreatedAt
        };

        await _elasticClient.IndexDocumentAsync(document);
    }
}

public class AccountSearchDoc
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}
