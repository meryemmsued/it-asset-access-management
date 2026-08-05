using ITAssetAccessManagement.Application.DTOs.AssetCategories;

namespace ITAssetAccessManagement.Application.Interfaces;

public interface IAssetCategoryService
{
    Task<IEnumerable<AssetCategoryResponse>> GetAllAsync();
}