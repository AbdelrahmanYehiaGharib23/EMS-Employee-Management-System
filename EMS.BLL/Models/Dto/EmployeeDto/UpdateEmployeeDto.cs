using EMS.DAL.Entities.EmployeeEntity;
using EMS.DAL.Entities.Shared.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.EmployeeDto
{
    public class UpdateEmployeeDto
    {
        public int Id { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "MinLength is 2")]
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public decimal Salary { get; set; }
        public bool IsMarred { get; set; }
        public DateOnly HiringTime { get; set; }
        public Gender Gender { get; set; }
        public EmployeeType EmployeeType { get; set; }
        public int? DepartmentId { get; set; }
        public IFormFile? Image {  get; set; }
    }
}
