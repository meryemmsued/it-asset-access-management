using ITAssetAccessManagement.Domain.Common;

namespace ITAssetAccessManagement.Domain.Entities;

public class User : AuditableEntity
{
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ManagerId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public Department? Department { get; set; }
    public Team? Team { get; set; }
    public User? Manager { get; set; }

    public ICollection<User> ManagedUsers { get; set; } = new List<User>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}