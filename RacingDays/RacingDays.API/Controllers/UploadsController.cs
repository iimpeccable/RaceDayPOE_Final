using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RacingDays.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UploadsController(IConfiguration config)
        {
            _config = config;
        }

        // POST: api/uploads/banner
        [HttpPost("banner")]
        public async Task<IActionResult> UploadBanner(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided.");

            // Retrieves connection string from appsettings.json or defaults to Azurite local emulator
            string connectionString = _config.GetConnectionString("AzureBlobStorage") ?? "UseDevelopmentStorage=true";
            string containerName = "raceday-images";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container exists with public blob read access
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