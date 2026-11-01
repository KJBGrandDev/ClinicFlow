#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Tests;

public static class RbacSecurityTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("CLINICFLOW RBAC & SERVICE-LEVEL GUARDS VERIFICATION TEST RUNNER");
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

        // Setup mock sessions
        User adminUser = new() { UserId = 1, Username = "admin", RoleName = "Admin", FullName = "System Administrator", RoleId = 1 };
        User doctorUser = new() { UserId = 2, Username = "drsantos", RoleName = "Doctor", FullName = "Dr. Santos", RoleId = 2 };
        User secretaryUser = new() { UserId = 3, Username = "secretary", RoleName = "Secretary", FullName = "Front Desk Secretary", RoleId = 3 };
        User pharmacistUser = new() { UserId = 4, Username = "pharmacist", RoleName = "Pharmacist", FullName = "Central Pharmacist", RoleId = 4 };

        // Test 1: Unauthenticated session throws on RequireRole
        try
        {
            UserSession.Clear();
            UserSession.RequireRole("Admin");
            Report("Test 1: Unauthenticated session throws UnauthorizedAccessException on RequireRole", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 1: Unauthenticated session throws UnauthorizedAccessException on RequireRole", true);
        }
        catch (Exception ex)
        {
            Report("Test 1: Unauthenticated session throws UnauthorizedAccessException on RequireRole", false, ex.Message);
        }

        // Test 2: Unauthenticated session HasRole returns false
        try
        {
            UserSession.Clear();
            bool hasAdmin = UserSession.HasRole("Admin");
            bool hasAny = UserSession.HasRole();
            Report("Test 2: Unauthenticated session HasRole returns false", !hasAdmin && !hasAny);
        }
        catch (Exception ex)
        {
            Report("Test 2: Unauthenticated session HasRole returns false", false, ex.Message);
        }

        // Test 3: Doctor session accessing Admin-only throws UnauthorizedAccessException
        try
        {
            UserSession.Clear();
            UserSession.Start(doctorUser);
            UserSession.RequireRole("Admin");
            Report("Test 3: Doctor session throws UnauthorizedAccessException for Admin requirement", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 3: Doctor session throws UnauthorizedAccessException for Admin requirement", true);
        }
        catch (Exception ex)
        {
            Report("Test 3: Doctor session throws UnauthorizedAccessException for Admin requirement", false, ex.Message);
        }

        // Test 4: Admin session passes Admin requirement
        try
        {
            UserSession.Clear();
            UserSession.Start(adminUser);
            UserSession.RequireRole("Admin");
            Report("Test 4: Admin session successfully passes Admin requirement", true);
        }
        catch (Exception ex)
        {
            Report("Test 4: Admin session successfully passes Admin requirement", false, ex.Message);
        }

        // Test 5: Multi-role allowed (Doctor, Admin) permits Doctor
        try
        {
            UserSession.Clear();
            UserSession.Start(doctorUser);
            UserSession.RequireRole("Doctor", "Admin");
            Report("Test 5: Multi-role requirement ('Doctor', 'Admin') permits Doctor session", true);
        }
        catch (Exception ex)
        {
            Report("Test 5: Multi-role requirement ('Doctor', 'Admin') permits Doctor session", false, ex.Message);
        }

        // Test 6: Multi-role allowed (Doctor, Admin) blocks Secretary
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            UserSession.RequireRole("Doctor", "Admin");
            Report("Test 6: Multi-role requirement ('Doctor', 'Admin') blocks Secretary session", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 6: Multi-role requirement ('Doctor', 'Admin') blocks Secretary session", true);
        }
        catch (Exception ex)
        {
            Report("Test 6: Multi-role requirement ('Doctor', 'Admin') blocks Secretary session", false, ex.Message);
        }

        // Test 7: Role convenience properties reflect active session correctly
        try
        {
            UserSession.Clear();
            UserSession.Start(pharmacistUser);
            bool ok = UserSession.IsPharmacist && !UserSession.IsAdmin && !UserSession.IsDoctor && !UserSession.IsSecretary;
            Report("Test 7: Role convenience properties (IsPharmacist, etc.) reflect active session", ok);
        }
        catch (Exception ex)
        {
            Report("Test 7: Role convenience properties (IsPharmacist, etc.) reflect active session", false, ex.Message);
        }

        // Test 8: DoctorManagementForm constructor blocks non-Admin (Secretary)
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            _ = new DoctorManagementForm();
            Report("Test 8: DoctorManagementForm constructor blocks non-Admin (Secretary)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 8: DoctorManagementForm constructor blocks non-Admin (Secretary)", true);
        }
        catch (Exception ex)
        {
            Report("Test 8: DoctorManagementForm constructor blocks non-Admin (Secretary)", false, ex.Message);
        }

        // Test 9: SecretaryManagementForm constructor blocks non-Admin (Doctor)
        try
        {
            UserSession.Clear();
            UserSession.Start(doctorUser);
            _ = new SecretaryManagementForm();
            Report("Test 9: SecretaryManagementForm constructor blocks non-Admin (Doctor)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 9: SecretaryManagementForm constructor blocks non-Admin (Doctor)", true);
        }
        catch (Exception ex)
        {
            Report("Test 9: SecretaryManagementForm constructor blocks non-Admin (Doctor)", false, ex.Message);
        }

        // Test 10: PharmacistManagementForm constructor blocks non-Admin (Secretary)
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            _ = new PharmacistManagementForm();
            Report("Test 10: PharmacistManagementForm constructor blocks non-Admin (Secretary)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 10: PharmacistManagementForm constructor blocks non-Admin (Secretary)", true);
        }
        catch (Exception ex)
        {
            Report("Test 10: PharmacistManagementForm constructor blocks non-Admin (Secretary)", false, ex.Message);
        }

        // Test 11: DoctorConsultationService.StartConsultation blocks Secretary
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            DoctorConsultationService consultationService = new();
            consultationService.StartConsultation(999);
            Report("Test 11: DoctorConsultationService.StartConsultation blocks Secretary", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 11: DoctorConsultationService.StartConsultation blocks Secretary", true);
        }
        catch (Exception ex)
        {
            Report("Test 11: DoctorConsultationService.StartConsultation blocks Secretary", false, ex.Message);
        }

        // Test 12: DoctorConsultationService.CompleteConsultation blocks Secretary
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            DoctorConsultationService consultationService = new();
            consultationService.CompleteConsultation(1, 1, 1, "Fever", "Notes", "Flu", "Rest", []);
            Report("Test 12: DoctorConsultationService.CompleteConsultation blocks Secretary", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 12: DoctorConsultationService.CompleteConsultation blocks Secretary", true);
        }
        catch (Exception ex)
        {
            Report("Test 12: DoctorConsultationService.CompleteConsultation blocks Secretary", false, ex.Message);
        }

        // Test 13: DoctorConsultationService.DeferPatient blocks Secretary
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            DoctorConsultationService consultationService = new();
            consultationService.DeferPatient(1, 2);
            Report("Test 13: DoctorConsultationService.DeferPatient blocks Secretary", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 13: DoctorConsultationService.DeferPatient blocks Secretary", true);
        }
        catch (Exception ex)
        {
            Report("Test 13: DoctorConsultationService.DeferPatient blocks Secretary", false, ex.Message);
        }

        // Test 14: DoctorConsultationService.EmergencyPostpone blocks Secretary
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            DoctorConsultationService consultationService = new();
            consultationService.EmergencyPostpone(1);
            Report("Test 14: DoctorConsultationService.EmergencyPostpone blocks Secretary", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 14: DoctorConsultationService.EmergencyPostpone blocks Secretary", true);
        }
        catch (Exception ex)
        {
            Report("Test 14: DoctorConsultationService.EmergencyPostpone blocks Secretary", false, ex.Message);
        }

        // Test 15: AppointmentService.CheckInPatient blocks Pharmacist
        try
        {
            UserSession.Clear();
            UserSession.Start(pharmacistUser);
            AppointmentService appointmentService = new();
            appointmentService.CheckInPatient(1, 1, 1);
            Report("Test 15: AppointmentService.CheckInPatient blocks Pharmacist", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 15: AppointmentService.CheckInPatient blocks Pharmacist", true);
        }
        catch (Exception ex)
        {
            Report("Test 15: AppointmentService.CheckInPatient blocks Pharmacist", false, ex.Message);
        }

        // Test 16: DoctorRepository.RegisterDoctorWithAccount blocks non-Admin
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            DoctorRepository docRepo = new();
            docRepo.RegisterDoctorWithAccount("Test", "Doc", "LIC123", "Pedia", "09123456789", "testdoc", "Doc@123456");
            Report("Test 16: DoctorRepository.RegisterDoctorWithAccount blocks non-Admin (Secretary)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 16: DoctorRepository.RegisterDoctorWithAccount blocks non-Admin (Secretary)", true);
        }
        catch (Exception ex)
        {
            Report("Test 16: DoctorRepository.RegisterDoctorWithAccount blocks non-Admin (Secretary)", false, ex.Message);
        }

        // Test 17: SecretaryRepository.RegisterSecretaryWithAccount blocks non-Admin
        try
        {
            UserSession.Clear();
            UserSession.Start(doctorUser);
            SecretaryRepository secRepo = new();
            secRepo.RegisterSecretaryWithAccount("Test", "Sec", "09123456789", "Desk A", "sec@clinic.com", "testsec", "Sec@123456");
            Report("Test 17: SecretaryRepository.RegisterSecretaryWithAccount blocks non-Admin (Doctor)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 17: SecretaryRepository.RegisterSecretaryWithAccount blocks non-Admin (Doctor)", true);
        }
        catch (Exception ex)
        {
            Report("Test 17: SecretaryRepository.RegisterSecretaryWithAccount blocks non-Admin (Doctor)", false, ex.Message);
        }

        // Test 18: PharmacyRepository.SavePharmacist blocks non-Admin
        try
        {
            UserSession.Clear();
            UserSession.Start(doctorUser);
            PharmacyRepository pharRepo = new();
            pharRepo.SavePharmacist(new PharmacistRecord { PharmacistId = 0, FirstName = "Phar", LastName = "Test", Username = "phartest" }, "Phar@123456");
            Report("Test 18: PharmacyRepository.SavePharmacist blocks non-Admin (Doctor)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 18: PharmacyRepository.SavePharmacist blocks non-Admin (Doctor)", true);
        }
        catch (Exception ex)
        {
            Report("Test 18: PharmacyRepository.SavePharmacist blocks non-Admin (Doctor)", false, ex.Message);
        }

        // Test 19: UserManagementService.CreateUserAsync blocks non-Admin
        try
        {
            UserSession.Clear();
            UserSession.Start(doctorUser);
            SqlConnectionFactory factory = new();
            UserRepository userRepo = new(factory);
            RoleRepository roleRepo = new(factory);
            AuditLogRepository auditRepo = new(factory);
            AuditService auditService = new(auditRepo);
            UserManagementService userMgmt = new(userRepo, roleRepo, auditService);

            await userMgmt.CreateUserAsync("unauthuser", "Pass@123456", "Test User", 2);
            Report("Test 19: UserManagementService.CreateUserAsync blocks non-Admin (Doctor)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 19: UserManagementService.CreateUserAsync blocks non-Admin (Doctor)", true);
        }
        catch (Exception ex)
        {
            Report("Test 19: UserManagementService.CreateUserAsync blocks non-Admin (Doctor)", false, ex.Message);
        }

        // Test 20: UserManagementService.ResetPasswordAsync blocks non-Admin
        try
        {
            UserSession.Clear();
            UserSession.Start(secretaryUser);
            SqlConnectionFactory factory = new();
            UserRepository userRepo = new(factory);
            RoleRepository roleRepo = new(factory);
            AuditLogRepository auditRepo = new(factory);
            AuditService auditService = new(auditRepo);
            UserManagementService userMgmt = new(userRepo, roleRepo, auditService);

            await userMgmt.ResetPasswordAsync(1, "NewPassword@123");
            Report("Test 20: UserManagementService.ResetPasswordAsync blocks non-Admin (Secretary)", false, "Did not throw");
        }
        catch (UnauthorizedAccessException)
        {
            Report("Test 20: UserManagementService.ResetPasswordAsync blocks non-Admin (Secretary)", true);
        }
        catch (Exception ex)
        {
            Report("Test 20: UserManagementService.ResetPasswordAsync blocks non-Admin (Secretary)", false, ex.Message);
        }

        // Reset session
        UserSession.Clear();

        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine($"RBAC TEST SUMMARY: {passed} PASSED, {failed} FAILED");
        Console.WriteLine("===============================================================");

        return failed == 0 ? 0 : 1;
    }
}
