#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Repositories;

public sealed class SecretaryAccountItem
{
    public int SecretaryId { get; set; }
    public int? UserId { get; set; }
    public string SecretaryCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string ContactNumber { get; set; } = string.Empty;
    public string AssignedDesk { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string StatusDisplay => IsActive ? "Active" : "Archived";
}

public sealed class SecretaryRepository
{
    private static bool _schemaInitialized = false;
    private static readonly object _schemaLock = new();

    public SecretaryRepository()
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
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Secretary')
BEGIN
    INSERT INTO dbo.Roles (RoleName, Description, IsActive, CreatedAT)
    VALUES ('Secretary', 'Front-desk secretary managing patient registrations, appointments, and lobby queue.', 1, SYSUTCDATETIME());
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Secretaries')
BEGIN
    CREATE TABLE dbo.Secretaries (
        SecretaryId INT IDENTITY(1,1) PRIMARY KEY,
        SecretaryCode NVARCHAR(20) NOT NULL,
        UserId INT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        ContactNumber NVARCHAR(30) NULL,
        AssignedDesk NVARCHAR(100) NULL,
        EmailAddress NVARCHAR(150) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL
    );
END
";
                using var cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                _schemaInitialized = true;
            }
            catch
            {
                // Suppress if already exists or permission restricted
            }
        }
    }

    private void SeedInitialSecretariesIfEmpty()
    {
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();

            const string checkSql = "SELECT COUNT(1) FROM dbo.Secretaries;";
            using var cmdCheck = new SqlCommand(checkSql, conn);
            int count = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0);
            if (count > 0) return;

            // Seed default front-desk secretaries
            RegisterSecretaryWithAccount(
                firstName: "Ana",
                lastName: "Reyes",
                contactNumber: "09171234567",
                assignedDesk: "Main Lobby Reception Desk",
                emailAddress: "ana.reyes@clinicflow.ph",
                username: "secretary",
                plainPassword: "password123");

            RegisterSecretaryWithAccount(
                firstName: "Maria",
                lastName: "Santos",
                contactNumber: "09289876543",
                assignedDesk: "Appointments & Scheduling Desk",
                emailAddress: "maria.santos@clinicflow.ph",
                username: "msantos",
                plainPassword: "password123");
        }
        catch
        {
            // Seed failure should never crash the app
        }
    }

    public List<SecretaryAccountItem> GetAllSecretaries()
    {
        EnsureSchema();
        SeedInitialSecretariesIfEmpty();
        const string sql = @"
SELECT 
    s.SecretaryId,
    s.UserId,
    s.SecretaryCode,
    s.FirstName,
    s.LastName,
    ISNULL(s.ContactNumber, N'N/A') AS ContactNumber,
    ISNULL(s.AssignedDesk, N'Main Reception Desk') AS AssignedDesk,
    ISNULL(s.EmailAddress, N'') AS EmailAddress,
    ISNULL(u.Username, N'(No Login)') AS Username,
    s.IsActive
FROM dbo.Secretaries s
LEFT JOIN dbo.Users u ON u.UserId = s.UserId
ORDER BY s.LastName, s.FirstName;";

        var list = new List<SecretaryAccountItem>();
        using var conn = DatabaseConnection.CreateConnection();
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new SecretaryAccountItem
            {
                SecretaryId = rdr.GetInt32(0),
                UserId = rdr.IsDBNull(1) ? null : rdr.GetInt32(1),
                SecretaryCode = rdr.GetString(2),
                FirstName = rdr.GetString(3),
                LastName = rdr.GetString(4),
                ContactNumber = rdr.GetString(5),
                AssignedDesk = rdr.GetString(6),
                EmailAddress = rdr.GetString(7),
                Username = rdr.GetString(8),
                IsActive = rdr.GetBoolean(9)
            });
        }
        return list;
    }

    public (bool Success, string Message, int SecretaryId) RegisterSecretaryWithAccount(
        string firstName,
        string lastName,
        string contactNumber,
        string assignedDesk,
        string emailAddress,
        string username,
        string plainPassword)
    {
        UserSession.RequireRole("Admin");
        EnsureSchema();
        using var conn = DatabaseConnection.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            // 1. Verify username uniqueness
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

            // 2. Resolve RoleId for Secretary (fallback to Receptionist or Role 2)
            int secretaryRoleId = 2;
            const string getRoleSql = @"
SELECT TOP 1 RoleId FROM dbo.Roles 
WHERE RoleName IN ('Secretary', 'Receptionist')
ORDER BY CASE WHEN RoleName = 'Secretary' THEN 1 ELSE 2 END;";

            using (var cmdRole = new SqlCommand(getRoleSql, conn, tran))
            {
                var roleObj = cmdRole.ExecuteScalar();
                if (roleObj != null && roleObj != DBNull.Value)
                    secretaryRoleId = Convert.ToInt32(roleObj);
            }

            // 3. Create Login in dbo.Users
            string passwordHash = PasswordHasher.HashPassword(plainPassword);
            string fullName = $"{firstName.Trim()} {lastName.Trim()}";

            const string insertUserSql = @"
INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, PasswordSalt, IsActive, CreatedAt)
OUTPUT INSERTED.UserId
VALUES (@Username, @FullName, @RoleId, @PasswordHash, N'', 1, SYSUTCDATETIME());";

            int newUserId;
            using (var cmdUser = new SqlCommand(insertUserSql, conn, tran))
            {
                cmdUser.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username.Trim();
                cmdUser.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = fullName;
                cmdUser.Parameters.Add("@RoleId", SqlDbType.Int).Value = secretaryRoleId;
                cmdUser.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = passwordHash;
                newUserId = Convert.ToInt32(cmdUser.ExecuteScalar());
            }

            // 4. Create Secretary Record
            const string insertSecSql = @"
