namespace ClinicFlow_Unified.Models;

public sealed class Doctor
{
    public int DoctorId { get; set; }

    public string DoctorCode { get; set; } = string.Empty;

    public int? UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = string.Empty;

    public string? Suffix { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Specialty { get; set; }

    public string? ContactNumber { get; set; }

    public string? EmailAddress { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}