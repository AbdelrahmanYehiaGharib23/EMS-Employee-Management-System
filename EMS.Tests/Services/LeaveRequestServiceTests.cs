using EMS.BLL.Models.Dto.LeaveDto;
using EMS.BLL.Services.Identity;
using EMS.BLL.Services.LeaveServices;
using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Contracts.UnitOfWork;
using EMS.DAL.Entities.LeaveEntity;
using EMS.DAL.Entities.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EMS.Tests.Services;

public class LeaveRequestServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILeaveRequestRepository> _leaveRequests = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IConfiguration> _configuration = new();

    private LeaveRequestService CreateService()
    {
        _unitOfWork.SetupGet(unitOfWork => unitOfWork.LeaveRequestRepository)
            .Returns(_leaveRequests.Object);

        return new LeaveRequestService(
            _unitOfWork.Object,
            _emailSender.Object,
            _configuration.Object);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_AddsPendingRequestAndNotifiesHr()
    {
        var dto = new CreateLeaveRequestDto
        {
            EmployeeId = 42,
            StartDate = new DateTime(2026, 11, 3),
            EndDate = new DateTime(2026, 11, 6),
            Reason = "Family event"
        };
        LeaveRequest? addedRequest = null;
        _leaveRequests
            .Setup(repository => repository.Add(It.IsAny<LeaveRequest>()))
            .Callback<LeaveRequest>(request => addedRequest = request);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(1);
        _configuration.Setup(configuration => configuration["MailSettings:HrEmail"])
            .Returns("hr@example.com");
        _emailSender
            .Setup(sender => sender.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await CreateService().CreateLeaveRequestAsync(dto);

        Assert.Equal(1, result);
        Assert.NotNull(addedRequest);
        Assert.Equal(dto.EmployeeId, addedRequest.EmployeeId);
        Assert.Equal(dto.StartDate, request.StartDate);
        Assert.Equal(dto.EndDate, request.EndDate);
        Assert.Equal(dto.Reason, request.Reason);
        Assert.Equal(LeaveStatus.Pending, request.Status);
        Assert.True(request.RequestedOn <= DateTime.UtcNow);
        _emailSender.Verify(sender => sender.SendEmailAsync(
            "hr@example.com",
            It.Is<string>(subject => subject.Contains("#42", StringComparison.Ordinal)),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ApproveRequestAsync_WhenRequestIsPending_ApprovesAndPersistsIt()
    {
        var request = new LeaveRequest
        {
            Id = 7,
            Status = LeaveStatus.Pending,
            Employee = new() { Email = "employee@example.com" }
        };
        _leaveRequests.Setup(repository => repository.GetByIdAsync(request.Id))
            .ReturnsAsync(request);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(1);
        _emailSender
            .Setup(sender => sender.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await CreateService().ApproveRequestAsync(request.Id, "manager@example.com");

        Assert.True(result);
        Assert.Equal(LeaveStatus.Approved, request.Status);
        Assert.Equal("manager@example.com", request.ApprovedBy);
        Assert.NotNull(request.ApprovedOn);
        _leaveRequests.Verify(repository => repository.Update(request), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
        _emailSender.Verify(sender => sender.SendEmailAsync(
            "employee@example.com",
            "Leave request approved",
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ApproveRequestAsync_WhenRequestDoesNotExist_ReturnsFalseWithoutSaving()
    {
        _leaveRequests.Setup(repository => repository.GetByIdAsync(404))
            .ReturnsAsync((LeaveRequest?)null);

        var result = await CreateService().ApproveRequestAsync(404, "manager@example.com");

        Assert.False(result);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Never);
        _leaveRequests.Verify(repository => repository.Update(It.IsAny<LeaveRequest>()), Times.Never);
    }

    [Fact]
    public async Task RejectRequestAsync_WhenRequestIsPending_RejectsAndPersistsIt()
    {
        var request = new LeaveRequest { Id = 9, Status = LeaveStatus.Pending };
        _leaveRequests.Setup(repository => repository.GetByIdAsync(request.Id))
            .ReturnsAsync(request);
        _unitOfWork.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(1);

        var result = await CreateService().RejectRequestAsync(request.Id, "manager@example.com");

        Assert.True(result);
        Assert.Equal(LeaveStatus.Rejected, request.Status);
        Assert.Equal("manager@example.com", request.ApprovedBy);
        Assert.NotNull(request.ApprovedOn);
        _leaveRequests.Verify(repository => repository.Update(request), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPendingRequestsAsync_ReturnsRequestsInOldestFirstOrder()
    {
        var newer = new LeaveRequest
        {
            Id = 2,
            Status = LeaveStatus.Pending,
            RequestedOn = new DateTime(2026, 2, 2)
        };
        var older = new LeaveRequest
        {
            Id = 1,
            Status = LeaveStatus.Pending,
            RequestedOn = new DateTime(2026, 2, 1)
        };
        _leaveRequests.Setup(repository => repository.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync((IEnumerable<LeaveRequest>)new[] { newer, older });

        var result = (await CreateService().GetPendingRequestsAsync()).ToList();

        Assert.Equal(new[] { older.Id, newer.Id }, result.Select(request => request.Id));
        Assert.All(result, request => Assert.Equal(LeaveStatus.Pending, request.Status));
    }
}
