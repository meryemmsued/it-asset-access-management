using ITAssetAccessManagement.Domain.Common;

namespace ITAssetAccessManagement.Domain.Entities;

public class Department : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<User> Users { get; set; } = new List<User>();
}