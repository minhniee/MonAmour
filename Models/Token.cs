using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Token
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string TokenValue { get; set; } = null!;

    public string TokenType { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? UsedAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
