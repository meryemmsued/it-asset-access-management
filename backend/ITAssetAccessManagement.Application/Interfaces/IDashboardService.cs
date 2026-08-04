using ITAssetAccessManagement.Application.DTOs.Dashboard;

namespace ITAssetAccessManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();
}