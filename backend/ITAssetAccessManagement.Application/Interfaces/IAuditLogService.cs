using ITAssetAccessManagement.Application.DTOs.AuditLogs;
using ITAssetAccessManagement.Application.DTOs.Common;

namespace ITAssetAccessManagement.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        Guid? userId,
        string action,
        string entityType,
        Guid? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null,
        string? userAgent = null);

    Task<PagedResult<AuditLogResponse>> GetAllAsync(
        int page,
        int pageSize);

    Task<AuditLogResponse?> GetByIdAsync(Guid id);
}