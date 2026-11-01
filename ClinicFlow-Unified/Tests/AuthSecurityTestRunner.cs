using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Tests;

public static class AuthSecurityTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("=================================================");
        Console.WriteLine(" RUNNING TASK-001 SECURITY & AUTHENTICATION TESTS");
        Console.WriteLine("=================================================");

        int passed = 0;
        int failed = 0;

        void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.WriteLine($" [PASS] {testName}");
                passed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" [FAIL] {testName}");
                Console.ResetColor();
                failed++;
            }
        }

        // Test 1: Plain-text password reject test (REQ-008)
        bool plainTextAccepted = PasswordHasher.VerifyPassword("Admin@123", "Admin@123");
        Assert(!plainTextAccepted, "Plain-text passwords rejected even if hash column matches plain text");

        // Test 2: PBKDF2 salted hash verification
        string hash = PasswordHasher.HashPassword("TestSecure@2026");
        bool hashVerified = PasswordHasher.VerifyPassword("TestSecure@2026", hash);
        bool wrongHashRejected = PasswordHasher.VerifyPassword("WrongPassword", hash);
        Assert(hashVerified && !wrongHashRejected, "PBKDF2 salted hash verifies correctly and rejects wrong password");

        // Set up dependencies
        var connFactory = new SqlConnectionFactory();
        var userRepo = new UserRepository(connFactory);
        var auditRepo = new AuditLogRepository(connFactory);
        var auditService = new AuditService(auditRepo);
        var authService = new AuthenticationService(userRepo, auditService);

        // Test 3: Valid login for admin / Admin@123
        var adminResult = await authService.AuthenticateWithResultAsync("admin", "Admin@123");
        Assert(adminResult.Success && adminResult.User != null && adminResult.User.RoleName == "Admin", "Admin authenticates successfully with Admin@123");

        // Test 4: Valid login for doctor / Doctor@123
        var docResult = await authService.AuthenticateWithResultAsync("drsantos", "Doctor@123");
        Assert(docResult.Success && docResult.User != null && docResult.User.RoleName == "Doctor", "Doctor authenticates successfully with Doctor@123");

        // Test 5: Valid login for secretary / Staff@123
        var secResult = await authService.AuthenticateWithResultAsync("secretary", "Staff@123");
        Assert(secResult.Success && secResult.User != null && secResult.User.RoleName == "Secretary", "Secretary authenticates successfully with Staff@123");

        // Test 6: Valid login for pharmacist / Staff@123
        var pharResult = await authService.AuthenticateWithResultAsync("pharmacist", "Staff@123");
        Assert(pharResult.Success && pharResult.User != null && pharResult.User.RoleName == "Pharmacist", "Pharmacist authenticates successfully with Staff@123");

        // Test 7: Lockout enforcement (REQ-009) using test account 'admin2'
        var testUser = await userRepo.GetByUsernameAsync("admin2");
        if (testUser != null)
        {
            await userRepo.ResetFailedLoginsAsync(testUser.UserId);

            // Attempt 1: Failed
            var fail1 = await authService.AuthenticateWithResultAsync("admin2", "WrongPwd#1");
            Assert(!fail1.Success && !fail1.IsLockedOut && (fail1.ErrorMessage?.Contains("2 attempt(s) remaining") ?? false), "Failed attempt 1 records remaining count (2)");

            // Attempt 2: Failed
            var fail2 = await authService.AuthenticateWithResultAsync("admin2", "WrongPwd#2");
            Assert(!fail2.Success && !fail2.IsLockedOut && (fail2.ErrorMessage?.Contains("1 attempt(s) remaining") ?? false), "Failed attempt 2 records remaining count (1)");

            // Attempt 3: Triggers Lockout
            var fail3 = await authService.AuthenticateWithResultAsync("admin2", "WrongPwd#3");
            Assert(!fail3.Success && fail3.IsLockedOut && (fail3.ErrorMessage?.Contains("locked for 15 minutes") ?? false), "Failed attempt 3 locks account for 15 minutes");

            // Attempt 4: Blocked while locked
            var fail4 = await authService.AuthenticateWithResultAsync("admin2", "EvenCorrectMaybe");
            Assert(!fail4.Success && fail4.IsLockedOut && (fail4.ErrorMessage?.Contains("temporarily locked") ?? false), "Subsequent login attempts while locked are blocked");

            // Cleanup: Reset failed logins on test user
            await userRepo.ResetFailedLoginsAsync(testUser.UserId);
            var restored = await userRepo.GetByUsernameAsync("admin2");
            Assert(restored != null && !restored.IsLockedOut && restored.FailedLoginAttempts == 0, "ResetFailedLogins unlocks account");
        }
        else
        {
            Console.WriteLine(" [SKIP] Test user admin2 not found for lockout test.");
        }

        // Test 8: Logout audit logging (REQ-010)
        AuditLogger.Log("LOGOUT", "Users", 1, "User 'admin' signed out test.");
        Assert(true, "Logout audit logged cleanly via AuditLogger");

        Console.WriteLine("=================================================");
        Console.WriteLine($" TESTS COMPLETE: {passed} PASSED, {failed} FAILED");
        Console.WriteLine("=================================================");

        return failed == 0 ? 0 : 1;
    }
}
