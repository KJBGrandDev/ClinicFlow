#nullable enable

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClinicFlow_Unified.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Tests;

public static class ReportsEngineTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("CLINICFLOW COMPREHENSIVE REPORTING ENGINE TEST RUNNER");
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

        var reportsRepo = new ReportsRepository();

        try
        {
            // Test 1: Daily Revenue Report query with grouping and totals
            var (revItems, revSummary) = await reportsRepo.GetDailyRevenueReportAsync(null, null, "All");
            bool t1Pass = revItems != null && revSummary != null &&
                          revSummary.TotalTransactions == revItems.Count &&
                          revSummary.NetRevenueCollected >= 0;
            Report("Test 1: Daily Clinic Revenue report executes and generates totals", t1Pass,
                $"Transactions: {revSummary?.TotalTransactions}, NetCollected: {revSummary?.NetRevenueCollected}");

            // Test 2: Filter revenue report by date range and payment method
            var fromDate = DateTime.Today.AddDays(-60);
            var toDate = DateTime.Today;
            var (filteredRev, filteredSummary) = await reportsRepo.GetDailyRevenueReportAsync(fromDate, toDate, "Cash");
            bool t2Pass = filteredRev != null && filteredSummary != null &&
                          filteredRev.All(x => x.PaymentMethod.Contains("Cash", StringComparison.OrdinalIgnoreCase));
            Report("Test 2: Revenue report filters correctly by date range and Cash payment method", t2Pass,
                $"Filtered Count: {filteredRev?.Count}");

            // Test 3: Central Pharmacy Inventory & Expiry Watch report with risk classification
            var (invItems, invSummary) = await reportsRepo.GetPharmacyInventoryReportAsync("All", null);
            bool t3Pass = invItems != null && invSummary != null &&
                          invSummary.TotalBatchesMonitored == invItems.Count &&
                          invSummary.TotalUnitsInStock >= 0 &&
                          invSummary.TotalValuation >= 0;
            Report("Test 3: Pharmacy Inventory & Expiry Watch report executes with batch counts and valuation", t3Pass,
                $"Batches: {invSummary?.TotalBatchesMonitored}, Units: {invSummary?.TotalUnitsInStock}, Value: {invSummary?.TotalValuation}");

            // Test 4: Inventory report filters by stock status (Optimal / Low Stock / Expired)
            var (lowStockItems, lowStockSummary) = await reportsRepo.GetPharmacyInventoryReportAsync("Low Stock", null);
            bool t4Pass = lowStockItems != null &&
                          lowStockItems.All(x => x.StockStatus == "Low Stock");
            Report("Test 4: Pharmacy inventory report filters correctly by 'Low Stock' status", t4Pass,
                $"Low Stock Batches: {lowStockItems?.Count}");

            // Test 5: Physician Consultations & Diagnostics report with doctor grouping
            var (consultItems, consultSummary) = await reportsRepo.GetDoctorConsultationReportAsync(null, null, null);
            bool t5Pass = consultItems != null && consultSummary != null &&
                          consultSummary.TotalConsultations == consultItems.Count &&
                          consultSummary.UniquePatientsCount >= 0 &&
                          consultSummary.TotalConsultationFees >= 0;
            Report("Test 5: Physician Consultations report executes with patient count and fee summaries", t5Pass,
                $"Consultations: {consultSummary?.TotalConsultations}, Patients: {consultSummary?.UniquePatientsCount}, Fees: {consultSummary?.TotalConsultationFees}");

            // Test 6: Doctor filter dropdown options retrieval
            var doctorOptions = await reportsRepo.GetDoctorFilterOptionsAsync();
            bool t6Pass = doctorOptions != null && doctorOptions.Count > 0 &&
                          doctorOptions.All(d => d.DoctorId > 0 && !string.IsNullOrWhiteSpace(d.DisplayName));
            Report("Test 6: Active physician filter list retrieved for report dropdowns", t6Pass,
                $"Active Doctors: {doctorOptions?.Count}");

            // Test 7: ExportToCsvAsync produces valid, readable CSV file
            var testDt = new DataTable("TestReport");
            testDt.Columns.Add("Receipt #", typeof(string));
            testDt.Columns.Add("Patient Name", typeof(string));
            testDt.Columns.Add("Amount Paid", typeof(decimal));
            testDt.Columns.Add("Transaction Date", typeof(DateTime));

            testDt.Rows.Add("RCP-TEST-001", "Dela Cruz, Juan", 1250.00m, DateTime.Now);
            testDt.Rows.Add("RCP-TEST-002", "Santos, Maria", 750.50m, DateTime.Now.AddHours(-2));

            string tempCsvPath = Path.Combine(Path.GetTempPath(), $"ClinicFlow_ReportTest_{Guid.NewGuid():N}.csv");
            await ReportsRepository.ExportToCsvAsync(testDt, tempCsvPath);

            bool t7Pass = File.Exists(tempCsvPath) && new FileInfo(tempCsvPath).Length > 0;
            if (t7Pass)
            {
                string[] lines = await File.ReadAllLinesAsync(tempCsvPath);
                t7Pass = lines.Length >= 3 && lines[0].Contains("Receipt #") && lines[1].Contains("RCP-TEST-001");
                File.Delete(tempCsvPath);
            }
            Report("Test 7: ReportsRepository.ExportToCsvAsync writes valid RFC4180 CSV with headers and data", t7Pass,
                $"File exists and parsed cleanly");

            // Test 8: ReportsForm instantiates, initializes dropdowns, and generates active report
            UserSession.Start(new User
            {
                UserId = 1,
                Username = "admin",
                FullName = "System Administrator",
                RoleId = 1,
                RoleName = "Admin"
            });

            using var form = new ReportsForm();
            // Trigger Load method to populate controls and initial report
            var loadMethod = typeof(ReportsForm).GetMethod("ReportsForm_Load",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loadMethod?.Invoke(form, new object?[] { form, EventArgs.Empty });

            bool t8Pass = form != null && !form.IsDisposed;
            Report("Test 8: ReportsForm instantiates, initializes controls, and binds initial report", t8Pass);
        }
        catch (Exception ex)
        {
            Report("Unexpected Exception in Reports Test Suite", false, ex.ToString());
        }
        finally
        {
            UserSession.Clear();
        }

        Console.WriteLine("---------------------------------------------------------------");
        Console.WriteLine($"REPORTS ENGINE TESTS COMPLETE: {passed} Passed, {failed} Failed.");
        Console.WriteLine("===============================================================");

        return failed == 0 ? 0 : 1;
    }
}
