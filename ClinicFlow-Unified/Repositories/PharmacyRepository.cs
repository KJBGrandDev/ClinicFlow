#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Repositories;

public sealed class PharmacyRepository
{
    // =========================================================================
    // 1. STRICT PAID-ONLY DISPENSING QUEUE
    // =========================================================================
    public List<PharmacyQueueItem> GetDispenseQueue(string search = "")
    {
        // Enforce: Consultation MUST be completed, and Payment MUST be processed at Cashier.
        // Prescriptions must have 'Paid / Ready for Dispense' or items with AvailabilityStatus = 'Paid'.
        const string sql = @"
SELECT 
    rx.PrescriptionId,
    rx.PrescriptionNumber,
    ISNULL(inv.InvoiceNumber, N'N/A') AS InvoiceNumber,
    ISNULL(p.ReceiptNumber, N'N/A') AS ReceiptNumber,
    a.AppointmentId,
    pt.PatientId,
    pt.PatientNumber,
    CONCAT(pt.LastName, N', ', pt.FirstName) AS PatientName,
    DATEDIFF(YEAR, pt.BirthDate, GETDATE()) AS Age,
    pt.Sex,
    d.DoctorId,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(d.LicenseNumber, N'LIC-DVO-001') AS DoctorLicense,
    ISNULL(d.Specialty, N'General Medicine') AS Specialty,
    COUNT(pi.PrescriptionItemId) AS ItemCount,
    ISNULL(p.AmountPaid, 0) AS TotalAmountPaid,
    ISNULL(p.PaymentDate, rx.CreatedAt) AS PaidAt,
    rx.PrescriptionStatus
FROM dbo.Prescriptions rx
INNER JOIN dbo.Consultations c ON c.ConsultationId = rx.ConsultationId
INNER JOIN dbo.PatientVisits v ON v.VisitId = c.VisitId
INNER JOIN dbo.Appointments a ON a.AppointmentId = v.AppointmentId
INNER JOIN dbo.Patients pt ON pt.PatientId = rx.PatientId
INNER JOIN dbo.Doctors d ON d.DoctorId = rx.DoctorId
LEFT JOIN dbo.Invoices inv ON inv.AppointmentId = a.AppointmentId AND inv.PaymentStatus = N'Paid'
LEFT JOIN dbo.Payments p ON p.InvoiceId = inv.InvoiceId
LEFT JOIN dbo.PrescriptionItems pi ON pi.PrescriptionId = rx.PrescriptionId
WHERE a.Status = N'Completed'
  AND (rx.PrescriptionStatus = N'Paid / Ready for Dispense' OR EXISTS (
      SELECT 1 FROM dbo.PrescriptionItems subPi 
      WHERE subPi.PrescriptionId = rx.PrescriptionId AND subPi.AvailabilityStatus = N'Paid'
  ))
  AND (@Search = '' OR (
      pt.LastName LIKE @SearchLike OR 
      pt.FirstName LIKE @SearchLike OR 
      pt.PatientNumber LIKE @SearchLike OR 
      rx.PrescriptionNumber LIKE @SearchLike OR 
      p.ReceiptNumber LIKE @SearchLike
  ))
GROUP BY 
    rx.PrescriptionId, rx.PrescriptionNumber, inv.InvoiceNumber, p.ReceiptNumber,
    a.AppointmentId, pt.PatientId, pt.PatientNumber, pt.LastName, pt.FirstName,
    pt.BirthDate, pt.Sex, d.DoctorId, d.FirstName, d.LastName, d.LicenseNumber,
    d.Specialty, p.AmountPaid, p.PaymentDate, rx.CreatedAt, rx.PrescriptionStatus
ORDER BY PaidAt ASC;";

        var list = new List<PharmacyQueueItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = search.Trim();
        cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 100).Value = $"%{search.Trim()}%";
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new PharmacyQueueItem
            {
                PrescriptionId = rdr.GetInt32(0),
                PrescriptionNumber = rdr.GetString(1),
                InvoiceNumber = rdr.GetString(2),
                ReceiptNumber = rdr.GetString(3),
                AppointmentId = rdr.GetInt32(4),
                PatientId = rdr.GetInt32(5),
                PatientNumber = rdr.GetString(6),
                PatientName = rdr.GetString(7),
                Age = rdr.GetInt32(8),
                Sex = rdr.GetString(9),
                DoctorId = rdr.GetInt32(10),
                DoctorName = rdr.GetString(11),
                DoctorLicense = rdr.GetString(12),
                Specialty = rdr.GetString(13),
                ItemCount = rdr.GetInt32(14),
                TotalAmountPaid = rdr.GetDecimal(15),
                PaidAt = rdr.GetDateTime(16),
                PrescriptionStatus = rdr.GetString(17)
            });
        }
        return list;
    }

    public List<PharmacyPrescriptionItem> GetPrescriptionDetails(int prescriptionId)
    {
        const string sql = @"
SELECT 
    pi.PrescriptionItemId,
    pi.PrescriptionId,
    m.MedicineId,
    m.GenericName,
    m.BrandName,
    m.DosageForm,
    m.Strength,
    ISNULL(m.Category, N'General') AS Category,
    pi.QuantityPrescribed,
    pi.Instructions,
    m.UnitPrice,
    pi.PatientChoice,
    pi.AvailabilityStatus,
    ISNULL((
        SELECT SUM(inv.QuantityOnHand) 
        FROM dbo.MedicineInventory inv 
        WHERE inv.MedicineId = m.MedicineId AND inv.ExpiryDate >= CAST(GETDATE() AS DATE)
    ), 0) AS InHouseStock
FROM dbo.PrescriptionItems pi
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE pi.PrescriptionId = @PrescriptionId
ORDER BY pi.PrescriptionItemId ASC;";

        var list = new List<PharmacyPrescriptionItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@PrescriptionId", SqlDbType.Int).Value = prescriptionId;
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new PharmacyPrescriptionItem
            {
                PrescriptionItemId = rdr.GetInt32(0),
                PrescriptionId = rdr.GetInt32(1),
                MedicineId = rdr.GetInt32(2),
                GenericName = rdr.GetString(3),
                BrandName = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                DosageForm = rdr.GetString(5),
                Strength = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                Category = rdr.GetString(7),
                QuantityPrescribed = rdr.GetInt32(8),
                Instructions = rdr.GetString(9),
                UnitPrice = rdr.GetDecimal(10),
                PatientChoice = rdr.GetString(11),
                AvailabilityStatus = rdr.GetString(12),
                InHouseStock = rdr.GetInt32(13)
            });
        }
        return list;
    }

    // =========================================================================
    // 2. DISPENSING & INVENTORY DEDUCTION (1-CLICK & INDIVIDUAL)
    // =========================================================================
    public (bool Success, string Message) DispensePrescription(int prescriptionId, int userId)
    {
        var items = GetPrescriptionDetails(prescriptionId);
        if (items.Count == 0)
            return (false, "Prescription has no items to dispense.");

        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            foreach (var item in items)
            {
                // Only process items that are Paid and not yet Dispensed or Out of Stock
                if (item.AvailabilityStatus == "Dispensed" || item.AvailabilityStatus == "Out of Stock / External")
                    continue;

                if (item.InHouseStock < item.QuantityPrescribed)
                {
                    tran.Rollback();
                    return (false, $"Insufficient stock for {item.GenericName}. Available: {item.InHouseStock}, Prescribed: {item.QuantityPrescribed}. Please tag as Out of Stock to print an external Rx.");
                }

                // Deduct stock using FIFO by ExpiryDate
                int remainingToDeduct = item.QuantityPrescribed;

                const string invSql = @"
SELECT InventoryId, QuantityOnHand 
FROM dbo.MedicineInventory 
WHERE MedicineId = @MedicineId AND QuantityOnHand > 0 AND ExpiryDate >= CAST(GETDATE() AS DATE)
ORDER BY ExpiryDate ASC, InventoryId ASC;";

                using var cmdInv = new SqlCommand(invSql, conn, tran);
                cmdInv.Parameters.Add("@MedicineId", SqlDbType.Int).Value = item.MedicineId;

                var inventoryRows = new List<(int InvId, int Qty)>();
                using (var rdr = cmdInv.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        inventoryRows.Add((rdr.GetInt32(0), rdr.GetInt32(1)));
                    }
                }

                foreach (var invRow in inventoryRows)
                {
                    if (remainingToDeduct <= 0) break;
                    int take = Math.Min(remainingToDeduct, invRow.Qty);

                    const string deductSql = @"
UPDATE dbo.MedicineInventory 
SET QuantityOnHand = QuantityOnHand - @Take, LastUpdated = SYSUTCDATETIME()
WHERE InventoryId = @InvId;";

                    using var cmdDeduct = new SqlCommand(deductSql, conn, tran);
                    cmdDeduct.Parameters.Add("@Take", SqlDbType.Int).Value = take;
                    cmdDeduct.Parameters.Add("@InvId", SqlDbType.Int).Value = invRow.InvId;
                    cmdDeduct.ExecuteNonQuery();

                    remainingToDeduct -= take;
                }

                // Update item status
                const string updateItemSql = @"
UPDATE dbo.PrescriptionItems 
SET AvailabilityStatus = N'Dispensed' 
WHERE PrescriptionItemId = @ItemId;";

                using var cmdUpd = new SqlCommand(updateItemSql, conn, tran);
                cmdUpd.Parameters.Add("@ItemId", SqlDbType.Int).Value = item.PrescriptionItemId;
                cmdUpd.ExecuteNonQuery();
            }

            // Update prescription status
            const string updateRxSql = @"
