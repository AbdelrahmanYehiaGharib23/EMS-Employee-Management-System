using EMS.BLL.Models.Dto.Dashboard_Dto;
using EMS.BLL.Models.Dto.DashboardDto.DashboardAnalyticsDto;
using EMS.BLL.Models.Dto.DashboardDto;
using EMS.DAL.Entities.AttendanceEntity;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text;

namespace EMS.BLL.Services.DashboardServices
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public DashboardService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async System.Threading.Tasks.Task<List<MostPresentEmployeeDto>> GetMostPresentEmployeesAsync(int top = 5, int days = 30)
        {
            var since = DateOnly.FromDateTime(DateTime.Now.AddDays(-days + 1));

            var attendances = (await _unitOfWork.AttendanceRepository.GetAllAsync())
                .Where(a => a.Date >= since)
                .ToList();

            var results = attendances
                .GroupBy(a => a.EmployeeId)
                .Select(g => new MostPresentEmployeeDto
                {
                    EmployeeId = g.Key,
                    Name = (_unitOfWork.EmployeeRepository.GetById(g.Key))?.Name,
                    PresentDays = g.Count(a => a.Status == AttendanceStatus.Present),
                    LateCount = g.Count(a => a.Status == AttendanceStatus.Late),
                    TotalOvertimeMinutes = g.Sum(a => a.OvertimeMinutes)
                })
                .OrderByDescending(x => x.PresentDays)
                .ThenByDescending(x => x.TotalOvertimeMinutes)
                .Take(top)
                .ToList();

            return results;
        }

        public async System.Threading.Tasks.Task<bool> CheckInByEmailAsync(string userEmail, double latitude, double longitude, DateTime? timestamp = null)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                return false;

            var normalized = userEmail.Trim().ToLowerInvariant();
            var employee = (await _unitOfWork.EmployeeRepository.FindAsync(e => e.Email != null && e.Email.Trim().ToLower() == normalized)).FirstOrDefault();

            if (employee == null)
                return false;

            var checkInTime = timestamp ?? DateTime.Now;
            var date = DateOnly.FromDateTime(checkInTime);

            var existing = (await _unitOfWork.AttendanceRepository.GetAllAsync())
                .FirstOrDefault(a => a.EmployeeId == employee.Id && a.Date == date);

            if (existing != null && existing.CheckIn.HasValue)
                return false;

            // Get company location for geolocation validation
            var companyLocation = (await _unitOfWork.CompanyLocationRepository.GetAllAsync()).FirstOrDefault();
            bool insideCompany = false;

            if (companyLocation != null)
            {
                var distance = CalculateDistance(latitude, longitude, companyLocation.Latitude, companyLocation.Longitude);
                insideCompany = distance <= companyLocation.AllowedRadiusMeters;
            }

            if (existing == null)
            {
                existing = new Attendance
                {
                    EmployeeId = employee.Id,
                    Date = date,
                    CheckIn = checkInTime,
                    CheckOut = null,
                    OvertimeMinutes = 0,
                    CreatedOn = DateTime.UtcNow,
                    Status = AttendanceStatus.Present,
                    Latitude = latitude,
                    Longitude = longitude,
                    IsInsideCompany = insideCompany
                };

                if (checkInTime.TimeOfDay > new TimeSpan(8, 35, 0))
                    existing.Status = AttendanceStatus.Late;

                _unitOfWork.AttendanceRepository.Add(existing);
            }
            else
            {
                existing.CheckIn = checkInTime;
                existing.Latitude = latitude;
                existing.Longitude = longitude;
                existing.IsInsideCompany = insideCompany;

                if (checkInTime.TimeOfDay > new TimeSpan(8, 35, 0))
                    existing.Status = AttendanceStatus.Late;
                else
                    existing.Status = AttendanceStatus.Present;

                _unitOfWork.AttendanceRepository.Update(existing);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async System.Threading.Tasks.Task<bool> CheckOutByEmailAsync(string userEmail, DateTime? timestamp = null)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                return false;

            var normalized = userEmail.Trim().ToLowerInvariant();
            var employee = (await _unitOfWork.EmployeeRepository.FindAsync(e => e.Email != null && e.Email.Trim().ToLower() == normalized)).FirstOrDefault();

            if (employee == null)
                return false;

            var checkOutTime = timestamp ?? DateTime.Now;
            var date = DateOnly.FromDateTime(checkOutTime);

            var existing = (await _unitOfWork.AttendanceRepository.GetAllAsync())
                .FirstOrDefault(a => a.EmployeeId == employee.Id && a.Date == date);

            if (existing == null || existing.CheckOut.HasValue)
                return false;

            existing.CheckOut = checkOutTime;

            if (existing.CheckIn.HasValue)
            {
                var minutes = (int)Math.Max(0, (existing.CheckOut.Value - existing.CheckIn.Value).TotalMinutes);
                var overtime = Math.Max(0, minutes - (8 * 60));
                existing.OvertimeMinutes = overtime;
            }

            _unitOfWork.AttendanceRepository.Update(existing);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async System.Threading.Tasks.Task<DashboardStatsDto> GetStatsAsync()
        {
            return new DashboardStatsDto
            {
                TotalEmployees = await _unitOfWork.EmployeeRepository.CountAsync(),
                TotalDepartments = await _unitOfWork.DepartmentRepository.CountAsync(),
                NewEmployeesLast30Days = (await _unitOfWork.EmployeeRepository.FindAsync(e => e.CreatedOn >= DateTime.UtcNow.AddDays(-30))).Count()
            };
        }

        public async System.Threading.Tasks.Task<DashboardAnalyticsDto> GetAnalyticsAsync()
        {
            var analytics = await _cache.GetOrCreateAsync("dashboard_analytics", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var employees = (await _unitOfWork.EmployeeRepository.GetAllAsync()).ToList();
                var analytics = new DashboardAnalyticsDto();

                analytics.DepartmentChart = employees
                    .GroupBy(e => e.Department?.Name ?? "Unassigned")
                    .Select(g => new DepartmentChartDto { Department = g.Key!, Count = g.Count() })
                    .ToList();

                analytics.GenderDistribution = new GenderDistributionDto
                {
                    Male = employees.Count(e => e.Gender == Gender.male),
                    Female = employees.Count(e => e.Gender == Gender.female)
                };

                var salaries = employees.Select(e => e.Salary).DefaultIfEmpty(0m);

                analytics.SalaryAnalytics = new SalaryAnalyticsDto
                {
                    Average = salaries.Average(),
                    Max = salaries.Max(),
                    Min = salaries.Min()
                };

                return analytics;
            });

            return analytics ?? new DashboardAnalyticsDto();
        }

        public async System.Threading.Tasks.Task<List<HiringTrendDto>> GetHiringTrendAsync()
        {
            return (await _unitOfWork.EmployeeRepository.GetAllAsync())
                .GroupBy(e => e.HiringTime.Month)
                .Select(g => new HiringTrendDto
                {
                    MonthNumber = g.Key,
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    Count = g.Count()
                })
                .OrderBy(x => x.MonthNumber)
                .ToList();
        }

        public async System.Threading.Tasks.Task<byte[]> GenerateAnalyticsReportCsvAsync()
        {
            var analytics = await GetAnalyticsAsync();
            var hiring = await GetHiringTrendAsync();

            var sb = new StringBuilder();

            sb.AppendLine("Department,Count");
            foreach (var d in analytics.DepartmentChart)
                sb.AppendLine($"\"{d.Department}\",{d.Count}");

            sb.AppendLine();
            sb.AppendLine("Gender,Count");
            sb.AppendLine($"Male,{analytics.GenderDistribution.Male}");
            sb.AppendLine($"Female,{analytics.GenderDistribution.Female}");

            sb.AppendLine();
            sb.AppendLine("Metric,Value");
            sb.AppendLine($"Average,{analytics.SalaryAnalytics.Average.ToString(CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Max,{analytics.SalaryAnalytics.Max.ToString(CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Min,{analytics.SalaryAnalytics.Min.ToString(CultureInfo.InvariantCulture)}");

            sb.AppendLine();
            sb.AppendLine("Month,Count");
            foreach (var h in hiring)
                sb.AppendLine($"{h.Month},{h.Count}");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }
    }
}
