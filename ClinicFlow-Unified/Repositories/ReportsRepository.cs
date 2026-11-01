#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Repositories;

public sealed class ReportsRepository
{
    // =========================================================================
    // 1. REVENUE REPORT
    // =========================================================================
    public async Task<(List<DailyRevenueReportItem> Items, RevenueReportSummary Summary)> GetDailyRevenueReportAsync(
        DateTime? fromDate, 
        DateTime? toDate, 
        string? paymentMethod = null)
    {
        var items = new List<DailyRevenueReportItem>();
        var summary = new RevenueReportSummary();

        using var conn = DatabaseConnection.CreateConnection();
        await conn.OpenAsync();

        var query = new StringBuilder(@"
SELECT 
    PaymentId,
    ReceiptNumber,
    InvoiceNumber,
    PaymentDate,
    PatientId,
    PatientName,
    DoctorId,
    DoctorName,
    PaymentMethod,
    SubTotal,
    DiscountType,
    DiscountAmount,
    AmountPaid,
    ChangeAmount,
    Notes
FROM dbo.vw_DailyRevenueReport
WHERE 1 = 1");

        using var cmd = new SqlCommand();
        cmd.Connection = conn;

        if (fromDate.HasValue)
        {
            query.Append(" AND PaymentDate >= @FromDate");
            cmd.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = fromDate.Value.Date;
        }

        if (toDate.HasValue)
        {
            query.Append(" AND PaymentDate <= @ToDate");
            cmd.Parameters.Add("@ToDate", SqlDbType.DateTime2).Value = toDate.Value.Date.AddDays(1).AddTicks(-1);
        }

        if (!string.IsNullOrWhiteSpace(paymentMethod) && !string.Equals(paymentMethod, "All", StringComparison.OrdinalIgnoreCase))
        {
            query.Append(" AND PaymentMethod = @PaymentMethod");
            cmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = paymentMethod;
        }

        query.Append(" ORDER BY PaymentDate DESC;");
        cmd.CommandText = query.ToString();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = new DailyRevenueReportItem
            {
                PaymentId = reader.GetInt32(0),
                ReceiptNumber = reader.GetString(1),
                InvoiceNumber = reader.GetString(2),
                PaymentDate = reader.GetDateTime(3),
                PatientId = reader.GetInt32(4),
                PatientName = reader.GetString(5),
                DoctorId = reader.GetInt32(6),
                DoctorName = reader.GetString(7),
                PaymentMethod = reader.GetString(8),
                SubTotal = reader.GetDecimal(9),
                DiscountType = reader.GetString(10),
                DiscountAmount = reader.GetDecimal(11),
                AmountPaid = reader.GetDecimal(12),
                ChangeAmount = reader.GetDecimal(13),
                Notes = reader.IsDBNull(14) ? null : reader.GetString(14)
            };

            items.Add(item);

            // Group subtotals
            summary.GrossSubtotal += item.SubTotal;
            summary.TotalDiscountsGranted += item.DiscountAmount;
            summary.NetRevenueCollected += item.AmountPaid;

            string method = item.PaymentMethod.ToLowerInvariant();
            if (method.Contains("cash"))
                summary.CashCollections += item.AmountPaid;
            else if (method.Contains("gcash") || method.Contains("maya") || method.Contains("qr"))
                summary.GCashCollections += item.AmountPaid;
            else if (method.Contains("card") || method.Contains("credit") || method.Contains("debit"))
                summary.CardCollections += item.AmountPaid;
            else
                summary.OtherCollections += item.AmountPaid;
        }

        summary.TotalTransactions = items.Count;
        return (items, summary);
    }

