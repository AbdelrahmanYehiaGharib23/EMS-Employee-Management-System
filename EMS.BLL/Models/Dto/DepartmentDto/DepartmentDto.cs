using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.DepartmentDto
{
    public record DepartmentDto(int Id,string Code, string? Name,string? Description,DateOnly? DateOfCreation);
}
