using EMS.BLL.Models.Dto.EmployeeDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Services.EmployeeServices
{
    public interface IEmployeeService
    {
        System.Threading.Tasks.Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(string? EmployeeSearchByName);
        System.Threading.Tasks.Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(int id);
        System.Threading.Tasks.Task<EmployeeDetailsDto?> GetEmployeeByEmailAsync(string email);
        // Dashboard and analytics for the employee (personal view)
        System.Threading.Tasks.Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeId);
        System.Threading.Tasks.Task<int> CreateEmployeeAsync(CreateEmployeeDto employee);
        System.Threading.Tasks.Task<int> UpdateEmployeeAsync(UpdateEmployeeDto employee);
        System.Threading.Tasks.Task<bool> DeleteEmployeeAsync(int? id);
    }
}
