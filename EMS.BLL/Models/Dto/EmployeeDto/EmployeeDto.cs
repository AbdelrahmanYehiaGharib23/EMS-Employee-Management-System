using EMS.DAL.Entities.EmployeeEntity;
using EMS.DAL.Entities.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.EmployeeDto
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "MinLength is 2")]
        [Display(Name = "Name")]
        public string? Name { get; set; }
        [Display(Name = "Email")]
        public string? Email { get; set; }
        [Display(Name = "PhoneNumber")]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Age")]
        public int? Age { get; set; }
        [Display(Name = "Address")]
        public string? Address { get; set; }
        [Display(Name = "Salary")]
        public decimal Salary { get; set; }
        public bool IsMarred { get; set; }
        public DateOnly HiringTime { get; set; }
        public string? Gender { get; set; }
        [Display(Name = "Type")]
        public string? EmployeeType { get; set; }
        [Display(Name = "Department")]
        public string? DepartmentName { get; set; }

        public string? ImageName { get; set; }
    }
}
