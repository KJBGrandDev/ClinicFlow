#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Tests;

public static class DatabaseBackupRestoreTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("CLINICFLOW DATABASE BACKUP & RESTORE UTILITY TEST RUNNER");
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

        var maintenanceService = new DatabaseMaintenanceService();
        string? generatedBackupPath = null;

        try
        {
            // Test 1: Non-admin session is blocked from BackupDatabaseAsync
            UserSession.Start(new User
            {
                UserId = 2,
                Username = "drsantos",
                FullName = "Dr. Maria Santos",
                RoleId = 2,
                RoleName = "Doctor"
            });

            bool t1Pass = false;
            try
            {
                await maintenanceService.BackupDatabaseAsync();
            }
            catch (UnauthorizedAccessException)
            {
                t1Pass = true;
            }
            Report("Test 1: Non-admin session is blocked from BackupDatabaseAsync", t1Pass);

            // Test 2: Non-admin session is blocked from RestoreDatabaseAsync
            bool t2Pass = false;
            try
            {
                await maintenanceService.RestoreDatabaseAsync("dummy.bak");
            }
            catch (UnauthorizedAccessException)
            {
                t2Pass = true;
            }
            Report("Test 2: Non-admin session is blocked from RestoreDatabaseAsync", t2Pass);

            // Test 3: Non-admin session is blocked from DatabaseBackupRestoreDialog constructor
            bool t3Pass = false;
            try
            {
                using var dialog = new DatabaseBackupRestoreDialog();
            }
            catch (UnauthorizedAccessException)
            {
                t3Pass = true;
            }
            // Reset synchronization context so UI control instantiation does not trap async continuations in CLI mode
            System.Threading.SynchronizationContext.SetSynchronizationContext(null);
            Report("Test 3: Non-admin session is blocked from opening DatabaseBackupRestoreDialog", t3Pass);

            // Switch to Admin Session
            UserSession.Start(new User
            {
                UserId = 1,
                Username = "admin",
                FullName = "System Administrator",
                RoleId = 1,
                RoleName = "Admin"
            });

            // Test 4: Admin session generates full database backup to temporary .bak file
            string backupDir = maintenanceService.GetDefaultBackupDirectory();
            string testBackupFile = Path.Combine(backupDir, $"ClinicFlow_TestBackup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");

            var (success, message, createdPath) = await maintenanceService.BackupDatabaseAsync(testBackupFile);
            generatedBackupPath = createdPath;

            bool t4Pass = success && !string.IsNullOrWhiteSpace(createdPath) && File.Exists(createdPath);
            Report("Test 4: Admin session successfully executes database backup to .bak file", t4Pass,
                $"Result: {message}");

            // Test 5: Backup file has valid size on disk (> 1 MB for SQL Server)
            bool t5Pass = false;
            long fileSize = 0;
            if (File.Exists(generatedBackupPath))
            {
                fileSize = new FileInfo(generatedBackupPath).Length;
                t5Pass = fileSize > 1024 * 1024; // > 1 MB
            }
            Report("Test 5: Generated .bak file exists on disk with valid file size (> 1MB)", t5Pass,
                $"Size: {fileSize:N0} bytes");

            // Test 6: Verify backup file structure via RESTORE VERIFYONLY
            var (verifySuccess, verifyMsg) = await maintenanceService.VerifyBackupAsync(generatedBackupPath!);
            bool t6Pass = verifySuccess;
            Report("Test 6: Admin session verifies backup file structure using RESTORE VERIFYONLY", t6Pass,
                $"VerifyResult: {verifyMsg}");

            // Test 7: Nonexistent backup file restore returns clean error without crashing
            string fakePath = Path.Combine(backupDir, $"Nonexistent_{Guid.NewGuid():N}.bak");
            var (fakeSuccess, fakeMsg) = await maintenanceService.RestoreDatabaseAsync(fakePath);
            bool t7Pass = !fakeSuccess && fakeMsg.Contains("not exist", StringComparison.OrdinalIgnoreCase);
            Report("Test 7: Attempt to restore nonexistent backup path returns clean error handling", t7Pass,
                $"FakeMsg: {fakeMsg}");

            // Test 8: Audit log captures BACKUP action in dbo.AuditLogs
            var connFactory = new SqlConnectionFactory();
            var auditRepo = new AuditLogRepository(connFactory);
            var recentLogs = await auditRepo.SearchLogsAsync(actionType: "BACKUP", fromDate: DateTime.Today.AddDays(-1), toDate: DateTime.Today.AddDays(1));
            bool t8Pass = recentLogs != null && recentLogs.Count > 0 &&
                          recentLogs.Any(l => l.ActionType == "BACKUP");
            Report("Test 8: Database backup action is recorded in dbo.AuditLogs", t8Pass,
                $"Recent BACKUP logs: {recentLogs?.Count}");
        }
        catch (Exception ex)
        {
            Report("Unexpected Exception in Backup/Restore Test Suite", false, ex.ToString());
        }
        finally
        {
            // Clean up temporary backup test file
            if (!string.IsNullOrWhiteSpace(generatedBackupPath) && File.Exists(generatedBackupPath))
            {
                try
                {
                    File.Delete(generatedBackupPath);
                }
                catch
                {
                    // Suppress if locked
                }
            }

            UserSession.Clear();
        }

        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine($"DATABASE BACKUP & RESTORE TESTS COMPLETE: {passed} Passed, {failed} Failed.");
        Console.WriteLine("===============================================================");

        return failed == 0 ? 0 : 1;
    }
}
