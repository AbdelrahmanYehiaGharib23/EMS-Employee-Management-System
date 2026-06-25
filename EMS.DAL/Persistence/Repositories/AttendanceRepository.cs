using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Entities.AttendanceEntity;
using EMS.DAL.Persistence.Data.DbInitializer;

namespace EMS.DAL.Persistence.Repositories
{
    public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
