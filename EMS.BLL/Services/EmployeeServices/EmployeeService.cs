using AutoMapper;
using EMS.BLL.Models.Dto.EmployeeDto;
using EMS.BLL.Services.AttachementService;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities.EmployeeEntity;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMS.DAL.Entities.AttendanceEntity;
using EMS.DAL.Entities.Shared.Enums;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace EMS.BLL.Services.EmployeeServices
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachementService _attachementService;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IUnitOfWork unitOfWork,IMapper mapper,IAttachementService attachementService, ILogger<EmployeeService> logger)
        {
            _UnitOfWork = unitOfWork;
            _mapper = mapper;
            _attachementService = attachementService;
            _logger = logger;
        }

        public async System.Threading.Tasks.Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeId)
        {
            var now = DateTime.Now;
            var since30 = DateOnly.FromDateTime(now.AddDays(-30));

            var attendances = (await _UnitOfWork.AttendanceRepository.FindAsync(a => a.EmployeeId == employeeId && a.Date >= since30)).ToList();

            var dto = new EmployeeDashboardDto();

            // total worked minutes in last 30 days
            int totalMinutes = 0;
            int lateCount = 0;
            int daysWithRecord = attendances.Select(a => a.Date).Distinct().Count();

            foreach (var a in attendances)
            {
                if (a.CheckIn.HasValue && a.CheckOut.HasValue)
                {
                    var minutes = (int)Math.Max(0, (a.CheckOut.Value - a.CheckIn.Value).TotalMinutes);
                    totalMinutes += minutes;
                }

                if (a.Status == AttendanceStatus.Late)
                    lateCount++;
            }

            dto.TotalHoursLast30Days = Math.Round(totalMinutes / 60m, 2);

            // productivity: ratio of attended days to expected (30 days) weighted by punctuality
            var expectedDays = 30;
            var attendanceRatio = expectedDays > 0 ? (double)daysWithRecord / expectedDays : 0;
            var punctualityPenalty = Math.Min(1.0, (double)lateCount / Math.Max(1, daysWithRecord));
            var score = (int)Math.Round((attendanceRatio * 80 + (1 - punctualityPenalty) * 20) * 100 / 100);
            dto.ProductivityScore = Math.Clamp(score, 0, 100);

            // daily hours for last 7 days
            var last7 = Enumerable.Range(0, 7).Select(i => DateOnly.FromDateTime(now.Date.AddDays(-i))).ToList();
            foreach (var d in last7)
            {
                var rec = attendances.FirstOrDefault(a => a.Date == d);
                decimal hours = 0;
                if (rec != null && rec.CheckIn.HasValue && rec.CheckOut.HasValue)
                {
                    hours = Math.Round((decimal)((rec.CheckOut.Value - rec.CheckIn.Value).TotalMinutes / 60.0), 2);
                }
                dto.DailyHours.Add(new DailyHourDto { Date = d.ToDateTime(TimeOnly.MinValue), Hours = hours });
            }

            // Behavior flags (English)
            if (lateCount >= 3) dto.BehaviorFlags.Add("Frequently Late");
            var absentDays = expectedDays - daysWithRecord;
            if (absentDays >= 5) dto.BehaviorFlags.Add("High Absence");
            if (dto.TotalHoursLast30Days > expectedDays * 9) dto.BehaviorFlags.Add("High Overtime");

            // Recommendations (English)
            if (dto.BehaviorFlags.Contains("Frequently Late"))
            {
                dto.Recommendations.Add("Try to arrive 5 minutes before the scheduled start time to reduce lateness.");
            }
            if (dto.BehaviorFlags.Contains("High Absence"))
            {
                dto.Recommendations.Add("Review your leave balance and contact your manager to clarify absences.");
            }
            if (dto.BehaviorFlags.Contains("High Overtime"))
            {
                dto.Recommendations.Add("Review task distribution or request support to reduce overtime hours.");
            }

            if (!dto.Recommendations.Any())
                dto.Recommendations.Add("Good performance — keep it up! Try to maintain punctual attendance.");

            return dto;
        }
        public async System.Threading.Tasks.Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _UnitOfWork.EmployeeRepository.GetByIdAsync(id);
            if (employee == null)
                return null;
            return _mapper.Map<Employee, EmployeeDetailsDto>(employee);
        }

        public async System.Threading.Tasks.Task<EmployeeDetailsDto?> GetEmployeeByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalized = email.Trim().ToLowerInvariant();

            var employee = (await _UnitOfWork.EmployeeRepository.FindAsync(e => e.Email != null && e.Email.Trim().ToLower() == normalized)).FirstOrDefault();

            if (employee == null)
                return null;

            return _mapper.Map<Employee, EmployeeDetailsDto>(employee);
        }
        public async System.Threading.Tasks.Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(string?EmployeeSearchByName)
        {
            IEnumerable<Employee> employees;
            if (!string.IsNullOrEmpty(EmployeeSearchByName))
            {
                var normalizedSearch = EmployeeSearchByName.Trim().ToLowerInvariant();
                employees = await _UnitOfWork.EmployeeRepository.GetAllAsync(E => E.Name != null && E.Name.ToLower().Contains(normalizedSearch));
            }
            else
            {
                employees = await _UnitOfWork.EmployeeRepository.GetAllAsync();
            }
            var employeeDto = _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeDto>>(employees);
            return employeeDto;
        }
        public async System.Threading.Tasks.Task<int> CreateEmployeeAsync(CreateEmployeeDto employee)
        {
            var employeeToCreate = _mapper.Map<CreateEmployeeDto, Employee>(employee);
            if (employee.Image is not null) {
             employeeToCreate.ImageName = await _attachementService.UploadAsync(employee.Image, "Images");
            }
            try
            {
                _logger.LogDebug("EmployeeService.CreateEmployeeAsync: adding employee entity for Email={Email}", employeeToCreate.Email);
                _UnitOfWork.EmployeeRepository.Add(employeeToCreate);
                _logger.LogDebug("EmployeeService.CreateEmployeeAsync: calling CompleteAsync to save employee");
                await _UnitOfWork.CompleteAsync();
                _logger.LogInformation("EmployeeService.CreateEmployeeAsync: saved employee, assigned Id={Id}", employeeToCreate.Id);
                return employeeToCreate?.Id ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmployeeService.CreateEmployeeAsync: failed to create employee for Email={Email}", employee?.Email);
                throw; // rethrow so controller can handle/log
            }

        }
        public async System.Threading.Tasks.Task<int> UpdateEmployeeAsync(UpdateEmployeeDto employee)
        {
            var Newemployee = await _UnitOfWork.EmployeeRepository.GetByIdAsync(employee.Id);

            if (Newemployee == null)
                return 0;

            _mapper.Map(employee, Newemployee); // update same entity
            if (employee.Image is not null)
            {
                if (!string.IsNullOrEmpty(Newemployee.ImageName))
                {
                    await _attachementService.DeleteAsync(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", "Images", Newemployee.ImageName));
                }
                Newemployee.ImageName = await _attachementService.UploadAsync(employee.Image, "Images");
            }
            _UnitOfWork.EmployeeRepository.Update(Newemployee);
            return await _UnitOfWork.CompleteAsync();
        }


        public async System.Threading.Tasks.Task<bool> DeleteEmployeeAsync(int? id)
        {
            if (id == null) return false;
            var employee = await _UnitOfWork.EmployeeRepository.GetByIdAsync(id.Value);
            if (employee == null) return false;
            _UnitOfWork.EmployeeRepository.Remove(employee);
            return await _UnitOfWork.CompleteAsync() > 0;

        }

      
    }
}
