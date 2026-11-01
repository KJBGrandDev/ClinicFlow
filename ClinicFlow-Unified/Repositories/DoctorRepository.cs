#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Repositories;

public sealed class DoctorAccountItem
{
    public int DoctorId { get; set; }
    public int? UserId { get; set; }
    public string DoctorCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"Dr. {FirstName} {LastName}";
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string StatusDisplay => IsActive ? "Active" : "Archived";
}

public sealed class DoctorRepository
{
    public List<DoctorAccountItem> GetAllDoctors()
    {
        const string sql = @"
SELECT 
    d.DoctorId,
    d.UserId,
    d.DoctorCode,
    d.FirstName,
    d.LastName,
    ISNULL(d.LicenseNumber, N'N/A') AS LicenseNumber,
    ISNULL(d.Specialty, N'Family Medicine') AS Specialty,
    ISNULL(d.ContactNumber, N'N/A') AS ContactNumber,
    ISNULL(u.Username, N'(No Login)') AS Username,
    d.IsActive
FROM dbo.Doctors d
LEFT JOIN dbo.Users u ON u.UserId = d.UserId
ORDER BY d.LastName, d.FirstName;";

        var list = new List<DoctorAccountItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new DoctorAccountItem
            {
                DoctorId = rdr.GetInt32(0),
                UserId = rdr.IsDBNull(1) ? null : rdr.GetInt32(1),
                DoctorCode = rdr.GetString(2),
                FirstName = rdr.GetString(3),
                LastName = rdr.GetString(4),
                LicenseNumber = rdr.GetString(5),
                Specialty = rdr.GetString(6),
                ContactNumber = rdr.GetString(7),
                Username = rdr.GetString(8),
                IsActive = rdr.GetBoolean(9)
            });
        }
        return list;
    }

    public (bool Success, string Message, int DoctorId) RegisterDoctorWithAccount(
        string firstName,
        string lastName,
        string licenseNumber,
        string specialty,
        string contactNumber,
        string username,
        string plainPassword)
    {
        UserSession.RequireRole("Admin");
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            const string checkUserSql = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username;";
            using (var cmdCheck = new SqlCommand(checkUserSql, conn, tran))
            {
                cmdCheck.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username.Trim();
                if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                {
                    tran.Rollback();
                    return (false, $"Username '{username}' is already taken. Please choose another.", 0);
                }
            }

            int doctorRoleId = 2;
            const string getRoleSql = "SELECT TOP 1 RoleId FROM dbo.Roles WHERE RoleName = 'Doctor';";
            using (var cmdRole = new SqlCommand(getRoleSql, conn, tran))
            {
                var roleObj = cmdRole.ExecuteScalar();
                if (roleObj != null && roleObj != DBNull.Value)
                    doctorRoleId = Convert.ToInt32(roleObj);
            }

            string passwordHash = PasswordHasher.HashPassword(plainPassword);
            string fullName = $"Dr. {firstName.Trim()} {lastName.Trim()}";

            const string insertUserSql = @"
INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, PasswordSalt, IsActive, CreatedAt)
OUTPUT INSERTED.UserId
VALUES (@Username, @FullName, @RoleId, @PasswordHash, N'', 1, SYSUTCDATETIME());";

            int newUserId;
            using (var cmdUser = new SqlCommand(insertUserSql, conn, tran))
            {
                cmdUser.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username.Trim();
                cmdUser.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = fullName;
                cmdUser.Parameters.Add("@RoleId", SqlDbType.Int).Value = doctorRoleId;
                cmdUser.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = passwordHash;
                newUserId = Convert.ToInt32(cmdUser.ExecuteScalar());
            }

            const string insertDocSql = @"
DECLARE @NextDocNum INT = (SELECT ISNULL(MAX(DoctorId), 0) + 1 FROM dbo.Doctors);
DECLARE @DocCode NVARCHAR(20) = CONCAT(N'DOC-', RIGHT(CONCAT(N'0000', @NextDocNum), 4));

