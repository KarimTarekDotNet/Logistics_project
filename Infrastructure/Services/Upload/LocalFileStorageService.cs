using Application.Interfaces.Services.Shipments.Core;
using Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using nClam;

namespace Infrastructure.Services.Shipments.Core
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new BusinessRuleException("File is required.");

            if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
                throw new BusinessRuleException("Web root path is not configured.");

            var folderPath = Path.Combine(_environment.WebRootPath, folder); // wwwroot/shipments/....(example)

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(file.FileName); // .pdf, .jpg

            var originalName = Path.GetFileNameWithoutExtension(file.FileName); // invoice.pdf -> invoice

            var sanitizedFileName = originalName
                .Replace(" ", "-")
                .Replace("/", "")
                .Replace("\\", "");

            var fileName = $"{Guid.NewGuid()}_{sanitizedFileName}{extension}";

            var fullPath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);

            await file.CopyToAsync(stream);

            return Path.Combine(folder, fileName).Replace("\\", "/");
        }

        public async Task DeleteAsync(string path)
        {
            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new BusinessRuleException("Web root path is not configured.");

            var fullPath = Path.Combine(webRootPath, path);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
    public class ClamAvFileScanner : IFileSecurityService
    {
        private readonly ClamClient _client;
        private readonly IConfiguration _configuration;

        public ClamAvFileScanner(IConfiguration configuration)
        {
            _configuration = configuration;

            var host = _configuration.GetValue<string>("ClamAV:Host")!;
            var port = _configuration.GetValue<int>("ClamAV:Port");

            _client = new ClamClient(host, port);
        }

        public async Task ScanAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BusinessRuleException("File is required.");

            await using var stream = file.OpenReadStream();

            var result = await _client.SendAndScanFileAsync(stream);

            if (result.Result == ClamScanResults.Clean)
                return;

            if (result.Result == ClamScanResults.VirusDetected)
                throw new BusinessRuleException("Uploaded file contains a virus.");

            throw new BusinessRuleException($"File scan failed: {result.RawResult}");
        }

        public async Task ValidateAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BusinessRuleException("File is required.");

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new BusinessRuleException("File extension is not allowed.");

            var maxSizeInBytes = 5 * 1024 * 1024;

            if (file.Length > maxSizeInBytes)
                throw new BusinessRuleException("File size exceeds the allowed limit.");
        }
    }
}
