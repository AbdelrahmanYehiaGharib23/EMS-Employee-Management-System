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
        public System.Threading.Tasks.Task<string?> UploadAsync(Microsoft.AspNetCore.Http.IFormFile file, string folderName);
        public System.Threading.Tasks.Task<bool> DeleteAsync(string filePath);
    }
}
