using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMS.DAL.Entities.AttendanceEntity;

namespace EMS.DAL.Contracts.Repositories
{
    public interface IAttendanceRepository : IGenericRepository<Attendance>
    {
    }
}
