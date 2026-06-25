using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.DepartmentDto
{
    public record CreateDepartmentDto([Required(ErrorMessage ="Name Is Required !!!")]string? Name,[Required] string? Code, string? Description, DateOnly? DateOfCreation);
}
