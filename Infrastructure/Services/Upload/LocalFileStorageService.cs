using Application.Interfaces.Services.Shipments.Core;
using Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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
}
