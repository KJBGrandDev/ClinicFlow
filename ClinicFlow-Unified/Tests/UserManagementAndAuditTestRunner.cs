#nullable enable

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Tests;

public static class UserManagementAndAuditTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("CLINICFLOW USER MANAGEMENT & AUDIT LOG VIEWER TEST RUNNER");
        Console.WriteLine("===============================================================");

        int passed = 0;
        int failed = 0;

        void Report(string testName, bool success, string detail = "")
        {
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASS] {testName}");
                Console.ResetColor();
                passed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName} - {detail}");
                Console.ResetColor();
                failed++;
            }
        }

        // Setup Admin Session
        UserSession.Start(new User
        {
            UserId = 1,
            Username = "admin",
            RoleName = "Admin",
            FullName = "System Administrator",
            RoleId = 1
        });

        var connFactory = new SqlConnectionFactory();
        var userRepo = new UserRepository(connFactory);
        var roleRepo = new RoleRepository(connFactory);
        var auditRepo = new AuditLogRepository(connFactory);
        var auditService = new AuditService(auditRepo);
        var userService = new UserManagementService(userRepo, roleRepo, auditService);

        int testUserId = 0;
        string testUsername = $"testuser_{Guid.NewGuid():N}"[..18];

        try
        {
            // Test 1: UserManagementService creates new user with salted PBKDF2 hash
            var roles = await userService.GetActiveRolesAsync();
            int doctorRoleId = roles.FirstOrDefault(r => r.RoleName == "Doctor")?.RoleId ?? 2;

            testUserId = await userService.CreateUserAsync(testUsername, "InitialPass@123", "Test Managed User", doctorRoleId);
            var created = await userRepo.GetByUsernameAsync(testUsername);

            if (created != null && created.UserId == testUserId &&
                !string.IsNullOrWhiteSpace(created.PasswordSalt) &&
                PasswordHasher.VerifyPassword("InitialPass@123", created.PasswordHash, created.PasswordSalt))
            {
                Report("Test 1: UserManagementService creates new user with salted PBKDF2 hash", true);
            }
            else
            {
                Report("Test 1: UserManagementService creates new user with salted PBKDF2 hash", false, "User not found or password hash invalid");
            }

            // Test 2: UserManagementService updates user details (full name & role)
            if (testUserId > 0)
            {
                int secRoleId = roles.FirstOrDefault(r => r.RoleName == "Secretary")?.RoleId ?? 3;
                await userService.UpdateUserDetailsAsync(testUserId, "Updated Full Name", secRoleId);

                var updated = await userRepo.GetByUsernameAsync(testUsername);
                if (updated != null && updated.FullName == "Updated Full Name" && updated.RoleId == secRoleId)
                {
                    Report("Test 2: UserManagementService updates user details (name & role)", true);
                }
                else
                {
                    Report("Test 2: UserManagementService updates user details (name & role)", false, $"Name={updated?.FullName}, Role={updated?.RoleId}");
                }
            }
            else
            {
                Report("Test 2: UserManagementService updates user details (name & role)", false, "Prerequisite Test 1 failed");
            }

            // Test 3: UserManagementService deactivates and reactivates user
            if (testUserId > 0)
            {
                await userService.SetUserActiveStatusAsync(testUserId, false);
                var deactivated = await userRepo.GetByUsernameAsync(testUsername);

                await userService.SetUserActiveStatusAsync(testUserId, true);
                var reactivated = await userRepo.GetByUsernameAsync(testUsername);

                if (deactivated != null && !deactivated.IsActive && reactivated != null && reactivated.IsActive)
                {
                    Report("Test 3: UserManagementService deactivates and reactivates user", true);
                }
                else
                {
                    Report("Test 3: UserManagementService deactivates and reactivates user", false, $"Deactivated={deactivated?.IsActive}, Reactivated={reactivated?.IsActive}");
                }
            }
            else
            {
                Report("Test 3: UserManagementService deactivates and reactivates user", false, "Prerequisite Test 1 failed");
            }

            // Test 4: UserManagementService self-deactivation protection
            try
            {
                await userService.SetUserActiveStatusAsync(1, false);
                Report("Test 4: UserManagementService blocks self-deactivation of current admin", false, "Did not throw");
            }
            catch (InvalidOperationException)
            {
                Report("Test 4: UserManagementService blocks self-deactivation of current admin", true);
            }
            catch (Exception ex)
            {
                Report("Test 4: UserManagementService blocks self-deactivation of current admin", false, $"Wrong exception: {ex.GetType().Name}");
            }

            // Test 5: UserManagementService resets password with new salt
            if (testUserId > 0)
            {
                await userService.ResetPasswordAsync(testUserId, "BrandNewSecret@999");
                var resetUser = await userRepo.GetByUsernameAsync(testUsername);

                if (resetUser != null && PasswordHasher.VerifyPassword("BrandNewSecret@999", resetUser.PasswordHash, resetUser.PasswordSalt))
                {
                    Report("Test 5: UserManagementService resets password with new salt", true);
                }
                else
                {
                    Report("Test 5: UserManagementService resets password with new salt", false, "New password did not verify");
                }
            }
            else
            {
                Report("Test 5: UserManagementService resets password with new salt", false, "Prerequisite Test 1 failed");
            }

            // Test 6: AuditLogRepository inserts and searches audit logs
            var logs = await auditRepo.SearchLogsAsync(actionType: "PASSWORD_RESET", top: 100);
            bool foundResetLog = logs.Any(l => l.Description.Contains($"user ID {testUserId}"));
            Report("Test 6: AuditLogRepository records and retrieves PASSWORD_RESET event", foundResetLog,
                foundResetLog ? "" : "PASSWORD_RESET audit log entry not found");

            // Test 7: AuditLogRepository filters by search pattern
            var searchedLogs = await auditRepo.SearchLogsAsync(search: "Administrator", top: 50);
            bool searchWorked = searchedLogs.Count > 0;
            Report("Test 7: AuditLogRepository filters by search pattern", searchWorked,
                $"Returned {searchedLogs.Count} records");

            // Test 8: UserManagementForm and AuditLogViewerForm enforce Admin RBAC
            UserSession.Start(new User { UserId = 99, Username = "nurse", RoleName = "Secretary", FullName = "Front Desk", RoleId = 3 });

            bool userMgmtBlocked = false;
            try
            {
                _ = new UserManagementForm();
            }
            catch (UnauthorizedAccessException)
            {
                userMgmtBlocked = true;
            }

            bool auditViewerBlocked = false;
            try
            {
                _ = new AuditLogViewerForm();
            }
            catch (UnauthorizedAccessException)
            {
                auditViewerBlocked = true;
            }

            Report("Test 8: UserManagementForm and AuditLogViewerForm enforce Admin RBAC",
                userMgmtBlocked && auditViewerBlocked,
                $"userMgmtBlocked={userMgmtBlocked}, auditViewerBlocked={auditViewerBlocked}");
        }
        finally
        {
            // Clean up test user and its audit logs
            if (testUserId > 0)
            {
                try
                {
                    using var conn = DatabaseConnection.CreateConnection();
                    using var cmd = new SqlCommand("DELETE FROM dbo.AuditLogs WHERE RecordId = @Uid AND EntityName = 'Users'; DELETE FROM dbo.Users WHERE UserId = @Uid;", conn);
                    cmd.Parameters.Add("@Uid", SqlDbType.Int).Value = testUserId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
            UserSession.Clear();
        }

        Console.WriteLine();
        Console.WriteLine($"USER MANAGEMENT & AUDIT TESTS COMPLETE: {passed} Passed, {failed} Failed.");
        Console.WriteLine("===============================================================");

        return failed == 0 ? 0 : 1;
    }
}