    // =========================================================================
    // 2. INVENTORY & EXPIRY REPORT
    // =========================================================================
    public async Task<(List<PharmacyInventoryReportItem> Items, InventoryReportSummary Summary)> GetPharmacyInventoryReportAsync(
        string? statusFilter = null, 
        string? searchTerm = null)
    {
        var items = new List<PharmacyInventoryReportItem>();
        var summary = new InventoryReportSummary();

        using var conn = DatabaseConnection.CreateConnection();
        await conn.OpenAsync();

        var query = new StringBuilder(@"
SELECT 
    InventoryId,
    MedicineId,
    GenericName,
    BrandName,
    DosageForm,
    Strength,
    BatchNumber,
    ExpiryDate,
    QuantityOnHand,
    ReorderLevel,
    UnitPrice,
    TotalInventoryValue,
    DaysUntilExpiry,
    StockStatus
FROM dbo.vw_PharmacyInventoryExpiryReport
WHERE 1 = 1");

        using var cmd = new SqlCommand();
        cmd.Connection = conn;

        if (!string.IsNullOrWhiteSpace(statusFilter) && !string.Equals(statusFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            query.Append(" AND StockStatus = @StockStatus");
            cmd.Parameters.Add("@StockStatus", SqlDbType.NVarChar, 50).Value = statusFilter;
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query.Append(" AND (GenericName LIKE @Search OR BrandName LIKE @Search OR BatchNumber LIKE @Search)");
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = $"%{searchTerm.Trim()}%";
        }

        query.Append(" ORDER BY ExpiryDate ASC, GenericName ASC;");
        cmd.CommandText = query.ToString();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = new PharmacyInventoryReportItem
            {
                InventoryId = reader.GetInt32(0),
                MedicineId = reader.GetInt32(1),
                GenericName = reader.GetString(2),
                BrandName = reader.GetString(3),
                DosageForm = reader.GetString(4),
                Strength = reader.GetString(5),
                BatchNumber = reader.GetString(6),
                ExpiryDate = reader.GetDateTime(7),
                QuantityOnHand = reader.GetInt32(8),
                ReorderLevel = reader.GetInt32(9),
                UnitPrice = reader.GetDecimal(10),
                TotalInventoryValue = reader.GetDecimal(11),
                DaysUntilExpiry = reader.GetInt32(12),
                StockStatus = reader.GetString(13)
            };

            items.Add(item);

            summary.TotalUnitsInStock += item.QuantityOnHand;
            summary.TotalValuation += item.TotalInventoryValue;

            switch (item.StockStatus)
            {
                case "Expired":
                    summary.ExpiredBatchCount++;
                    break;
                case "Near Expiry (<60d)":
                    summary.NearExpiryBatchCount++;
                    break;
                case "Low Stock":
                    summary.LowStockBatchCount++;
                    break;
                default:
                    summary.OptimalBatchCount++;
                    break;
            }
        }

        summary.TotalBatchesMonitored = items.Count;
        return (items, summary);
    }

    // =========================================================================
    // 3. DOCTOR CONSULTATION REPORT
    // =========================================================================
    public async Task<(List<DoctorConsultationReportItem> Items, ConsultationReportSummary Summary)> GetDoctorConsultationReportAsync(
        DateTime? fromDate, 
        DateTime? toDate, 
        int? doctorId = null)
    {
        var items = new List<DoctorConsultationReportItem>();
        var summary = new ConsultationReportSummary();
        var uniquePatients = new HashSet<int>();

        using var conn = DatabaseConnection.CreateConnection();
        await conn.OpenAsync();

        var query = new StringBuilder(@"
SELECT 
    ConsultationId,
    ConsultationDate,
    DoctorId,
    DoctorName,
    Specialty,
    PatientId,
    PatientNumber,
    PatientName,
    PatientAge,
    PatientSex,
    ChiefComplaint,
    Diagnosis,
    PrescribedItemsCount,
    ConsultationFee
FROM dbo.vw_DoctorConsultationReport
WHERE 1 = 1");

        using var cmd = new SqlCommand();
        cmd.Connection = conn;

        if (fromDate.HasValue)
        {
            query.Append(" AND ConsultationDate >= @FromDate");
            cmd.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = fromDate.Value.Date;
        }

        if (toDate.HasValue)
        {
            query.Append(" AND ConsultationDate <= @ToDate");
            cmd.Parameters.Add("@ToDate", SqlDbType.DateTime2).Value = toDate.Value.Date.AddDays(1).AddTicks(-1);
        }

        if (doctorId.HasValue && doctorId.Value > 0)
        {
            query.Append(" AND DoctorId = @DoctorId");
            cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId.Value;
        }

        query.Append(" ORDER BY ConsultationDate DESC;");
        cmd.CommandText = query.ToString();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = new DoctorConsultationReportItem
            {
                ConsultationId = reader.GetInt32(0),
                ConsultationDate = reader.GetDateTime(1),
                DoctorId = reader.GetInt32(2),
                DoctorName = reader.GetString(3),
                Specialty = reader.GetString(4),
                PatientId = reader.GetInt32(5),
                PatientNumber = reader.GetString(6),
                PatientName = reader.GetString(7),
                PatientAge = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                PatientSex = reader.GetString(9),
                ChiefComplaint = reader.GetString(10),
                Diagnosis = reader.GetString(11),
                PrescribedItemsCount = reader.GetInt32(12),
                ConsultationFee = reader.GetDecimal(13)
            };

            items.Add(item);
            uniquePatients.Add(item.PatientId);

            summary.TotalPrescriptionItems += item.PrescribedItemsCount;
            summary.TotalConsultationFees += item.ConsultationFee;

            if (!summary.ConsultationsByDoctor.ContainsKey(item.DoctorName))
                summary.ConsultationsByDoctor[item.DoctorName] = 0;
            summary.ConsultationsByDoctor[item.DoctorName]++;
        }

        summary.TotalConsultations = items.Count;
        summary.UniquePatientsCount = uniquePatients.Count;
        return (items, summary);
    }

    // =========================================================================
    // 4. DOCTOR DROPDOWN HELPER
    // =========================================================================
    public async Task<List<(int DoctorId, string DisplayName)>> GetDoctorFilterOptionsAsync()
    {
        var doctors = new List<(int DoctorId, string DisplayName)>();

        using var conn = DatabaseConnection.CreateConnection();
        await conn.OpenAsync();

        const string sql = "SELECT DoctorId, CONCAT(N'Dr. ', FirstName, N' ', LastName, N' (', Specialty, N')') FROM dbo.Doctors WHERE IsActive = 1 ORDER BY LastName;";
        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            doctors.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        return doctors;
    }

    // =========================================================================
    // 5. CSV EXPORT UTILITY (Satisfies REQ-019 export requirement)
    // =========================================================================
    public static async Task ExportToCsvAsync(DataTable dataTable, string filePath)
    {
        var sb = new StringBuilder();

        // Write Header
        var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => $"\"{c.ColumnName.Replace("\"", "\"\"")}\"");
        sb.AppendLine(string.Join(",", columnNames));

        // Write Rows
        foreach (DataRow row in dataTable.Rows)
        {
            var fields = row.ItemArray.Select(field =>
            {
                if (field == null || Convert.IsDBNull(field))
                    return "\"\"";

                if (field is DateTime dt)
                    return $"\"{dt:yyyy-MM-dd HH:mm:ss}\"";

                if (field is decimal dec)
                    return $"\"{dec:F2}\"";

                return $"\"{field.ToString()!.Replace("\"", "\"\"")}\"";
            });

            sb.AppendLine(string.Join(",", fields));
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
}
