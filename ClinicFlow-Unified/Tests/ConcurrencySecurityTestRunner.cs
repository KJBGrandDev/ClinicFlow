#nullable enable

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Tests;

public static class ConcurrencySecurityTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("CLINICFLOW CONCURRENCY & INTEGRITY VERIFICATION TEST RUNNER");
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

        // Mock Admin Session for auditing
        UserSession.Start(new User
        {
            UserId = 1,
            Username = "admin",
            RoleName = "Admin",
            FullName = "System Administrator",
            RoleId = 1
        });

        var patientRepo = new PatientRepository();
        var patientService = new PatientService();
        var secretaryRepo = new SecretaryRepository();

        int testPatientId = 0;
        byte[]? initialRowVersion = null;
        byte[]? updatedRowVersion = null;

        try
        {
            // Test 1: Register test patient and verify RowVersion is generated
            var testPatient = new Patient
            {
                FirstName = "Concurrency",
                LastName = "ConcurrencyTester",
                Sex = "Male",
                BirthDate = new DateTime(1990, 5, 20),
                Age = 35,
                ContactNumber = "09170001122",
                StreetAddress = "123 Concurrency Way",
                City = "Davao City",
                Municipality = "Davao"
            };

            var regRes = patientService.Register(testPatient);
            if (!regRes.Success)
            {
                Report("Test 1: Patient registration captures initial RowVersion", false, $"Registration failed: {regRes.Message}");
            }
            else
            {
                var createdList = patientRepo.Search(testPatient.LastName);
                var created = createdList.FirstOrDefault(p => p.LastName == testPatient.LastName);
                if (created != null && created.RowVersion != null && created.RowVersion.Length == 8)
                {
                    testPatientId = created.PatientId;
                    initialRowVersion = created.RowVersion;
                    Report("Test 1: Patient registration captures initial RowVersion", true);
                }
                else
                {
                    Report("Test 1: Patient registration captures initial RowVersion", false, "RowVersion was null or invalid length");
                }
            }

            // Test 2: Normal Patient update advances RowVersion
            if (testPatientId > 0 && initialRowVersion != null)
            {
                var pToUpdate = patientRepo.GetById(testPatientId);
                if (pToUpdate == null)
                {
                    Report("Test 2: Normal update advances RowVersion", false, "Patient not found via GetById");
                }
                else
                {
                    pToUpdate.StreetAddress = "456 Updated RowVersion Blvd";
                    patientRepo.Update(pToUpdate);

                    var pReloaded = patientRepo.GetById(testPatientId);
                    if (pReloaded != null && pReloaded.RowVersion != null && !pReloaded.RowVersion.SequenceEqual(initialRowVersion))
                    {
                        updatedRowVersion = pReloaded.RowVersion;
                        Report("Test 2: Normal update advances RowVersion", true);
                    }
                    else
                    {
                        Report("Test 2: Normal update advances RowVersion", false, "RowVersion did not advance after update");
                    }
                }
            }
            else
            {
                Report("Test 2: Normal update advances RowVersion", false, "Prerequisite Test 1 failed");
            }

            // Test 3: Stale RowVersion update throws DBConcurrencyException in Repository
            if (testPatientId > 0 && initialRowVersion != null && updatedRowVersion != null)
            {
                var stalePatient = patientRepo.GetById(testPatientId)!;
                stalePatient.RowVersion = initialRowVersion; // deliberately stale
                stalePatient.StreetAddress = "789 Stale Attempt Road";

                try
                {
                    patientRepo.Update(stalePatient);
                    Report("Test 3: Stale RowVersion throws DBConcurrencyException in Repository", false, "Did not throw DBConcurrencyException");
                }
                catch (DBConcurrencyException)
                {
                    Report("Test 3: Stale RowVersion throws DBConcurrencyException in Repository", true);
                }
                catch (Exception ex)
                {
                    Report("Test 3: Stale RowVersion throws DBConcurrencyException in Repository", false, $"Threw wrong exception: {ex.GetType().Name}");
                }
            }
            else
            {
                Report("Test 3: Stale RowVersion throws DBConcurrencyException in Repository", false, "Prerequisite Test 2 failed");
            }

            // Test 4: PatientService handles DBConcurrencyException gracefully
            if (testPatientId > 0 && initialRowVersion != null)
            {
                var stalePatient = patientRepo.GetById(testPatientId)!;
                stalePatient.RowVersion = initialRowVersion; // stale version
                stalePatient.StreetAddress = "999 Service Concurrency Road";

                var serviceRes = patientService.Update(stalePatient);
                if (!serviceRes.Success && serviceRes.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase))
                {
                    Report("Test 4: PatientService handles DBConcurrencyException gracefully", true);
                }
                else
                {
                    Report("Test 4: PatientService handles DBConcurrencyException gracefully", false, $"Unexpected response: Success={serviceRes.Success}, Message={serviceRes.Message}");
                }
            }
            else
            {
                Report("Test 4: PatientService handles DBConcurrencyException gracefully", false, "Prerequisite Test 1/2 failed");
            }

            // Test 5: Soft-delete sets IsActive = 0 and preserves record
            if (testPatientId > 0)
            {
                var delRes = patientService.Delete(testPatientId);
                var activeList = patientRepo.Search("Concurrency");
                var archivedList = patientRepo.GetArchivedPatients("Concurrency");

                bool activeHidden = activeList.All(p => p.PatientId != testPatientId);
                bool archivedFound = archivedList.Any(p => p.PatientId == testPatientId && !p.IsActive);

                if (delRes.Success && activeHidden && archivedFound)
                {
                    Report("Test 5: Patient soft-delete marks IsActive = 0 and preserves record", true);
                }
                else
                {
                    Report("Test 5: Patient soft-delete marks IsActive = 0 and preserves record", false, $"activeHidden={activeHidden}, archivedFound={archivedFound}");
                }
            }
            else
            {
                Report("Test 5: Patient soft-delete marks IsActive = 0 and preserves record", false, "Prerequisite failed");
            }

            // Test 6: Patient restore reactivates record (IsActive = 1)
            if (testPatientId > 0)
            {
                var restoreRes = patientService.Restore(testPatientId);
                var activeList = patientRepo.Search("Concurrency");
                bool restoredAndActive = activeList.Any(p => p.PatientId == testPatientId && p.IsActive);

                if (restoreRes.Success && restoredAndActive)
                {
                    Report("Test 6: Patient restore reactivates record (IsActive = 1)", true);
                }
                else
                {
                    Report("Test 6: Patient restore reactivates record (IsActive = 1)", false, $"restoreRes={restoreRes.Success}, restoredAndActive={restoredAndActive}");
                }
            }
            else
            {
                Report("Test 6: Patient restore reactivates record (IsActive = 1)", false, "Prerequisite failed");
            }

            // Test 7: Secretary relational integrity blocks hard delete when linked audit logs exist
            try
            {
                // Secretary 1 is seeded secretary with linked audit logs and appointments
                var (delOk, delMsg) = secretaryRepo.DeleteSecretary(1, "Maria Santos");
                if (!delOk && (delMsg.Contains("historical", StringComparison.OrdinalIgnoreCase) ||
                               delMsg.Contains("Archive Secretary", StringComparison.OrdinalIgnoreCase)))
                {
                    Report("Test 7: Secretary relational integrity blocks hard delete when linked records exist", true);
                }
                else
                {
                    Report("Test 7: Secretary relational integrity blocks hard delete when linked records exist", false, $"delOk={delOk}, message={delMsg}");
                }
            }
            catch (Exception ex)
            {
                Report("Test 7: Secretary relational integrity blocks hard delete when linked records exist", false, $"Unexpected exception: {ex.GetType().Name} - {ex.Message}");
            }
        }
        finally
        {
            // Clean up test patient record
            if (testPatientId > 0)
            {
                try
                {
                    using var conn = DatabaseConnection.CreateConnection();
                    using var cmd = new SqlCommand("DELETE FROM dbo.Patients WHERE PatientId = @PatientId;", conn);
                    cmd.Parameters.Add("@PatientId", SqlDbType.Int).Value = testPatientId;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
            UserSession.Clear();
        }

        Console.WriteLine();
        Console.WriteLine($"CONCURRENCY & INTEGRITY TESTS COMPLETE: {passed} Passed, {failed} Failed.");
        Console.WriteLine("===============================================================");

        return failed == 0 ? 0 : 1;
    }
}
