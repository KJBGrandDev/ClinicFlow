namespace ClinicFlow_Unified.Models;

public sealed class AuditLogEntry
{
    public long AuditLogId { get; set; }

    public int? UserId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? RecordId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? MachineName { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public DateTime LoggedAt { get; set; }
}