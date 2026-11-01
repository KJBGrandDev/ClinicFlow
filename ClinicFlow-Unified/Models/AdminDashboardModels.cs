#nullable enable

using System;

namespace ClinicFlow_Unified.Models;

public sealed class AdminOverviewStats
{
    public int TotalPatients { get; set; }
    public int TodayAppointments { get; set; }
    public int ActiveDoctors { get; set; }
    public int WaitingQueueCount { get; set; }
}

public sealed class AdminAuditItem
{
    public long AuditLogId { get; set; }
    public string PerformedBy { get; set; } = "System";
    public string ActionType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime LoggedAt { get; set; }
    public string TimeFormatted => LoggedAt.ToString("HH:mm");
}