DECLARE @NextNum INT = (SELECT ISNULL(MAX(SecretaryId), 0) + 1 FROM dbo.Secretaries);
DECLARE @SecCode NVARCHAR(20) = CONCAT(N'SEC-', RIGHT(CONCAT(N'0000', @NextNum), 4));

INSERT INTO dbo.Secretaries (SecretaryCode, UserId, FirstName, LastName, ContactNumber, AssignedDesk, EmailAddress, IsActive, CreatedAt)
OUTPUT INSERTED.SecretaryId
VALUES (@SecCode, @UserId, @FirstName, @LastName, @ContactNumber, @AssignedDesk, @EmailAddress, 1, SYSUTCDATETIME());";

            int newSecretaryId;
            using (var cmdSec = new SqlCommand(insertSecSql, conn, tran))
            {
                cmdSec.Parameters.Add("@UserId", SqlDbType.Int).Value = newUserId;
                cmdSec.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName.Trim();
                cmdSec.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName.Trim();
                cmdSec.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 30).Value = (object?)contactNumber?.Trim() ?? DBNull.Value;
                cmdSec.Parameters.Add("@AssignedDesk", SqlDbType.NVarChar, 100).Value = (object?)assignedDesk?.Trim() ?? DBNull.Value;
                cmdSec.Parameters.Add("@EmailAddress", SqlDbType.NVarChar, 150).Value = (object?)emailAddress?.Trim() ?? DBNull.Value;
                newSecretaryId = Convert.ToInt32(cmdSec.ExecuteScalar());
            }

            tran.Commit();
            return (true, $"Secretary {fullName} registered successfully with login username '{username}'.", newSecretaryId);
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return (false, $"Failed to register secretary: {ex.Message}", 0);
        }
    }

    public (bool Success, string Message) UpdateSecretaryProfile(
        int secretaryId,
        string firstName,
        string lastName,
        string contactNumber,
        string assignedDesk,
        string emailAddress)
    {
        UserSession.RequireRole("Admin");
        try
        {
            const string sql = @"
UPDATE dbo.Secretaries 
SET FirstName = @FirstName,
    LastName = @LastName,
    ContactNumber = @ContactNumber,
    AssignedDesk = @AssignedDesk,
    EmailAddress = @EmailAddress,
    UpdatedAt = SYSUTCDATETIME()
WHERE SecretaryId = @SecretaryId;";

            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@SecretaryId", SqlDbType.Int).Value = secretaryId;
            cmd.Parameters.Add("@FirstName", SqlDbType.NVarChar, 100).Value = firstName.Trim();
            cmd.Parameters.Add("@LastName", SqlDbType.NVarChar, 100).Value = lastName.Trim();
            cmd.Parameters.Add("@ContactNumber", SqlDbType.NVarChar, 30).Value = (object?)contactNumber?.Trim() ?? DBNull.Value;
            cmd.Parameters.Add("@AssignedDesk", SqlDbType.NVarChar, 100).Value = (object?)assignedDesk?.Trim() ?? DBNull.Value;
            cmd.Parameters.Add("@EmailAddress", SqlDbType.NVarChar, 150).Value = (object?)emailAddress?.Trim() ?? DBNull.Value;
            conn.Open();

            int rows = cmd.ExecuteNonQuery();
            return rows > 0 ? (true, "Secretary profile updated successfully.") : (false, "Secretary record not found.");
        }
        catch (Exception ex)
        {
            return (false, $"Error updating profile: {ex.Message}");
        }
    }

    public (bool Success, string Message) ToggleSecretaryArchive(int secretaryId, bool setInactive)
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

                const string secSql = @"
