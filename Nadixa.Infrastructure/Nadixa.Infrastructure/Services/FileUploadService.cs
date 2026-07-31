using Microsoft.AspNetCore.Hosting;
using Nadixa.Core.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Application.Interfaces;

namespace Nadixa.Infrastructure.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".jfif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        public FileUploadService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public bool IsAllowedExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return _allowedExtensions.Contains(extension);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, long fileLength, string folderName)
        {
            if (fileStream == null || fileLength == 0)
                throw new ArgumentException(AppMessages.InvalidFile);

            if (!IsAllowedExtension(fileName))
                throw new ArgumentException(AppMessages.InvalidImageFormat);

            if (fileLength > MaxFileSizeBytes)
                throw new ArgumentException(AppMessages.FileSizeExceeded);

            var inputFileExtension = Path.GetExtension(fileName);
            var generatedFileName = Guid.NewGuid().ToString() + inputFileExtension;

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var folderPath = Path.Combine(wwwRootPath, "images", folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, generatedFileName);

            try
            {
                await using var outputStream = new FileStream(filePath, FileMode.Create);
                await fileStream.CopyToAsync(outputStream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(AppMessages.ImageUploadError, ex);
            }

            return $"/images/{folderName}/{generatedFileName}";
        }


        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
