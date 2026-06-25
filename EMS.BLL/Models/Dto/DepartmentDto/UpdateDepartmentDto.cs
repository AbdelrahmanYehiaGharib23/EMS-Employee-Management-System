using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Models.Dto.DepartmentDto
{
    public record UpdateDepartmentDto
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public string? Code { get; init; }
        public string? Description { get; init; }
        public DateOnly? DateOfCreation { get; init; }
    }


}
