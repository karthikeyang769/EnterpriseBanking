using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SharedContracts.Responses;
using CustomerService.Application.Queries;
using CustomerService.Application.Commands;
using CustomerService.Application.DTOs;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(ApiResponse<CustomerDto>.SuccessResult(result, "Customer profile retrieved successfully."));
    }

    [HttpPut("{id}/address")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAddress(Guid id, [FromBody] UpdateCustomerAddressCommand command)
    {
        if (id != command.CustomerId)
        {
            return BadRequest(ApiResponse<bool>.FailureResult("CustomerId", "Customer ID in URL path must match command body.", "ID_MISMATCH"));
        }

        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Customer address updated successfully."));
    }
}
