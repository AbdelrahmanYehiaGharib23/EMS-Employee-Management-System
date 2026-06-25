using EMS.BLL.Models.Dto.DepartmentDto;


namespace EMS.BLL.Services.DepartmentServices
{
    public interface IDepartmentService
    {
        System.Threading.Tasks.Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync();
        System.Threading.Tasks.Task<DepartmentDetailsDto?> GetDepartmentByIdAsync(int id);
        System.Threading.Tasks.Task<int> CreateDepartmentAsync(CreateDepartmentDto department);
        System.Threading.Tasks.Task<int> UpdateDepartmentAsync(UpdateDepartmentDto department);
        System.Threading.Tasks.Task<bool> DeleteDepartmentAsync(int? id);
    }
}
