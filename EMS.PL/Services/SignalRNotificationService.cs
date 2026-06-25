using EMS.BLL.Services.Notification;
using EMS.PL.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace EMS.PL.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext, ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyRoleAsync(string role, string title, string message, object? payload = null)
        {
            try
            {
                try
                {
                    var payloadJson = payload == null ? "null" : System.Text.Json.JsonSerializer.Serialize(payload);
                    _logger.LogInformation("NotifyRoleAsync sending to group {Role}: {Title} Payload={Payload}", role, title, payloadJson);
                }
                catch { _logger.LogInformation("NotifyRoleAsync sending to group {Role}: {Title}", role, title); }

                // Try to extract sender info from payload (if present) to ensure clients always see who sent it
                string? sender = null;
                try
                {
                    if (payload != null)
                    {
                        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
                        using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
                        var root = doc.RootElement;
                        string? name = null;
                        string? email = null;
                        if (root.TryGetProperty("Name", out var pName) && pName.ValueKind == System.Text.Json.JsonValueKind.String) name = pName.GetString();
                        if (string.IsNullOrEmpty(name) && root.TryGetProperty("RequestedByName", out var pReqName) && pReqName.ValueKind == System.Text.Json.JsonValueKind.String) name = pReqName.GetString();
                        if (root.TryGetProperty("Email", out var pEmail) && pEmail.ValueKind == System.Text.Json.JsonValueKind.String) email = pEmail.GetString();
                        if (string.IsNullOrEmpty(email) && root.TryGetProperty("RequestedByEmail", out var pReqEmail) && pReqEmail.ValueKind == System.Text.Json.JsonValueKind.String) email = pReqEmail.GetString();
                        if (root.TryGetProperty("From", out var pFrom) && pFrom.ValueKind == System.Text.Json.JsonValueKind.String && string.IsNullOrEmpty(name)) name = pFrom.GetString();
                        if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email))
                            sender = (name ?? "") + (string.IsNullOrEmpty(email) ? "" : " <" + email + ">");
                    }
                }
                catch { /* ignore payload parsing errors */ }

                // If there are no connected clients in the role group, treat this as a soft-failure so callers
                // can fallback to email or per-user notifications. Use NotificationHub registry to check.
                var groupCount = NotificationHub.GetGroupCount(role);
                if (groupCount == 0)
                {
                    _logger.LogWarning("NotifyRoleAsync: no connected clients for group {Role}", role);
                    throw new InvalidOperationException($"No connected clients for role group '{role}'");
                }

                await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification", new { Title = title, Message = message, Payload = payload, Sender = sender });
                _logger.LogInformation("NotifyRoleAsync completed send to group {Role} (clients={Count})", role, groupCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotifyRoleAsync failed for group {Role}", role);
                throw;
            }
        }

        public async Task NotifyUserAsync(string userIdOrEmail, string title, string message, object? payload = null)
        {
            if (string.IsNullOrEmpty(userIdOrEmail)) return;
            var group = NotificationHub.GetUserGroup(userIdOrEmail);
            try
            {
                try
                {
                    var payloadJson = payload == null ? "null" : System.Text.Json.JsonSerializer.Serialize(payload);
                    _logger.LogInformation("NotifyUserAsync sending to user group {Group}: {Title} Payload={Payload}", group, title, payloadJson);
                }
                catch { _logger.LogInformation("NotifyUserAsync sending to user group {Group}: {Title}", group, title); }

                string? sender = null;
                try
                {
                    if (payload != null)
                    {
                        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
                        using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
                        var root = doc.RootElement;
                        string? name = null;
                        string? email = null;
                        if (root.TryGetProperty("Name", out var pName) && pName.ValueKind == System.Text.Json.JsonValueKind.String) name = pName.GetString();
                        if (string.IsNullOrEmpty(name) && root.TryGetProperty("RequestedByName", out var pReqName) && pReqName.ValueKind == System.Text.Json.JsonValueKind.String) name = pReqName.GetString();
                        if (root.TryGetProperty("Email", out var pEmail) && pEmail.ValueKind == System.Text.Json.JsonValueKind.String) email = pEmail.GetString();
                        if (string.IsNullOrEmpty(email) && root.TryGetProperty("RequestedByEmail", out var pReqEmail) && pReqEmail.ValueKind == System.Text.Json.JsonValueKind.String) email = pReqEmail.GetString();
                        if (root.TryGetProperty("From", out var pFrom) && pFrom.ValueKind == System.Text.Json.JsonValueKind.String && string.IsNullOrEmpty(name)) name = pFrom.GetString();
                        if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email))
                            sender = (name ?? "") + (string.IsNullOrEmpty(email) ? "" : " <" + email + ">");
                    }
                }
                catch { }

                await _hubContext.Clients.Group(group).SendAsync("ReceiveNotification", new { Title = title, Message = message, Payload = payload, Sender = sender });
                _logger.LogInformation("NotifyUserAsync completed send to group {Group}", group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotifyUserAsync failed for group {Group}", group);
                throw;
            }
        }
    }
}
