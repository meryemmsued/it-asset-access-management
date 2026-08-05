using ITAssetAccessManagement.Application.DTOs.AccessRequests;
using ITAssetAccessManagement.Application.DTOs.Common;

namespace ITAssetAccessManagement.Application.Interfaces;

public interface IAccessRequestService
{
    Task<PagedResult<AccessRequestSummaryResponse>>
        GetVisibleRequestsAsync(
            Guid currentUserId,
            bool isAdmin,
            int page,
            int pageSize);
            
    Task<PagedResult<AccessRequestSummaryResponse>>
        GetByUserAsync(
            Guid userId,
            int page,
            int pageSize);

    Task<AccessRequestResponse?> GetByIdAsync(
        Guid id);

    Task<bool> CanViewRequestAsync(
        Guid requestId,
        Guid currentUserId,
        bool isAdmin);

    Task<AccessRequestResponse> CreateAsync(
        CreateAccessRequestRequest request,
        Guid requestedByUserId);

    Task<bool> ApproveAsync(
        Guid id,
        ApproveAccessRequestRequest request,
        Guid approverUserId);

    Task<bool> RejectAsync(
        Guid id,
        RejectAccessRequestRequest request,
        Guid approverUserId);

    Task<bool> CancelAsync(
        Guid id,
        Guid requestedByUserId);
}