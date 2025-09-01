using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MonAmour.Models;

public partial class Role
{
    public int RoleId { get; set; }

    [Required]
    public string RoleName { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Static properties for role names
    public static class Names
    {
        public const string User = "user";
        public const string Admin = "admin";
    }
}
