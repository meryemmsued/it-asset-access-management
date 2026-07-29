using ITAssetAccessManagement.Application.DTOs.Assets;

namespace ITAssetAccessManagement.Application.Interfaces;

public interface IAssetService
{
    Task<IEnumerable<AssetSummaryResponse>> GetAllAsync();

    Task<AssetResponse?> GetByIdAsync(Guid id);

    Task<AssetResponse> CreateAsync(
        CreateAssetRequest request,
        Guid createdByUserId);

    Task<AssetResponse?> UpdateAsync(
        Guid id,
        UpdateAssetRequest request);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> AssignAsync(
        Guid assetId,
        AssignAssetRequest request,
        Guid assignedByUserId);

    Task<bool> ReturnAsync(
        Guid assetId,
        ReturnAssetRequest request);

    Task<IEnumerable<string>> GetStatusHistoryAsync(Guid assetId);
}