using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Services;

public sealed class UserManagementService
{
    private readonly UserRepository _userRepository;
    private readonly RoleRepository _roleRepository;
    private readonly AuditService _auditService;

    public UserManagementService(
        UserRepository userRepository,
        RoleRepository roleRepository,
        AuditService auditService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditService = auditService;
    }

    public async Task<int> CreateInitialAdminAsync(
        string username,
        string password,
        string fullName)
    {
        if (await _userRepository.AnyUsersExistAsync())
        {
            throw new InvalidOperationException(
                "The initial administrator can be created only when no users exist.");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException(
                "A username is required.",
                nameof(username));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException(
                "A full name is required.",
                nameof(fullName));
        }

        if (password.Length < 12)
        {
            throw new ArgumentException(
                "The password must contain at least 12 characters.",
                nameof(password));
        }

        Role? adminRole = await _roleRepository.GetActiveByNameAsync("Admin");

        if (adminRole is null)
        {
            throw new InvalidOperationException(
                "The Admin role was not found. Run 003_SeedRoles.sql first.");
        }

        User adminUser = new User
        {
            Username = username.Trim(),
            PasswordHash = PasswordHasher.HashPassword(password),
            FullName = fullName.Trim(),
            RoleId = adminRole.RoleId
        };

        int newUserId = await _userRepository.CreateAsync(adminUser);

        await _auditService.LogAsync(
            newUserId,
            "CREATE",
            "Users",
            newUserId.ToString(),
            $"Initial Admin account '{adminUser.Username}' was created.");

        return newUserId;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        UserSession.RequireRole("Admin");
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<int> CreateUserAsync(
        string username,
        string password,
        string fullName,
        int roleId)
    {
        UserSession.RequireRole("Admin");

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("A username is required.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("A full name is required.", nameof(fullName));
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new ArgumentException("Password must contain at least 6 characters.", nameof(password));
        }

        var existing = await _userRepository.GetByUsernameAsync(username.Trim());
        if (existing != null)
        {
            throw new InvalidOperationException($"Username '{username.Trim()}' is already taken.");
        }

        (string hash, string salt) = PasswordHasher.HashPasswordWithSalt(password);
        var user = new User
        {
            Username = username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            FullName = fullName.Trim(),
            RoleId = roleId
        };

        int newUserId = await _userRepository.CreateAsync(user);

        await _auditService.LogAsync(
            UserSession.UserId ?? newUserId,
            "CREATE",
            "Users",
            newUserId.ToString(),
            $"User account '{user.Username}' (Role ID {roleId}) was created.");

        return newUserId;
    }

    public async Task SetUserActiveStatusAsync(int userId, bool isActive)
    {
        UserSession.RequireRole("Admin");

        if (UserSession.UserId.HasValue && UserSession.UserId.Value == userId && !isActive)
        {
            throw new InvalidOperationException("You cannot deactivate your own active administrator account.");
        }

        await _userRepository.UpdateUserStatusAsync(userId, isActive);

        string action = isActive ? "REACTIVATE" : "DEACTIVATE";
        await _auditService.LogAsync(
            UserSession.UserId ?? 0,
            action,
            "Users",
            userId.ToString(),
            $"User account ID {userId} active status changed to {isActive}.");
    }

    public async Task ResetPasswordAsync(int userId, string newPassword)
    {
        UserSession.RequireRole("Admin");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            throw new ArgumentException("Password must contain at least 6 characters.", nameof(newPassword));
        }

        (string hash, string salt) = PasswordHasher.HashPasswordWithSalt(newPassword);
        await _userRepository.UpdatePasswordAsync(userId, hash, salt);

        await _auditService.LogAsync(
            UserSession.UserId ?? 0,
            "PASSWORD_RESET",
            "Users",
            userId.ToString(),
            $"Password reset by Administrator for user ID {userId}.");
    }

    public async Task UpdateUserDetailsAsync(int userId, string fullName, int roleId)
    {
        UserSession.RequireRole("Admin");

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("A full name is required.", nameof(fullName));
        }

        await _userRepository.UpdateUserDetailsAsync(userId, fullName, roleId);

        await _auditService.LogAsync(
            UserSession.UserId ?? 0,
            "UPDATE",
            "Users",
            userId.ToString(),
            $"User account ID {userId} details updated (Name: '{fullName.Trim()}', Role ID: {roleId}).");
    }

    public async Task<List<Role>> GetActiveRolesAsync()
    {
        UserSession.RequireRole("Admin");
        return await _roleRepository.GetAllActiveRolesAsync();
    }
}