UPDATE dbo.Prescriptions 
SET PrescriptionStatus = N'Dispensed' 
WHERE PrescriptionId = @PrescriptionId;";

            using var cmdRx = new SqlCommand(updateRxSql, conn, tran);
            cmdRx.Parameters.Add("@PrescriptionId", SqlDbType.Int).Value = prescriptionId;
            cmdRx.ExecuteNonQuery();

            tran.Commit();

            AuditLogger.Log("Update", "Prescriptions", prescriptionId, $"Dispensed prescription #{prescriptionId} and deducted inventory.");
            return (true, "Prescription successfully dispensed and inventory updated!");
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return (false, "Error dispensing prescription: " + ex.Message);
        }
    }

    public (bool Success, string Message) DispenseSingleItem(int prescriptionItemId, int userId)
    {
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            const string getItemSql = @"
SELECT pi.PrescriptionId, pi.MedicineId, pi.QuantityPrescribed, pi.AvailabilityStatus, m.GenericName
FROM dbo.PrescriptionItems pi
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE pi.PrescriptionItemId = @ItemId;";

            int rxId = 0, medId = 0, qty = 0;
            string status = "", generic = "";

            using (var cmd = new SqlCommand(getItemSql, conn, tran))
            {
                cmd.Parameters.Add("@ItemId", SqlDbType.Int).Value = prescriptionItemId;
                using var rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    rxId = rdr.GetInt32(0);
                    medId = rdr.GetInt32(1);
                    qty = rdr.GetInt32(2);
                    status = rdr.GetString(3);
                    generic = rdr.GetString(4);
                }
            }

            if (status == "Dispensed")
                return (false, $"{generic} is already dispensed.");

            // Check live stock
            const string stockSql = @"
SELECT ISNULL(SUM(QuantityOnHand), 0) 
FROM dbo.MedicineInventory 
WHERE MedicineId = @MedicineId AND ExpiryDate >= CAST(GETDATE() AS DATE);";

            int stock = 0;
            using (var cmdStock = new SqlCommand(stockSql, conn, tran))
            {
                cmdStock.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medId;
                stock = Convert.ToInt32(cmdStock.ExecuteScalar() ?? 0);
            }

            if (stock < qty)
            {
                tran.Rollback();
                return (false, $"Insufficient stock for {generic}. Available: {stock}, Prescribed: {qty}. Tag as Out of Stock to print external Rx.");
            }

            // Deduct FIFO
            int remainingToDeduct = qty;
            const string invSql = @"
SELECT InventoryId, QuantityOnHand 
FROM dbo.MedicineInventory 
WHERE MedicineId = @MedicineId AND QuantityOnHand > 0 AND ExpiryDate >= CAST(GETDATE() AS DATE)
ORDER BY ExpiryDate ASC, InventoryId ASC;";

            var invRows = new List<(int InvId, int Qty)>();
            using (var cmdInv = new SqlCommand(invSql, conn, tran))
            {
                cmdInv.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medId;
                using var rdr = cmdInv.ExecuteReader();
                while (rdr.Read())
                {
                    invRows.Add((rdr.GetInt32(0), rdr.GetInt32(1)));
                }
            }

            foreach (var r in invRows)
            {
                if (remainingToDeduct <= 0) break;
                int take = Math.Min(remainingToDeduct, r.Qty);

                const string deductSql = @"
UPDATE dbo.MedicineInventory 
SET QuantityOnHand = QuantityOnHand - @Take, LastUpdated = SYSUTCDATETIME()
WHERE InventoryId = @InvId;";

                using var cmdDeduct = new SqlCommand(deductSql, conn, tran);
                cmdDeduct.Parameters.Add("@Take", SqlDbType.Int).Value = take;
                cmdDeduct.Parameters.Add("@InvId", SqlDbType.Int).Value = r.InvId;
                cmdDeduct.ExecuteNonQuery();

                remainingToDeduct -= take;
            }

            const string updItemSql = "UPDATE dbo.PrescriptionItems SET AvailabilityStatus = N'Dispensed' WHERE PrescriptionItemId = @ItemId;";
            using (var cmdUpd = new SqlCommand(updItemSql, conn, tran))
            {
                cmdUpd.Parameters.Add("@ItemId", SqlDbType.Int).Value = prescriptionItemId;
                cmdUpd.ExecuteNonQuery();
            }

            // Check if all items in this prescription are finalized
            const string checkAllSql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.PrescriptionItems WHERE PrescriptionId = @RxId AND AvailabilityStatus NOT IN (N'Dispensed', N'Out of Stock / External'))
BEGIN
    UPDATE dbo.Prescriptions SET PrescriptionStatus = N'Dispensed' WHERE PrescriptionId = @RxId;
END";
            using (var cmdCheck = new SqlCommand(checkAllSql, conn, tran))
            {
                cmdCheck.Parameters.Add("@RxId", SqlDbType.Int).Value = rxId;
                cmdCheck.ExecuteNonQuery();
            }

            tran.Commit();
            AuditLogger.Log("Update", "PrescriptionItems", prescriptionItemId, $"Dispensed single item {generic} (Qty: {qty}).");
            return (true, $"{generic} dispensed successfully.");
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return (false, "Error: " + ex.Message);
        }
    }

    // =========================================================================
    // 3. OUT OF STOCK / EXTERNAL TAGGING & OFFICIAL RX DATA
    // =========================================================================
    public (bool Success, string Message) TagOutOfStock(int prescriptionItemId, int userId)
    {
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            const string getRxSql = "SELECT PrescriptionId FROM dbo.PrescriptionItems WHERE PrescriptionItemId = @ItemId;";
            int rxId = 0;
            using (var cmdGet = new SqlCommand(getRxSql, conn, tran))
            {
                cmdGet.Parameters.Add("@ItemId", SqlDbType.Int).Value = prescriptionItemId;
                rxId = Convert.ToInt32(cmdGet.ExecuteScalar() ?? 0);
            }

            const string updSql = @"
UPDATE dbo.PrescriptionItems 
SET AvailabilityStatus = N'Out of Stock / External' 
WHERE PrescriptionItemId = @ItemId;";

            using (var cmd = new SqlCommand(updSql, conn, tran))
            {
                cmd.Parameters.Add("@ItemId", SqlDbType.Int).Value = prescriptionItemId;
                cmd.ExecuteNonQuery();
            }

            // Check if all items in prescription are now finalized
            const string checkAllSql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.PrescriptionItems WHERE PrescriptionId = @RxId AND AvailabilityStatus NOT IN (N'Dispensed', N'Out of Stock / External'))
BEGIN
    UPDATE dbo.Prescriptions SET PrescriptionStatus = N'Dispensed' WHERE PrescriptionId = @RxId;
END";
            using (var cmdCheck = new SqlCommand(checkAllSql, conn, tran))
            {
                cmdCheck.Parameters.Add("@RxId", SqlDbType.Int).Value = rxId;
                cmdCheck.ExecuteNonQuery();
            }

            tran.Commit();
            AuditLogger.Log("Update", "PrescriptionItems", prescriptionItemId, "Tagged item as Out of Stock / External.");
            return (true, "Item marked as Out of Stock / External. An official doctor prescription slip is ready to print.");
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return (false, "Error tagging item: " + ex.Message);
        }
    }

    public PrescriptionSlipData? GetOfficialRxData(int prescriptionId)
    {
        const string sqlHeader = @"
SELECT 
    rx.PrescriptionId,
    rx.PrescriptionNumber,
    rx.CreatedAt,
    d.DoctorId,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(d.Specialty, N'General Medicine') AS Specialty,
    ISNULL(d.LicenseNumber, N'PRC-0091823') AS LicenseNumber,
    CONCAT(N'PTR No. 892', RIGHT(CONCAT(N'000', d.DoctorId), 4), N'-DVO') AS PtrNumber,
    CONCAT(N'S2-092', RIGHT(CONCAT(N'00', d.DoctorId), 3), N'-DVO') AS S2Number,
    pt.PatientId,
    pt.PatientNumber,
    CONCAT(pt.LastName, N', ', pt.FirstName) AS PatientName,
    DATEDIFF(YEAR, pt.BirthDate, GETDATE()) AS Age,
    pt.Sex,
    ISNULL(CONCAT(pt.City, N', Davao del Sur'), N'Davao City, Davao del Sur') AS PatientAddress
FROM dbo.Prescriptions rx
INNER JOIN dbo.Patients pt ON pt.PatientId = rx.PatientId
INNER JOIN dbo.Doctors d ON d.DoctorId = rx.DoctorId
WHERE rx.PrescriptionId = @PrescriptionId;";

        PrescriptionSlipData? slip = null;
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sqlHeader, conn);
        cmd.Parameters.Add("@PrescriptionId", SqlDbType.Int).Value = prescriptionId;
        conn.Open();

        using (var rdr = cmd.ExecuteReader())
        {
            if (rdr.Read())
            {
                slip = new PrescriptionSlipData
                {
                    PrescriptionId = rdr.GetInt32(0),
                    PrescriptionNumber = rdr.GetString(1),
                    DatePrescribed = rdr.GetDateTime(2),
                    DoctorId = rdr.GetInt32(3),
                    DoctorName = rdr.GetString(4),
                    Specialty = rdr.GetString(5),
                    LicenseNumber = rdr.GetString(6),
                    PtrNumber = rdr.GetString(7),
                    S2Number = rdr.GetString(8),
                    PatientId = rdr.GetInt32(9),
                    PatientNumber = rdr.GetString(10),
                    PatientName = rdr.GetString(11),
                    Age = rdr.GetInt32(12),
                    Sex = rdr.GetString(13),
                    PatientAddress = rdr.GetString(14)
                };
            }
        }

        if (slip == null) return null;

        // Fetch prescription items
        const string sqlItems = @"
