using EMS.BLL.Models.Dto.DepartmentDto;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities;
using EMS.DAL.Entities.DepartmentEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.BLL.Services.DepartmentServices
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _UnitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public async System.Threading.Tasks.Task<DepartmentDetailsDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _UnitOfWork.DepartmentRepository.GetByIdAsync(id);
            if (department is null)
                return null;

            var departmentDetailsDto = new DepartmentDetailsDto()
            {
                Id = department.Id,
                CreatedBy = department.CreatedBy,
                CreatedOn = department.CreatedOn,
                DateOfCreation = department.DateOfCreation,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                LastModifiedBy = department.LastModifiedBy,
                LastModifiedOn = department.LastModifiedOn,
            };
            return departmentDetailsDto;
        }
        public async System.Threading.Tasks.Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync()
        {
            var departments = await _UnitOfWork.DepartmentRepository.GetAllAsync();
            return departments.Select(department => new DepartmentDto(department.Id, department.Code, department.Name, department.Description, department.DateOfCreation)).ToList();
        }
        public async System.Threading.Tasks.Task<int> CreateDepartmentAsync(CreateDepartmentDto department)
        {
            var departmentToCreate = new Department
            {
                Name = department.Name ?? string.Empty,
                Code = department.Code ?? string.Empty,
                Description = department.Description,
                DateOfCreation = department.DateOfCreation
            };
            _UnitOfWork.DepartmentRepository.Add(departmentToCreate);
            return await _UnitOfWork.CompleteAsync();

        }
        public async System.Threading.Tasks.Task<int> UpdateDepartmentAsync(UpdateDepartmentDto department)
        {
            var departmentToUpdate = new Department
            {
                Id = department.Id,
                Name = department.Name ?? string.Empty,
                Code = department.Code ?? string.Empty,
                Description = department.Description,
                DateOfCreation = department.DateOfCreation
            };
            _UnitOfWork.DepartmentRepository.Update(departmentToUpdate);
            return await _UnitOfWork.CompleteAsync();
        }

        public async System.Threading.Tasks.Task<bool> DeleteDepartmentAsync(int? id)
        {
            if (id == null) return false;

            var department = await _UnitOfWork.DepartmentRepository.GetByIdAsync(id.Value);
            if (department == null)
                return false;

            _UnitOfWork.DepartmentRepository.Remove(department);
            return await _UnitOfWork.CompleteAsync() > 0;
        }





    }
}
