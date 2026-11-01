#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;

namespace ClinicFlow_Unified.Repositories;

public sealed class DoctorConsultationRepository
{
    public List<DoctorQueueItem> GetDoctorQueue(int? doctorId)
    {
        // Fetches all active waiting, called, and in-service patients for THIS doctor
        const string sql = @"
SELECT 
    q.QueueId,
    ISNULL(v.VisitId, 0) AS VisitId,
    a.AppointmentId,
    p.PatientId,
    p.PatientNumber,
    CONCAT(p.LastName, N', ', p.FirstName) AS PatientName,
    p.Sex,
    p.BirthDate,
    ISNULL(a.Reason, N'General Consultation') AS Reason,
    ISNULL(q.QueueNumber, 0) AS QueueNumber,
    CONVERT(VARCHAR(5), ISNULL(v.VisitStart, a.ScheduledStart), 108) AS ArrivedTime
FROM dbo.AppointmentQueue q
INNER JOIN dbo.Appointments a ON a.AppointmentId = q.AppointmentId
INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
LEFT JOIN dbo.PatientVisits v ON v.AppointmentId = a.AppointmentId
WHERE q.QueueDate = CAST(GETDATE() AS DATE)
  AND q.QueueStatus IN (N'Waiting', N'Called', N'In Service')
  AND (@DoctorId IS NULL OR a.DoctorId = @DoctorId)
ORDER BY 
    CASE q.QueueStatus
        WHEN N'In Service' THEN 1
        WHEN N'Called' THEN 2
        WHEN N'Waiting' THEN 3
        ELSE 4
    END,
    q.QueueNumber ASC;";

        var list = new List<DoctorQueueItem>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DoctorId", SqlDbType.Int).Value = (object?)doctorId ?? DBNull.Value;
        connection.Open();

        using var reader = command.ExecuteReader();
        var today = DateTime.Today;
        while (reader.Read())
        {
            var birthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate"));
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;

            list.Add(new DoctorQueueItem
            {
                QueueId = reader.GetInt32(reader.GetOrdinal("QueueId")),
                VisitId = reader.GetInt32(reader.GetOrdinal("VisitId")),
                AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                PatientNumber = reader.GetString(reader.GetOrdinal("PatientNumber")),
                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                Sex = reader.GetString(reader.GetOrdinal("Sex")),
                Age = age,
                Reason = reader.GetString(reader.GetOrdinal("Reason")),
                QueueNumber = reader.GetInt32(reader.GetOrdinal("QueueNumber")),
                ArrivedTime = reader.GetString(reader.GetOrdinal("ArrivedTime"))
            });
        }
        return list;
    }

