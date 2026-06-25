using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EMS.PL.ViewModels.Identity
{
    public class ProfileRequestViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public decimal? Salary { get; set; }
        public bool IsMarred { get; set; }
        public DateOnly? HiringTime { get; set; }
        // Bind date input as string to avoid model binding issues across frameworks.
        public string? HiringTimeString { get; set; }
        public string? Gender { get; set; }
        public string? EmployeeType { get; set; }
        public int? DepartmentId { get; set; }
        public IFormFile? Image { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }
    }
}
