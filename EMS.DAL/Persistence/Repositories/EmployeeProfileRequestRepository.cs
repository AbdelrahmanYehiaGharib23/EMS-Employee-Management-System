using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Entities.Requests;
using EMS.DAL.Persistence.Data.DbInitializer;

namespace EMS.DAL.Persistence.Repositories
{
    public class EmployeeProfileRequestRepository : GenericRepository<EmployeeProfileRequest>, IEmployeeProfileRequestRepository
    {
        public EmployeeProfileRequestRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
