namespace ITAssetAccessManagement.Application.DTOs.Users;

public sealed class UserRoleResponse
{
    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; }

    public Guid? AssignedByUserId { get; set; }
}