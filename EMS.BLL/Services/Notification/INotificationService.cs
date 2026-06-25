using System.Threading.Tasks;

namespace EMS.BLL.Services.Notification
{
    public interface INotificationService
    {
        Task NotifyRoleAsync(string role, string title, string message, object? payload = null);
        Task NotifyUserAsync(string userIdOrEmail, string title, string message, object? payload = null);
    }
}
