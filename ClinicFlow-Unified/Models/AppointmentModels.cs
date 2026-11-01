#nullable enable

using System;

namespace ClinicFlow_Unified.Models;

public sealed class DoctorLookupItem
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;

    public string DisplayText => $"{DoctorName} ({Specialty})";
}

public sealed class PatientLookupItem
{
    public int PatientId { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public bool IsAlreadyBookedToday { get; set; }

    public string DisplayText => $"{PatientNumber} - {FullName} ({ContactNumber})";
}

public sealed class PatientBookingConflictItem
{
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int QueueNumber { get; set; }
    public string QueueStatus { get; set; } = string.Empty;
    public string ScheduledTime { get; set; } = string.Empty;
}

public sealed class TimeSlotItem
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsNextAvailable { get; set; }

    public string TimeRangeText => $"{DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";

    public string DisplayText
    {
        get
        {
            if (!IsAvailable)
                return $"[TAKEN]  {TimeRangeText}  (Booked)";
            if (IsNextAvailable)
                return $"[NEXT OPEN]  {TimeRangeText}  (Earliest Available)";
            return $"[AVAILABLE]  {TimeRangeText}";
        }
    }
}

public sealed class QueueBoardItem
{
    public int QueueId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int QueueNumber { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string ScheduledTime { get; set; } = string.Empty;
    public string QueueStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public byte[]? RowVersion { get; set; }
}

public sealed class DisplacedAppointmentItem
{
    public int AppointmentId { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientNumber { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan ScheduledStart { get; set; }
    public TimeSpan ScheduledEnd { get; set; }
    public string ScheduledTime => $"{DateTime.Today.Add(ScheduledStart):hh:mm tt} - {DateTime.Today.Add(ScheduledEnd):hh:mm tt}";
    public string DisplacementReason { get; set; } = string.Empty;
    public string ContactStatus { get; set; } = "Pending Outreach";
    public byte[]? RowVersion { get; set; }
}

public sealed class PeerDoctorAvailableSlotItem
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TimeDisplay => $"{DoctorName} ({Specialty}) | {DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
}

public sealed class DoctorNextAvailableSlotItem
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string TimeDisplay => $"{Date:MMM dd, yyyy (ddd)} | {DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
}