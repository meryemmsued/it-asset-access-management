using ITAssetAccessManagement.Domain.Common;

namespace ITAssetAccessManagement.Domain.Entities;

public class Team : AuditableEntity
{
    public Guid DepartmentId { get; set; }
    public Guid? TeamLeadUserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Department Department { get; set; } = null!;
    public User? TeamLeadUser { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}