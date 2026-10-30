using EMS.BLL.Models.Dto.DepartmentDto;


namespace EMS.BLL.Services.DepartmentServices
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync();
        Task<DepartmentDetailsDto?> GetDepartmentByIdAsync(int id);
        Task<int> CreateDepartmentAsync(CreateDepartmentDto department);
        Task<int> UpdateDepartmentAsync(UpdateDepartmentDto department);
        Task<bool> DeleteDepartmentAsync(int? id);
    }
}
