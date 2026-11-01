#nullable enable

using System;

namespace ClinicFlow_Unified.Models;

public sealed class Patient
{
    public int PatientId { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? Suffix { get; set; }
    public string Sex { get; set; } = "Male";
    public DateTime BirthDate { get; set; } = new DateTime(1800, 1, 1);
    public int Age { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string? BlockAddress { get; set; }
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = "Davao City";
    public string Municipality { get; set; } = "Davao";
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }
    public string RowVersionHex => RowVersion != null ? Convert.ToHexString(RowVersion) : string.Empty;

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{LastName}, {FirstName} {Suffix}".Trim()
        : $"{LastName}, {FirstName} {MiddleName} {Suffix}".Trim();
}