UPDATE dbo.Secretaries 
SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() 
WHERE SecretaryId = @SecretaryId;";
                using (var cmdSec = new SqlCommand(secSql, conn, tran))
                {
                    cmdSec.Parameters.Add("@IsActive", SqlDbType.Bit).Value = newActiveStatus;
                    cmdSec.Parameters.Add("@SecretaryId", SqlDbType.Int).Value = secretaryId;
                    cmdSec.ExecuteNonQuery();
                }

                const string userSql = @"
UPDATE u 
SET u.IsActive = @IsActive, u.UpdatedAt = SYSUTCDATETIME()
FROM dbo.Users u
INNER JOIN dbo.Secretaries s ON s.UserId = u.UserId
WHERE s.SecretaryId = @SecretaryId;";
                using (var cmdUser = new SqlCommand(userSql, conn, tran))
                {
                    cmdUser.Parameters.Add("@IsActive", SqlDbType.Bit).Value = newActiveStatus;
                    cmdUser.Parameters.Add("@SecretaryId", SqlDbType.Int).Value = secretaryId;
                    cmdUser.ExecuteNonQuery();
                }

                tran.Commit();
                string statusWord = newActiveStatus ? "restored to Active" : "archived and deactivated";
                return (true, $"Secretary has been {statusWord} successfully.");
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return (false, $"Could not change secretary status: {ex.Message}");
        }
    }

    public (bool Success, string Message) DeleteSecretary(int secretaryId, string secretaryName)
    {
        UserSession.RequireRole("Admin");
        try
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();

            int? userId = null;
            const string getUserIdSql = "SELECT UserId FROM dbo.Secretaries WHERE SecretaryId = @SecretaryId;";
            using (var cmdGet = new SqlCommand(getUserIdSql, conn))
            {
                cmdGet.Parameters.Add("@SecretaryId", SqlDbType.Int).Value = secretaryId;
                var res = cmdGet.ExecuteScalar();
                if (res != null && res != DBNull.Value)
                    userId = Convert.ToInt32(res);
            }

            // Relational Safety Check: Verify existing linked records before allowing permanent deletion
            const string countSql = @"
DECLARE @LinkedCount INT = 0;

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NOT NULL AND @UserId IS NOT NULL
    SELECT @LinkedCount = @LinkedCount + COUNT(1) FROM dbo.AuditLogs WHERE UserId = @UserId;

IF OBJECT_ID(N'dbo.Appointments', N'U') IS NOT NULL AND @UserId IS NOT NULL
    SELECT @LinkedCount = @LinkedCount + COUNT(1) FROM dbo.Appointments WHERE CreatedByUserId = @UserId;

SELECT @LinkedCount;";

            int linkedRecords = 0;
            using (var cmdCount = new SqlCommand(countSql, conn))
            {
                cmdCount.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)userId ?? DBNull.Value;
                linkedRecords = Convert.ToInt32(cmdCount.ExecuteScalar() ?? 0);
            }

            if (linkedRecords > 0)
            {
                return (false, $"Cannot permanently delete secretary '{secretaryName}' because {linkedRecords} historical audit log(s) or transactional record(s) exist in the database.\n\nPlease use 'Archive Secretary' instead to deactivate their account while preserving operational audit history.");
            }

            using var tran = conn.BeginTransaction();
            try
            {
                const string delSecSql = "DELETE FROM dbo.Secretaries WHERE SecretaryId = @SecretaryId;";
                using (var cmdDel = new SqlCommand(delSecSql, conn, tran))
                {
                    cmdDel.Parameters.Add("@SecretaryId", SqlDbType.Int).Value = secretaryId;
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
                return (true, $"Secretary record for '{secretaryName}' has been permanently deleted.");
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return (false, $"Failed to delete secretary: {ex.Message}");
        }
    }

    public (bool Success, string Message) UpdateSecretaryCredentials(int userId, string newUsername, string? newPlainPassword = null)
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

    public (bool Success, string Message) ProvisionLoginForSecretary(int secretaryId, string fullName, string username, string plainPassword)
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
            const string roleSql = @"
