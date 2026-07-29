namespace ITAssetAccessManagement.Application.DTOs.Roles;

public sealed class UpdateRoleRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}