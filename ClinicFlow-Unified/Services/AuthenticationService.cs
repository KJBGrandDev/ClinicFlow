using System;
using System.Threading.Tasks;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Services;

public sealed class AuthenticationResult
{
    public bool Success { get; init; }
    public User? User { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsLockedOut { get; init; }
}

public sealed class AuthenticationService
{
    public const int MaxConsecutiveFailedAttempts = 3;
    public const int LockoutDurationMinutes = 15;

    private readonly UserRepository _userRepository;
    private readonly AuditService _auditService;

    public AuthenticationService(
        UserRepository userRepository,
        AuditService auditService)
    {
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<AuthenticationResult> AuthenticateWithResultAsync(
        string username,
        string password)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Please enter both username and password."
            };
        }

        string normalizedUsername = username.Trim();

        User? user = await _userRepository.GetByUsernameAsync(normalizedUsername);

        if (user is null || !user.IsActive)
        {
            await _auditService.LogAsync(
                null,
                "LOGIN_FAILED",
                "Users",
                null,
                $"Failed login attempt for unknown or inactive username '{normalizedUsername}'.");

            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Invalid username or password."
            };
        }

        // Check if account is currently locked out
        if (user.IsLockedOut)
        {
            int remainingMinutes = Math.Max(1, (int)Math.Ceiling((user.LockoutUntil!.Value - DateTime.UtcNow).TotalMinutes));

            await _auditService.LogAsync(
                user.UserId,
                "LOGIN_LOCKED",
                "Users",
                user.UserId.ToString(),
                $"Login rejected for locked account '{normalizedUsername}'. Remaining lockout: {remainingMinutes}m.");

            return new AuthenticationResult
            {
                Success = false,
                IsLockedOut = true,
                ErrorMessage = $"Account is temporarily locked due to consecutive failed login attempts. Please try again in {remainingMinutes} minute(s)."
            };
        }

        // Verify password using PBKDF2 with salt
        bool passwordIsValid = PasswordHasher.VerifyPassword(
            password,
            user.PasswordHash,
            user.PasswordSalt);

        if (!passwordIsValid)
        {
            await _userRepository.RecordFailedLoginAsync(user.UserId);
            int newAttempts = user.FailedLoginAttempts + 1;

            if (newAttempts >= MaxConsecutiveFailedAttempts)
            {
                await _auditService.LogAsync(
                    user.UserId,
                    "ACCOUNT_LOCKED",
                    "Users",
                    user.UserId.ToString(),
                    $"Account '{normalizedUsername}' was locked for {LockoutDurationMinutes} minutes after {newAttempts} consecutive failed login attempts.");

                return new AuthenticationResult
                {
                    Success = false,
                    IsLockedOut = true,
                    ErrorMessage = $"Account has been locked for {LockoutDurationMinutes} minutes due to {newAttempts} consecutive failed login attempts."
                };
            }

            int remainingAttempts = MaxConsecutiveFailedAttempts - newAttempts;

            await _auditService.LogAsync(
                user.UserId,
                "LOGIN_FAILED",
                "Users",
                user.UserId.ToString(),
                $"Failed login attempt ({newAttempts}/{MaxConsecutiveFailedAttempts}) for user '{normalizedUsername}'.");

            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = $"Invalid username or password. ({remainingAttempts} attempt(s) remaining before account lockout)."
            };
        }

        // Successful authentication: Reset failed counter if needed
        if (user.FailedLoginAttempts > 0)
        {
            await _userRepository.ResetFailedLoginsAsync(user.UserId);
        }

        await _auditService.LogAsync(
            user.UserId,
            "LOGIN_SUCCESS",
            "Users",
            user.UserId.ToString(),
            $"User '{normalizedUsername}' signed in successfully.");

        return new AuthenticationResult
        {
            Success = true,
            User = user
        };
    }

    public async Task<User?> AuthenticateAsync(
        string username,
        string password)
    {
        var result = await AuthenticateWithResultAsync(username, password);
        return result.User;
    }
}