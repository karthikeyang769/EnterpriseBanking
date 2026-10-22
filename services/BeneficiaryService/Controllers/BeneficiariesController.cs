using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Responses;
using BeneficiaryService.Domain.Entities;
using BeneficiaryService.Persistence;

namespace BeneficiaryService.Controllers;

[ApiController]
[Route("api/beneficiaries")]
public class BeneficiariesController : ControllerBase
{
    private readonly BeneficiaryDbContext _context;

    public BeneficiariesController(BeneficiaryDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Beneficiary>>> AddBeneficiary([FromBody] Beneficiary beneficiary)
    {
        beneficiary.Id = Guid.NewGuid();
        await _context.Beneficiaries.AddAsync(beneficiary);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<Beneficiary>.SuccessResult(beneficiary, "Beneficiary added successfully."));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Beneficiary>>>> GetBeneficiaries(Guid customerId)
    {
        var list = await _context.Beneficiaries
            .Where(b => b.CustomerId == customerId)
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<Beneficiary>>.SuccessResult(list, "Beneficiaries retrieved successfully."));
    }
}