SELECT TOP 1 RoleId FROM dbo.Roles 
WHERE RoleName IN ('Secretary', 'Receptionist')
ORDER BY CASE WHEN RoleName = 'Secretary' THEN 1 ELSE 2 END;";
            using (var cmdRole = new SqlCommand(roleSql, conn, tran))
            {
                var r = cmdRole.ExecuteScalar();
                if (r != null && r != DBNull.Value)
                    roleId = Convert.ToInt32(r);
            }

            string passwordHash = PasswordHasher.HashPassword(plainPassword);

            const string insertUserSql = @"
INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, PasswordSalt, IsActive, CreatedAt)
OUTPUT INSERTED.UserId
VALUES (@Username, @FullName, @RoleId, @PasswordHash, N'', 1, SYSUTCDATETIME());";

            int newUserId;
            using (var cmdUser = new SqlCommand(insertUserSql, conn, tran))
            {
                cmdUser.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username.Trim();
                cmdUser.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = fullName;
                cmdUser.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                cmdUser.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = passwordHash;
                newUserId = Convert.ToInt32(cmdUser.ExecuteScalar());
            }

            const string linkSql = "UPDATE dbo.Secretaries SET UserId = @UserId, UpdatedAt = SYSUTCDATETIME() WHERE SecretaryId = @SecretaryId;";
            using (var cmdLink = new SqlCommand(linkSql, conn, tran))
            {
                cmdLink.Parameters.Add("@UserId", SqlDbType.Int).Value = newUserId;
                cmdLink.Parameters.Add("@SecretaryId", SqlDbType.Int).Value = secretaryId;
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

    public (int SecretaryId, string SecretaryName, string AssignedDesk)? GetSecretaryByUserId(int userId)
    {
        EnsureSchema();
        const string sql = @"
SELECT TOP 1 
    s.SecretaryId,
    CONCAT(s.FirstName, N' ', s.LastName) AS SecretaryName,
    ISNULL(s.AssignedDesk, N'Main Reception Desk') AS AssignedDesk
FROM dbo.Secretaries s
WHERE s.UserId = @UserId AND s.IsActive = 1;";

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
