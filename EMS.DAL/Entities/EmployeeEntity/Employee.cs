using EMS.DAL.Entities.DepartmentEntity;
using EMS.DAL.Entities.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Entities.EmployeeEntity
{
    public class Employee:BaseEntity
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public string? Address { get; set; }
        public decimal Salary { get; set; }
        public bool IsMarred { get; set; }
        public DateTime HiringTime { get; set; }
        public Gender Gender { get; set; }
        public EmployeeType EmployeeType { get; set; }
        public int? DepartmentId { get; set; }

        // Navigational prop [ONE] 
        public virtual Department? Department { get; set; }
        public string? ImageName { get; set; }
    }
}
