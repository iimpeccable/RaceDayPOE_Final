using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RacingDays.API.Controllers
{
    // Uploads controller for Azure Blob Storage
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UploadsController(IConfiguration config)
        {
            _config = config;
        }

        // Upload banner file to Azure Blob container
        [HttpPost("banner")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UploadBanner(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided.");

            string connectionString = _config.GetConnectionString("AzureBlobStorage") ?? "UseDevelopmentStorage=true";
            string containerName = "racingdays-images";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return Ok(new { url = blobClient.Uri.ToString() });
        }
    }
}