using EMS.BLL.Models.Dto.LeaveDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.BLL.Services.LeaveServices
{
    public interface ILeaveRequestService
    {
        Task<int> CreateLeaveRequestAsync(CreateLeaveRequestDto dto);
        Task<IEnumerable<LeaveRequestDto>> GetRequestsForEmployeeAsync(int employeeId);
        Task<IEnumerable<LeaveRequestDto>> GetPendingRequestsAsync();
        Task<bool> ApproveRequestAsync(int id, string approver);
        Task<bool> RejectRequestAsync(int id, string approver);
    }
}
