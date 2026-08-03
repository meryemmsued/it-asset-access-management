using ITAssetAccessManagement.Application.DTOs.AuditLogs;

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

    Task<List<AuditLogResponse>> GetAllAsync();

    Task<AuditLogResponse?> GetByIdAsync(Guid id);
}