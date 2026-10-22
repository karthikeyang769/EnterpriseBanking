using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TransactionService.Domain.Repositories;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries;

public record GetTransactionsQuery(string AccountNumber) : IRequest<IEnumerable<TransactionDto>>;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, IEnumerable<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<IEnumerable<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByAccountNumberAsync(request.AccountNumber);
        return transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            SourceAccountNumber = t.SourceAccountNumber,
            TargetAccountNumber = t.TargetAccountNumber,
            Amount = t.Amount,
            TransactionType = t.TransactionType,
            ReferenceNumber = t.ReferenceNumber,
            Description = t.Description,
            Status = t.Status,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}
