namespace MonAmour.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public static class Names
    {
        public const string Admin = "admin";
        public const string User = "user";
    }
}
