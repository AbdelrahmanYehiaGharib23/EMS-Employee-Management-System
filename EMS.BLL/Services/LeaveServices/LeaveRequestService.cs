using EMS.BLL.Models.Dto.LeaveDto;
using EMS.BLL.Services.Identity;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities.LeaveEntity;
using EMS.DAL.Entities.Shared.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMS.BLL.Services.LeaveServices
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly EMS.BLL.Services.Notification.INotificationService? _notificationService;

        public LeaveRequestService(IUnitOfWork unitOfWork, IEmailSender emailSender, IConfiguration configuration, EMS.BLL.Services.Notification.INotificationService? notificationService = null)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        public async Task<int> CreateLeaveRequestAsync(CreateLeaveRequestDto dto)
        {
            var entity = new LeaveRequest
            {
                EmployeeId = dto.EmployeeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = LeaveStatus.Pending,
                RequestedOn = DateTime.UtcNow
            };

            _unitOfWork.LeaveRequestRepository.Add(entity);
            var result = await _unitOfWork.CompleteAsync();

            try
            {
                var hrEmail = _configuration["MailSettings:HrEmail"] ?? _configuration["MailSettings:Email"];
                if (!string.IsNullOrEmpty(hrEmail))
                {
                    var subject = $"New leave request from employee #{dto.EmployeeId}";
                    var body = $"Employee #{dto.EmployeeId} requested leave from {dto.StartDate:d} to {dto.EndDate:d}. Reason: {dto.Reason}";
                    await _emailSender.SendEmailAsync(hrEmail, subject, body);
                }
            }
            catch { /* swallow email errors to not break flow */ }

            // send realtime notification to HR role (if service registered)
            try
            {
                if (_notificationService != null)
                {
                    await _notificationService.NotifyRoleAsync("HR", "New leave request", $"Employee #{dto.EmployeeId} requested leave from {dto.StartDate:d} to {dto.EndDate:d}", new { EmployeeId = dto.EmployeeId, StartDate = dto.StartDate, EndDate = dto.EndDate });
                }
            }
            catch { }

            return result;
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetRequestsForEmployeeAsync(int employeeId)
        {
            var list = (await _unitOfWork.LeaveRequestRepository.FindAsync(l => l.EmployeeId == employeeId))
                .OrderByDescending(l => l.RequestedOn)
                .ToList();

            return await System.Threading.Tasks.Task.FromResult(list.Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status,
                RequestedOn = l.RequestedOn,
                ApprovedBy = l.ApprovedBy,
                ApprovedOn = l.ApprovedOn,
                EmployeeName = l.Employee?.Name
            }));
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync()
        {
            var list = (await _unitOfWork.LeaveRequestRepository.FindAsync(l => l.Status == LeaveStatus.Pending))
                .OrderBy(l => l.RequestedOn)
                .ToList();

            return await System.Threading.Tasks.Task.FromResult(list.Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status,
                RequestedOn = l.RequestedOn,
                ApprovedBy = l.ApprovedBy,
                ApprovedOn = l.ApprovedOn,
                EmployeeName = l.Employee?.Name
            }));
        }

        public async Task<bool> ApproveRequestAsync(int id, string approver)
        {
            var entity = await _unitOfWork.LeaveRequestRepository.GetByIdAsync(id);
            if (entity == null || entity.Status != LeaveStatus.Pending)
                return false;

            entity.Status = LeaveStatus.Approved;
            entity.ApprovedBy = approver;
            entity.ApprovedOn = DateTime.UtcNow;

            _unitOfWork.LeaveRequestRepository.Update(entity);
            var result = await _unitOfWork.CompleteAsync() > 0;

            // notify employee
            try
            {
                var email = entity.Employee?.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    var subject = "Leave request approved";
                    var body = $"Your leave request from {entity.StartDate:d} to {entity.EndDate:d} has been approved.";
                    await _emailSender.SendEmailAsync(email, subject, body);
                }
            }
            catch { }

            try
            {
                if (_notificationService != null)
                {
                    await _notificationService.NotifyUserAsync(entity.Employee?.Email ?? string.Empty, "Leave approved", $"Your leave request from {entity.StartDate:d} to {entity.EndDate:d} was approved by {approver}.", new { Id = entity.Id });
                }
            }
            catch { }

            return result;
        }

        public async Task<bool> RejectRequestAsync(int id, string approver)
        {
            var entity = await _unitOfWork.LeaveRequestRepository.GetByIdAsync(id);
            if (entity == null || entity.Status != LeaveStatus.Pending)
                return false;

            entity.Status = LeaveStatus.Rejected;
            entity.ApprovedBy = approver;
            entity.ApprovedOn = DateTime.UtcNow;

            _unitOfWork.LeaveRequestRepository.Update(entity);
            var result = await _unitOfWork.CompleteAsync() > 0;

            // notify employee
            try
            {
                var email = entity.Employee?.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    var subject = "Leave request rejected";
                    var body = $"Your leave request from {entity.StartDate:d} to {entity.EndDate:d} has been rejected.";
                    await _emailSender.SendEmailAsync(email, subject, body);
                }
            }
            catch { }

            try
            {
                if (_notificationService != null)
                {
                    await _notificationService.NotifyUserAsync(entity.Employee?.Email ?? string.Empty, "Leave rejected", $"Your leave request from {entity.StartDate:d} to {entity.EndDate:d} was rejected by {approver}.", new { Id = entity.Id });
                }
            }
            catch { }

            return result;
        }
    }
}
