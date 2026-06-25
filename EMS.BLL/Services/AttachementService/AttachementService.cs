using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Services.AttachementService
{
    public class AttachementService : IAttachementService
    {
        private static readonly List<string> allowedExtensions = new List<string> { ".png", ".jpg", ".jpeg" };
        private const int maxSize = 2_097_152; //2*1024*1024

        public async System.Threading.Tasks.Task<string?> UploadAsync(Microsoft.AspNetCore.Http.IFormFile file, string folderName)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension)) return null;
            if (file.Length == 0 || file.Length > maxSize) return null;
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", folderName);
            Directory.CreateDirectory(folderPath);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await file.CopyToAsync(fs);
            }
            return fileName;
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;
                // Use Task.Run to perform file delete without blocking caller thread for rare cases
                await System.Threading.Tasks.Task.Run(() => File.Delete(filePath));
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
