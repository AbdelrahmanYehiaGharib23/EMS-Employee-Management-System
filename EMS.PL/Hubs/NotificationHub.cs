using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using EMS.DAL.Entities.IdentityModel;
using System.Collections.Concurrent;

// Lightweight runtime registry of SignalR group membership so server-side code
// can detect whether a role group currently has any connected clients.

namespace EMS.PL.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NotificationHub> _logger;

        // group -> set of connection ids
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> s_groupMembers = new();
        // connection id -> set of groups
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> s_connectionGroups = new();

        public NotificationHub(UserManager<ApplicationUser> userManager, ILogger<NotificationHub> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user != null)
            {
                // first try the claims-based role check
                var addedToHr = false;
                try
                {
                    if (user.IsInRole("HR") || user.IsInRole("Admin"))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "HR");
                        _logger.LogInformation("Connection {ConnectionId} added to HR group via claims.", Context.ConnectionId);
                        // Register connection in the runtime registry so server-side can detect group count
                        RegisterConnectionToGroup(Context.ConnectionId, "HR");
                        addedToHr = true;
                    }
                }
                catch
                {
                    // ignore claim-based role resolution failures
                }

                // Resolve ApplicationUser via UserManager when possible and add to stable groups
                try
                {
                    var emailClaim = user.FindFirst(ClaimTypes.Email)?.Value;
                    ApplicationUser? appUser = null;

                    if (!string.IsNullOrEmpty(emailClaim))
                    {
                        appUser = await _userManager.FindByEmailAsync(emailClaim);
                    }

                    if (appUser == null && !string.IsNullOrEmpty(user.Identity?.Name))
                    {
                        appUser = await _userManager.FindByNameAsync(user.Identity.Name);
                    }

                    if (appUser != null)
                    {
                        // add personal groups for email/username and user id so server-side sends can target either
                        var personalIdentifier = !string.IsNullOrEmpty(appUser.Email) ? appUser.Email : appUser.UserName;
                        if (!string.IsNullOrEmpty(personalIdentifier))
                        {
                            var grp = GetUserGroup(personalIdentifier);
                            await Groups.AddToGroupAsync(Context.ConnectionId, grp);
                            RegisterConnectionToGroup(Context.ConnectionId, grp);
                        }

                        if (!string.IsNullOrEmpty(appUser.Id))
                        {
                            var grpId = GetUserGroup(appUser.Id);
                            await Groups.AddToGroupAsync(Context.ConnectionId, grpId);
                            RegisterConnectionToGroup(Context.ConnectionId, grpId);
                        }

                        try
                        {
                            var roles = await _userManager.GetRolesAsync(appUser);
                            if (!addedToHr && roles != null && (roles.Contains("HR") || roles.Contains("Admin")))
                            {
                                await Groups.AddToGroupAsync(Context.ConnectionId, "HR");
                                RegisterConnectionToGroup(Context.ConnectionId, "HR");
                            }
                        }
                        catch
                        {
                            // ignore role lookup errors
                        }
                    }
                    else
                    {
                        // fallback: use available claim/name as personal identifier
                        var identifier = !string.IsNullOrEmpty(emailClaim) ? emailClaim : user.Identity?.Name;
                        if (!string.IsNullOrEmpty(identifier))
                        {
                            var grp = GetUserGroup(identifier);
                            await Groups.AddToGroupAsync(Context.ConnectionId, grp);
                            RegisterConnectionToGroup(Context.ConnectionId, grp);
                        }
                    }
                }
                catch
                {
                    // ignore UserManager related errors
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // remove connection from all tracked groups
            if (s_connectionGroups.TryRemove(Context.ConnectionId, out var groups))
            {
                foreach (var kv in groups)
                {
                    var group = kv.Key;
                    if (s_groupMembers.TryGetValue(group, out var members))
                    {
                        members.TryRemove(Context.ConnectionId, out _);
                        if (members.IsEmpty)
                            s_groupMembers.TryRemove(group, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        private static void RegisterConnectionToGroup(string connectionId, string group)
        {
            var members = s_groupMembers.GetOrAdd(group, _ => new ConcurrentDictionary<string, bool>());
            members[connectionId] = true;

            var groups = s_connectionGroups.GetOrAdd(connectionId, _ => new ConcurrentDictionary<string, bool>());
            groups[group] = true;
        }

        // normalize the group name to avoid characters that break grouping (use lower-case and replace @ and .)
        public static string GetUserGroup(string userIdOrEmail)
        {
            if (string.IsNullOrEmpty(userIdOrEmail)) return "user_unknown";
            var norm = userIdOrEmail.Trim().ToLowerInvariant().Replace("@", "_").Replace(".", "_");
            return $"user_{norm}";
        }

        // returns number of currently connected clients in the named group
        public static int GetGroupCount(string group)
        {
            if (string.IsNullOrEmpty(group)) return 0;
            if (s_groupMembers.TryGetValue(group, out var members)) return members.Count;
            return 0;
        }
    }
}
