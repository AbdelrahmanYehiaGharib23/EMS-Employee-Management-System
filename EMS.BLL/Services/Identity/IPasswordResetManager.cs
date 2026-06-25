using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Services.Identity
{
    public interface IPasswordResetManager
    {
        Task SendOtpAsync(string email);
        Task<string> VerifyOtpAsync(string email, string otp);
        Task ResetPasswordAsync(string token, string newPassword);
    }
}
