namespace ClinicFlow_Unified.Models;

public sealed class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? PasswordSalt { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutUntil { get; set; }

    public bool IsLockedOut => LockoutUntil.HasValue && LockoutUntil.Value > DateTime.UtcNow;
}