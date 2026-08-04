using ITAssetAccessManagement.Application.DTOs.AuditLogs;
using ITAssetAccessManagement.Application.DTOs.Dashboard;
using ITAssetAccessManagement.Application.Interfaces;
using ITAssetAccessManagement.Domain.Enums;
using ITAssetAccessManagement.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ITAssetAccessManagement.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var totalUsers = await _context.Users.CountAsync();

        var activeUsers = await _context.Users
            .CountAsync(user => user.IsActive);

        var totalAssets = await _context.Assets.CountAsync();

        var availableAssets = await _context.Assets
            .CountAsync(asset =>
                asset.Status == AssetStatus.Available);

        var assignedAssets = await _context.Assets
            .CountAsync(asset =>
                asset.Status == AssetStatus.Assigned);

        var pendingAccessRequests =
            await _context.AccessRequests.CountAsync(request =>
                request.Status == AccessRequestStatus.Pending);

        var approvedAccessRequests =
            await _context.AccessRequests.CountAsync(request =>
                request.Status == AccessRequestStatus.Approved);

        var rejectedAccessRequests =
            await _context.AccessRequests.CountAsync(request =>
                request.Status == AccessRequestStatus.Rejected);

        var recentAuditLogs = await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Take(5)
            .Select(log => new AuditLogResponse
            {
                Id = log.Id,
                UserId = log.UserId,
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            })
            .ToListAsync();

        return new DashboardSummaryResponse
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalAssets = totalAssets,
            AvailableAssets = availableAssets,
            AssignedAssets = assignedAssets,
            PendingAccessRequests = pendingAccessRequests,
            ApprovedAccessRequests = approvedAccessRequests,
            RejectedAccessRequests = rejectedAccessRequests,
            RecentAuditLogs = recentAuditLogs
        };
    }
}