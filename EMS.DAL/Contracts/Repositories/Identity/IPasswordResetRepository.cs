using EMS.DAL.Entities.IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Contracts.Repositories.Identity
{
    public interface IPasswordResetRepository
    {
        Task AddTokenAsync(PasswordResetTokens token);
        Task InvalidateActiveTokensAsync(string userId);
        Task<PasswordResetTokens?> GetValidOtpTokenAsync(string userId, string otpHash);
        Task<PasswordResetTokens?> GetValidResetTokenAsync(string tokenHash);
        Task UpdateTokenAsync(PasswordResetTokens token);
    }
}
