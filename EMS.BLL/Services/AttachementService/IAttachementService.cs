using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Services.AttachementService
{
    public interface IAttachementService
    {
        public Task<string?> UploadAsync(IFormFile file, string folderName);
        public Task<bool> DeleteAsync(string filePath);
    }
}