SELECT 
    m.GenericName,
    m.BrandName,
    m.DosageForm,
    m.Strength,
    pi.QuantityPrescribed,
    pi.Instructions,
    CASE WHEN pi.AvailabilityStatus = N'Out of Stock / External' THEN 1 ELSE 0 END AS IsExternal
FROM dbo.PrescriptionItems pi
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE pi.PrescriptionId = @PrescriptionId
ORDER BY pi.PrescriptionItemId ASC;";

        using var cmdItems = new SqlCommand(sqlItems, conn);
        cmdItems.Parameters.Add("@PrescriptionId", SqlDbType.Int).Value = prescriptionId;

        using (var rdrItems = cmdItems.ExecuteReader())
        {
            while (rdrItems.Read())
            {
                slip.Items.Add(new PrescriptionSlipItem
                {
                    GenericName = rdrItems.GetString(0),
                    BrandName = rdrItems.IsDBNull(1) ? null : rdrItems.GetString(1),
                    DosageForm = rdrItems.GetString(2),
                    Strength = rdrItems.IsDBNull(3) ? null : rdrItems.GetString(3),
                    Quantity = rdrItems.GetInt32(4),
                    Instructions = rdrItems.GetString(5),
                    IsExternalUnavailable = rdrItems.GetInt32(6) == 1
                });
            }
        }

        return slip;
    }

    // =========================================================================
    // 4. INVENTORY & QOL RESTOCKING STATION
    // =========================================================================
    public List<PharmacyInventoryItem> GetInventory(string category = "All", string search = "")
    {
        const string sql = @"
WITH ActiveBatches AS (
    SELECT 
        InventoryId,
        MedicineId,
        BatchNumber,
        ExpiryDate,
        QuantityOnHand,
        ROW_NUMBER() OVER (PARTITION BY MedicineId ORDER BY ExpiryDate ASC, BatchNumber ASC, InventoryId ASC) AS BatchSeq
    FROM dbo.MedicineInventory
    WHERE QuantityOnHand > 0
),
TotalStocks AS (
    SELECT 
        MedicineId,
        SUM(QuantityOnHand) AS TotalStock,
        COUNT(*) AS ActiveBatchCount
    FROM dbo.MedicineInventory
    WHERE QuantityOnHand > 0
    GROUP BY MedicineId
)
SELECT 
    m.MedicineId,
    m.GenericName,
    m.BrandName,
    ISNULL(m.Category, N'General') AS Category,
    m.DosageForm,
    m.Strength,
    m.UnitPrice,
    m.ReorderLevel,
    ISNULL(ts.TotalStock, 0) AS TotalStock,
    ISNULL(ts.ActiveBatchCount, 0) AS ActiveBatchCount,
    ab.InventoryId,
    ab.BatchNumber,
    ab.ExpiryDate,
    ISNULL(ab.QuantityOnHand, 0) AS BatchQuantityOnHand,
    ISNULL(ab.BatchSeq, 1) AS BatchSeq,
    ISNULL(m.CostPrice, 0) AS CostPrice
FROM dbo.Medicines m
LEFT JOIN TotalStocks ts ON ts.MedicineId = m.MedicineId
LEFT JOIN ActiveBatches ab ON ab.MedicineId = m.MedicineId
WHERE m.IsActive = 1
  AND (@Category = N'All' OR m.Category = @Category)
  AND (@Search = '' OR (
      m.GenericName LIKE @SearchLike OR 
      m.BrandName LIKE @SearchLike OR 
      m.Category LIKE @SearchLike OR
      ab.BatchNumber LIKE @SearchLike
  ))
ORDER BY m.Category, m.GenericName, m.MedicineId, ISNULL(ab.BatchSeq, 1) ASC;";

        var list = new List<PharmacyInventoryItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 80).Value = category;
        cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = search.Trim();
        cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 100).Value = $"%{search.Trim()}%";
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new PharmacyInventoryItem
            {
                MedicineId = rdr.GetInt32(0),
                GenericName = rdr.GetString(1),
                BrandName = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                Category = rdr.GetString(3),
                DosageForm = rdr.GetString(4),
                Strength = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                UnitPrice = rdr.GetDecimal(6),
                ReorderLevel = rdr.GetInt32(7),
                TotalStock = rdr.GetInt32(8),
                ActiveBatchCount = rdr.GetInt32(9),
                InventoryId = rdr.IsDBNull(10) ? null : rdr.GetInt32(10),
                BatchNumber = rdr.IsDBNull(11) ? null : rdr.GetString(11),
                ExpiryDate = rdr.IsDBNull(12) ? null : rdr.GetDateTime(12),
                QuantityOnHand = rdr.GetInt32(13),
                BatchSeq = Convert.ToInt32(rdr[14]),
                CostPrice = rdr.GetDecimal(15)
            });
        }
        return list;
    }

    public (bool Success, string Message) QuickRestock(int medicineId, int quantityToAdd, int userId)
    {
        return RestockMedicine(medicineId, quantityToAdd, $"BAT-{DateTime.Now:yyyyMMdd}", DateTime.Now.AddYears(2), userId);
    }

    public (bool Success, string Message) RestockMedicine(int medicineId, int quantityToAdd, string? batchNumber, DateTime? expiryDate, int userId, decimal? costPrice = null)
    {
        if (quantityToAdd <= 0)
            return (false, "Quantity must be greater than 0.");

        EnsurePharmacyFinancialSchemaExists();

        string batch = string.IsNullOrWhiteSpace(batchNumber) ? $"BAT-{DateTime.Now:yyyyMMdd}" : batchNumber.Trim();
        DateTime expiry = expiryDate ?? DateTime.Now.AddYears(2);
        decimal cost = costPrice.HasValue && costPrice.Value >= 0 ? costPrice.Value : 0.00m;

        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            // Check if batch exists
            const string checkBatchSql = @"
SELECT InventoryId 
FROM dbo.MedicineInventory 
WHERE MedicineId = @MedicineId AND BatchNumber = @Batch;";

            int? existingInvId = null;
            using (var cmdCheck = new SqlCommand(checkBatchSql, conn, tran))
            {
                cmdCheck.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medicineId;
                cmdCheck.Parameters.Add("@Batch", SqlDbType.NVarChar, 50).Value = batch;
                var obj = cmdCheck.ExecuteScalar();
                if (obj != null && obj != DBNull.Value) existingInvId = Convert.ToInt32(obj);
            }

            if (existingInvId.HasValue)
            {
                const string updSql = @"
UPDATE dbo.MedicineInventory 
SET QuantityOnHand = QuantityOnHand + @Qty, 
    CostPrice = CASE WHEN @Cost > 0 THEN @Cost ELSE CostPrice END,
    LastUpdated = SYSUTCDATETIME()
WHERE InventoryId = @InvId;

IF @Cost > 0
BEGIN
    UPDATE dbo.Medicines SET CostPrice = @Cost WHERE MedicineId = @MedicineId;
END";

                using var cmdUpd = new SqlCommand(updSql, conn, tran);
                cmdUpd.Parameters.Add("@Qty", SqlDbType.Int).Value = quantityToAdd;
                cmdUpd.Parameters.Add("@Cost", SqlDbType.Decimal).Value = cost;
                cmdUpd.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medicineId;
                cmdUpd.Parameters.Add("@InvId", SqlDbType.Int).Value = existingInvId.Value;
                cmdUpd.ExecuteNonQuery();
            }
            else
            {
                const string insSql = @"
INSERT INTO dbo.MedicineInventory (MedicineId, BatchNumber, ExpiryDate, QuantityOnHand, CostPrice, LastUpdated)
VALUES (@MedicineId, @Batch, @Expiry, @Qty, @Cost, SYSUTCDATETIME());

IF @Cost > 0
BEGIN
    UPDATE dbo.Medicines SET CostPrice = @Cost WHERE MedicineId = @MedicineId;
END";

                using var cmdIns = new SqlCommand(insSql, conn, tran);
                cmdIns.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medicineId;
                cmdIns.Parameters.Add("@Batch", SqlDbType.NVarChar, 50).Value = batch;
                cmdIns.Parameters.Add("@Expiry", SqlDbType.Date).Value = expiry.Date;
                cmdIns.Parameters.Add("@Qty", SqlDbType.Int).Value = quantityToAdd;
                cmdIns.Parameters.Add("@Cost", SqlDbType.Decimal).Value = cost;
                cmdIns.ExecuteNonQuery();
            }

            tran.Commit();

            AuditLogger.Log("Insert", "MedicineInventory", medicineId, $"Restocked {quantityToAdd} pcs for Medicine #{medicineId} (Batch: {batch}).");
            return (true, $"Restocked +{quantityToAdd} pcs successfully!");
        }
        catch (Exception ex)
        {
            tran.Rollback();
            string msg = ErrorLogger.Log(ex, "PharmacyRepository.RestockMedicine");
            return (false, "Error restocking medicine: " + msg);
        }
    }

    public (bool Success, string Message) AdjustInventory(int medicineId, int quantityDeducted, string reason, int userId)
    {
        if (quantityDeducted <= 0) return (false, "Deduction quantity must be greater than 0.");
        if (string.IsNullOrWhiteSpace(reason)) return (false, "A reason for inventory adjustment is required.");

        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            const string invSql = @"
SELECT InventoryId, QuantityOnHand 
FROM dbo.MedicineInventory 
WHERE MedicineId = @MedicineId AND QuantityOnHand > 0
ORDER BY ExpiryDate ASC, InventoryId ASC;";

            var invRows = new List<(int InvId, int Qty)>();
            using (var cmdInv = new SqlCommand(invSql, conn, tran))
            {
                cmdInv.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medicineId;
                using var rdr = cmdInv.ExecuteReader();
                while (rdr.Read())
                {
                    invRows.Add((rdr.GetInt32(0), rdr.GetInt32(1)));
                }
            }

            int totalAvailable = invRows.Sum(r => r.Qty);
            if (totalAvailable < quantityDeducted)
            {
                tran.Rollback();
                return (false, $"Cannot write off {quantityDeducted} units. Total available stock is {totalAvailable}.");
            }

            int remaining = quantityDeducted;
            foreach (var r in invRows)
            {
                if (remaining <= 0) break;
                int take = Math.Min(remaining, r.Qty);

                const string deductSql = @"
UPDATE dbo.MedicineInventory 
SET QuantityOnHand = QuantityOnHand - @Take, LastUpdated = SYSUTCDATETIME()
WHERE InventoryId = @InvId;";

                using var cmdDeduct = new SqlCommand(deductSql, conn, tran);
                cmdDeduct.Parameters.Add("@Take", SqlDbType.Int).Value = take;
                cmdDeduct.Parameters.Add("@InvId", SqlDbType.Int).Value = r.InvId;
                cmdDeduct.ExecuteNonQuery();

                remaining -= take;
            }

            tran.Commit();

            AuditLogger.Log("Update", "MedicineInventory", medicineId, $"Adjusted/Written-off {quantityDeducted} pcs for Medicine #{medicineId}. Reason: {reason}");
            return (true, $"Successfully written off {quantityDeducted} units. Reason logged: {reason}");
        }
        catch (Exception ex)
        {
            tran.Rollback();
            string msg = ErrorLogger.Log(ex, "PharmacyRepository.AdjustInventory");
            return (false, "Error adjusting inventory: " + msg);
        }
    }

    public void EnsureDisposalTableExists()
    {
        const string sql = @"
IF OBJECT_ID(N'dbo.MedicineDisposalLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MedicineDisposalLogs
    (
        DisposalId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
        MedicineId INT NOT NULL,
        BatchNumber NVARCHAR(60) NOT NULL,
        ExpiryDate DATE NULL,
        QuantityDisposed INT NOT NULL,
        UnitPrice DECIMAL(12, 2) NOT NULL CONSTRAINT DF_Disposal_UnitPrice DEFAULT (0),
        TotalLossAmount DECIMAL(12, 2) NOT NULL CONSTRAINT DF_Disposal_TotalLoss DEFAULT (0),
        Reason NVARCHAR(100) NOT NULL,
        DisposalMethod NVARCHAR(150) NULL,
        Notes NVARCHAR(500) NULL,
        DisposedByUserId INT NULL,
        DisposedBy NVARCHAR(100) NOT NULL CONSTRAINT DF_Disposal_DisposedBy DEFAULT (N'Central Pharmacist'),
        DisposedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Disposal_DisposedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Disposal_Medicines FOREIGN KEY (MedicineId) REFERENCES dbo.Medicines(MedicineId)
    );
END;";

        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void EnsurePharmacyFinancialSchemaExists()
    {
        EnsureDisposalTableExists();

        const string sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Medicines') AND name = 'CostPrice')
BEGIN
    ALTER TABLE dbo.Medicines ADD CostPrice DECIMAL(12, 2) NOT NULL CONSTRAINT DF_Medicines_CostPrice DEFAULT (0.00);
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.MedicineInventory') AND name = 'CostPrice')
BEGIN
    ALTER TABLE dbo.MedicineInventory ADD CostPrice DECIMAL(12, 2) NULL;
END;";

        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public PharmacyFinancialSummary GetFinancialAnalytics(string period = "This Month")
    {
        EnsurePharmacyFinancialSchemaExists();

        var summary = new PharmacyFinancialSummary();

        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();

        // Date conditions
        string dateCondPrescription = period switch
        {
            "Today" => "AND CAST(p.CreatedAt AS DATE) = CAST(GETDATE() AS DATE)",
            "This Week" => "AND p.CreatedAt >= DATEADD(day, 1 - DATEPART(weekday, GETDATE()), CAST(GETDATE() AS DATE))",
            "This Month" => "AND p.CreatedAt >= DATEADD(day, 1 - DAY(GETDATE()), CAST(GETDATE() AS DATE))",
            "Year-to-Date" => "AND p.CreatedAt >= DATEFROMPARTS(YEAR(GETDATE()), 1, 1)",
            _ => ""
        };

        string dateCondDisposal = period switch
        {
            "Today" => "WHERE CAST(d.DisposedAt AS DATE) = CAST(GETDATE() AS DATE)",
            "This Week" => "WHERE d.DisposedAt >= DATEADD(day, 1 - DATEPART(weekday, GETDATE()), CAST(GETDATE() AS DATE))",
            "This Month" => "WHERE d.DisposedAt >= DATEADD(day, 1 - DAY(GETDATE()), CAST(GETDATE() AS DATE))",
            "Year-to-Date" => "WHERE d.DisposedAt >= DATEFROMPARTS(YEAR(GETDATE()), 1, 1)",
            _ => ""
        };

        // Query 1: Dispensed Revenue & COGS
        string revSql = $@"
SELECT 
    ISNULL(SUM(pi.QuantityPrescribed * m.UnitPrice), 0) AS GrossSales,
    ISNULL(SUM(pi.QuantityPrescribed * CASE WHEN m.CostPrice > 0 THEN m.CostPrice ELSE m.UnitPrice * 0.65 END), 0) AS COGS,
    COUNT(DISTINCT p.PrescriptionId) AS PrescriptionsCount
FROM dbo.PrescriptionItems pi
INNER JOIN dbo.Prescriptions p ON p.PrescriptionId = pi.PrescriptionId
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE pi.AvailabilityStatus = N'Dispensed' {dateCondPrescription};";

        using (var cmdRev = new SqlCommand(revSql, conn))
        using (var rdrRev = cmdRev.ExecuteReader())
        {
            if (rdrRev.Read())
            {
                summary.GrossSalesRevenue = rdrRev.GetDecimal(0);
                summary.CostOfGoodsSold = rdrRev.GetDecimal(1);
                summary.PrescriptionsDispensedCount = rdrRev.GetInt32(2);
            }
        }

        // Query 2: Disposal Losses
        string dispSql = $@"
SELECT 
    ISNULL(SUM(d.TotalLossAmount), 0) AS TotalLoss,
    COUNT(*) AS DisposalCount
FROM dbo.MedicineDisposalLogs d
{dateCondDisposal};";

        using (var cmdDisp = new SqlCommand(dispSql, conn))
        using (var rdrDisp = cmdDisp.ExecuteReader())
        {
            if (rdrDisp.Read())
            {
                summary.TotalDisposalLoss = rdrDisp.GetDecimal(0);
                summary.DisposalCount = rdrDisp.GetInt32(1);
            }
        }

        // Query 3: Missed Opportunity (Out of Stock tagged)
        string oosSql = $@"
SELECT 
    ISNULL(SUM(pi.QuantityPrescribed * m.UnitPrice), 0) AS MissedSales,
    COUNT(*) AS MissedItemsCount
FROM dbo.PrescriptionItems pi
INNER JOIN dbo.Prescriptions p ON p.PrescriptionId = pi.PrescriptionId
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE pi.AvailabilityStatus = N'Out of Stock / External' {dateCondPrescription};";

        using (var cmdOos = new SqlCommand(oosSql, conn))
        using (var rdrOos = cmdOos.ExecuteReader())
        {
            if (rdrOos.Read())
            {
                summary.MissedSalesOpportunity = rdrOos.GetDecimal(0);
                summary.MissedRxItemsCount = rdrOos.GetInt32(1);
            }
        }

        // Query 4: Active Inventory Capital & Capital at Risk (Expiry <= 30 days)
        const string invSql = @"
SELECT 
    ISNULL(SUM(inv.QuantityOnHand * m.UnitPrice), 0) AS TotalCapital,
    ISNULL(SUM(CASE WHEN inv.ExpiryDate <= DATEADD(day, 30, CAST(GETDATE() AS DATE)) THEN inv.QuantityOnHand * m.UnitPrice ELSE 0 END), 0) AS CapitalAtRisk,
    COUNT(DISTINCT m.MedicineId) AS FormulationsCount
FROM dbo.MedicineInventory inv
INNER JOIN dbo.Medicines m ON m.MedicineId = inv.MedicineId
WHERE inv.QuantityOnHand > 0 AND m.IsActive = 1;";

        using (var cmdInv = new SqlCommand(invSql, conn))
        using (var rdrInv = cmdInv.ExecuteReader())
        {
            if (rdrInv.Read())
            {
                summary.ActiveInventoryCapital = rdrInv.GetDecimal(0);
                summary.CapitalAtRisk30Days = rdrInv.GetDecimal(1);
                summary.FormulationsCount = rdrInv.GetInt32(2);
            }
        }

        // Query 5: Top Revenue Drugs
        string topSql = $@"
SELECT TOP 10
    m.MedicineId,
    m.GenericName,
    m.BrandName,
    m.DosageForm,
    m.Strength,
    ISNULL(m.Category, N'General') AS Category,
    SUM(pi.QuantityPrescribed) AS UnitsDispensed,
    SUM(pi.QuantityPrescribed * m.UnitPrice) AS GrossRevenue,
    SUM(pi.QuantityPrescribed * (m.UnitPrice - CASE WHEN m.CostPrice > 0 THEN m.CostPrice ELSE m.UnitPrice * 0.65 END)) AS EstProfit
FROM dbo.PrescriptionItems pi
INNER JOIN dbo.Prescriptions p ON p.PrescriptionId = pi.PrescriptionId
INNER JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE pi.AvailabilityStatus = N'Dispensed' {dateCondPrescription}
GROUP BY m.MedicineId, m.GenericName, m.BrandName, m.DosageForm, m.Strength, m.Category
ORDER BY GrossRevenue DESC;";

        using (var cmdTop = new SqlCommand(topSql, conn))
        using (var rdrTop = cmdTop.ExecuteReader())
        {
            while (rdrTop.Read())
            {
                int medId = rdrTop.GetInt32(0);
                string gen = rdrTop.GetString(1);
                string? br = rdrTop.IsDBNull(2) ? null : rdrTop.GetString(2);
                string form = rdrTop.GetString(3);
                string? str = rdrTop.IsDBNull(4) ? null : rdrTop.GetString(4);
                string cat = rdrTop.GetString(5);
                int units = rdrTop.GetInt32(6);
                decimal rev = rdrTop.GetDecimal(7);
                decimal profit = rdrTop.GetDecimal(8);

                string dispName = string.IsNullOrWhiteSpace(br)
                    ? $"{gen} ({str} {form})"
                    : $"{gen} [{br}] ({str} {form})";

                decimal share = summary.GrossSalesRevenue > 0 ? (rev / summary.GrossSalesRevenue) * 100m : 0m;

                summary.TopRevenueDrugs.Add(new TopRevenueDrugItem
                {
                    MedicineId = medId,
                    MedicineDisplayName = dispName,
                    Category = cat,
                    UnitsDispensed = units,
                    GrossRevenue = rev,
                    EstimatedProfit = profit,
                    SalesSharePct = Math.Round(share, 1)
                });
            }
        }

        // Query 6: Imminent Expiry Risks (< 30 days)
        const string riskSql = @"
SELECT 
    inv.MedicineId,
    m.GenericName,
    m.BrandName,
    m.DosageForm,
    m.Strength,
    inv.BatchNumber,
    inv.ExpiryDate,
    inv.QuantityOnHand,
    m.UnitPrice
FROM dbo.MedicineInventory inv
INNER JOIN dbo.Medicines m ON m.MedicineId = inv.MedicineId
WHERE inv.QuantityOnHand > 0 
  AND inv.ExpiryDate <= DATEADD(day, 30, CAST(GETDATE() AS DATE))
  AND m.IsActive = 1
ORDER BY inv.ExpiryDate ASC;";

        using (var cmdRisk = new SqlCommand(riskSql, conn))
        using (var rdrRisk = cmdRisk.ExecuteReader())
        {
            while (rdrRisk.Read())
            {
                string gen = rdrRisk.GetString(1);
                string? br = rdrRisk.IsDBNull(2) ? null : rdrRisk.GetString(2);
                string form = rdrRisk.GetString(3);
                string? str = rdrRisk.IsDBNull(4) ? null : rdrRisk.GetString(4);

                string dispName = string.IsNullOrWhiteSpace(br)
                    ? $"{gen} ({str} {form})"
                    : $"{gen} [{br}] ({str} {form})";

                summary.ImminentExpiryRisks.Add(new AtRiskExpiryItem
                {
                    MedicineId = rdrRisk.GetInt32(0),
                    MedicineDisplayName = dispName,
                    BatchNumber = rdrRisk.GetString(5),
                    ExpiryDate = rdrRisk.IsDBNull(6) ? null : rdrRisk.GetDateTime(6),
                    QuantityOnHand = rdrRisk.GetInt32(7),
                    UnitPrice = rdrRisk.GetDecimal(8)
                });
            }
        }

        // Query 7: Loss Reasons Breakdown
        string lossReasonSql = $@"
SELECT 
    d.Reason,
    COUNT(*) AS BatchesDestroyed,
    SUM(d.QuantityDisposed) AS UnitsDestroyed,
    SUM(d.TotalLossAmount) AS TotalLoss
FROM dbo.MedicineDisposalLogs d
{dateCondDisposal}
GROUP BY d.Reason
ORDER BY TotalLoss DESC;";

        using (var cmdLoss = new SqlCommand(lossReasonSql, conn))
        using (var rdrLoss = cmdLoss.ExecuteReader())
        {
            while (rdrLoss.Read())
            {
                string rsn = rdrLoss.GetString(0);
                int batches = rdrLoss.GetInt32(1);
                int units = rdrLoss.GetInt32(2);
                decimal lossAmt = rdrLoss.GetDecimal(3);
                decimal share = summary.TotalDisposalLoss > 0 ? (lossAmt / summary.TotalDisposalLoss) * 100m : 0m;

                summary.LossReasons.Add(new DisposalLossReasonItem
                {
                    Reason = rsn,
                    BatchesDestroyed = batches,
                    UnitsDestroyed = units,
                    TotalLossAmount = lossAmt,
                    LossSharePct = Math.Round(share, 1)
                });
            }
        }

        return summary;
    }

    public List<DisposalCandidateItem> GetDisposalCandidates(string filter = "AllExpiring")
    {
        EnsureDisposalTableExists();

        string whereClause = filter switch
        {
            "ExpiredOnly" => "AND inv.ExpiryDate < CAST(GETDATE() AS DATE)",
            "Critical30" => "AND inv.ExpiryDate <= DATEADD(day, 30, CAST(GETDATE() AS DATE))",
            "NearExpiry60" => "AND inv.ExpiryDate <= DATEADD(day, 60, CAST(GETDATE() AS DATE))",
            "Standard90" => "AND inv.ExpiryDate <= DATEADD(day, 90, CAST(GETDATE() AS DATE))",
            "AllActive" => "",
            _ => "AND (inv.ExpiryDate <= DATEADD(day, 90, CAST(GETDATE() AS DATE)) OR inv.ExpiryDate < CAST(GETDATE() AS DATE))"
        };

        string sql = $@"
SELECT 
    m.MedicineId,
    inv.InventoryId,
    m.GenericName,
    m.BrandName,
    m.DosageForm,
    m.Strength,
    inv.BatchNumber,
    inv.ExpiryDate,
    inv.QuantityOnHand,
    m.UnitPrice
FROM dbo.MedicineInventory inv
INNER JOIN dbo.Medicines m ON m.MedicineId = inv.MedicineId
WHERE m.IsActive = 1 AND inv.QuantityOnHand > 0 {whereClause}
ORDER BY 
    CASE WHEN inv.ExpiryDate < CAST(GETDATE() AS DATE) THEN 0 ELSE 1 END,
    inv.ExpiryDate ASC,
    m.GenericName ASC;";

        var list = new List<DisposalCandidateItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            int qty = rdr.GetInt32(8);
            list.Add(new DisposalCandidateItem
            {
                MedicineId = rdr.GetInt32(0),
                InventoryId = rdr.GetInt32(1),
                GenericName = rdr.GetString(2),
                BrandName = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                DosageForm = rdr.GetString(4),
                Strength = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                BatchNumber = rdr.GetString(6),
                ExpiryDate = rdr.IsDBNull(7) ? null : rdr.GetDateTime(7),
                QuantityOnHand = qty,
                QuantityToDispose = qty,
                UnitPrice = rdr.GetDecimal(9)
            });
        }
        return list;
    }

    public (bool Success, string Message, int DisposalId) DisposeBatch(
        int medicineId,
        string batchNumber,
        int quantityToDispose,
        string reason,
        string? disposalMethod,
        string? notes,
        int userId,
        string userName)
    {
        EnsureDisposalTableExists();

        if (quantityToDispose <= 0)
            return (false, "Quantity to dispose must be greater than 0.", 0);
        if (string.IsNullOrWhiteSpace(reason))
            return (false, "Disposal reason is required.", 0);

        string cleanBatch = batchNumber.Trim();
        string cleanReason = reason.Trim();
        string cleanMethod = string.IsNullOrWhiteSpace(disposalMethod) ? "Standard Clinic Decommissioning Protocol" : disposalMethod.Trim();
        string cleanUser = string.IsNullOrWhiteSpace(userName) ? "Central Pharmacist" : userName.Trim();

        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            // 1. Check medicine & specific batch
            const string chkSql = @"
SELECT inv.InventoryId, inv.QuantityOnHand, inv.ExpiryDate, m.GenericName, m.UnitPrice
FROM dbo.MedicineInventory inv
INNER JOIN dbo.Medicines m ON m.MedicineId = inv.MedicineId
WHERE inv.MedicineId = @MedicineId AND inv.BatchNumber = @Batch;";

            int invId = 0;
            int currentQty = 0;
            DateTime? expiry = null;
            string genName = "";
            decimal unitPrice = 0;

            using (var cmdChk = new SqlCommand(chkSql, conn, tran))
            {
                cmdChk.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medicineId;
                cmdChk.Parameters.Add("@Batch", SqlDbType.NVarChar, 60).Value = cleanBatch;
                using var rdr = cmdChk.ExecuteReader();
                if (rdr.Read())
                {
                    invId = rdr.GetInt32(0);
                    currentQty = rdr.GetInt32(1);
                    expiry = rdr.IsDBNull(2) ? null : rdr.GetDateTime(2);
                    genName = rdr.GetString(3);
                    unitPrice = rdr.GetDecimal(4);
                }
                else
                {
                    tran.Rollback();
                    return (false, $"Batch '{cleanBatch}' for Medicine #{medicineId} was not found in inventory.", 0);
                }
            }

            if (currentQty < quantityToDispose)
            {
                tran.Rollback();
                return (false, $"Cannot dispose {quantityToDispose} units. Current stock for lot '{cleanBatch}' is only {currentQty} units.", 0);
            }

            // 2. Decrement or zero-out lot
            const string updSql = @"
UPDATE dbo.MedicineInventory
SET QuantityOnHand = QuantityOnHand - @Take,
    LastUpdated = SYSUTCDATETIME()
WHERE InventoryId = @InvId;";

            using (var cmdUpd = new SqlCommand(updSql, conn, tran))
            {
                cmdUpd.Parameters.Add("@Take", SqlDbType.Int).Value = quantityToDispose;
                cmdUpd.Parameters.Add("@InvId", SqlDbType.Int).Value = invId;
                cmdUpd.ExecuteNonQuery();
            }

            decimal totalLoss = quantityToDispose * unitPrice;

            // 3. Insert into MedicineDisposalLogs
            const string insLogSql = @"
INSERT INTO dbo.MedicineDisposalLogs
(MedicineId, BatchNumber, ExpiryDate, QuantityDisposed, UnitPrice, TotalLossAmount, Reason, DisposalMethod, Notes, DisposedByUserId, DisposedBy, DisposedAt)
OUTPUT INSERTED.DisposalId
VALUES
(@MedicineId, @Batch, @Expiry, @Qty, @UnitPrice, @Loss, @Reason, @Method, @Notes, @UserId, @User, SYSUTCDATETIME());";

            int disposalId = 0;
            using (var cmdIns = new SqlCommand(insLogSql, conn, tran))
            {
                cmdIns.Parameters.Add("@MedicineId", SqlDbType.Int).Value = medicineId;
                cmdIns.Parameters.Add("@Batch", SqlDbType.NVarChar, 60).Value = cleanBatch;
                cmdIns.Parameters.Add("@Expiry", SqlDbType.Date).Value = (object?)expiry?.Date ?? DBNull.Value;
                cmdIns.Parameters.Add("@Qty", SqlDbType.Int).Value = quantityToDispose;
                cmdIns.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = unitPrice;
                cmdIns.Parameters.Add("@Loss", SqlDbType.Decimal).Value = totalLoss;
                cmdIns.Parameters.Add("@Reason", SqlDbType.NVarChar, 100).Value = cleanReason;
                cmdIns.Parameters.Add("@Method", SqlDbType.NVarChar, 150).Value = cleanMethod;
                cmdIns.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value = (object?)notes?.Trim() ?? DBNull.Value;
                cmdIns.Parameters.Add("@UserId", SqlDbType.Int).Value = userId > 0 ? userId : DBNull.Value;
                cmdIns.Parameters.Add("@User", SqlDbType.NVarChar, 100).Value = cleanUser;

                disposalId = (int)cmdIns.ExecuteScalar()!;
            }

            tran.Commit();

            AuditLogger.Log("Disposal", "MedicineInventory", medicineId, $"Disposed {quantityToDispose} pcs of '{genName}' (Lot: {cleanBatch}). Reason: {cleanReason}. Financial Loss: ₱{totalLoss:N2}. Authorized by: {cleanUser}");

            return (true, $"Disposed {quantityToDispose} pcs of '{genName}' (Lot: {cleanBatch}) successfully.", disposalId);
        }
        catch (Exception ex)
        {
            tran.Rollback();
            string msg = ErrorLogger.Log(ex, "PharmacyRepository.ExecuteDisposal");
            return (false, "Error executing disposal: " + msg, 0);
        }
    }

    public List<MedicineDisposalLogItem> GetDisposalLogs(string search = "")
    {
        EnsureDisposalTableExists();

        const string sql = @"
SELECT 
    d.DisposalId,
    d.MedicineId,
    m.GenericName,
    m.BrandName,
    m.DosageForm,
    m.Strength,
    d.BatchNumber,
    d.ExpiryDate,
    d.QuantityDisposed,
    d.UnitPrice,
    d.TotalLossAmount,
    d.Reason,
    d.DisposalMethod,
    d.Notes,
    d.DisposedByUserId,
    d.DisposedBy,
    d.DisposedAt
FROM dbo.MedicineDisposalLogs d
INNER JOIN dbo.Medicines m ON m.MedicineId = d.MedicineId
WHERE (@Search = '' OR (
    m.GenericName LIKE @SearchLike OR
    m.BrandName LIKE @SearchLike OR
    d.BatchNumber LIKE @SearchLike OR
    d.Reason LIKE @SearchLike OR
    d.DisposedBy LIKE @SearchLike
))
ORDER BY d.DisposedAt DESC;";

        var list = new List<MedicineDisposalLogItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = search.Trim();
        cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 100).Value = $"%{search.Trim()}%";
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new MedicineDisposalLogItem
            {
                DisposalId = rdr.GetInt32(0),
                MedicineId = rdr.GetInt32(1),
                GenericName = rdr.GetString(2),
                BrandName = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                DosageForm = rdr.GetString(4),
                Strength = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                BatchNumber = rdr.GetString(6),
                ExpiryDate = rdr.IsDBNull(7) ? null : rdr.GetDateTime(7),
                QuantityDisposed = rdr.GetInt32(8),
                UnitPrice = rdr.GetDecimal(9),
                TotalLossAmount = rdr.GetDecimal(10),
                Reason = rdr.GetString(11),
                DisposalMethod = rdr.IsDBNull(12) ? "Standard Protocol" : rdr.GetString(12),
                Notes = rdr.IsDBNull(13) ? null : rdr.GetString(13),
                DisposedByUserId = rdr.IsDBNull(14) ? null : rdr.GetInt32(14),
                DisposedBy = rdr.GetString(15),
                DisposedAt = rdr.GetDateTime(16)
            });
        }
        return list;
    }

    public (bool Success, string Message, int MedicineId) AddMedicine(
        string genericName,
        string? brandName,
        string category,
        string dosageForm,
        string? strength,
        decimal unitPrice,
        int reorderLevel,
        int initialStock,
        string? batchNumber,
        DateTime? expiryDate,
        int userId,
        decimal costPrice = 0.00m)
    {
        if (string.IsNullOrWhiteSpace(genericName))
            return (false, "Generic name is required.", 0);
        if (string.IsNullOrWhiteSpace(category))
            return (false, "Category is required.", 0);
        if (string.IsNullOrWhiteSpace(dosageForm))
            return (false, "Dosage form is required.", 0);
        if (unitPrice < 0)
            return (false, "Unit price cannot be negative.", 0);

        EnsurePharmacyFinancialSchemaExists();

        string gen = genericName.Trim();
        string? brand = string.IsNullOrWhiteSpace(brandName) ? null : brandName.Trim();
        string cat = category.Trim();
        string form = dosageForm.Trim();
        string? str = string.IsNullOrWhiteSpace(strength) ? null : strength.Trim();
        string batch = string.IsNullOrWhiteSpace(batchNumber) ? $"BAT-{DateTime.Now:yyyyMMdd}" : batchNumber.Trim();
        DateTime expiry = expiryDate ?? DateTime.Now.AddYears(2);
        decimal cost = costPrice >= 0 ? costPrice : 0.00m;

        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            // Check duplicate
            const string checkDupSql = @"
SELECT MedicineId FROM dbo.Medicines 
WHERE GenericName = @Generic AND DosageForm = @Form AND ISNULL(Strength, N'') = ISNULL(@Strength, N'') AND IsActive = 1;";

            using (var cmdCheck = new SqlCommand(checkDupSql, conn, tran))
            {
                cmdCheck.Parameters.Add("@Generic", SqlDbType.NVarChar, 150).Value = gen;
                cmdCheck.Parameters.Add("@Form", SqlDbType.NVarChar, 60).Value = form;
                cmdCheck.Parameters.Add("@Strength", SqlDbType.NVarChar, 60).Value = (object?)str ?? DBNull.Value;
                var dup = cmdCheck.ExecuteScalar();
                if (dup != null && dup != DBNull.Value)
                {
                    tran.Rollback();
                    return (false, $"A medication with the generic name '{gen}', form '{form}', and strength '{str ?? "N/A"}' already exists in the formulary.", 0);
                }
            }

            const string insMedSql = @"
INSERT INTO dbo.Medicines (GenericName, BrandName, Category, DosageForm, Strength, UnitPrice, CostPrice, ReorderLevel, IsActive)
VALUES (@Generic, @Brand, @Category, @Form, @Strength, @UnitPrice, @CostPrice, @Reorder, 1);
SELECT SCOPE_IDENTITY();";

            int newMedId;
            using (var cmdIns = new SqlCommand(insMedSql, conn, tran))
            {
                cmdIns.Parameters.Add("@Generic", SqlDbType.NVarChar, 150).Value = gen;
                cmdIns.Parameters.Add("@Brand", SqlDbType.NVarChar, 150).Value = (object?)brand ?? DBNull.Value;
                cmdIns.Parameters.Add("@Category", SqlDbType.NVarChar, 80).Value = cat;
                cmdIns.Parameters.Add("@Form", SqlDbType.NVarChar, 60).Value = form;
                cmdIns.Parameters.Add("@Strength", SqlDbType.NVarChar, 60).Value = (object?)str ?? DBNull.Value;
                cmdIns.Parameters.Add("@UnitPrice", SqlDbType.Decimal).Value = unitPrice;
                cmdIns.Parameters.Add("@CostPrice", SqlDbType.Decimal).Value = cost;
                cmdIns.Parameters.Add("@Reorder", SqlDbType.Int).Value = reorderLevel > 0 ? reorderLevel : 10;
                newMedId = Convert.ToInt32(cmdIns.ExecuteScalar());
            }

            if (initialStock > 0)
            {
                const string insInvSql = @"
INSERT INTO dbo.MedicineInventory (MedicineId, BatchNumber, ExpiryDate, QuantityOnHand, CostPrice, LastUpdated)
VALUES (@MedicineId, @Batch, @Expiry, @Qty, @CostPrice, SYSUTCDATETIME());";

                using var cmdInv = new SqlCommand(insInvSql, conn, tran);
                cmdInv.Parameters.Add("@MedicineId", SqlDbType.Int).Value = newMedId;
                cmdInv.Parameters.Add("@Batch", SqlDbType.NVarChar, 50).Value = batch;
                cmdInv.Parameters.Add("@Expiry", SqlDbType.Date).Value = expiry.Date;
                cmdInv.Parameters.Add("@Qty", SqlDbType.Int).Value = initialStock;
                cmdInv.Parameters.Add("@CostPrice", SqlDbType.Decimal).Value = cost;
                cmdInv.ExecuteNonQuery();
            }

            tran.Commit();
            AuditLogger.Log("Insert", "Medicines", newMedId, $"Added new medication '{gen}' (Brand: {brand ?? "None"}, Category: {cat}, Form: {form}, Initial Stock: {initialStock}).");
            return (true, $"'{gen}' has been added to the pharmacy formulary successfully.", newMedId);
        }
        catch (Exception ex)
        {
            tran.Rollback();
            string msg = ErrorLogger.Log(ex, "PharmacyRepository.AddMedicineWithInitialStock");
            return (false, "Error adding medicine: " + msg, 0);
        }
    }

    public List<string> GetCategories()
    {
        const string sql = "SELECT DISTINCT Category FROM dbo.Medicines WHERE Category IS NOT NULL AND IsActive = 1 ORDER BY Category;";
        var list = new List<string> { "All" };
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();
        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(rdr.GetString(0));
        }
        return list;
    }

    // =========================================================================
    // 5. DISPENSED AUDIT HISTORY
    // =========================================================================
    public List<DispensedAuditItem> GetDispensedHistory(string search = "")
    {
        const string sql = @"
SELECT 
    rx.PrescriptionId,
    rx.PrescriptionNumber,
    ISNULL(p.ReceiptNumber, N'N/A') AS ReceiptNumber,
    CONCAT(pt.LastName, N', ', pt.FirstName) AS PatientName,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(STRING_AGG(CONCAT(m.GenericName, N' (', pi.QuantityPrescribed, N')'), CHAR(13) + CHAR(10)) WITHIN GROUP (ORDER BY pi.PrescriptionItemId ASC), N'N/A') AS MedicineSummary,
    ISNULL(SUM(pi.QuantityPrescribed), 0) AS TotalQuantity,
    ISNULL(p.AmountPaid, 0) AS TotalAmount,
    ISNULL(rx.CreatedAt, GETDATE()) AS DispensedAt,
    ISNULL(u.FullName, N'Central Pharmacist') AS DispensedBy,
    rx.PrescriptionStatus
FROM dbo.Prescriptions rx
INNER JOIN dbo.Patients pt ON pt.PatientId = rx.PatientId
INNER JOIN dbo.Doctors d ON d.DoctorId = rx.DoctorId
LEFT JOIN dbo.Consultations c ON c.ConsultationId = rx.ConsultationId
LEFT JOIN dbo.PatientVisits v ON v.VisitId = c.VisitId
LEFT JOIN dbo.Appointments a ON a.AppointmentId = v.AppointmentId
LEFT JOIN dbo.Invoices inv ON inv.AppointmentId = a.AppointmentId
LEFT JOIN dbo.Payments p ON p.InvoiceId = inv.InvoiceId
LEFT JOIN dbo.Users u ON u.UserId = p.ProcessedByUserId
LEFT JOIN dbo.PrescriptionItems pi ON pi.PrescriptionId = rx.PrescriptionId
LEFT JOIN dbo.Medicines m ON m.MedicineId = pi.MedicineId
WHERE rx.PrescriptionStatus = N'Dispensed'
  AND (@Search = '' OR (
      pt.LastName LIKE @SearchLike OR 
      pt.FirstName LIKE @SearchLike OR 
      CONCAT(pt.LastName, N', ', pt.FirstName) LIKE @SearchLike OR
      CONCAT(pt.FirstName, N' ', pt.LastName) LIKE @SearchLike OR
      rx.PrescriptionNumber LIKE @SearchLike OR 
      p.ReceiptNumber LIKE @SearchLike OR
      d.LastName LIKE @SearchLike OR
      d.FirstName LIKE @SearchLike OR
      EXISTS (
          SELECT 1 FROM dbo.PrescriptionItems pi2
          INNER JOIN dbo.Medicines m2 ON m2.MedicineId = pi2.MedicineId
          WHERE pi2.PrescriptionId = rx.PrescriptionId
            AND (m2.GenericName LIKE @SearchLike OR ISNULL(m2.BrandName, '') LIKE @SearchLike)
      )
  ))
GROUP BY 
    rx.PrescriptionId, rx.PrescriptionNumber, p.ReceiptNumber, pt.LastName, pt.FirstName,
    d.FirstName, d.LastName, p.AmountPaid, rx.CreatedAt, u.FullName, rx.PrescriptionStatus
ORDER BY rx.PrescriptionId DESC;";

        var list = new List<DispensedAuditItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = search.Trim();
        cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 100).Value = $"%{search.Trim()}%";
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            string rawSummary = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
            string formattedSummary = rawSummary.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);

            list.Add(new DispensedAuditItem
            {
                PrescriptionId = rdr.GetInt32(0),
                PrescriptionNumber = rdr.GetString(1),
                ReceiptNumber = rdr.GetString(2),
                PatientName = rdr.GetString(3),
                DoctorName = rdr.GetString(4),
                MedicineSummary = formattedSummary,
                TotalQuantity = rdr.IsDBNull(6) ? 0 : rdr.GetInt32(6),
                TotalAmount = rdr.GetDecimal(7),
                DispensedAt = rdr.GetDateTime(8),
                DispensedBy = rdr.GetString(9),
                Status = rdr.GetString(10)
            });
        }
        return list;
    }

    // =========================================================================
    // 6. PHARMACIST MANAGEMENT (ADMIN)
    // =========================================================================
    public List<PharmacistRecord> GetAllPharmacists()
    {
        const string sql = @"
SELECT 
    ph.PharmacistId,
    ph.PharmacistCode,
    ph.UserId,
    u.Username,
    ph.FirstName,
    ph.LastName,
    ph.LicenseNumber,
    ph.ContactNumber,
    ph.EmailAddress,
    ph.IsActive,
    ph.CreatedAt
FROM dbo.Pharmacists ph
INNER JOIN dbo.Users u ON u.UserId = ph.UserId
ORDER BY ph.PharmacistId ASC;";

        var list = new List<PharmacistRecord>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new PharmacistRecord
            {
                PharmacistId = rdr.GetInt32(0),
                PharmacistCode = rdr.GetString(1),
                UserId = rdr.GetInt32(2),
                Username = rdr.GetString(3),
                FirstName = rdr.GetString(4),
                LastName = rdr.GetString(5),
                LicenseNumber = rdr.GetString(6),
                ContactNumber = rdr.GetString(7),
                EmailAddress = rdr.GetString(8),
                IsActive = rdr.GetBoolean(9),
                CreatedAt = rdr.GetDateTime(10)
            });
        }
        return list;
    }

    public (int PharmacistId, string PharmacistName, string LicenseNumber)? GetPharmacistByUserId(int userId)
    {
        const string sql = @"
SELECT PharmacistId, CONCAT(FirstName, N' ', LastName, N', RPh') AS FullName, LicenseNumber 
FROM dbo.Pharmacists 
WHERE UserId = @UserId AND IsActive = 1;";

        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            return (rdr.GetInt32(0), rdr.GetString(1), rdr.GetString(2));
        }
        return null;
    }

    public (bool Success, string Message) SavePharmacist(PharmacistRecord ph, string? rawPassword = null)
    {
        UserSession.RequireRole("Admin");
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            int userId = ph.UserId;

            if (ph.PharmacistId == 0)
            {
                // New user account with RoleId = 5 (Pharmacist)
                if (string.IsNullOrWhiteSpace(rawPassword)) rawPassword = "Admin@123";

                string checkUserSql = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username;";
                using (var cmdCheck = new SqlCommand(checkUserSql, conn, tran))
                {
                    cmdCheck.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = ph.Username.Trim();
                    if ((int)cmdCheck.ExecuteScalar()! > 0)
                    {
                        tran.Rollback();
                        return (false, "Username already exists.");
                    }
                }

                string hash = PasswordHasher.HashPassword(rawPassword);
                const string insUserSql = @"
INSERT INTO dbo.Users (Username, PasswordHash, RoleId, FullName, Email, IsActive, CreatedAt)
OUTPUT INSERTED.UserId
VALUES (@Username, @Hash, 5, @FullName, @Email, 1, SYSUTCDATETIME());";

                using (var cmdUser = new SqlCommand(insUserSql, conn, tran))
                {
                    cmdUser.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = ph.Username.Trim();
                    cmdUser.Parameters.Add("@Hash", SqlDbType.NVarChar, 255).Value = hash;
                    cmdUser.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = $"{ph.FirstName} {ph.LastName}, RPh";
                    cmdUser.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = ph.EmailAddress.Trim();
                    userId = (int)cmdUser.ExecuteScalar()!;
                }

                // Generate pharmacist code
                const string nextCodeSql = @"
DECLARE @NextNum INT = (SELECT ISNULL(MAX(PharmacistId), 0) + 1 FROM dbo.Pharmacists);
SELECT CONCAT('PHAR-', RIGHT(CONCAT('000', @NextNum), 3));";
                string code;
                using (var cmdCode = new SqlCommand(nextCodeSql, conn, tran))
                {
                    code = (string)cmdCode.ExecuteScalar()!;
                }

                const string insPhSql = @"
INSERT INTO dbo.Pharmacists (PharmacistCode, UserId, FirstName, LastName, LicenseNumber, ContactNumber, EmailAddress, IsActive, CreatedAt)
VALUES (@Code, @UserId, @FirstName, @LastName, @License, @Contact, @Email, 1, SYSUTCDATETIME());";

                using (var cmdPh = new SqlCommand(insPhSql, conn, tran))
                {
                    cmdPh.Parameters.Add("@Code", SqlDbType.NVarChar, 20).Value = code;
                    cmdPh.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmdPh.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = ph.FirstName.Trim();
                    cmdPh.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = ph.LastName.Trim();
                    cmdPh.Parameters.Add("@License", SqlDbType.NVarChar, 50).Value = ph.LicenseNumber.Trim();
                    cmdPh.Parameters.Add("@Contact", SqlDbType.NVarChar, 20).Value = ph.ContactNumber.Trim();
                    cmdPh.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = ph.EmailAddress.Trim();
                    cmdPh.ExecuteNonQuery();
                }
            }
            else
            {
                // Update existing pharmacist
                const string updUserSql = @"
UPDATE dbo.Users 
SET FullName = @FullName, Email = @Email, UpdatedAt = SYSUTCDATETIME()
WHERE UserId = @UserId;";

                using (var cmdUser = new SqlCommand(updUserSql, conn, tran))
                {
                    cmdUser.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = $"{ph.FirstName} {ph.LastName}, RPh";
                    cmdUser.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = ph.EmailAddress.Trim();
                    cmdUser.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmdUser.ExecuteNonQuery();
                }

                if (!string.IsNullOrWhiteSpace(rawPassword))
                {
                    string hash = PasswordHasher.HashPassword(rawPassword);
                    const string updPwdSql = "UPDATE dbo.Users SET PasswordHash = @Hash WHERE UserId = @UserId;";
                    using var cmdPwd = new SqlCommand(updPwdSql, conn, tran);
                    cmdPwd.Parameters.Add("@Hash", SqlDbType.NVarChar, 255).Value = hash;
                    cmdPwd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmdPwd.ExecuteNonQuery();
                }

                const string updPhSql = @"
UPDATE dbo.Pharmacists 
SET FirstName = @FirstName, LastName = @LastName, LicenseNumber = @License,
    ContactNumber = @Contact, EmailAddress = @Email, UpdatedAt = SYSUTCDATETIME()
WHERE PharmacistId = @PhId;";

                using (var cmdPh = new SqlCommand(updPhSql, conn, tran))
                {
                    cmdPh.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = ph.FirstName.Trim();
                    cmdPh.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = ph.LastName.Trim();
                    cmdPh.Parameters.Add("@License", SqlDbType.NVarChar, 50).Value = ph.LicenseNumber.Trim();
                    cmdPh.Parameters.Add("@Contact", SqlDbType.NVarChar, 20).Value = ph.ContactNumber.Trim();
                    cmdPh.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = ph.EmailAddress.Trim();
                    cmdPh.Parameters.Add("@PhId", SqlDbType.Int).Value = ph.PharmacistId;
                    cmdPh.ExecuteNonQuery();
                }
            }

            tran.Commit();
            return (true, "Pharmacist profile saved successfully.");
        }
        catch (Exception ex)
        {
            tran.Rollback();
            string msg = ErrorLogger.Log(ex, "PharmacyRepository.SavePharmacist");
            return (false, "Error saving pharmacist: " + msg);
        }
    }

    public (bool Success, string Message) TogglePharmacistActive(int pharmacistId)
    {
        UserSession.RequireRole("Admin");
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        const string sql = @"
UPDATE ph
SET ph.IsActive = CASE WHEN ph.IsActive = 1 THEN 0 ELSE 1 END,
    ph.UpdatedAt = SYSUTCDATETIME()
FROM dbo.Pharmacists ph
WHERE ph.PharmacistId = @PharmacistId;

UPDATE u
SET u.IsActive = ph.IsActive
FROM dbo.Users u
INNER JOIN dbo.Pharmacists ph ON ph.UserId = u.UserId
WHERE ph.PharmacistId = @PharmacistId;";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@PharmacistId", SqlDbType.Int).Value = pharmacistId;
        cmd.ExecuteNonQuery();
        return (true, "Pharmacist active status toggled successfully.");
    }

    public (bool Success, string Message) DeletePharmacist(int pharmacistId, string pharmacistName)
    {
        UserSession.RequireRole("Admin");
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                int? userId = null;
                const string getUserIdSql = "SELECT UserId FROM dbo.Pharmacists WHERE PharmacistId = @PharmacistId;";
                using (var cmdGet = new SqlCommand(getUserIdSql, conn, tran))
                {
                    cmdGet.Parameters.Add("@PharmacistId", SqlDbType.Int).Value = pharmacistId;
                    var res = cmdGet.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                        userId = Convert.ToInt32(res);
                }

                const string delPhSql = "DELETE FROM dbo.Pharmacists WHERE PharmacistId = @PharmacistId;";
                using (var cmdDel = new SqlCommand(delPhSql, conn, tran))
                {
                    cmdDel.Parameters.Add("@PharmacistId", SqlDbType.Int).Value = pharmacistId;
                    cmdDel.ExecuteNonQuery();
                }

                if (userId.HasValue)
                {
                    const string delUserSql = "DELETE FROM dbo.Users WHERE UserId = @UserId;";
                    using (var cmdDelUser = new SqlCommand(delUserSql, conn, tran))
                    {
                        cmdDelUser.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
                        cmdDelUser.ExecuteNonQuery();
                    }
                }

                tran.Commit();
                return (true, $"Pharmacist record for '{pharmacistName}' has been permanently deleted.");
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return (false, $"Failed to delete pharmacist: {ex.Message}");
        }
    }

    public (bool Success, string Message) UpdatePharmacistCredentials(int userId, string newUsername, string? newPlainPassword = null)
    {
        UserSession.RequireRole("Admin");
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();

            const string checkSql = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username AND UserId <> @UserId;";
            using (var cmdCheck = new SqlCommand(checkSql, conn))
            {
                cmdCheck.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = newUsername.Trim();
                cmdCheck.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                {
                    return (false, $"Username '{newUsername}' is already taken by another account. Please choose another.");
                }
            }

            string sql = !string.IsNullOrWhiteSpace(newPlainPassword)
                ? "UPDATE dbo.Users SET Username = @Username, PasswordHash = @Hash, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId;"
                : "UPDATE dbo.Users SET Username = @Username, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = newUsername.Trim();
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            if (!string.IsNullOrWhiteSpace(newPlainPassword))
            {
                string newHash = PasswordHasher.HashPassword(newPlainPassword.Trim());
                cmd.Parameters.Add("@Hash", SqlDbType.NVarChar, 512).Value = newHash;
            }

            int rows = cmd.ExecuteNonQuery();
            return rows > 0
                ? (true, string.IsNullOrWhiteSpace(newPlainPassword) ? "Login username updated successfully." : "Username and password updated successfully.")
                : (false, "User account not found.");
        }
        catch (Exception ex)
        {
            return (false, $"Could not update credentials: {ex.Message}");
        }
    }
}
