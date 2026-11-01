#nullable enable

using System;

namespace ClinicFlow_Unified.Models;

public sealed class AuditLogViewItem
{
    public long AuditLogId { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; } = "System";
    public string UserFullName { get; set; } = "System Service";
    public string ActionType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? MachineName { get; set; }
    public DateTime LoggedAt { get; set; }
}