    public void SetQueueInService(int queueId)
    {
        try
        {
            const string sql = @"
UPDATE dbo.AppointmentQueue 
SET QueueStatus = N'In Service', CalledAt = ISNULL(CalledAt, SYSUTCDATETIME()) 
WHERE QueueId = @QueueId AND QueueStatus IN (N'Waiting', N'Called');

UPDATE dbo.Appointments
SET Status = N'In Service'
WHERE AppointmentId = (SELECT AppointmentId FROM dbo.AppointmentQueue WHERE QueueId = @QueueId);";

            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@QueueId", SqlDbType.Int).Value = queueId;
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    public void DeferPatient(int currentQueueId, int nextQueueId)
    {
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string swapSql = @"
DECLARE @Q1 INT, @Q2 INT;
SELECT @Q1 = QueueNumber FROM dbo.AppointmentQueue WHERE QueueId = @Id1;
SELECT @Q2 = QueueNumber FROM dbo.AppointmentQueue WHERE QueueId = @Id2;

UPDATE dbo.AppointmentQueue SET QueueNumber = @Q2 WHERE QueueId = @Id1;
UPDATE dbo.AppointmentQueue SET QueueNumber = @Q1 WHERE QueueId = @Id2;";

            using var cmd = new SqlCommand(swapSql, connection, transaction);
            cmd.Parameters.Add("@Id1", SqlDbType.Int).Value = currentQueueId;
            cmd.Parameters.Add("@Id2", SqlDbType.Int).Value = nextQueueId;
            cmd.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public (DateTime NextDate, int PatientsRescheduled) EmergencyPostponeDoctor(int doctorId)
    {
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string scheduleSql = "SELECT DayOfWeek FROM dbo.DoctorSchedules WHERE DoctorId = @DoctorId AND IsActive = 1;";
            var activeDays = new HashSet<int>();
            using (var cmdDays = new SqlCommand(scheduleSql, connection, transaction))
            {
                cmdDays.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                using var rdr = cmdDays.ExecuteReader();
                while (rdr.Read())
                {
                    activeDays.Add((int)rdr.GetByte(0));
                }
            }

            if (activeDays.Count == 0)
            {
                activeDays.Add(1);
                activeDays.Add(2);
                activeDays.Add(3);
                activeDays.Add(4);
                activeDays.Add(5);
            }

            DateTime targetDate = DateTime.Today.AddDays(1);
            while (true)
            {
                int dow = (int)targetDate.DayOfWeek;
                if (dow == 0) dow = 7;
                if (activeDays.Contains(dow)) break;
                targetDate = targetDate.AddDays(1);
            }

            const string rescheduleSql = @"
UPDATE q
SET q.QueueDate = @NextDate,
    q.QueueStatus = N'Booked',
    q.CalledAt = NULL
FROM dbo.AppointmentQueue q
INNER JOIN dbo.Appointments a ON a.AppointmentId = q.AppointmentId
WHERE q.QueueDate = CAST(GETDATE() AS DATE)
  AND a.DoctorId = @DoctorId
  AND q.QueueStatus NOT IN (N'Completed');

UPDATE a
SET a.AppointmentDate = @NextDate,
    a.Status = N'Confirmed'
FROM dbo.Appointments a
INNER JOIN dbo.AppointmentQueue q ON q.AppointmentId = a.AppointmentId
WHERE a.AppointmentDate = CAST(GETDATE() AS DATE)
  AND a.DoctorId = @DoctorId
  AND a.Status NOT IN (N'Completed', N'Cancelled');

SELECT @@ROWCOUNT;";

            int count;
            using (var cmdUpdate = new SqlCommand(rescheduleSql, connection, transaction))
            {
                cmdUpdate.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmdUpdate.Parameters.Add("@NextDate", SqlDbType.Date).Value = targetDate.Date;
                count = Convert.ToInt32(cmdUpdate.ExecuteScalar() ?? 0);
            }

            const string cancelVisitsSql = @"
UPDATE v
SET v.VisitStatus = N'Cancelled'
FROM dbo.PatientVisits v
INNER JOIN dbo.Appointments a ON a.AppointmentId = v.AppointmentId
WHERE a.DoctorId = @DoctorId AND v.VisitStatus = N'Open';";

            using (var cmdVisits = new SqlCommand(cancelVisitsSql, connection, transaction))
            {
                cmdVisits.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmdVisits.ExecuteNonQuery();
            }

            transaction.Commit();
            return (targetDate, count);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public List<MedicineItem> GetMedicines()
    {
        const string sql = @"
SELECT 
    m.MedicineId,
    m.GenericName,
    m.BrandName,
    ISNULL(m.Category, N'General') AS Category,
    m.DosageForm,
    m.Strength,
    m.UnitPrice,
    ISNULL(SUM(inv.QuantityOnHand), 0) AS AvailableStock
FROM dbo.Medicines m
LEFT JOIN dbo.MedicineInventory inv ON inv.MedicineId = m.MedicineId AND inv.ExpiryDate >= CAST(GETDATE() AS DATE)
WHERE m.IsActive = 1
GROUP BY m.MedicineId, m.GenericName, m.BrandName, m.Category, m.DosageForm, m.Strength, m.UnitPrice
ORDER BY m.Category, m.GenericName;";

        var list = new List<MedicineItem>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new MedicineItem
            {
                MedicineId = reader.GetInt32(reader.GetOrdinal("MedicineId")),
                GenericName = reader.GetString(reader.GetOrdinal("GenericName")),
                BrandName = reader.IsDBNull(reader.GetOrdinal("BrandName")) ? null : reader.GetString(reader.GetOrdinal("BrandName")),
                Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? "General" : reader.GetString(reader.GetOrdinal("Category")),
                DosageForm = reader.GetString(reader.GetOrdinal("DosageForm")),
                Strength = reader.IsDBNull(reader.GetOrdinal("Strength")) ? null : reader.GetString(reader.GetOrdinal("Strength")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                AvailableStock = reader.GetInt32(reader.GetOrdinal("AvailableStock"))
            });
        }
        return list;
    }

    public int SaveConsultation(
        int appointmentId,
        int patientId,
        int doctorId,
        string chiefComplaint,
        string clinicalNotes,
        string diagnosis,
        string recommendation,
        List<PrescriptionLineItem> prescriptions,
        int? userId)
    {
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int visitId;
            const string findVisitSql = "SELECT TOP 1 VisitId FROM dbo.PatientVisits WHERE AppointmentId = @AppointmentId;";
            using (var cmdFind = new SqlCommand(findVisitSql, connection, transaction))
            {
                cmdFind.Parameters.Add("@AppointmentId", SqlDbType.Int).Value = appointmentId;
                var existing = cmdFind.ExecuteScalar();
                if (existing != null && existing != DBNull.Value)
                {
                    visitId = (int)existing;
                }
                else
                {
                    const string createVisitSql = @"
INSERT INTO dbo.PatientVisits (AppointmentId, PatientId, DoctorId, VisitStart, VisitStatus)
OUTPUT INSERTED.VisitId
VALUES (@AppointmentId, @PatientId, @DoctorId, SYSUTCDATETIME(), N'Open');";
                    using var cmdCreate = new SqlCommand(createVisitSql, connection, transaction);
                    cmdCreate.Parameters.Add("@AppointmentId", SqlDbType.Int).Value = appointmentId;
                    cmdCreate.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
                    cmdCreate.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    visitId = (int)cmdCreate.ExecuteScalar()!;
                }
            }

            int consultationId;
            const string upsertConsultationSql = @"
IF EXISTS (SELECT 1 FROM dbo.Consultations WHERE VisitId = @VisitId)
BEGIN
    UPDATE dbo.Consultations
    SET ChiefComplaint = @ChiefComplaint,
        ClinicalNotes = @ClinicalNotes,
        Diagnosis = @Diagnosis,
        DoctorRecommendation = @DoctorRecommendation,
        RecordedAt = SYSUTCDATETIME()
    WHERE VisitId = @VisitId;

    SELECT ConsultationId FROM dbo.Consultations WHERE VisitId = @VisitId;
END
ELSE
BEGIN
    INSERT INTO dbo.Consultations (VisitId, ChiefComplaint, ClinicalNotes, Diagnosis, DoctorRecommendation, RecordedByUserId, RecordedAt)
    OUTPUT INSERTED.ConsultationId
    VALUES (@VisitId, @ChiefComplaint, @ClinicalNotes, @Diagnosis, @DoctorRecommendation, @RecordedByUserId, SYSUTCDATETIME());
END;";

            using (var cmdConsultation = new SqlCommand(upsertConsultationSql, connection, transaction))
            {
                cmdConsultation.Parameters.Add("@VisitId", SqlDbType.Int).Value = visitId;
                cmdConsultation.Parameters.Add("@ChiefComplaint", SqlDbType.NVarChar, 500).Value = chiefComplaint.Trim();
                cmdConsultation.Parameters.Add("@ClinicalNotes", SqlDbType.NVarChar, -1).Value = (object?)clinicalNotes?.Trim() ?? DBNull.Value;
                cmdConsultation.Parameters.Add("@Diagnosis", SqlDbType.NVarChar, 500).Value = (object?)diagnosis?.Trim() ?? DBNull.Value;
                cmdConsultation.Parameters.Add("@DoctorRecommendation", SqlDbType.NVarChar, -1).Value = (object?)recommendation?.Trim() ?? DBNull.Value;
                cmdConsultation.Parameters.Add("@RecordedByUserId", SqlDbType.Int).Value = (object?)userId ?? DBNull.Value;
                consultationId = (int)cmdConsultation.ExecuteScalar()!;
            }

            const string updateVisitSql = @"
UPDATE dbo.PatientVisits SET VisitStatus = N'Completed', VisitEnd = SYSUTCDATETIME() WHERE VisitId = @VisitId;
UPDATE dbo.Appointments SET Status = N'Completed' WHERE AppointmentId = @AppointmentId;
UPDATE dbo.AppointmentQueue SET QueueStatus = N'Completed', CompletedAt = SYSUTCDATETIME() WHERE AppointmentId = @AppointmentId;";

            using var cmdUpdateVisit = new SqlCommand(updateVisitSql, connection, transaction);
            cmdUpdateVisit.Parameters.Add("@VisitId", SqlDbType.Int).Value = visitId;
            cmdUpdateVisit.Parameters.Add("@AppointmentId", SqlDbType.Int).Value = appointmentId;
            cmdUpdateVisit.ExecuteNonQuery();

            if (prescriptions.Count > 0)
            {
                const string checkRxSql = @"
DECLARE @ExistingRxId INT = (SELECT TOP 1 PrescriptionId FROM dbo.Prescriptions WHERE ConsultationId = @ConsultationId);
IF @ExistingRxId IS NOT NULL
BEGIN
    DELETE FROM dbo.PrescriptionItems WHERE PrescriptionId = @ExistingRxId;
    SELECT @ExistingRxId;
END
ELSE
BEGIN
    DECLARE @NextSeq INT = (SELECT ISNULL(MAX(PrescriptionId), 0) + 1 FROM dbo.Prescriptions);
    DECLARE @RxNum NVARCHAR(40) = CONCAT(N'RX-', CONVERT(VARCHAR(8), GETDATE(), 112), N'-', RIGHT(CONCAT(N'0000', @NextSeq), 4));
    
    INSERT INTO dbo.Prescriptions (PrescriptionNumber, ConsultationId, PatientId, DoctorId, PrescriptionStatus, CreatedAt)
    VALUES (@RxNum, @ConsultationId, @PatientId, @DoctorId, N'Recommended', SYSUTCDATETIME());
    
    SELECT CAST(SCOPE_IDENTITY() AS INT);
END;";

                using var cmdRx = new SqlCommand(checkRxSql, connection, transaction);
                cmdRx.Parameters.Add("@ConsultationId", SqlDbType.Int).Value = consultationId;
                cmdRx.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
                cmdRx.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                int rxId = Convert.ToInt32(cmdRx.ExecuteScalar()!);

                const string rxItemSql = @"
INSERT INTO dbo.PrescriptionItems (PrescriptionId, MedicineId, QuantityPrescribed, Instructions, AvailabilityStatus, PatientChoice)
VALUES (@PrescriptionId, @MedicineId, @QuantityPrescribed, @Instructions, N'Pending', @PatientChoice);";

                foreach (var item in prescriptions)
                {
                    using var cmdItem = new SqlCommand(rxItemSql, connection, transaction);
                    cmdItem.Parameters.Add("@PrescriptionId", SqlDbType.Int).Value = rxId;
                    cmdItem.Parameters.Add("@MedicineId", SqlDbType.Int).Value = item.MedicineId;
                    cmdItem.Parameters.Add("@QuantityPrescribed", SqlDbType.Int).Value = item.Quantity;
                    cmdItem.Parameters.Add("@Instructions", SqlDbType.NVarChar, 300).Value = item.Instructions.Trim();
                    cmdItem.Parameters.Add("@PatientChoice", SqlDbType.NVarChar, 30).Value = item.FulfillmentChoice;
                    cmdItem.ExecuteNonQuery();
                }
            }

            transaction.Commit();
            return consultationId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}