INSERT INTO dbo.Doctors (DoctorCode, UserId, FirstName, LastName, LicenseNumber, Specialty, ContactNumber, IsActive, CreatedAt)
OUTPUT INSERTED.DoctorId
VALUES (@DocCode, @UserId, @FirstName, @LastName, @LicenseNumber, @Specialty, @ContactNumber, 1, SYSUTCDATETIME());";

            int newDoctorId;
            using (var cmdDoc = new SqlCommand(insertDocSql, conn, tran))
            {
                cmdDoc.Parameters.Add("@UserId", SqlDbType.Int).Value = newUserId;
                cmdDoc.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName.Trim();
                cmdDoc.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName.Trim();
                cmdDoc.Parameters.Add("@LicenseNumber", SqlDbType.NVarChar, 50).Value = (object?)licenseNumber?.Trim() ?? DBNull.Value;
                cmdDoc.Parameters.Add("@Specialty", SqlDbType.NVarChar, 100).Value = specialty.Trim();
                cmdDoc.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 30).Value = (object?)contactNumber?.Trim() ?? DBNull.Value;
                newDoctorId = Convert.ToInt32(cmdDoc.ExecuteScalar());
            }

            const string scheduleSql = @"
INSERT INTO dbo.DoctorSchedules (DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes, MaxPatientsPerSlot, EffectiveFrom, IsActive, CreatedAt)
VALUES 
(@DoctorId, 1, '08:00:00', '17:00:00', 30, 1, CAST(GETDATE() AS DATE), 1, SYSUTCDATETIME()),
(@DoctorId, 2, '08:00:00', '17:00:00', 30, 1, CAST(GETDATE() AS DATE), 1, SYSUTCDATETIME()),
(@DoctorId, 3, '08:00:00', '17:00:00', 30, 1, CAST(GETDATE() AS DATE), 1, SYSUTCDATETIME()),
(@DoctorId, 4, '08:00:00', '17:00:00', 30, 1, CAST(GETDATE() AS DATE), 1, SYSUTCDATETIME()),
(@DoctorId, 5, '08:00:00', '17:00:00', 30, 1, CAST(GETDATE() AS DATE), 1, SYSUTCDATETIME());";

            using (var cmdSched = new SqlCommand(scheduleSql, conn, tran))
            {
                cmdSched.Parameters.Add("@DoctorId", SqlDbType.Int).Value = newDoctorId;
                cmdSched.ExecuteNonQuery();
            }

            tran.Commit();
            return (true, $"Doctor {fullName} registered successfully with login username '{username}'.", newDoctorId);
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return (false, $"Failed to register doctor: {ex.Message}", 0);
        }
    }

    public (bool Success, string Message) UpdateDoctorProfile(int doctorId, string firstName, string lastName, string licenseNumber, string specialty, string contactNumber)
    {
        UserSession.RequireRole("Admin");
        try
        {
            const string sql = @"
UPDATE dbo.Doctors 
SET FirstName = @FirstName,
    LastName = @LastName,
    LicenseNumber = @LicenseNumber,
    Specialty = @Specialty,
    ContactNumber = @ContactNumber,
    UpdatedAt = SYSUTCDATETIME()
WHERE DoctorId = @DoctorId;";

            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
            cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName.Trim();
            cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName.Trim();
            cmd.Parameters.Add("@LicenseNumber", SqlDbType.NVarChar, 50).Value = (object?)licenseNumber?.Trim() ?? DBNull.Value;
            cmd.Parameters.Add("@Specialty", SqlDbType.NVarChar, 100).Value = specialty.Trim();
            cmd.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 30).Value = (object?)contactNumber?.Trim() ?? DBNull.Value;
            conn.Open();

            int rows = cmd.ExecuteNonQuery();
            return rows > 0 ? (true, "Doctor profile updated successfully.") : (false, "Doctor record not found.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating profile: {ex.Message}");
        }
    }

    public (bool Success, string Message) ToggleDoctorArchive(int doctorId, bool setInactive)
    {
        UserSession.RequireRole("Admin");
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                bool newActiveStatus = !setInactive;

                // 1. Update dbo.Doctors
                const string docSql = @"
UPDATE dbo.Doctors 
SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() 
WHERE DoctorId = @DoctorId;";
                using (var cmdDoc = new SqlCommand(docSql, conn, tran))
                {
                    cmdDoc.Parameters.Add("@IsActive", SqlDbType.Bit).Value = newActiveStatus;
                    cmdDoc.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmdDoc.ExecuteNonQuery();
                }

                // 2. Update linked login account in dbo.Users
                const string userSql = @"
UPDATE u 
SET u.IsActive = @IsActive, u.UpdatedAt = SYSUTCDATETIME()
FROM dbo.Users u
INNER JOIN dbo.Doctors d ON d.UserId = u.UserId
WHERE d.DoctorId = @DoctorId;";
                using (var cmdUser = new SqlCommand(userSql, conn, tran))
                {
                    cmdUser.Parameters.Add("@IsActive", SqlDbType.Bit).Value = newActiveStatus;
                    cmdUser.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmdUser.ExecuteNonQuery();
                }

                tran.Commit();
                string statusWord = newActiveStatus ? "restored to Active" : "archived and deactivated";
                return (true, $"Doctor has been {statusWord} successfully.");
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return (false, $"Could not change doctor status: {ex.Message}");
        }
    }

    public (bool Success, string Message) DeleteDoctor(int doctorId, string doctorName)
    {
        UserSession.RequireRole("Admin");
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();

            // 1. Safety Check: Verify existing appointments, patient visits, and prescriptions
            // (Consultations link through PatientVisits, not directly via DoctorId)
            const string countSql = @"
DECLARE @LinkedCount INT = 0;

IF OBJECT_ID(N'dbo.Appointments', N'U') IS NOT NULL
    SELECT @LinkedCount = @LinkedCount + COUNT(1) FROM dbo.Appointments WHERE DoctorId = @DoctorId;

IF OBJECT_ID(N'dbo.PatientVisits', N'U') IS NOT NULL
    SELECT @LinkedCount = @LinkedCount + COUNT(1) FROM dbo.PatientVisits WHERE DoctorId = @DoctorId;

IF OBJECT_ID(N'dbo.Prescriptions', N'U') IS NOT NULL
    SELECT @LinkedCount = @LinkedCount + COUNT(1) FROM dbo.Prescriptions WHERE DoctorId = @DoctorId;

SELECT @LinkedCount;";

            int linkedRecords;
            using (var cmdCount = new SqlCommand(countSql, conn))
            {
                cmdCount.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                linkedRecords = Convert.ToInt32(cmdCount.ExecuteScalar() ?? 0);
            }

            if (linkedRecords > 0)
            {
                return (false, $"Cannot permanently delete {doctorName} because {linkedRecords} patient appointment(s) or clinical record(s) exist in the database.\n\nPlease use 'Archive Doctor' instead to remove them from active practice while preserving medical history.");
            }

            // 2. Safe to permanently delete (zero patient records attached)
            using var tran = conn.BeginTransaction();
            try
            {
                const string delExSql = @"IF OBJECT_ID(N'dbo.DoctorScheduleExceptions', N'U') IS NOT NULL DELETE FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId;";
                using (var cmd = new SqlCommand(delExSql, conn, tran))
                {
                    cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmd.ExecuteNonQuery();
                }

                const string delWkSql = @"IF OBJECT_ID(N'dbo.DoctorWeeklySchedules', N'U') IS NOT NULL DELETE FROM dbo.DoctorWeeklySchedules WHERE DoctorId = @DoctorId;";
                using (var cmd = new SqlCommand(delWkSql, conn, tran))
                {
                    cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmd.ExecuteNonQuery();
                }

                const string delSchedSql = @"IF OBJECT_ID(N'dbo.DoctorSchedules', N'U') IS NOT NULL DELETE FROM dbo.DoctorSchedules WHERE DoctorId = @DoctorId;";
                using (var cmd = new SqlCommand(delSchedSql, conn, tran))
                {
                    cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmd.ExecuteNonQuery();
                }

                int? userId = null;
                using (var cmdUser = new SqlCommand("SELECT UserId FROM dbo.Doctors WHERE DoctorId = @DoctorId;", conn, tran))
                {
                    cmdUser.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    var uVal = cmdUser.ExecuteScalar();
                    if (uVal != null && uVal != DBNull.Value) userId = Convert.ToInt32(uVal);
                }

                using (var cmdDoc = new SqlCommand("DELETE FROM dbo.Doctors WHERE DoctorId = @DoctorId;", conn, tran))
                {
                    cmdDoc.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmdDoc.ExecuteNonQuery();
                }

                if (userId.HasValue)
                {
                    using (var cmdDelUser = new SqlCommand("DELETE FROM dbo.Users WHERE UserId = @UserId;", conn, tran))
                    {
                        cmdDelUser.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
                        cmdDelUser.ExecuteNonQuery();
                    }
                }

                tran.Commit();
                return (true, $"{doctorName} and their portal login were permanently deleted.");
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return (false, $"Failed to delete doctor: {ex.Message}");
        }
    }

    public (bool Success, string Message) ResetDoctorPassword(int userId, string newPlainPassword)
    {
        UserSession.RequireRole("Admin");
        try
        {
            string newHash = PasswordHasher.HashPassword(newPlainPassword);
            const string sql = "UPDATE dbo.Users SET PasswordHash = @Hash, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId;";

            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Hash", SqlDbType.NVarChar, 512).Value = newHash;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            conn.Open();

            int rows = cmd.ExecuteNonQuery();
            return rows > 0 ? (true, "Password updated successfully.") : (false, "User account not found.");
        }
        catch (Exception ex)
        {
            return (false, $"Could not reset password: {ex.Message}");
        }
    }

    public (bool Success, string Message) UpdateDoctorCredentials(int userId, string newUsername, string? newPlainPassword = null)
    {
        UserSession.RequireRole("Admin");
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();

            // Check if another user already has this username
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

            string sql;
            if (!string.IsNullOrWhiteSpace(newPlainPassword))
            {
                sql = "UPDATE dbo.Users SET Username = @Username, PasswordHash = @Hash, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId;";
            }
            else
            {
                sql = "UPDATE dbo.Users SET Username = @Username, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId;";
            }

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

    public (bool Success, string Message) ProvisionLoginForDoctor(int doctorId, string fullName, string username, string plainPassword)
    {
        UserSession.RequireRole("Admin");
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            const string checkSql = "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username;";
            using (var cmdCheck = new SqlCommand(checkSql, conn, tran))
            {
                cmdCheck.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username.Trim();
                if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                {
                    tran.Rollback();
                    return (false, $"Username '{username}' is already taken.");
                }
            }

            int roleId = 2;
            const string roleSql = "SELECT TOP 1 RoleId FROM dbo.Roles WHERE RoleName = 'Doctor';";
            using (var cmdRole = new SqlCommand(roleSql, conn, tran))
            {
                var r = cmdRole.ExecuteScalar();
                if (r != null && r != DBNull.Value) roleId = Convert.ToInt32(r);
            }

            string hash = PasswordHasher.HashPassword(plainPassword);
            const string userSql = @"
INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, PasswordSalt, IsActive, CreatedAt)
OUTPUT INSERTED.UserId
VALUES (@Username, @FullName, @RoleId, @Hash, N'', 1, SYSUTCDATETIME());";

            int newUserId;
            using (var cmdUser = new SqlCommand(userSql, conn, tran))
            {
                cmdUser.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username.Trim();
                cmdUser.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = fullName;
                cmdUser.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                cmdUser.Parameters.Add("@Hash", SqlDbType.NVarChar, 512).Value = hash;
                newUserId = Convert.ToInt32(cmdUser.ExecuteScalar());
            }

            const string linkSql = "UPDATE dbo.Doctors SET UserId = @UserId, UpdatedAt = SYSUTCDATETIME() WHERE DoctorId = @DoctorId;";
            using (var cmdLink = new SqlCommand(linkSql, conn, tran))
            {
                cmdLink.Parameters.Add("@UserId", SqlDbType.Int).Value = newUserId;
                cmdLink.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmdLink.ExecuteNonQuery();
            }

            tran.Commit();
            return (true, $"Login account '{username}' created and linked successfully.");
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return (false, $"Error creating login: {ex.Message}");
        }
    }

    public (int DoctorId, string DoctorName, string Specialty)? GetDoctorByUserId(int userId)
    {
        const string sql = @"
SELECT TOP 1 
    d.DoctorId,
    CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
    ISNULL(d.Specialty, N'Family Medicine') AS Specialty
FROM dbo.Doctors d
WHERE d.UserId = @UserId AND d.IsActive = 1;";

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
}