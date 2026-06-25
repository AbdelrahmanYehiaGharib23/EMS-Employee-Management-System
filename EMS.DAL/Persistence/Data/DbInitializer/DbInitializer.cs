using EMS.DAL.Contracts;
using EMS.DAL.Entities.DepartmentEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMS.DAL.Persistence.Data.DbInitializer
{
    public class DbInitializer : IDbInitalizer
    {
        private readonly ApplicationDbContext _dbContext;

        public DbInitializer(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void Initilaize()
        {
            if (_dbContext.Database.GetPendingMigrations().Any())
            {
                _dbContext.Database.Migrate();
            }
            
        }

        public void Seed()
        {
            if (!_dbContext.Departments.Any())
            {
                var departmentsData=File.ReadAllText("../EMS.DAL/Persistence/Data/Seeds/departments.json");
                var departments=JsonSerializer.Deserialize<List<Department>>(departmentsData);
                if (departments?.Count() > 0) {
                    _dbContext.Departments.AddRange(departments);
                    _dbContext.SaveChanges();
                }
            }
        }
    }
}
