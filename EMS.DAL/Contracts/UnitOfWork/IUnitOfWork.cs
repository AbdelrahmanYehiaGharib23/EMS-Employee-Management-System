using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Contracts.Repositories.Identity;
using EMS.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Contracts.UnitOfWork
{
    public interface IUnitOfWork
    {
        public IDepartmentRepository DepartmentRepository { get; }
        public IEmployeeRepository EmployeeRepository { get; }
        public IAttendanceRepository AttendanceRepository { get; }
        public ILeaveRequestRepository LeaveRequestRepository { get; }
        IPasswordResetRepository PasswordResetRepository { get; }
        ICompanyLocationRepository CompanyLocationRepository { get; }
        public IEmployeeProfileRequestRepository EmployeeProfileRequestRepository { get; }
        public IAuditLogRepository AuditLogRepository { get; }
        int Complete();
        System.Threading.Tasks.Task<int> CompleteAsync();
        //void Dispose();
    }
}
