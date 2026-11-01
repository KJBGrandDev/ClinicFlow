namespace ClinicFlow_Unified.Models;

public sealed class DoctorSchedule
{
    public int DoctorScheduleId { get; set; }

    public int DoctorId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public short SlotDurationMinutes { get; set; }

    public short MaxPatientsPerSlot { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}