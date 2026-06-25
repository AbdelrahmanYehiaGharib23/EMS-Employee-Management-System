using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities.IdentityModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Services.Identity
{
    public class PasswordResetManager:IPasswordResetManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public PasswordResetManager(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // إرسال OTP
        public async Task SendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return;

            var otp = GenerateOtp();
            var otpHash = Hash(otp);

            await _unitOfWork.PasswordResetRepository.InvalidateActiveTokensAsync(user.Id);

            var tokenEntity = new PasswordResetTokens
            {
                UserId = user.Id,
                TokenHash = otpHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _unitOfWork.PasswordResetRepository.AddTokenAsync(tokenEntity);
            await _unitOfWork.CompleteAsync();

            await _emailSender.SendEmailAsync(
                email,
                "OTP Code",
                $"<p>Your OTP is <strong>{otp}</strong>. It expires in 10 minutes.</p>");
        }

       
        public async Task<string> VerifyOtpAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("Invalid OTP");

            var otpHash = Hash(otp);
            var tokenEntity = await _unitOfWork.PasswordResetRepository.GetValidOtpTokenAsync(user.Id, otpHash);
            if (tokenEntity == null) throw new Exception("Invalid or expired OTP");

          
            tokenEntity.IsUsed = true;
            await _unitOfWork.PasswordResetRepository.UpdateTokenAsync(tokenEntity);
            await _unitOfWork.CompleteAsync();

            
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetTokenHash = Hash(resetToken);

            await _unitOfWork.PasswordResetRepository.AddTokenAsync(new PasswordResetTokens
            {
                UserId = user.Id,
                TokenHash = resetTokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            });
            await _unitOfWork.CompleteAsync();

            return resetToken;
        }

        //Change Password
        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var tokenHash = Hash(token);
            var tokenEntity = await _unitOfWork.PasswordResetRepository.GetValidResetTokenAsync(tokenHash);
            if (tokenEntity == null || tokenEntity.IsUsed) throw new Exception("Invalid or expired token");

            var user = await _userManager.FindByIdAsync(tokenEntity.UserId);
            if (user == null) throw new Exception("Invalid token");

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded) throw new Exception("Failed to reset password");

            tokenEntity.IsUsed = true;
            await _unitOfWork.PasswordResetRepository.UpdateTokenAsync(tokenEntity);
            await _unitOfWork.CompleteAsync();
        }

        private static string GenerateOtp()
        {
            return RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString("D6");
        }

        private static string Hash(string input)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input)));
        }
    }
}
