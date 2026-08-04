using ITAssetAccessManagement.Application.DTOs.AuditLogs;

namespace ITAssetAccessManagement.Application.DTOs.Dashboard;

public sealed class DashboardSummaryResponse
{
    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int TotalAssets { get; set; }

    public int AvailableAssets { get; set; }

    public int AssignedAssets { get; set; }

    public int PendingAccessRequests { get; set; }

    public int ApprovedAccessRequests { get; set; }

    public int RejectedAccessRequests { get; set; }

    public List<AuditLogResponse> RecentAuditLogs { get; set; }
        = new();
}