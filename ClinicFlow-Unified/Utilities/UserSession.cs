#nullable enable

using ClinicFlow_Unified.Models;

namespace ClinicFlow_Unified.Utilities;

public static class UserSession
{
    public static int? UserId { get; private set; }
    public static int? RoleId { get; private set; }
    public static string RoleName { get; private set; } = string.Empty;
    public static string FullName { get; private set; } = string.Empty;
    public static string Username { get; private set; } = string.Empty;

    // Doctor specific session mapping
    public static int? DoctorId { get; set; }
    public static string? DoctorSpecialty { get; set; }

    // Secretary specific session mapping
    public static int? SecretaryId { get; set; }
    public static string? SecretaryDesk { get; set; }

    // Pharmacist specific session mapping
    public static int? PharmacistId { get; set; }
    public static string? PharmacistLicense { get; set; }

    public static bool IsAuthenticated => UserId.HasValue;

    public static void Start(User user)
    {
        UserId = user.UserId;
        RoleId = user.RoleId;
        RoleName = user.RoleName;
        FullName = user.FullName;
        Username = user.Username;
    }

    public static void SetActiveDoctor(int doctorId, string doctorName, string specialty)
    {
        DoctorId = doctorId;
        FullName = doctorName;
        DoctorSpecialty = specialty;
    }

    public static void SetActiveSecretary(int secretaryId, string secretaryName, string assignedDesk)
    {
        SecretaryId = secretaryId;
        FullName = secretaryName;
        SecretaryDesk = assignedDesk;
    }

    public static void SetActivePharmacist(int pharmacistId, string pharmacistName, string licenseNumber)
    {
        PharmacistId = pharmacistId;
        FullName = pharmacistName;
        PharmacistLicense = licenseNumber;
    }

    public static bool HasRole(params string[] allowedRoles)
    {
        if (!IsAuthenticated || string.IsNullOrWhiteSpace(RoleName))
        {
            return false;
        }

        if (allowedRoles == null || allowedRoles.Length == 0)
        {
            return true;
        }

        foreach (var role in allowedRoles)
        {
            if (string.Equals(RoleName, role, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static void RequireRole(params string[] allowedRoles)
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Action requires an authenticated user session.");
        }

        if (!HasRole(allowedRoles))
        {
            string userDisplay = !string.IsNullOrWhiteSpace(Username) ? Username : (FullName ?? "User");
            string currentRole = !string.IsNullOrWhiteSpace(RoleName) ? RoleName : "Unknown";
            string required = (allowedRoles != null && allowedRoles.Length > 0)
                ? string.Join(", ", allowedRoles)
                : "Authorized Role";

            throw new UnauthorizedAccessException(
                $"Access Denied: User '{userDisplay}' with role '{currentRole}' is not authorized to access this module or perform this operation. Required role: {required}.");
        }
    }

    public static bool IsAdmin => HasRole("Admin");
    public static bool IsDoctor => HasRole("Doctor");
    public static bool IsSecretary => HasRole("Secretary");
    public static bool IsPharmacist => HasRole("Pharmacist");

    public static void Clear()
    {
        UserId = null;
        RoleId = null;
        RoleName = string.Empty;
        FullName = string.Empty;
        Username = string.Empty;
        DoctorId = null;
        DoctorSpecialty = null;
        SecretaryId = null;
        SecretaryDesk = null;
        PharmacistId = null;
        PharmacistLicense = null;
    }
}
