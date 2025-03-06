using CarDealership.Storage.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace CarDealership.Storage.Endpoints.Api
{
    [ApiController]
    [Route("api/storage")]
    public class FileStorageController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public FileStorageController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromQuery] string directory)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не выбран");
            }

            using MemoryStream ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            await _fileStorageService.UploadFileAsync(directory, file.FileName, ms, file.ContentType);

            return Ok("Файл успешно загружен");
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string directory, [FromQuery] string fileName)
        {
            var fileStream = await _fileStorageService.DownloadFileAsync(directory, fileName);
            return File(fileStream, "application/octet-stream", fileName);
        }
    }
}
    