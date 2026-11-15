using TeamBrain.Domain.Common;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Domain.Entities;

public class User : BaseEntity
{
    public string EntraObjectId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<UserRoleAssignment> RoleAssignments { get; set; } = new List<UserRoleAssignment>();
}

public class UserRoleAssignment : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public UserRole Role { get; set; }
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
}

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<UserRoleAssignment> Members { get; set; } = new List<UserRoleAssignment>();
}
