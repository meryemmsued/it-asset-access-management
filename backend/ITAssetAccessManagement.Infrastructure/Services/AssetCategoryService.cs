using ITAssetAccessManagement.Application.DTOs.AssetCategories;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccessManagement.Infrastructure.Services;

public sealed class AssetCategoryService : IAssetCategoryService
{
    private readonly ApplicationDbContext _context;

    public AssetCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetCategoryResponse>> GetAllAsync()
    {
        return await _context.AssetCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new AssetCategoryResponse
            {
                Id = x.Id,
                Name = x.Name,
                AssetType = x.AssetType.ToString()
            })
            .ToListAsync();
    }
}