using System;
using System.Collections.Generic;

namespace MonAmour.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? NotificationType { get; set; }

    public bool? IsRead { get; set; }

    public string? ActionUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual User User { get; set; } = null!;
}
