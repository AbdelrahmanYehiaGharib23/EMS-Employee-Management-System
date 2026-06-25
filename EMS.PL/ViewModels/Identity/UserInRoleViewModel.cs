namespace EMS.PL.ViewModels.Identity
{
    public class UserInRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsInRole { get; set; }
    }
}
