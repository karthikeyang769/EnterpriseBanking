using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Responses;
using NotificationService.Domain.Entities;
using NotificationService.Persistence;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _context;

    public NotificationsController(NotificationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationLog>>>> GetLogs()
    {
        var logs = await _context.NotificationLogs
            .AsNoTracking()
            .OrderByDescending(l => l.SentAt)
            .ToListAsync();
            
        return Ok(ApiResponse<IEnumerable<NotificationLog>>.SuccessResult(logs, "Notification logs retrieved successfully."));
    }
}
