using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Responses;
using AuditService.Domain.Entities;
using AuditService.Persistence;

namespace AuditService.Controllers;

[ApiController]
[Route("api/audits")]
public class AuditsController : ControllerBase
{
    private readonly AuditDbContext _context;

    public AuditsController(AuditDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AuditLog>>>> GetLogs()
    {
        var logs = await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(l => l.OccurredOn)
            .ToListAsync();
            
        return Ok(ApiResponse<IEnumerable<AuditLog>>.SuccessResult(logs, "Audit logs retrieved successfully."));
    }
}
