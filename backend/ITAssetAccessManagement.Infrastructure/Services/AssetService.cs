using ITAssetAccessManagement.Application.DTOs.Assets;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Persistence.Contexts;
using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccessManagement.Infrastructure.Services;

public sealed class AssetService : IAssetService
{
    private readonly ApplicationDbContext _context;

    public AssetService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetSummaryResponse>> GetAllAsync()
    {
        var assets = await _context.Assets
            .AsNoTracking()
            .Include(asset => asset.AssetCategory)
            .OrderBy(asset => asset.Name)
            .Select(asset => new AssetSummaryResponse
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                Name = asset.Name,
                Category = asset.AssetCategory.Name,
                Status = asset.Status
            })
            .ToListAsync();

        return assets;
    }

    public async Task<AssetResponse?> GetByIdAsync(Guid id)
    {
        var asset = await _context.Assets
            .AsNoTracking()
            .Include(asset => asset.AssetCategory)
            .Include(asset => asset.PhysicalDetail)
            .Include(asset => asset.DigitalDetail)
            .FirstOrDefaultAsync(asset => asset.Id == id);

        if (asset is null)
            return null;

        return new AssetResponse
        {
            Id = asset.Id,
            AssetCategoryId = asset.AssetCategoryId,
            AssetCode = asset.AssetCode,
            Name = asset.Name,
            Description = asset.Description,
            Status = asset.Status,
            PurchaseDate = asset.PurchaseDate,
            PurchasePrice = asset.PurchasePrice,
            WarrantyExpirationDate = asset.WarrantyExpirationDate,

            SerialNumber = asset.PhysicalDetail?.SerialNumber,
            Manufacturer = asset.PhysicalDetail?.Manufacturer,
            Model = asset.PhysicalDetail?.Model,
            Location = asset.PhysicalDetail?.Location,
            Condition = asset.PhysicalDetail?.Condition,

            LicenseKey = asset.DigitalDetail?.LicenseKey,
            Version = asset.DigitalDetail?.Version,
            LicenseType = asset.DigitalDetail?.LicenseType,
            LicenseStartDate = asset.DigitalDetail?.LicenseStartDate,
            LicenseExpirationDate = asset.DigitalDetail?.LicenseExpirationDate,
            MaximumUsers = asset.DigitalDetail?.MaximumUsers
        };
    }

    public async Task<AssetResponse> CreateAsync(
        CreateAssetRequest request,
        Guid createdByUserId)
    {
        var category = await _context.AssetCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.AssetCategoryId);

        if (category is null)
            throw new InvalidOperationException("Asset category not found.");

        var assetCodeExists = await _context.Assets
            .AnyAsync(a => a.AssetCode == request.AssetCode);

        if (assetCodeExists)
            throw new InvalidOperationException("Asset code already exists.");

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            AssetCategoryId = request.AssetCategoryId,
            AssetCode = request.AssetCode,
            Name = request.Name,
            Description = request.Description,
            Status = AssetStatus.Available,
            PurchaseDate = request.PurchaseDate,
            PurchasePrice = request.PurchasePrice,
            WarrantyExpirationDate = request.WarrantyExpirationDate,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (category.AssetType == AssetType.Physical)
        {
            asset.PhysicalDetail = new PhysicalAssetDetail
            {
                AssetId = asset.Id,
                SerialNumber = request.SerialNumber,
                Manufacturer = request.Manufacturer,
                Model = request.Model,
                Location = request.Location,
                Condition = request.Condition
            };
        }
        else
        {
            asset.DigitalDetail = new DigitalAssetDetail
            {
                AssetId = asset.Id,
                LicenseKey = request.LicenseKey,
                Version = request.Version,
                LicenseType = request.LicenseType,
                LicenseStartDate = request.LicenseStartDate,
                LicenseExpirationDate = request.LicenseExpirationDate,
                MaximumUsers = request.MaximumUsers
            };
        }

        _context.Assets.Add(asset);

        _context.AssetStatusHistories.Add(new AssetStatusHistory
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            OldStatus = null,
            NewStatus = AssetStatus.Available,
            ChangedByUserId = createdByUserId,
            ChangedAt = DateTime.UtcNow,
            ChangeReason = "Asset created"
        });

        await _context.SaveChangesAsync();

        return await GetByIdAsync(asset.Id)
            ?? throw new InvalidOperationException("Failed to load created asset.");
    }

    public async Task<AssetResponse?> UpdateAsync(
        Guid id,
        UpdateAssetRequest request)
    {
        var asset = await _context.Assets
            .Include(a => a.PhysicalDetail)
            .Include(a => a.DigitalDetail)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset is null)
            return null;

        var oldStatus = asset.Status;

        asset.Name = request.Name;
        asset.Description = request.Description;
        asset.Status = request.Status;
        asset.PurchaseDate = request.PurchaseDate;
        asset.PurchasePrice = request.PurchasePrice;
        asset.WarrantyExpirationDate = request.WarrantyExpirationDate;
        asset.UpdatedAt = DateTime.UtcNow;

        if (asset.PhysicalDetail is not null)
        {
            asset.PhysicalDetail.SerialNumber = request.SerialNumber;
            asset.PhysicalDetail.Manufacturer = request.Manufacturer;
            asset.PhysicalDetail.Model = request.Model;
            asset.PhysicalDetail.Location = request.Location;
            asset.PhysicalDetail.Condition = request.Condition;
        }

        if (asset.DigitalDetail is not null)
        {
            asset.DigitalDetail.LicenseKey = request.LicenseKey;
            asset.DigitalDetail.Version = request.Version;
            asset.DigitalDetail.LicenseType = request.LicenseType;
            asset.DigitalDetail.LicenseStartDate = request.LicenseStartDate;
            asset.DigitalDetail.LicenseExpirationDate = request.LicenseExpirationDate;
            asset.DigitalDetail.MaximumUsers = request.MaximumUsers;
        }

        if (oldStatus != request.Status)
        {
            _context.AssetStatusHistories.Add(new AssetStatusHistory
            {
                Id = Guid.NewGuid(),
                AssetId = asset.Id,
                OldStatus = oldStatus,
                NewStatus = request.Status,
                ChangedByUserId = asset.CreatedByUserId,
                ChangedAt = DateTime.UtcNow,
                ChangeReason = "Asset updated"
            });
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(asset.Id);
    }


    public async Task<bool> DeleteAsync(Guid id)
    {
        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset is null)
            return false;

        var assignments = await _context.AssetAssignments
            .Where(a => a.AssetId == id)
            .ToListAsync();

        var statusHistories = await _context.AssetStatusHistories
            .Where(h => h.AssetId == id)
            .ToListAsync();

        var physicalDetail = await _context.PhysicalAssetDetails
            .FirstOrDefaultAsync(p => p.AssetId == id);

        var digitalDetail = await _context.DigitalAssetDetails
            .FirstOrDefaultAsync(d => d.AssetId == id);

        _context.AssetAssignments.RemoveRange(assignments);
        _context.AssetStatusHistories.RemoveRange(statusHistories);

        if (physicalDetail is not null)
            _context.PhysicalAssetDetails.Remove(physicalDetail);

        if (digitalDetail is not null)
            _context.DigitalAssetDetails.Remove(digitalDetail);

        _context.Assets.Remove(asset);

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> AssignAsync(
        Guid assetId,
        AssignAssetRequest request,
        Guid assignedByUserId)
    {
        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == assetId);

        if (asset is null)
            return false;

        if (asset.Status != AssetStatus.Available)
            return false;

        var userExists = await _context.Users
            .AnyAsync(u => u.Id == request.AssignedToUserId);

        if (!userExists)
            return false;

        asset.Status = AssetStatus.Assigned;
        asset.UpdatedAt = DateTime.UtcNow;

        _context.AssetAssignments.Add(new AssetAssignment
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            AssignedToUserId = request.AssignedToUserId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow,
            Notes = request.Notes,
            Status = AssetAssignmentStatus.Active
        });

        _context.AssetStatusHistories.Add(new AssetStatusHistory
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            OldStatus = AssetStatus.Available,
            NewStatus = AssetStatus.Assigned,
            ChangedByUserId = assignedByUserId,
            ChangedAt = DateTime.UtcNow,
            ChangeReason = "Asset assigned"
        });

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReturnAsync(
        Guid assetId,
        ReturnAssetRequest request)
    {
        var assignment = await _context.AssetAssignments
            .Where(a =>
                a.AssetId == assetId &&
                a.Status == AssetAssignmentStatus.Active)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync();

        if (assignment is null)
            return false;

        var asset = await _context.Assets
            .FirstAsync(a => a.Id == assetId);

        assignment.Status = AssetAssignmentStatus.Returned;
        assignment.ReturnedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            assignment.Notes = request.Notes;
        }

        asset.Status = AssetStatus.Available;
        asset.UpdatedAt = DateTime.UtcNow;

        _context.AssetStatusHistories.Add(new AssetStatusHistory
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            OldStatus = AssetStatus.Assigned,
            NewStatus = AssetStatus.Available,
            ChangedByUserId = assignment.AssignedByUserId,
            ChangedAt = DateTime.UtcNow,
            ChangeReason = "Asset returned"
        });

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<string>> GetStatusHistoryAsync(Guid assetId)
    {
        return await _context.AssetStatusHistories
            .AsNoTracking()
            .Where(h => h.AssetId == assetId)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h =>
                $"{h.ChangedAt:u} | {h.OldStatus} -> {h.NewStatus} | {h.ChangeReason}")
            .ToListAsync();
    }

}