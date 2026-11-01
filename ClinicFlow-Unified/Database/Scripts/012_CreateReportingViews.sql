USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   012: Reporting Views for Transaction Reports (REQ-019, REQ-026)
   ========================================================================== */

-- 1. Daily Clinic Revenue & Collections View
CREATE OR ALTER VIEW dbo.vw_DailyRevenueReport
AS
SELECT 
    p.PaymentId,
    p.ReceiptNumber,
    inv.InvoiceNumber,
    p.PaymentDate,
    inv.PatientId,
    CONCAT(pt.LastName, N', ', pt.FirstName) AS PatientName,
    d.DoctorId,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    p.PaymentMethod,
    inv.SubTotal,
    inv.DiscountType,
    inv.DiscountAmount,
    p.AmountPaid,
    p.ChangeAmount,
    p.Notes
FROM dbo.Payments p
INNER JOIN dbo.Invoices inv ON inv.InvoiceId = p.InvoiceId
INNER JOIN dbo.Patients pt ON pt.PatientId = inv.PatientId
INNER JOIN dbo.Doctors d ON d.DoctorId = inv.DoctorId;
GO

-- 2. Central Pharmacy Stock Level & Expiry Risk View
CREATE OR ALTER VIEW dbo.vw_PharmacyInventoryExpiryReport
AS
SELECT 
    mi.InventoryId,
    m.MedicineId,
    m.GenericName,
    ISNULL(m.BrandName, N'') AS BrandName,
    ISNULL(m.DosageForm, N'Oral') AS DosageForm,
    ISNULL(m.Strength, N'Standard') AS Strength,
    mi.BatchNumber,
    mi.ExpiryDate,
    mi.QuantityOnHand,
    m.ReorderLevel,
    m.UnitPrice,
    CAST(mi.QuantityOnHand * m.UnitPrice AS DECIMAL(18,2)) AS TotalInventoryValue,
    DATEDIFF(DAY, CAST(GETDATE() AS DATE), mi.ExpiryDate) AS DaysUntilExpiry,
    CASE 
        WHEN mi.ExpiryDate < CAST(GETDATE() AS DATE) THEN N'Expired'
        WHEN DATEDIFF(DAY, CAST(GETDATE() AS DATE), mi.ExpiryDate) <= 60 THEN N'Near Expiry (<60d)'
        WHEN mi.QuantityOnHand <= m.ReorderLevel THEN N'Low Stock'
        ELSE N'Optimal'
    END AS StockStatus
FROM dbo.MedicineInventory mi
INNER JOIN dbo.Medicines m ON m.MedicineId = mi.MedicineId;
GO

-- 3. Physician Consultations & Diagnostic Activity View
CREATE OR ALTER VIEW dbo.vw_DoctorConsultationReport
AS
SELECT 
    c.ConsultationId,
    c.RecordedAt AS ConsultationDate,
    d.DoctorId,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(d.Specialty, N'General Medicine') AS Specialty,
    pt.PatientId,
    pt.PatientNumber,
    CONCAT(pt.LastName, N', ', pt.FirstName) AS PatientName,
    DATEDIFF(YEAR, pt.BirthDate, CAST(c.RecordedAt AS DATE)) AS PatientAge,
    pt.Sex AS PatientSex,
    c.ChiefComplaint,
    ISNULL(c.Diagnosis, N'Routine Checkup / Evaluation') AS Diagnosis,
    ISNULL(rxCount.ItemCount, 0) AS PrescribedItemsCount,
    ISNULL(inv.ConsultationFee, 500.00) AS ConsultationFee
FROM dbo.Consultations c
INNER JOIN dbo.PatientVisits v ON v.VisitId = c.VisitId
INNER JOIN dbo.Doctors d ON d.DoctorId = v.DoctorId
INNER JOIN dbo.Patients pt ON pt.PatientId = v.PatientId
LEFT JOIN dbo.Invoices inv ON inv.ConsultationId = c.ConsultationId
LEFT JOIN (
    SELECT rx.ConsultationId, COUNT(pi.PrescriptionItemId) AS ItemCount
    FROM dbo.Prescriptions rx
    LEFT JOIN dbo.PrescriptionItems pi ON pi.PrescriptionId = rx.PrescriptionId
    GROUP BY rx.ConsultationId
) rxCount ON rxCount.ConsultationId = c.ConsultationId;
GO
