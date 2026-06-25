using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Contracts.Repositories.Identity;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Interfaces;
using EMS.DAL.Persistence.Data.DbInitializer;
using EMS.DAL.Persistence.Repositories;
using EMS.DAL.Persistence.Repositories.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly Lazy<IDepartmentRepository> _DepartmentRepository;
        private readonly Lazy<IEmployeeRepository> _EmployeeRepository;
        private readonly Lazy<IAttendanceRepository> _AttendanceRepository;
        private readonly Lazy<ILeaveRequestRepository> _LeaveRequestRepository;
        private readonly Lazy<IPasswordResetRepository> _PasswordResetRepository;
        private readonly Lazy<ICompanyLocationRepository> _companyLocationRepository;
        private readonly Lazy<IEmployeeProfileRequestRepository> _employeeProfileRequestRepository;
        private readonly Lazy<IAuditLogRepository> _AuditLogRepository;
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext; 
            _DepartmentRepository = new Lazy<IDepartmentRepository>(()=>new DepartmentRepository(dbContext));
            _EmployeeRepository = new Lazy<IEmployeeRepository>(()=>new EmployeeRepository(dbContext));
            _AttendanceRepository = new Lazy<IAttendanceRepository>(()=> new AttendanceRepository(dbContext));
            _LeaveRequestRepository = new Lazy<ILeaveRequestRepository>(()=> new LeaveRequestRepository(dbContext));
            _PasswordResetRepository = new Lazy<IPasswordResetRepository>(() => new PasswordResetRepository(dbContext));
            _companyLocationRepository =new Lazy<ICompanyLocationRepository>(() =>
                                        new CompanyLocationRepository(dbContext));
            _employeeProfileRequestRepository = new Lazy<IEmployeeProfileRequestRepository>(() => new EmployeeProfileRequestRepository(dbContext));
            _AuditLogRepository = new Lazy<IAuditLogRepository>(() => new AuditLogRepository(dbContext));
        }
        public IDepartmentRepository DepartmentRepository => _DepartmentRepository.Value;
        public IEmployeeRepository EmployeeRepository => _EmployeeRepository.Value;
        public IAttendanceRepository AttendanceRepository => _AttendanceRepository.Value;
        public ILeaveRequestRepository LeaveRequestRepository => _LeaveRequestRepository.Value;
        public IPasswordResetRepository PasswordResetRepository => _PasswordResetRepository.Value;
        public ICompanyLocationRepository CompanyLocationRepository => _companyLocationRepository.Value;
        public IEmployeeProfileRequestRepository EmployeeProfileRequestRepository => _employeeProfileRequestRepository.Value;
        public IAuditLogRepository AuditLogRepository => _AuditLogRepository.Value;

        public int Complete()
        {
            return _dbContext.SaveChanges();
        }

        public async System.Threading.Tasks.Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }


        // No need to call Dispose manually here.
        // DbContext is managed by Dependency Injection and registered via AddDbContext.
        // It will be disposed automatically at the end of the request lifecycle.

        //public void Dispose()
        //{
        //    _dbContext.Dispose();
        //}
    }
}
