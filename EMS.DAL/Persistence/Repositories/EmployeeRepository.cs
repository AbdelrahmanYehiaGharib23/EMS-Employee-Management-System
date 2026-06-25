using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Entities;
using EMS.DAL.Persistence.Data.DbInitializer;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace EMS.DAL.Persistence.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public Employee? GetByIdWithDepartment(int id)
        {
            return _dbContext.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<Employee> GetAll()
        {
            return _dbContext.Employees
                .Include(e => e.Department)
                .AsNoTracking()
                .ToList();
        }
    }
}
