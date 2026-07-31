using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, long fileLength, string folderName);
        void DeleteFile(string relativePath);
        bool IsAllowedExtension(string fileName);
    }
}
