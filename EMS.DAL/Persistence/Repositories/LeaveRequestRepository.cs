using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Entities.LeaveEntity;
using EMS.DAL.Persistence.Data.DbInitializer;

namespace EMS.DAL.Persistence.Repositories
{
    public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
