#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;

namespace ClinicFlow_Unified.Repositories;

public sealed class PatientHistoryRepository
{
    public PatientSummaryHeader? GetPatientSummary(int patientId)
    {
        const string sql = @"
SELECT 
    p.PatientId,
    p.PatientNumber,
    CONCAT(p.LastName, N', ', p.FirstName) AS FullName,
    p.BirthDate,
    p.Sex,
    p.ContactNumber,
    CONCAT(p.StreetAddress, N', ', p.City) AS Address,
    (SELECT COUNT(1) FROM dbo.PatientVisits WHERE PatientId = p.PatientId AND VisitStatus = N'Completed') AS TotalVisitsCount
FROM dbo.Patients p
WHERE p.PatientId = @PatientId;";

        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
        connection.Open();

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        var birthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate"));
        var today = DateTime.Today;
        var age = 0;
        if (birthDate.Year > 1800)
        {
            age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            age = Math.Max(0, age);
        }

        return new PatientSummaryHeader
        {
            PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
            PatientNumber = reader.GetString(reader.GetOrdinal("PatientNumber")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Age = age,
            Sex = reader.GetString(reader.GetOrdinal("Sex")),
            ContactNumber = reader.GetString(reader.GetOrdinal("ContactNumber")),
            Address = reader.GetString(reader.GetOrdinal("Address")),
            TotalVisitsCount = reader.GetInt32(reader.GetOrdinal("TotalVisitsCount"))
        };
    }

    public List<PatientEncounterRecord> GetEncounters(int patientId)
    {
        const string sql = @"
SELECT 
    c.ConsultationId,
    c.VisitId,
    c.RecordedAt,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(d.Specialty, N'General Medicine') AS Specialty,
    c.ChiefComplaint,
    ISNULL(c.Diagnosis, N'N/A') AS Diagnosis,
    ISNULL(c.ClinicalNotes, N'') AS ClinicalNotes,
    ISNULL(c.DoctorRecommendation, N'') AS DoctorRecommendation
FROM dbo.Consultations c
INNER JOIN dbo.PatientVisits v ON v.VisitId = c.VisitId
INNER JOIN dbo.Doctors d ON d.DoctorId = v.DoctorId
WHERE v.PatientId = @PatientId
ORDER BY c.RecordedAt DESC;";

        var encounters = new List<PatientEncounterRecord>();
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();

        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                encounters.Add(new PatientEncounterRecord
                {
                    ConsultationId = reader.GetInt32(reader.GetOrdinal("ConsultationId")),
                    VisitId = reader.GetInt32(reader.GetOrdinal("VisitId")),
                    RecordedAt = reader.GetDateTime(reader.GetOrdinal("RecordedAt")),
                    DoctorName = reader.GetString(reader.GetOrdinal("DoctorName")),
                    Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                    ChiefComplaint = reader.GetString(reader.GetOrdinal("ChiefComplaint")),
                    Diagnosis = reader.GetString(reader.GetOrdinal("Diagnosis")),
                    ClinicalNotes = reader.GetString(reader.GetOrdinal("ClinicalNotes")),
                    DoctorRecommendation = reader.GetString(reader.GetOrdinal("DoctorRecommendation"))
                });
            }
        }

        // Fetch linked Prescriptions and Lab Orders for each encounter
        const string rxSql = @"
SELECT 
    rx.ConsultationId,
    rx.PrescriptionNumber,
    CONCAT(m.GenericName, CASE WHEN m.BrandName IS NOT NULL THEN CONCAT(N' [', m.BrandName, N']') ELSE N'' END, N' (', m.Strength, N' ', m.DosageForm, N')') AS MedicineName,
    item.QuantityPrescribed AS Quantity,
    item.Instructions,
    item.PatientChoice AS FulfillmentChoice,
    rx.PrescriptionStatus
FROM dbo.Prescriptions rx
INNER JOIN dbo.PrescriptionItems item ON item.PrescriptionId = rx.PrescriptionId
INNER JOIN dbo.Medicines m ON m.MedicineId = item.MedicineId
WHERE rx.PatientId = @PatientId;";

        using (var cmdRx = new SqlCommand(rxSql, connection))
        {
            cmdRx.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
            using var reader = cmdRx.ExecuteReader();
            while (reader.Read())
            {
                int cId = reader.GetInt32(reader.GetOrdinal("ConsultationId"));
                var match = encounters.Find(e => e.ConsultationId == cId);
                if (match != null)
                {
                    match.Prescriptions.Add(new PastPrescriptionItem
                    {
                        ConsultationId = cId,
                        PrescriptionNumber = reader.GetString(reader.GetOrdinal("PrescriptionNumber")),
                        MedicineName = reader.GetString(reader.GetOrdinal("MedicineName")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        Instructions = reader.GetString(reader.GetOrdinal("Instructions")),
                        FulfillmentChoice = reader.GetString(reader.GetOrdinal("FulfillmentChoice")),
                        PrescriptionStatus = reader.GetString(reader.GetOrdinal("PrescriptionStatus"))
                    });
                }
            }
        }

        const string labSql = @"
SELECT 
    lab.ConsultationId,
    lab.RequestNumber,
    t.TestCode,
    t.TestName,
    t.Fee,
    lab.RequestStatus,
    ISNULL(res.ResultText, N'Pending Evaluation') AS ResultText
FROM dbo.LaboratoryRequests lab
INNER JOIN dbo.LaboratoryTests t ON t.LaboratoryTestId = lab.LaboratoryTestId
LEFT JOIN dbo.LaboratoryResults res ON res.LaboratoryRequestId = lab.LaboratoryRequestId
WHERE lab.PatientId = @PatientId;";

        using (var cmdLab = new SqlCommand(labSql, connection))
        {
            cmdLab.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
            using var reader = cmdLab.ExecuteReader();
            while (reader.Read())
            {
                int cId = reader.GetInt32(reader.GetOrdinal("ConsultationId"));
                var match = encounters.Find(e => e.ConsultationId == cId);
                if (match != null)
                {
                    match.LabRequests.Add(new PastLabRequestItem
                    {
                        ConsultationId = cId,
                        RequestNumber = reader.GetString(reader.GetOrdinal("RequestNumber")),
                        TestCode = reader.GetString(reader.GetOrdinal("TestCode")),
                        TestName = reader.GetString(reader.GetOrdinal("TestName")),
                        Fee = reader.GetDecimal(reader.GetOrdinal("Fee")),
                        RequestStatus = reader.GetString(reader.GetOrdinal("RequestStatus")),
                        ResultText = reader.GetString(reader.GetOrdinal("ResultText"))
                    });
                }
            }
        }

        return encounters;
    }
}