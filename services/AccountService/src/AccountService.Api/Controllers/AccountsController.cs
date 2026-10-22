using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SharedContracts.Responses;
using AccountService.Application.Queries;
using AccountService.Application.Commands;
using AccountService.Application.DTOs;

namespace AccountService.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AccountDto>.SuccessResult(result, "Account created successfully."));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccountById(Guid id)
    {
        var result = await _mediator.Send(new GetAccountByIdQuery(id));
        return Ok(ApiResponse<AccountDto>.SuccessResult(result, "Account details retrieved successfully."));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AccountDto>>>> GetAccountsByCustomerId(Guid customerId)
    {
        var result = await _mediator.Send(new GetAccountsByCustomerIdQuery(customerId));
        return Ok(ApiResponse<IEnumerable<AccountDto>>.SuccessResult(result, "Customer accounts retrieved successfully."));
    }
}
