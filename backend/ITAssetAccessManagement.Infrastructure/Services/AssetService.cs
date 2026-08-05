using ITAssetAccessManagement.Application.DTOs.Assets;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using ITAssetAccessManagement.Application.DTOs.Common;

namespace ITAssetAccessManagement.Infrastructure.Services;

public sealed class AssetService : IAssetService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public AssetService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<AssetSummaryResponse>> GetAllAsync(
        int page,
        int pageSize)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var query = _context.Assets
            .AsNoTracking()
            .OrderBy(asset => asset.Name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(asset => new AssetSummaryResponse
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                Name = asset.Name,
                Category = asset.AssetCategory.Name,
                Status = asset.Status
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        return new PagedResult<AssetSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
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
            .FirstOrDefaultAsync(
                category => category.Id == request.AssetCategoryId);

        if (category is null)
            throw new InvalidOperationException("Asset category not found.");

        var assetCodeExists = await _context.Assets
            .AnyAsync(asset => asset.AssetCode == request.AssetCode);

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

        _context.AssetStatusHistories.Add(
            new AssetStatusHistory
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

        await _auditLogService.LogAsync(
            userId: createdByUserId,
            action: "ASSET_CREATED",
            entityType: "Asset",
            entityId: asset.Id,
            oldValues: null,
            newValues: new
            {
                asset.AssetCode,
                asset.Name,
                asset.Description,
                Status = asset.Status.ToString(),
                asset.AssetCategoryId,
                asset.PurchaseDate,
                asset.PurchasePrice,
                asset.WarrantyExpirationDate
            });

        return await GetByIdAsync(asset.Id)
            ?? throw new InvalidOperationException(
                "Failed to load created asset.");
    }

    public async Task<AssetResponse?> UpdateAsync(
        Guid id,
        UpdateAssetRequest request)
    {
        var asset = await _context.Assets
            .Include(asset => asset.PhysicalDetail)
            .Include(asset => asset.DigitalDetail)
            .FirstOrDefaultAsync(asset => asset.Id == id);

        if (asset is null)
            return null;

        var oldValues = new
        {
            asset.Name,
            asset.Description,
            Status = asset.Status.ToString(),
            asset.PurchaseDate,
            asset.PurchasePrice,
            asset.WarrantyExpirationDate,

            SerialNumber = asset.PhysicalDetail?.SerialNumber,
            Manufacturer = asset.PhysicalDetail?.Manufacturer,
            Model = asset.PhysicalDetail?.Model,
            Location = asset.PhysicalDetail?.Location,
            Condition = asset.PhysicalDetail?.Condition?.ToString(),

            LicenseKey = asset.DigitalDetail?.LicenseKey,
            Version = asset.DigitalDetail?.Version,
            LicenseType = asset.DigitalDetail?.LicenseType?.ToString(),
            LicenseStartDate = asset.DigitalDetail?.LicenseStartDate,
            LicenseExpirationDate =
                asset.DigitalDetail?.LicenseExpirationDate,
            MaximumUsers = asset.DigitalDetail?.MaximumUsers
        };

        var oldStatus = asset.Status;

        asset.Name = request.Name;
        asset.Description = request.Description;
        asset.Status = request.Status;
        asset.PurchaseDate = request.PurchaseDate;
        asset.PurchasePrice = request.PurchasePrice;
        asset.WarrantyExpirationDate =
            request.WarrantyExpirationDate;
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
            asset.DigitalDetail.LicenseStartDate =
                request.LicenseStartDate;
            asset.DigitalDetail.LicenseExpirationDate =
                request.LicenseExpirationDate;
            asset.DigitalDetail.MaximumUsers = request.MaximumUsers;
        }

        if (oldStatus != request.Status)
        {
            _context.AssetStatusHistories.Add(
                new AssetStatusHistory
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

        await _auditLogService.LogAsync(
            userId: asset.CreatedByUserId,
            action: "ASSET_UPDATED",
            entityType: "Asset",
            entityId: asset.Id,
            oldValues: oldValues,
            newValues: new
            {
                asset.Name,
                asset.Description,
                Status = asset.Status.ToString(),
                asset.PurchaseDate,
                asset.PurchasePrice,
                asset.WarrantyExpirationDate,

                SerialNumber = asset.PhysicalDetail?.SerialNumber,
                Manufacturer = asset.PhysicalDetail?.Manufacturer,
                Model = asset.PhysicalDetail?.Model,
                Location = asset.PhysicalDetail?.Location,
                Condition =
                    asset.PhysicalDetail?.Condition?.ToString(),

                LicenseKey = asset.DigitalDetail?.LicenseKey,
                Version = asset.DigitalDetail?.Version,
                LicenseType =
                    asset.DigitalDetail?.LicenseType?.ToString(),
                LicenseStartDate =
                    asset.DigitalDetail?.LicenseStartDate,
                LicenseExpirationDate =
                    asset.DigitalDetail?.LicenseExpirationDate,
                MaximumUsers =
                    asset.DigitalDetail?.MaximumUsers
            });

        return await GetByIdAsync(asset.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var asset = await _context.Assets
            .FirstOrDefaultAsync(asset => asset.Id == id);

        if (asset is null)
            return false;

        var deletedByUserId = asset.CreatedByUserId;

        var deletedAssetValues = new
        {
            asset.AssetCode,
            asset.Name,
            asset.Description,
            Status = asset.Status.ToString(),
            asset.AssetCategoryId
        };

        var assignments = await _context.AssetAssignments
            .Where(assignment => assignment.AssetId == id)
            .ToListAsync();

        var statusHistories = await _context.AssetStatusHistories
            .Where(history => history.AssetId == id)
            .ToListAsync();

        var physicalDetail = await _context.PhysicalAssetDetails
            .FirstOrDefaultAsync(detail => detail.AssetId == id);

        var digitalDetail = await _context.DigitalAssetDetails
            .FirstOrDefaultAsync(detail => detail.AssetId == id);

        _context.AssetAssignments.RemoveRange(assignments);
        _context.AssetStatusHistories.RemoveRange(statusHistories);

        if (physicalDetail is not null)
            _context.PhysicalAssetDetails.Remove(physicalDetail);

        if (digitalDetail is not null)
            _context.DigitalAssetDetails.Remove(digitalDetail);

        _context.Assets.Remove(asset);

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            userId: deletedByUserId,
            action: "ASSET_DELETED",
            entityType: "Asset",
            entityId: id,
            oldValues: deletedAssetValues,
            newValues: null);

        return true;
    }

    public async Task<bool> AssignAsync(
        Guid assetId,
        AssignAssetRequest request,
        Guid assignedByUserId)
    {
        var asset = await _context.Assets
            .FirstOrDefaultAsync(asset => asset.Id == assetId);

        if (asset is null)
            return false;

        if (asset.Status != AssetStatus.Available)
            return false;

        var userExists = await _context.Users
            .AnyAsync(user => user.Id == request.AssignedToUserId);

        if (!userExists)
            return false;

        var oldStatus = asset.Status;

        asset.Status = AssetStatus.Assigned;
        asset.UpdatedAt = DateTime.UtcNow;

        var assignment = new AssetAssignment
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            AssignedToUserId = request.AssignedToUserId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow,
            Notes = request.Notes,
            Status = AssetAssignmentStatus.Active
        };

        _context.AssetAssignments.Add(assignment);

        _context.AssetStatusHistories.Add(
            new AssetStatusHistory
            {
                Id = Guid.NewGuid(),
                AssetId = asset.Id,
                OldStatus = oldStatus,
                NewStatus = AssetStatus.Assigned,
                ChangedByUserId = assignedByUserId,
                ChangedAt = DateTime.UtcNow,
                ChangeReason = "Asset assigned"
            });

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            userId: assignedByUserId,
            action: "ASSET_ASSIGNED",
            entityType: "Asset",
            entityId: asset.Id,
            oldValues: new
            {
                Status = oldStatus.ToString()
            },
            newValues: new
            {
                Status = asset.Status.ToString(),
                assignment.Id,
                assignment.AssignedToUserId,
                assignment.AssignedByUserId,
                assignment.AssignedAt,
                assignment.Notes
            });

        return true;
    }

    public async Task<bool> ReturnAsync(
        Guid assetId,
        ReturnAssetRequest request)
    {
        var assignment = await _context.AssetAssignments
            .Where(assignment =>
                assignment.AssetId == assetId &&
                assignment.Status ==
                AssetAssignmentStatus.Active)
            .OrderByDescending(assignment => assignment.AssignedAt)
            .FirstOrDefaultAsync();

        if (assignment is null)
            return false;

        var asset = await _context.Assets
            .FirstAsync(asset => asset.Id == assetId);

        var oldAssetStatus = asset.Status;
        var oldAssignmentStatus = assignment.Status;

        assignment.Status = AssetAssignmentStatus.Returned;
        assignment.ReturnedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            assignment.Notes = request.Notes;
        }

        asset.Status = AssetStatus.Available;
        asset.UpdatedAt = DateTime.UtcNow;

        _context.AssetStatusHistories.Add(
            new AssetStatusHistory
            {
                Id = Guid.NewGuid(),
                AssetId = asset.Id,
                OldStatus = oldAssetStatus,
                NewStatus = AssetStatus.Available,
                ChangedByUserId = assignment.AssignedByUserId,
                ChangedAt = DateTime.UtcNow,
                ChangeReason = "Asset returned"
            });

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            userId: assignment.AssignedByUserId,
            action: "ASSET_RETURNED",
            entityType: "Asset",
            entityId: asset.Id,
            oldValues: new
            {
                AssetStatus = oldAssetStatus.ToString(),
                AssignmentStatus =
                    oldAssignmentStatus.ToString()
            },
            newValues: new
            {
                AssetStatus = asset.Status.ToString(),
                AssignmentStatus =
                    assignment.Status.ToString(),
                assignment.ReturnedAt,
                assignment.Notes
            });

        return true;
    }

    public async Task<IEnumerable<string>> GetStatusHistoryAsync(
        Guid assetId)
    {
        return await _context.AssetStatusHistories
            .AsNoTracking()
            .Where(history => history.AssetId == assetId)
            .OrderByDescending(history => history.ChangedAt)
            .Select(history =>
                $"{history.ChangedAt:u} | " +
                $"{history.OldStatus} -> " +
                $"{history.NewStatus} | " +
                $"{history.ChangeReason}")
            .ToListAsync();
    }
}