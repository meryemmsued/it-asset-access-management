using ITAssetAccessManagement.Application.DTOs.AccessRequests;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Domain.Entities;
using ITAssetAccessManagement.Domain.Enums;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using ITAssetAccessManagement.Application.DTOs.Common;

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

    public async Task<PagedResult<AccessRequestSummaryResponse>>
        GetVisibleRequestsAsync(
            Guid currentUserId,
            bool isAdmin,
            int page,
            int pageSize)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var query = _context.AccessRequests
            .AsNoTracking()
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(accessRequest =>
                accessRequest.RequestedByUser.Team != null &&
                accessRequest.RequestedByUser.Team.TeamLeadUserId ==
                currentUserId);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(accessRequest =>
                accessRequest.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            
            .Select(accessRequest =>
                new AccessRequestSummaryResponse
                {
                    Id = accessRequest.Id,

                    RequestedByUserId =
                        accessRequest.RequestedByUserId,

                    RequestedBy =
                        accessRequest.RequestedByUser.FirstName +
                        " " +
                        accessRequest.RequestedByUser.LastName,

                    AssetName = accessRequest.Asset.Name,

                    Status = accessRequest.Status,

                    CreatedAt = accessRequest.CreatedAt,

                    CanApprove =
                        accessRequest.Status ==
                            AccessRequestStatus.Pending &&
                        accessRequest.RequestedByUserId !=
                            currentUserId &&
                        (
                            isAdmin ||
                            (
                                accessRequest.RequestedByUser.Team != null &&
                                accessRequest.RequestedByUser.Team
                                    .TeamLeadUserId == currentUserId
                            )
                        ),

                    CanCancel =
                        accessRequest.Status ==
                            AccessRequestStatus.Pending &&
                        accessRequest.RequestedByUserId ==
                            currentUserId
                })
            .ToListAsync();

        return new PagedResult<AccessRequestSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize)
        };
    }

        public async Task<PagedResult<AccessRequestSummaryResponse>>
        GetByUserAsync(
            Guid userId,
            int page,
            int pageSize)
    {
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 10;

        if (pageSize > 100)
            pageSize = 100;

        var query = _context.AccessRequests
            .AsNoTracking()
            .Where(accessRequest =>
                accessRequest.RequestedByUserId == userId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(accessRequest =>
                accessRequest.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(accessRequest =>
                new AccessRequestSummaryResponse
                {
                    Id = accessRequest.Id,

                    RequestedByUserId =
                        accessRequest.RequestedByUserId,

                    RequestedBy =
                        accessRequest.RequestedByUser.FirstName +
                        " " +
                        accessRequest.RequestedByUser.LastName,

                    AssetName =
                        accessRequest.Asset.Name,

                    Status =
                        accessRequest.Status,

                    CreatedAt =
                        accessRequest.CreatedAt,

                    CanApprove = false,

                    CanCancel =
                        accessRequest.Status ==
                        AccessRequestStatus.Pending
                })
            .ToListAsync();

        return new PagedResult<AccessRequestSummaryResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize)
        };
    }

    public async Task<AccessRequestResponse?> GetByIdAsync(Guid id)
    {
        return await _context.AccessRequests
            .AsNoTracking()
            .Where(accessRequest => accessRequest.Id == id)
            .Select(accessRequest => new AccessRequestResponse
            {
                Id = accessRequest.Id,
                AssetId = accessRequest.AssetId,
                AssetName = accessRequest.Asset.Name,
                RequestedByUserId =
                    accessRequest.RequestedByUserId,
                RequestedBy =
                    accessRequest.RequestedByUser.FirstName +
                    " " +
                    accessRequest.RequestedByUser.LastName,
                Reason = accessRequest.Reason,
                RequestedStartDate =
                    accessRequest.RequestedStartDate,
                RequestedEndDate =
                    accessRequest.RequestedEndDate,
                Status = accessRequest.Status,
                CreatedAt = accessRequest.CreatedAt,
                ApprovalComment = accessRequest.Approvals
                    .OrderByDescending(approval =>
                        approval.DecidedAt)
                    .Select(approval => approval.Comment)
                    .FirstOrDefault(),
                DecidedAt = accessRequest.Approvals
                    .OrderByDescending(approval =>
                        approval.DecidedAt)
                    .Select(approval => approval.DecidedAt)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CanViewRequestAsync(
        Guid requestId,
        Guid currentUserId,
        bool isAdmin)
    {
        if (isAdmin)
            return true;

        var requestInfo = await _context.AccessRequests
            .AsNoTracking()
            .Where(accessRequest =>
                accessRequest.Id == requestId)
            .Select(accessRequest => new
            {
                accessRequest.RequestedByUserId,
                RequesterTeamLeadUserId =
                    accessRequest.RequestedByUser.Team != null
                        ? accessRequest.RequestedByUser.Team
                            .TeamLeadUserId
                        : null
            })
            .FirstOrDefaultAsync();

        if (requestInfo is null)
            return false;

        if (requestInfo.RequestedByUserId == currentUserId)
            return true;

        return requestInfo.RequesterTeamLeadUserId ==
               currentUserId;
    }

    public async Task<AccessRequestResponse> CreateAsync(
        CreateAccessRequestRequest request,
        Guid requestedByUserId)
    {
        var assetExists = await _context.Assets
            .AnyAsync(asset => asset.Id == request.AssetId);

        if (!assetExists)
        {
            throw new InvalidOperationException(
                "Asset not found.");
        }

        var userExists = await _context.Users
            .AnyAsync(user => user.Id == requestedByUserId);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "User not found.");
        }

        if (request.RequestedStartDate.HasValue &&
            request.RequestedEndDate.HasValue &&
            request.RequestedEndDate <
            request.RequestedStartDate)
        {
            throw new InvalidOperationException(
                "Requested end date cannot be earlier than start date.");
        }

        var alreadyPending = await _context.AccessRequests
            .AnyAsync(accessRequest =>
                accessRequest.AssetId == request.AssetId &&
                accessRequest.RequestedByUserId ==
                requestedByUserId &&
                accessRequest.Status ==
                AccessRequestStatus.Pending);

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
            RequestedAccessType =
                request.RequestedAccessType,
            Reason = request.Reason,
            RequestedStartDate =
                request.RequestedStartDate,
            RequestedEndDate =
                request.RequestedEndDate,
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
            .FirstOrDefaultAsync(currentRequest =>
                currentRequest.Id == id);

        if (accessRequest is null)
            return false;

        if (accessRequest.Status !=
            AccessRequestStatus.Pending)
        {
            return false;
        }

        var canResolve = await CanResolveRequestAsync(
            approverUserId,
            accessRequest.RequestedByUserId);

        if (!canResolve)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to approve this access request.");
        }

        var oldStatus = accessRequest.Status;

        accessRequest.Status =
            AccessRequestStatus.Approved;
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
            .FirstOrDefaultAsync(currentRequest =>
                currentRequest.Id == id);

        if (accessRequest is null)
            return false;

        if (accessRequest.Status !=
            AccessRequestStatus.Pending)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Comment))
            return false;

        var canResolve = await CanResolveRequestAsync(
            approverUserId,
            accessRequest.RequestedByUserId);

        if (!canResolve)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to reject this access request.");
        }

        var oldStatus = accessRequest.Status;

        accessRequest.Status =
            AccessRequestStatus.Rejected;
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
            .FirstOrDefaultAsync(currentRequest =>
                currentRequest.Id == id &&
                currentRequest.RequestedByUserId ==
                requestedByUserId);

        if (accessRequest is null)
            return false;

        if (accessRequest.Status !=
            AccessRequestStatus.Pending)
        {
            return false;
        }

        var oldStatus = accessRequest.Status;

        accessRequest.Status =
            AccessRequestStatus.Cancelled;
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

    private async Task<bool> CanResolveRequestAsync(
        Guid approverUserId,
        Guid requesterUserId)
    {
        // No user may approve or reject their own request.
        if (approverUserId == requesterUserId)
            return false;

        var approverRoles = await _context.UserRoles
            .AsNoTracking()
            .Where(userRole =>
                userRole.UserId == approverUserId)
            .Select(userRole => userRole.Role.Name)
            .ToListAsync();

        // Admin may resolve requests belonging to any user.
        if (approverRoles.Contains("Admin"))
            return true;

        // Anyone other than a Team Lead is unauthorized.
        if (!approverRoles.Contains("Team Lead"))
            return false;

        var requesterTeamId = await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == requesterUserId)
            .Select(user => user.TeamId)
            .FirstOrDefaultAsync();

        if (!requesterTeamId.HasValue)
            return false;

        // Team Lead may only resolve requests made by
        // users belonging to their own team.
        return await _context.Teams
            .AsNoTracking()
            .AnyAsync(team =>
                team.Id == requesterTeamId.Value &&
                team.TeamLeadUserId == approverUserId);
    }
}