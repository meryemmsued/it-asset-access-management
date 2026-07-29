using ITAssetAccessManagement.Application.DTOs.AccessRequests;

namespace ITAssetAccessManagement.Application.Interfaces;

public interface IAccessRequestService
{
    Task<IEnumerable<AccessRequestSummaryResponse>> GetAllAsync();

    Task<IEnumerable<AccessRequestSummaryResponse>> GetByUserAsync(
        Guid userId);

    Task<AccessRequestResponse?> GetByIdAsync(
        Guid id);

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