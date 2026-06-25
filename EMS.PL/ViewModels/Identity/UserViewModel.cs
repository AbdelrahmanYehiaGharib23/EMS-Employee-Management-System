namespace EMS.PL.ViewModels.Identity
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string>? Roles { get; set; }

    }
}
