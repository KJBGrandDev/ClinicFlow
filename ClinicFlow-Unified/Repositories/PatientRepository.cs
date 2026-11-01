#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Repositories;

public sealed class PatientRepository
{
    public List<Patient> Search(string search, bool showArchived = false)
    {
        const string sql = @"
SELECT 
    PatientId, PatientNumber, FirstName, MiddleName, LastName, Suffix, Sex, BirthDate,
    ContactNumber, BlockAddress, StreetAddress, City, Municipality,
    EmergencyContactName, EmergencyContactNumber, IsActive, RowVersion
FROM dbo.Patients
WHERE IsActive = @IsActive
  AND (@Search = N'' 
       OR PatientNumber LIKE @Pattern 
       OR FirstName LIKE @Pattern 
       OR LastName LIKE @Pattern 
       OR ContactNumber LIKE @Pattern)
ORDER BY LastName, FirstName;";

        var list = new List<Patient>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = !showArchived;
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = search.Trim();
        command.Parameters.Add("@Pattern", SqlDbType.NVarChar, 102).Value = $"%{search.Trim()}%";
        connection.Open();

        using var reader = command.ExecuteReader();
        var today = DateTime.Today;
        int rowVersionOrdinal = -1;
        try { rowVersionOrdinal = reader.GetOrdinal("RowVersion"); } catch { }

        while (reader.Read())
        {
            var birthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate"));
            int age = 0;
            if (birthDate.Year > 1800)
            {
                age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;
                age = Math.Max(0, age);
            }

            byte[]? rowVersion = null;
            if (rowVersionOrdinal >= 0 && !reader.IsDBNull(rowVersionOrdinal))
            {
                rowVersion = (byte[])reader.GetValue(rowVersionOrdinal);
            }

            list.Add(new Patient
            {
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                PatientNumber = reader.GetString(reader.GetOrdinal("PatientNumber")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName")) ? null : reader.GetString(reader.GetOrdinal("MiddleName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Suffix = reader.IsDBNull(reader.GetOrdinal("Suffix")) ? null : reader.GetString(reader.GetOrdinal("Suffix")),
                Sex = reader.GetString(reader.GetOrdinal("Sex")),
                BirthDate = birthDate,
                Age = age,
                ContactNumber = reader.GetString(reader.GetOrdinal("ContactNumber")),
                BlockAddress = reader.IsDBNull(reader.GetOrdinal("BlockAddress")) ? null : reader.GetString(reader.GetOrdinal("BlockAddress")),
                StreetAddress = reader.GetString(reader.GetOrdinal("StreetAddress")),
                City = reader.GetString(reader.GetOrdinal("City")),
                Municipality = reader.GetString(reader.GetOrdinal("Municipality")),
                EmergencyContactName = reader.IsDBNull(reader.GetOrdinal("EmergencyContactName")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContactName")),
                EmergencyContactNumber = reader.IsDBNull(reader.GetOrdinal("EmergencyContactNumber")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContactNumber")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                RowVersion = rowVersion
            });
        }
        return list;
    }

    public List<Patient> GetArchivedPatients(string search = "") => Search(search, showArchived: true);

    public Patient? GetById(int patientId)
    {
        const string sql = @"
SELECT 
    PatientId, PatientNumber, FirstName, MiddleName, LastName, Suffix, Sex, BirthDate,
    ContactNumber, BlockAddress, StreetAddress, City, Municipality,
    EmergencyContactName, EmergencyContactNumber, IsActive, RowVersion
FROM dbo.Patients
WHERE PatientId = @PatientId;";

        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
        connection.Open();

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        var today = DateTime.Today;
        var birthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate"));
        int age = 0;
        if (birthDate.Year > 1800)
        {
            age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            age = Math.Max(0, age);
        }

        byte[]? rowVersion = null;
        int rvOrd = -1;
        try { rvOrd = reader.GetOrdinal("RowVersion"); } catch { }
        if (rvOrd >= 0 && !reader.IsDBNull(rvOrd))
        {
            rowVersion = (byte[])reader.GetValue(rvOrd);
        }

        return new Patient
        {
            PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
            PatientNumber = reader.GetString(reader.GetOrdinal("PatientNumber")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName")) ? null : reader.GetString(reader.GetOrdinal("MiddleName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Suffix = reader.IsDBNull(reader.GetOrdinal("Suffix")) ? null : reader.GetString(reader.GetOrdinal("Suffix")),
            Sex = reader.GetString(reader.GetOrdinal("Sex")),
            BirthDate = birthDate,
            Age = age,
            ContactNumber = reader.GetString(reader.GetOrdinal("ContactNumber")),
            BlockAddress = reader.IsDBNull(reader.GetOrdinal("BlockAddress")) ? null : reader.GetString(reader.GetOrdinal("BlockAddress")),
            StreetAddress = reader.GetString(reader.GetOrdinal("StreetAddress")),
            City = reader.GetString(reader.GetOrdinal("City")),
            Municipality = reader.GetString(reader.GetOrdinal("Municipality")),
            EmergencyContactName = reader.IsDBNull(reader.GetOrdinal("EmergencyContactName")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContactName")),
            EmergencyContactNumber = reader.IsDBNull(reader.GetOrdinal("EmergencyContactNumber")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContactNumber")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            RowVersion = rowVersion
        };
    }

    private static void ValidatePatient(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FirstName))
            throw new ArgumentException("First Name is required.", nameof(patient.FirstName));
        if (string.IsNullOrWhiteSpace(patient.LastName))
            throw new ArgumentException("Last Name is required.", nameof(patient.LastName));
        if (patient.BirthDate > DateTime.Today)
            throw new ArgumentException("Birth date cannot be in the future.", nameof(patient.BirthDate));
        if (patient.BirthDate <= new DateTime(1800, 1, 1) || patient.BirthDate.Year < 1900)
            throw new ArgumentException("A valid birth date is required (cannot be 1/1/1800 or unconfigured).", nameof(patient.BirthDate));
    }

    public int Insert(Patient patient)
    {
        ValidatePatient(patient);

        const string sql = @"
INSERT INTO dbo.Patients
(FirstName, MiddleName, LastName, Suffix, Sex, BirthDate, ContactNumber, BlockAddress, StreetAddress, City, Municipality, EmergencyContactName, EmergencyContactNumber, IsActive, CreatedAt)
OUTPUT INSERTED.PatientId, INSERTED.RowVersion
VALUES
(@FirstName, @MiddleName, @LastName, @Suffix, @Sex, @BirthDate, @ContactNumber, @BlockAddress, @StreetAddress, @City, @Municipality, @EmergencyContactName, @EmergencyContactNumber, 1, SYSUTCDATETIME());";

        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, patient);
        connection.Open();

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            int newId = reader.GetInt32(0);
            if (!reader.IsDBNull(1))
            {
                patient.RowVersion = (byte[])reader.GetValue(1);
            }
            patient.PatientId = newId;
            return newId;
        }
        throw new InvalidOperationException("Failed to insert patient record.");
    }

    public void Update(Patient patient)
    {
        ValidatePatient(patient);

        const string sql = @"
UPDATE dbo.Patients
SET FirstName = @FirstName,
    MiddleName = @MiddleName,
    LastName = @LastName,
    Suffix = @Suffix,
    Sex = @Sex,
    BirthDate = @BirthDate,
    ContactNumber = @ContactNumber,
    BlockAddress = @BlockAddress,
    StreetAddress = @StreetAddress,
    City = @City,
    Municipality = @Municipality,
    EmergencyContactName = @EmergencyContactName,
    EmergencyContactNumber = @EmergencyContactNumber,
    UpdatedAt = SYSUTCDATETIME()
OUTPUT INSERTED.RowVersion
WHERE PatientId = @PatientId
  AND (@RowVersion IS NULL OR RowVersion = @RowVersion);";

        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patient.PatientId;
        command.Parameters.Add("@RowVersion", SqlDbType.Binary, 8).Value = (object?)patient.RowVersion ?? DBNull.Value;
        AddParameters(command, patient);
        connection.Open();

        var newRowVersion = command.ExecuteScalar();
        if (newRowVersion == null || newRowVersion == DBNull.Value)
        {
            // Determine whether record was changed concurrently or missing
            const string checkSql = "SELECT RowVersion, IsActive FROM dbo.Patients WHERE PatientId = @PatientId;";
            using var checkCmd = new SqlCommand(checkSql, connection);
            checkCmd.Parameters.Add("@PatientId", SqlDbType.Int).Value = patient.PatientId;
            using var reader = checkCmd.ExecuteReader();
            if (reader.Read())
            {
                throw new DBConcurrencyException(
                    "Concurrency Conflict: This patient record was modified or updated by another user or session. " +
                    "Please refresh the directory before reapplying modifications.");
            }
            else
            {
                throw new InvalidOperationException("Patient record was not found or has been deleted.");
            }
        }

        patient.RowVersion = (byte[])newRowVersion;
    }

    public void Delete(int patientId)
    {
        const string sql = "UPDATE dbo.Patients SET IsActive = 0, UpdatedAt = SYSUTCDATETIME() WHERE PatientId = @PatientId;";
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
        connection.Open();
        command.ExecuteNonQuery();
    }

    public void Restore(int patientId)
    {
        const string sql = "UPDATE dbo.Patients SET IsActive = 1, UpdatedAt = SYSUTCDATETIME() WHERE PatientId = @PatientId;";
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
        connection.Open();
        int rows = command.ExecuteNonQuery();
        if (rows == 0)
        {
            throw new InvalidOperationException("Patient record not found.");
        }
        AuditLogger.Log("RESTORE", "Patients", patientId, $"Patient #{patientId} restored to active roster.");
    }

    private static void AddParameters(SqlCommand command, Patient patient)
    {
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = patient.FirstName.Trim();
        command.Parameters.Add("@MiddleName", SqlDbType.NVarChar, 100).Value = (object?)patient.MiddleName?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = patient.LastName.Trim();
        command.Parameters.Add("@Suffix", SqlDbType.NVarChar, 20).Value = (object?)patient.Suffix?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@Sex", SqlDbType.NVarChar, 20).Value = patient.Sex.Trim();
        command.Parameters.Add("@BirthDate", SqlDbType.Date).Value = patient.BirthDate.Date;
        command.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 30).Value = patient.ContactNumber.Trim();
        command.Parameters.Add("@BlockAddress", SqlDbType.NVarChar, 80).Value = (object?)patient.BlockAddress?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@StreetAddress", SqlDbType.NVarChar, 120).Value = patient.StreetAddress.Trim();
        command.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = patient.City.Trim();
        command.Parameters.Add("@Municipality", SqlDbType.NVarChar, 100).Value = patient.Municipality.Trim();
        command.Parameters.Add("@EmergencyContactName", SqlDbType.NVarChar, 120).Value = (object?)patient.EmergencyContactName?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@EmergencyContactNumber", SqlDbType.NVarChar, 30).Value = (object?)patient.EmergencyContactNumber?.Trim() ?? DBNull.Value;
    }
}