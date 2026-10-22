using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using SharedContracts.Responses;
using DocumentService.Domain.Entities;
using DocumentService.Persistence;

namespace DocumentService.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentDbContext _context;
    private readonly IMinioClient _minioClient;

    public DocumentsController(DocumentDbContext context, IMinioClient minioClient)
    {
        _context = context;
        _minioClient = minioClient;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<DocumentLog>>> UploadDocument(
        [FromForm] Guid customerId,
        [FromForm] string documentType,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<DocumentLog>.FailureResult("File", "No file uploaded.", "MISSING_FILE"));
        }

        var bucketName = "kyc-documents";
        var extension = Path.GetExtension(file.FileName);
        var objectName = $"{customerId}/{Guid.NewGuid()}{extension}";

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

            // Upload file stream to MinIO
            using (var stream = file.OpenReadStream())
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(file.Length)
                    .WithContentType(file.ContentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
            }

            // Record log in Db
            var docLog = new DocumentLog
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                DocumentType = documentType,
                FileName = file.FileName,
                ObjectName = objectName,
                UploadedAt = DateTime.UtcNow
            };

            await _context.DocumentLogs.AddAsync(docLog);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<DocumentLog>.SuccessResult(docLog, "KYC document uploaded and logged successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DocumentLog>.FailureResult("Upload", ex.Message, "MINIO_UPLOAD_FAILED"));
        }
    }
}
