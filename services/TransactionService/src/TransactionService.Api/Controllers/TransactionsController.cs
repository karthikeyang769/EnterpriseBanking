using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SharedContracts.Responses;
using TransactionService.Application.Queries;
using TransactionService.Application.Commands;
using TransactionService.Application.DTOs;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<ApiResponse<string>>> Transfer([FromBody] TransferMoneyCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<string>.SuccessResult(result, "Money transferred successfully."));
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<ApiResponse<string>>> Deposit([FromBody] DepositCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<string>.SuccessResult(result, "Deposit completed successfully."));
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<ApiResponse<string>>> Withdraw([FromBody] WithdrawCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<string>.SuccessResult(result, "Withdrawal completed successfully."));
    }

    [HttpGet("history/{accountNumber}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetHistory(string accountNumber)
    {
        var result = await _mediator.Send(new GetTransactionsQuery(accountNumber));
        return Ok(ApiResponse<IEnumerable<TransactionDto>>.SuccessResult(result, "Transaction history retrieved successfully."));
    }
}
