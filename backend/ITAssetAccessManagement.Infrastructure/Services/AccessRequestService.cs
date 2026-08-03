using ITAssetAccessManagement.Application.DTOs.AccessRequests;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccessManagement.Infrastructure.Services;

public sealed class AccessRequestService : IAccessRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public AccessRequestService(
        ApplicationDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IEnumerable<AccessRequestSummaryResponse>> GetAllAsync()
    {
        return await _context.AccessRequests
            .AsNoTracking()
            .Include(x => x.RequestedByUser)
            .Include(x => x.Asset)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AccessRequestSummaryResponse
            {
                Id = x.Id,
                RequestedBy =
                    x.RequestedByUser.FirstName + " " +
                    x.RequestedByUser.LastName,
                AssetName = x.Asset.Name,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<AccessRequestSummaryResponse>> GetByUserAsync(
        Guid userId)
    {
        return await _context.AccessRequests
            .AsNoTracking()
            .Include(x => x.RequestedByUser)
            .Include(x => x.Asset)
            .Where(x => x.RequestedByUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AccessRequestSummaryResponse
            {
                Id = x.Id,
                RequestedBy =
                    x.RequestedByUser.FirstName + " " +
                    x.RequestedByUser.LastName,
                AssetName = x.Asset.Name,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AccessRequestResponse?> GetByIdAsync(Guid id)
    {
        return await _context.AccessRequests
            .AsNoTracking()
            .Include(x => x.RequestedByUser)
            .Include(x => x.Asset)
            .Include(x => x.Approvals)
            .Where(x => x.Id == id)
            .Select(x => new AccessRequestResponse
            {
                Id = x.Id,
                AssetId = x.AssetId,
                AssetName = x.Asset.Name,
                RequestedByUserId = x.RequestedByUserId,
                RequestedBy =
                    x.RequestedByUser.FirstName + " " +
                    x.RequestedByUser.LastName,
                Reason = x.Reason,
                RequestedStartDate = x.RequestedStartDate,
                RequestedEndDate = x.RequestedEndDate,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                ApprovalComment = x.Approvals
                    .OrderByDescending(a => a.DecidedAt)
                    .Select(a => a.Comment)
                    .FirstOrDefault(),
                DecidedAt = x.Approvals
                    .OrderByDescending(a => a.DecidedAt)
                    .Select(a => a.DecidedAt)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AccessRequestResponse> CreateAsync(
        CreateAccessRequestRequest request,
        Guid requestedByUserId)
    {
        var assetExists = await _context.Assets
            .AnyAsync(x => x.Id == request.AssetId);

        if (!assetExists)
            throw new InvalidOperationException("Asset not found.");

        var userExists = await _context.Users
            .AnyAsync(x => x.Id == requestedByUserId);

        if (!userExists)
            throw new InvalidOperationException("User not found.");

        if (request.RequestedStartDate.HasValue &&
            request.RequestedEndDate.HasValue &&
            request.RequestedEndDate < request.RequestedStartDate)
        {
            throw new InvalidOperationException(
                "Requested end date cannot be earlier than start date.");
        }

        var alreadyPending = await _context.AccessRequests
            .AnyAsync(x =>
                x.AssetId == request.AssetId &&
                x.RequestedByUserId == requestedByUserId &&
                x.Status == AccessRequestStatus.Pending);

        if (alreadyPending)
        {
            throw new InvalidOperationException(
                "A pending access request already exists for this asset.");
        }

        var accessRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            RequestedByUserId = requestedByUserId,
            AssetId = request.AssetId,
            RequestedAccessType = request.RequestedAccessType,
            Reason = request.Reason,
            RequestedStartDate = request.RequestedStartDate,
            RequestedEndDate = request.RequestedEndDate,
            RequestedAt = DateTime.UtcNow,
            ResolvedAt = null,
            Status = AccessRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AccessRequests.Add(accessRequest);

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            userId: requestedByUserId,
            action: "ACCESS_REQUEST_CREATED",
            entityType: "AccessRequest",
            entityId: accessRequest.Id,
            oldValues: null,
            newValues: new
            {
                accessRequest.AssetId,
                accessRequest.RequestedAccessType,
                accessRequest.Reason,
                accessRequest.RequestedStartDate,
                accessRequest.RequestedEndDate,
                Status = accessRequest.Status.ToString()
            });

        return await GetByIdAsync(accessRequest.Id)
            ?? throw new InvalidOperationException(
                "Failed to load created access request.");
    }

    public async Task<bool> ApproveAsync(
        Guid id,
        ApproveAccessRequestRequest request,
        Guid approverUserId)
    {
        var accessRequest = await _context.AccessRequests
            .FirstOrDefaultAsync(x => x.Id == id);

        if (accessRequest is null)
            return false;

        if (accessRequest.Status != AccessRequestStatus.Pending)
            return false;

        var approverExists = await _context.Users
            .AnyAsync(x => x.Id == approverUserId);

        if (!approverExists)
            return false;

        var oldStatus = accessRequest.Status;

        accessRequest.Status = AccessRequestStatus.Approved;
        accessRequest.ResolvedAt = DateTime.UtcNow;
        accessRequest.UpdatedAt = DateTime.UtcNow;

        _context.AccessRequestApprovals.Add(
            new AccessRequestApproval
            {
                Id = Guid.NewGuid(),
                AccessRequestId = accessRequest.Id,
                ApproverUserId = approverUserId,
                ApprovalOrder = 1,
                Decision = ApprovalDecision.Approved,
                Comment = request.Comment,
                DecidedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            userId: approverUserId,
            action: "ACCESS_REQUEST_APPROVED",
            entityType: "AccessRequest",
            entityId: accessRequest.Id,
            oldValues: new
            {
                Status = oldStatus.ToString()
            },
            newValues: new
            {
                Status = accessRequest.Status.ToString(),
                accessRequest.ResolvedAt,
                request.Comment
            });

        return true;
    }

    public async Task<bool> RejectAsync(
        Guid id,
        RejectAccessRequestRequest request,
        Guid approverUserId)
    {
        var accessRequest = await _context.AccessRequests
            .FirstOrDefaultAsync(x => x.Id == id);

        if (accessRequest is null)
            return false;

        if (accessRequest.Status != AccessRequestStatus.Pending)
            return false;

        if (string.IsNullOrWhiteSpace(request.Comment))
            return false;

        var approverExists = await _context.Users
            .AnyAsync(x => x.Id == approverUserId);

        if (!approverExists)
            return false;

        var oldStatus = accessRequest.Status;

        accessRequest.Status = AccessRequestStatus.Rejected;
        accessRequest.ResolvedAt = DateTime.UtcNow;
        accessRequest.UpdatedAt = DateTime.UtcNow;

        _context.AccessRequestApprovals.Add(
            new AccessRequestApproval
            {
                Id = Guid.NewGuid(),
                AccessRequestId = accessRequest.Id,
                ApproverUserId = approverUserId,
                Decision = ApprovalDecision.Rejected,
                ApprovalOrder = 1,
                Comment = request.Comment,
                DecidedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            userId: approverUserId,
            action: "ACCESS_REQUEST_REJECTED",
            entityType: "AccessRequest",
            entityId: accessRequest.Id,
            oldValues: new
            {
                Status = oldStatus.ToString()
            },
            newValues: new
            {
                Status = accessRequest.Status.ToString(),
                accessRequest.ResolvedAt,
                request.Comment
            });

        return true;
    }

    public async Task<bool> CancelAsync(
        Guid id,
        Guid requestedByUserId)
    {
        var accessRequest = await _context.AccessRequests
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.RequestedByUserId == requestedByUserId);

        if (accessRequest is null)
            return false;

        if (accessRequest.Status != AccessRequestStatus.Pending)
            return false;

        var oldStatus = accessRequest.Status;

        accessRequest.Status = AccessRequestStatus.Cancelled;
        accessRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            userId: requestedByUserId,
            action: "ACCESS_REQUEST_CANCELLED",
            entityType: "AccessRequest",
            entityId: accessRequest.Id,
            oldValues: new
            {
                Status = oldStatus.ToString()
            },
            newValues: new
            {
                Status = accessRequest.Status.ToString()
            });

        return true;
    }
}