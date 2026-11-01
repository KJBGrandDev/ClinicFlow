#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Repositories
{
    public sealed class BillingRepository
    {
        private static bool _schemaInitialized = false;
        private static readonly object _schemaLock = new();

        public BillingRepository()
        {
            EnsureSchema();
        }

        private static void EnsureSchema()
        {
            if (_schemaInitialized) return;

            lock (_schemaLock)
            {
                if (_schemaInitialized) return;

                try
                {
                    using var conn = DatabaseConnection.CreateConnection();
                    conn.Open();

                    const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE dbo.Invoices (
        InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
        InvoiceNumber NVARCHAR(30) NOT NULL UNIQUE,
        AppointmentId INT NOT NULL,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        ConsultationId INT NULL,
        ConsultationFee DECIMAL(18,2) NOT NULL DEFAULT 500.00,
        PharmacyTotal DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        SubTotal DECIMAL(18,2) NOT NULL DEFAULT 500.00,
        DiscountType NVARCHAR(50) NOT NULL DEFAULT N'None',
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        TotalAmountDue DECIMAL(18,2) NOT NULL DEFAULT 500.00,
        PaymentStatus NVARCHAR(30) NOT NULL DEFAULT N'Unpaid',
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentId INT IDENTITY(1,1) PRIMARY KEY,
        ReceiptNumber NVARCHAR(30) NOT NULL UNIQUE,
        InvoiceId INT NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL DEFAULT N'Cash',
        AmountPaid DECIMAL(18,2) NOT NULL,
        ChangeAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        ProcessedByUserId INT NULL,
        PaymentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Notes NVARCHAR(250) NULL
    );
END;";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    _schemaInitialized = true;
                }
                catch
                {
                    // Graceful fallback
                }
            }
        }

        public List<PendingCheckoutItem> GetPendingCheckouts(string search = "")
        {
            EnsureSchema();
            const string sql = @"
SELECT 
    a.AppointmentId,
    a.AppointmentNumber,
    p.PatientId,
    p.PatientNumber,
    CONCAT(p.LastName, N', ', p.FirstName) AS PatientName,
    d.DoctorId,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(d.Specialty, N'General Medicine') AS Specialty,
    CONVERT(VARCHAR(5), a.ScheduledStart, 108) AS ScheduledTime,
    c.ConsultationId,
    ISNULL(c.Diagnosis, N'General Medical Consultation') AS Diagnosis,
    ISNULL(rx.PrescriptionStatus, N'None') AS PrescriptionStatus,
    ISNULL(inv.TotalAmountDue, 500.00 + ISNULL(rxTotals.TotalRx, 0.00)) AS TotalEstimated
FROM dbo.Appointments a
INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
INNER JOIN dbo.Doctors d ON d.DoctorId = a.DoctorId
OUTER APPLY (
    SELECT TOP 1 v.VisitId
    FROM dbo.PatientVisits v
    WHERE v.AppointmentId = a.AppointmentId
    ORDER BY v.VisitId DESC
) v
CROSS APPLY (
    SELECT TOP 1 c.ConsultationId, c.Diagnosis
    FROM dbo.Consultations c
    WHERE c.VisitId = v.VisitId
    ORDER BY c.ConsultationId DESC
) c
OUTER APPLY (
    SELECT TOP 1 rx.PrescriptionId, rx.PrescriptionStatus
    FROM dbo.Prescriptions rx
    WHERE rx.ConsultationId = c.ConsultationId
    ORDER BY rx.PrescriptionId DESC
) rx
OUTER APPLY (
    SELECT SUM(pi.QuantityPrescribed * m.UnitPrice) AS TotalRx
    FROM dbo.Prescriptions rx2
    INNER JOIN dbo.PrescriptionItems pi ON pi.PrescriptionId = rx2.PrescriptionId
    INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
    WHERE rx2.ConsultationId = c.ConsultationId
      AND pi.PatientChoice = N'Clinic Pharmacy'
) rxTotals
LEFT JOIN dbo.Invoices inv ON inv.AppointmentId = a.AppointmentId
WHERE a.Status = N'Completed'
  AND (inv.PaymentStatus IS NULL OR inv.PaymentStatus <> N'Paid')
  AND (@Search = N'' 
       OR p.LastName LIKE @Pattern 
       OR p.FirstName LIKE @Pattern 
       OR p.PatientNumber LIKE @Pattern 
       OR a.AppointmentNumber LIKE @Pattern)
ORDER BY a.AppointmentDate DESC, a.ScheduledStart DESC;";

            var list = new List<PendingCheckoutItem>();
            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = search.Trim();
            cmd.Parameters.Add("@Pattern", SqlDbType.NVarChar, 102).Value = $"%{search.Trim()}%";
            conn.Open();

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new PendingCheckoutItem
                {
                    AppointmentId = rdr.GetInt32(0),
                    AppointmentNumber = rdr.GetString(1),
                    PatientId = rdr.GetInt32(2),
                    PatientNumber = rdr.GetString(3),
                    PatientName = rdr.GetString(4),
                    DoctorId = rdr.GetInt32(5),
                    DoctorName = rdr.GetString(6),
                    Specialty = rdr.GetString(7),
                    ScheduledTime = rdr.GetString(8),
                    ConsultationId = rdr.IsDBNull(9) ? null : rdr.GetInt32(9),
                    Diagnosis = rdr.GetString(10),
                    PrescriptionStatus = rdr.GetString(11),
                    TotalEstimated = rdr.GetDecimal(12)
                });
            }

            return list;
        }

        public BillDetails? GetBillDetails(int appointmentId)
        {
            EnsureSchema();
            const string sqlHeader = @"
SELECT 
    a.AppointmentId,
    a.AppointmentNumber,
    p.PatientId,
    p.PatientNumber,
    CONCAT(p.LastName, N', ', p.FirstName) AS PatientName,
    d.DoctorId,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(d.Specialty, N'General Medicine') AS Specialty,
    c.ConsultationId,
    ISNULL(c.Diagnosis, N'General Medical Consultation') AS Diagnosis,
    ISNULL(inv.ConsultationFee, 500.00) AS ConsultationFee
FROM dbo.Appointments a
INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
INNER JOIN dbo.Doctors d ON d.DoctorId = a.DoctorId
OUTER APPLY (
    SELECT TOP 1 v.VisitId
    FROM dbo.PatientVisits v
    WHERE v.AppointmentId = a.AppointmentId
    ORDER BY v.VisitId DESC
) v
CROSS APPLY (
    SELECT TOP 1 c.ConsultationId, c.Diagnosis
    FROM dbo.Consultations c
    WHERE c.VisitId = v.VisitId
    ORDER BY c.ConsultationId DESC
) c
LEFT JOIN dbo.Invoices inv ON inv.AppointmentId = a.AppointmentId
WHERE a.AppointmentId = @AppointmentId;";

            BillDetails? bill = null;
            using var conn = DatabaseConnection.CreateConnection();
            using var cmdHeader = new SqlCommand(sqlHeader, conn);
            cmdHeader.Parameters.Add("@AppointmentId", SqlDbType.Int).Value = appointmentId;
            conn.Open();

            using (var rdr = cmdHeader.ExecuteReader())
            {
                if (rdr.Read())
                {
                    bill = new BillDetails
                    {
                        AppointmentId = rdr.GetInt32(0),
                        AppointmentNumber = rdr.GetString(1),
                        PatientId = rdr.GetInt32(2),
                        PatientNumber = rdr.GetString(3),
                        PatientName = rdr.GetString(4),
                        DoctorId = rdr.GetInt32(5),
                        DoctorName = rdr.GetString(6),
                        Specialty = rdr.GetString(7),
                        ConsultationId = rdr.IsDBNull(8) ? null : rdr.GetInt32(8),
                        Diagnosis = rdr.GetString(9),
                        ConsultationFee = rdr.GetDecimal(10)
                    };
                }
            }

            if (bill == null) return null;

            // Load prescribed medications fulfilled at Clinic Pharmacy
            if (bill.ConsultationId.HasValue)
            {
                const string sqlMeds = @"
SELECT 
    pi.PrescriptionItemId,
    m.MedicineId,
    m.GenericName,
    m.BrandName,
    m.Strength,
    m.DosageForm,
    pi.QuantityPrescribed,
    m.UnitPrice,
    pi.PatientChoice
FROM dbo.Prescriptions rx
INNER JOIN dbo.PrescriptionItems pi ON pi.PrescriptionId = rx.PrescriptionId
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE rx.ConsultationId = @ConsultationId
  AND pi.PatientChoice = N'Clinic Pharmacy';";

                using (var cmdMeds = new SqlCommand(sqlMeds, conn))
                {
                    cmdMeds.Parameters.Add("@ConsultationId", SqlDbType.Int).Value = bill.ConsultationId.Value;

                    using var rdrMeds = cmdMeds.ExecuteReader();
                    decimal pharmacyTotal = 0;
                    while (rdrMeds.Read())
                    {
                        var med = new BillableMedicineItem
                        {
                            PrescriptionItemId = rdrMeds.GetInt32(0),
                            MedicineId = rdrMeds.GetInt32(1),
                            GenericName = rdrMeds.GetString(2),
                            BrandName = rdrMeds.IsDBNull(3) ? null : rdrMeds.GetString(3),
                            Strength = rdrMeds.IsDBNull(4) ? null : rdrMeds.GetString(4),
                            DosageForm = rdrMeds.GetString(5),
                            Quantity = rdrMeds.GetInt32(6),
                            UnitPrice = rdrMeds.GetDecimal(7),
                            PatientChoice = rdrMeds.GetString(8)
                        };
                        pharmacyTotal += med.TotalPrice;
                        bill.Medicines.Add(med);
                    }
                    bill.PharmacyTotal = pharmacyTotal;
                }
            }

            return bill;
        }

        public (bool Success, string Message, PaymentReceiptResult? Receipt) ProcessPayment(ProcessPaymentRequest req)
        {
            EnsureSchema();
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // 1. Get or Create Invoice
                int invoiceId;
                string invoiceNumber;

                const string checkInvoiceSql = "SELECT InvoiceId, InvoiceNumber FROM dbo.Invoices WHERE AppointmentId = @AppointmentId;";
                using (var cmdCheck = new SqlCommand(checkInvoiceSql, conn, tran))
                {
                    cmdCheck.Parameters.Add("@AppointmentId", SqlDbType.Int).Value = req.AppointmentId;
                    using var rdr = cmdCheck.ExecuteReader();
                    if (rdr.Read())
                    {
                        invoiceId = rdr.GetInt32(0);
                        invoiceNumber = rdr.GetString(1);
                    }
                    else
                    {
                        rdr.Close();

                        // Generate next sequential invoice number: INV-YYYYMMDD-XXXX
                        const string nextInvSql = @"
DECLARE @Prefix VARCHAR(12) = CONCAT('INV-', CONVERT(VARCHAR(8), GETDATE(), 112), '-');
DECLARE @NextNum INT = (SELECT ISNULL(MAX(InvoiceId), 0) + 1 FROM dbo.Invoices);
SELECT CONCAT(@Prefix, RIGHT(CONCAT('0000', @NextNum), 4));";
                        using var cmdNum = new SqlCommand(nextInvSql, conn, tran);
                        invoiceNumber = (string)cmdNum.ExecuteScalar()!;

                        const string insertInvSql = @"
INSERT INTO dbo.Invoices 
    (InvoiceNumber, AppointmentId, PatientId, DoctorId, ConsultationId, ConsultationFee, PharmacyTotal, SubTotal, DiscountType, DiscountAmount, TotalAmountDue, PaymentStatus, CreatedAt)
OUTPUT INSERTED.InvoiceId
VALUES 
    (@InvoiceNumber, @AppointmentId, @PatientId, @DoctorId, @ConsultationId, @ConsultationFee, @PharmacyTotal, @SubTotal, @DiscountType, @DiscountAmount, @TotalAmountDue, N'Paid', SYSUTCDATETIME());";

                        using var cmdIns = new SqlCommand(insertInvSql, conn, tran);
                        cmdIns.Parameters.Add("@InvoiceNumber", SqlDbType.NVarChar, 30).Value = invoiceNumber;
                        cmdIns.Parameters.Add("@AppointmentId", SqlDbType.Int).Value = req.AppointmentId;
                        cmdIns.Parameters.Add("@PatientId", SqlDbType.Int).Value = req.PatientId;
                        cmdIns.Parameters.Add("@DoctorId", SqlDbType.Int).Value = req.DoctorId;
                        cmdIns.Parameters.Add("@ConsultationId", SqlDbType.Int).Value = (object?)req.ConsultationId ?? DBNull.Value;
                        cmdIns.Parameters.Add("@ConsultationFee", SqlDbType.Decimal).Value = req.ConsultationFee;
                        cmdIns.Parameters.Add("@PharmacyTotal", SqlDbType.Decimal).Value = req.PharmacyTotal;
                        cmdIns.Parameters.Add("@SubTotal", SqlDbType.Decimal).Value = req.ConsultationFee + req.PharmacyTotal;
                        cmdIns.Parameters.Add("@DiscountType", SqlDbType.NVarChar, 50).Value = req.DiscountType;
                        cmdIns.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = req.DiscountAmount;
                        cmdIns.Parameters.Add("@TotalAmountDue", SqlDbType.Decimal).Value = req.TotalAmountDue;
                        invoiceId = (int)cmdIns.ExecuteScalar()!;
                    }
                }

                // If invoice already existed, update its values to Paid
                const string updateInvSql = @"
UPDATE dbo.Invoices
SET ConsultationFee = @ConsultationFee,
    PharmacyTotal = @PharmacyTotal,
    SubTotal = @SubTotal,
    DiscountType = @DiscountType,
    DiscountAmount = @DiscountAmount,
    TotalAmountDue = @TotalAmountDue,
    PaymentStatus = N'Paid',
    UpdatedAt = SYSUTCDATETIME()
WHERE InvoiceId = @InvoiceId;";
                using (var cmdUpd = new SqlCommand(updateInvSql, conn, tran))
                {
                    cmdUpd.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;
                    cmdUpd.Parameters.Add("@ConsultationFee", SqlDbType.Decimal).Value = req.ConsultationFee;
                    cmdUpd.Parameters.Add("@PharmacyTotal", SqlDbType.Decimal).Value = req.PharmacyTotal;
                    cmdUpd.Parameters.Add("@SubTotal", SqlDbType.Decimal).Value = req.ConsultationFee + req.PharmacyTotal;
                    cmdUpd.Parameters.Add("@DiscountType", SqlDbType.NVarChar, 50).Value = req.DiscountType;
                    cmdUpd.Parameters.Add("@DiscountAmount", SqlDbType.Decimal).Value = req.DiscountAmount;
                    cmdUpd.Parameters.Add("@TotalAmountDue", SqlDbType.Decimal).Value = req.TotalAmountDue;
                    cmdUpd.ExecuteNonQuery();
                }

                // 2. Generate Receipt Number: OR-YYYY-XXXX
                const string nextOrSql = @"
DECLARE @Prefix VARCHAR(8) = CONCAT('OR-', CONVERT(VARCHAR(4), YEAR(GETDATE())), '-');
DECLARE @NextNum INT = (SELECT ISNULL(MAX(PaymentId), 0) + 1 FROM dbo.Payments);
SELECT CONCAT(@Prefix, RIGHT(CONCAT('00000', @NextNum), 5));";
                string receiptNumber;
                using (var cmdOr = new SqlCommand(nextOrSql, conn, tran))
                {
                    receiptNumber = (string)cmdOr.ExecuteScalar()!;
                }

                // 3. Insert Payment
                const string insertPaymentSql = @"
INSERT INTO dbo.Payments (ReceiptNumber, InvoiceId, PaymentMethod, AmountPaid, ChangeAmount, ProcessedByUserId, PaymentDate, Notes)
VALUES (@ReceiptNumber, @InvoiceId, @PaymentMethod, @AmountPaid, @ChangeAmount, @ProcessedByUserId, SYSUTCDATETIME(), @Notes);";

                using (var cmdPay = new SqlCommand(insertPaymentSql, conn, tran))
                {
                    cmdPay.Parameters.Add("@ReceiptNumber", SqlDbType.NVarChar, 30).Value = receiptNumber;
                    cmdPay.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;
                    cmdPay.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = req.PaymentMethod;
                    cmdPay.Parameters.Add("@AmountPaid", SqlDbType.Decimal).Value = req.AmountPaid;
                    cmdPay.Parameters.Add("@ChangeAmount", SqlDbType.Decimal).Value = req.ChangeAmount;
                    cmdPay.Parameters.Add("@ProcessedByUserId", SqlDbType.Int).Value = (object?)req.ProcessedByUserId ?? DBNull.Value;
                    cmdPay.Parameters.Add("@Notes", SqlDbType.NVarChar, 250).Value = (object?)req.Notes ?? DBNull.Value;
                    cmdPay.ExecuteNonQuery();
                }

                // 4. Update Prescription Status to 'Paid'
                if (req.ConsultationId.HasValue)
                {
                    const string updateRxSql = @"
UPDATE dbo.Prescriptions 
SET PrescriptionStatus = N'Paid / Ready for Dispense' 
WHERE ConsultationId = @ConsultationId;

UPDATE pi
SET pi.AvailabilityStatus = N'Paid'
FROM dbo.PrescriptionItems pi
INNER JOIN dbo.Prescriptions rx ON rx.PrescriptionId = pi.PrescriptionId
WHERE rx.ConsultationId = @ConsultationId
  AND pi.PatientChoice = N'Clinic Pharmacy';";

                    using var cmdRx = new SqlCommand(updateRxSql, conn, tran);
                    cmdRx.Parameters.Add("@ConsultationId", SqlDbType.Int).Value = req.ConsultationId.Value;
                    cmdRx.ExecuteNonQuery();
                }

                tran.Commit();

                AuditLogger.Log("Insert", "Payments", invoiceId, $"Processed payment for {receiptNumber}, Total: ₱{req.TotalAmountDue:N2}");

                var result = new PaymentReceiptResult
                {
                    Success = true,
                    Message = $"Payment processed successfully. Official Receipt: {receiptNumber}",
                    InvoiceNumber = invoiceNumber,
                    ReceiptNumber = receiptNumber,
                    TotalAmountPaid = req.AmountPaid,
                    ChangeAmount = req.ChangeAmount,
                    PaymentDate = DateTime.Now
                };

                return (true, result.Message, result);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                string msg = ErrorLogger.Log(ex, "BillingRepository.ProcessPayment");
                return (false, $"Payment processing failed: {msg}", null);
            }
        }

        public List<PaymentHistoryItem> GetRecentPayments(int top = 50)
        {
            EnsureSchema();
            const string sql = @"
SELECT TOP (@Top)
    p.PaymentId,
    p.ReceiptNumber,
    inv.InvoiceNumber,
    CONCAT(pat.LastName, N', ', pat.FirstName) AS PatientName,
    CONCAT(N'Dr. ', doc.FirstName, N' ', doc.LastName) AS DoctorName,
    p.PaymentMethod,
    p.AmountPaid,
    p.PaymentDate,
    ISNULL(u.FullName, ISNULL(u.Username, N'Secretary')) AS ProcessedBy
FROM dbo.Payments p
INNER JOIN dbo.Invoices inv ON inv.InvoiceId = p.InvoiceId
INNER JOIN dbo.Patients pat ON pat.PatientId = inv.PatientId
INNER JOIN dbo.Doctors doc ON doc.DoctorId = inv.DoctorId
LEFT JOIN dbo.Users u ON u.UserId = p.ProcessedByUserId
ORDER BY p.PaymentDate DESC;";

            var list = new List<PaymentHistoryItem>();
            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
            conn.Open();

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new PaymentHistoryItem
                {
                    PaymentId = rdr.GetInt32(0),
                    ReceiptNumber = rdr.GetString(1),
                    InvoiceNumber = rdr.GetString(2),
                    PatientName = rdr.GetString(3),
                    DoctorName = rdr.GetString(4),
                    PaymentMethod = rdr.GetString(5),
                    AmountPaid = rdr.GetDecimal(6),
                    PaymentDate = rdr.GetDateTime(7),
                    ProcessedBy = rdr.GetString(8)
                });
            }

            return list;
        }

        public ReceiptData? GetReceiptData(int paymentId)
        {
            EnsureSchema();
            const string sqlHeader = @"
SELECT 
    p.PaymentId,
    p.ReceiptNumber,
    inv.InvoiceNumber,
    p.PaymentDate,
    pat.PatientNumber,
    CONCAT(pat.LastName, N', ', pat.FirstName) AS PatientName,
    CONCAT(N'Dr. ', doc.FirstName, N' ', doc.LastName) AS DoctorName,
    ISNULL(doc.Specialty, N'General Medicine') AS Specialty,
    inv.ConsultationId,
    inv.ConsultationFee,
    inv.PharmacyTotal,
    inv.SubTotal,
    inv.DiscountType,
    inv.DiscountAmount,
    inv.TotalAmountDue,
    p.PaymentMethod,
    p.AmountPaid,
    p.ChangeAmount,
    ISNULL(u.FullName, ISNULL(u.Username, N'Reception Desk')) AS CashierName,
    ISNULL(p.Notes, N'') AS Notes
FROM dbo.Payments p
INNER JOIN dbo.Invoices inv ON inv.InvoiceId = p.InvoiceId
INNER JOIN dbo.Patients pat ON pat.PatientId = inv.PatientId
INNER JOIN dbo.Doctors doc ON doc.DoctorId = inv.DoctorId
LEFT JOIN dbo.Users u ON u.UserId = p.ProcessedByUserId
WHERE p.PaymentId = @PaymentId;";

            ReceiptData? data = null;
            int? consultId = null;

            using var conn = DatabaseConnection.CreateConnection();
            using var cmdHeader = new SqlCommand(sqlHeader, conn);
            cmdHeader.Parameters.Add("@PaymentId", SqlDbType.Int).Value = paymentId;
            conn.Open();

            using (var rdr = cmdHeader.ExecuteReader())
            {
                if (rdr.Read())
                {
                    consultId = rdr.IsDBNull(8) ? null : rdr.GetInt32(8);
                    data = new ReceiptData
                    {
                        ReceiptNumber = rdr.GetString(1),
                        InvoiceNumber = rdr.GetString(2),
                        PaymentDate = rdr.GetDateTime(3),
                        PatientNumber = rdr.GetString(4),
                        PatientName = rdr.GetString(5),
                        DoctorName = rdr.GetString(6),
                        Specialty = rdr.GetString(7),
                        ConsultationFee = rdr.GetDecimal(9),
                        PharmacyTotal = rdr.GetDecimal(10),
                        Subtotal = rdr.GetDecimal(11),
                        DiscountType = rdr.GetString(12),
                        DiscountAmount = rdr.GetDecimal(13),
                        TotalAmountDue = rdr.GetDecimal(14),
                        PaymentMethod = rdr.GetString(15),
                        AmountPaid = rdr.GetDecimal(16),
                        ChangeAmount = rdr.GetDecimal(17),
                        CashierName = rdr.GetString(18),
                        Notes = rdr.GetString(19),
                        Items = new List<ReceiptItemLine>
                        {
                            new ReceiptItemLine
                            {
                                Description = "Doctor Consultation Fee",
                                ItemType = "Consultation",
                                Quantity = 1,
                                UnitPrice = rdr.GetDecimal(9)
                            }
                        }
                    };
                }
            }

            if (data == null) return null;

            if (consultId.HasValue)
            {
                const string sqlMeds = @"
SELECT 
    m.GenericName,
    m.BrandName,
    pi.QuantityPrescribed,
    m.UnitPrice
FROM dbo.Prescriptions rx
INNER JOIN dbo.PrescriptionItems pi ON pi.PrescriptionId = rx.PrescriptionId
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE rx.ConsultationId = @ConsultationId
  AND pi.PatientChoice = N'Clinic Pharmacy';";

                using var cmdMeds = new SqlCommand(sqlMeds, conn);
                cmdMeds.Parameters.Add("@ConsultationId", SqlDbType.Int).Value = consultId.Value;
                using var rdrMeds = cmdMeds.ExecuteReader();
                while (rdrMeds.Read())
                {
                    string genName = rdrMeds.GetString(0);
                    string? brand = rdrMeds.IsDBNull(1) ? null : rdrMeds.GetString(1);
                    string disp = string.IsNullOrWhiteSpace(brand) ? genName : $"{brand} ({genName})";

                    data.Items.Add(new ReceiptItemLine
                    {
                        Description = disp,
                        ItemType = "Pharmacy Medication",
                        Quantity = rdrMeds.GetInt32(2),
                        UnitPrice = rdrMeds.GetDecimal(3)
                    });
                }
                rdrMeds.Close();
            }

            return data;
        }
    }
}

