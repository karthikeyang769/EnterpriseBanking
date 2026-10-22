using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using SharedContracts.Responses;
using ReportingService.Domain.Entities;
using ReportingService.Persistence;

namespace ReportingService.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportingDbContext _context;
    private readonly IMinioClient _minioClient;

    public ReportsController(ReportingDbContext context, IMinioClient minioClient)
    {
        _context = context;
        _minioClient = minioClient;
    }

    [HttpPost("statement")]
    public async Task<ActionResult<ApiResponse<ReportLog>>> GenerateStatement(
        [FromBody] GenerateStatementRequest request)
    {
        if (string.IsNullOrEmpty(request.AccountNumber))
        {
            return BadRequest(ApiResponse<ReportLog>.FailureResult("AccountNumber", "Account number is required.", "MISSING_ACCOUNT_NUMBER"));
        }

        var bucketName = "statements";
        var fileName = $"statement_{request.AccountNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        try
        {
            // Verify bucket exists, if not create it
            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }

            // Generate statement CSV format
            var sb = new StringBuilder();
            sb.AppendLine("Date,TransactionType,Amount,ReferenceNumber,Description,Status");
            sb.AppendLine($"{DateTime.UtcNow.AddDays(-2):yyyy-MM-dd HH:mm:ss},Deposit,10000.00,TXN000104822,Initial Deposit,Success");
            sb.AppendLine($"{DateTime.UtcNow.AddDays(-1):yyyy-MM-dd HH:mm:ss},Withdrawal,500.00,TXN000109923,ATM Withdrawal,Success");
            sb.AppendLine($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},Transfer,1200.00,TXN000210041,Rent Payment,Success");

            var fileContent = sb.ToString();
            var byteArray = Encoding.UTF8.GetBytes(fileContent);

            using (var stream = new MemoryStream(byteArray))
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(byteArray.Length)
                    .WithContentType("text/csv");

                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            // Record log in Db
            var reportLog = new ReportLog
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                AccountNumber = request.AccountNumber,
                ReportUrl = $"/{bucketName}/{fileName}",
                GeneratedAt = DateTime.UtcNow
            };

            await _context.ReportLogs.AddAsync(reportLog);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<ReportLog>.SuccessResult(reportLog, "Statement report generated and uploaded successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ReportLog>.FailureResult("Statement", ex.Message, "STATEMENT_GENERATION_FAILED"));
        }
    }
}

public class GenerateStatementRequest
{
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
}
