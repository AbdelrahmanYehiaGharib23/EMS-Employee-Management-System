using EMS.DAL.Entities.DepartmentEntity;
using EMS.DAL.Interfaces;
using EMS.DAL.Persistence.Data.DbInitializer;

namespace EMS.DAL.Persistence.Repositories
{
    public class DepartmentRepository(ApplicationDbContext dbContext):GenericRepository<Department>(dbContext) , IDepartmentRepository
    {
    }
}
