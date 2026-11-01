#nullable enable

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Tests;

public static class DashboardShellTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("CLINICFLOW DASHBOARD SHELL & IDENTITY BADGE TEST RUNNER");
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

        try
        {
            // Test 1: Admin session initializes persistent identity card with full name and ADMIN badge
            UserSession.Start(new User
            {
                UserId = 1,
                Username = "admin",
                FullName = "System Administrator",
                RoleId = 1,
                RoleName = "Admin"
            });

            using var dashboard = new MainDashboardForm();
            dashboard.UpdateIdentityBadge();

            bool t1Pass = dashboard.LoggedInUserFullName == "System Administrator" &&
                          dashboard.LoggedInUserRole == "ADMIN" &&
                          dashboard.LoggedInUserStatus.Contains("@admin");
            Report("Test 1: Admin session initializes persistent identity card with name and ADMIN badge", t1Pass,
                $"Name: '{dashboard.LoggedInUserFullName}', Role: '{dashboard.LoggedInUserRole}', Status: '{dashboard.LoggedInUserStatus}'");

            // Test 2: Doctor session updates identity card with Doctor name and DOCTOR role badge
            UserSession.Start(new User
            {
                UserId = 2,
                Username = "drsantos",
                FullName = "Dr. Maria Santos",
                RoleId = 2,
                RoleName = "Doctor"
            });
            dashboard.UpdateIdentityBadge();
            bool t2Pass = dashboard.LoggedInUserFullName == "Dr. Maria Santos" &&
                          dashboard.LoggedInUserRole == "DOCTOR" &&
                          dashboard.LoggedInUserStatus.Contains("@drsantos");
            Report("Test 2: Doctor session updates identity card with Doctor name and DOCTOR role badge", t2Pass,
                $"Name: '{dashboard.LoggedInUserFullName}', Role: '{dashboard.LoggedInUserRole}'");

            // Test 3: Secretary session updates identity card with Secretary name and SECRETARY role badge
            UserSession.Start(new User
            {
                UserId = 3,
                Username = "secretary",
                FullName = "Ana Reyes",
                RoleId = 3,
                RoleName = "Secretary"
            });
            dashboard.UpdateIdentityBadge();
            bool t3Pass = dashboard.LoggedInUserFullName == "Ana Reyes" &&
                          dashboard.LoggedInUserRole == "SECRETARY" &&
                          dashboard.LoggedInUserStatus.Contains("@secretary");
            Report("Test 3: Secretary session updates identity card with Secretary name and SECRETARY role badge", t3Pass,
                $"Name: '{dashboard.LoggedInUserFullName}', Role: '{dashboard.LoggedInUserRole}'");

            // Test 4: Pharmacist session updates identity card with Pharmacist name and PHARMACIST role badge
            UserSession.Start(new User
            {
                UserId = 4,
                Username = "pharmacist",
                FullName = "Carlos Dalisay, RPh",
                RoleId = 4,
                RoleName = "Pharmacist"
            });
            dashboard.UpdateIdentityBadge();
            bool t4Pass = dashboard.LoggedInUserFullName == "Carlos Dalisay, RPh" &&
                          dashboard.LoggedInUserRole == "PHARMACIST" &&
                          dashboard.LoggedInUserStatus.Contains("@pharmacist");
            Report("Test 4: Pharmacist session updates identity card with Pharmacist name and PHARMACIST role badge", t4Pass,
                $"Name: '{dashboard.LoggedInUserFullName}', Role: '{dashboard.LoggedInUserRole}'");

            // Test 5: Live summary indicators on overview dashboard >= 3 (Patients, Appointments, Doctors, Queue)
            var overviewRepo = new AdminDashboardRepository();
            var stats = overviewRepo.GetOverviewStats();
            bool t5Pass = stats != null &&
                          stats.TotalPatients >= 0 &&
                          stats.TodayAppointments >= 0 &&
                          stats.ActiveDoctors >= 0 &&
                          stats.WaitingQueueCount >= 0;
            Report("Test 5: Live summary indicators on overview dashboard >= 3 indicators", t5Pass,
                $"Stats: Patients={stats?.TotalPatients}, Appts={stats?.TodayAppointments}, Docs={stats?.ActiveDoctors}, Queue={stats?.WaitingQueueCount}");

            // Test 6: Navigation suite includes all required modules
            // Reset to Admin session for full module access check
            UserSession.Start(new User
            {
                UserId = 1,
                Username = "admin",
                FullName = "System Administrator",
                RoleId = 1,
                RoleName = "Admin"
            });
            using var adminDashboard = new MainDashboardForm();
            // Trigger Load to apply role visibility
            var loadMethod = typeof(MainDashboardForm).GetMethod("MainDashboardForm_Load",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadMethod?.Invoke(adminDashboard, new object?[] { adminDashboard, EventArgs.Empty });

            bool t6Pass = adminDashboard.HasReportsButton && adminDashboard.HasBackupButton &&
                          adminDashboard.IsReportsAllowed && adminDashboard.IsBackupAllowed;
            Report("Test 6: Navigation suite includes Reports & Analytics and Database Backup modules", t6Pass,
                $"HasReports={adminDashboard.HasReportsButton}, HasBackup={adminDashboard.HasBackupButton}, ReportsAllowed={adminDashboard.IsReportsAllowed}, BackupAllowed={adminDashboard.IsBackupAllowed}");

            // Test 7: Role visibility hides administrative modules (Backup) from Doctor session
            UserSession.Start(new User
            {
                UserId = 2,
                Username = "drsantos",
                FullName = "Dr. Maria Santos",
                RoleId = 2,
                RoleName = "Doctor"
            });
            using var doctorDashboard = new MainDashboardForm();
            loadMethod?.Invoke(doctorDashboard, new object?[] { doctorDashboard, EventArgs.Empty });

            bool t7Pass = !doctorDashboard.IsBackupAllowed; // Doctor cannot see database backup
            Report("Test 7: Role visibility properly hides administrative backup button from Doctor session", t7Pass,
                $"Doctor IsBackupAllowed: {doctorDashboard.IsBackupAllowed}");
        }
        catch (Exception ex)
        {
            Report("Unexpected Exception in Dashboard Test Suite", false, ex.ToString());
        }
        finally
        {
            UserSession.Clear();
        }

        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine($"DASHBOARD SHELL TESTS COMPLETE: {passed} Passed, {failed} Failed.");
        Console.WriteLine("===============================================================");

        return failed == 0 ? 0 : 1;
    }
}
