#nullable enable

using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;

namespace ClinicFlow_Unified.Repositories;

public sealed class DatabaseMaintenanceRepository
{
    public Task<string> GetDefaultBackupDirectoryAsync() => Task.FromResult(GetDefaultBackupDirectory());

    public string GetDefaultBackupDirectory()
    {
        string defaultDir = @"C:\ClinicFlow\Backups";
        if (!Directory.Exists(defaultDir))
        {
            try
            {
                Directory.CreateDirectory(defaultDir);
            }
            catch
            {
                defaultDir = Path.Combine(AppContext.BaseDirectory, "Backups");
                if (!Directory.Exists(defaultDir)) Directory.CreateDirectory(defaultDir);
            }
        }
        return defaultDir;
    }

    public async Task<string> BackupDatabaseAsync(string? targetPath = null)
    {
        string dbName = DatabaseConnection.DatabaseName;
        string backupDir = await GetDefaultBackupDirectoryAsync();

        string destinationFile = string.IsNullOrWhiteSpace(targetPath)
            ? Path.Combine(backupDir, $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak")
            : targetPath;

        string backupName = $"{dbName}-Full Database Backup {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        string? targetDir = Path.GetDirectoryName(destinationFile);
        if (!string.IsNullOrWhiteSpace(targetDir) && !Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        using var conn = DatabaseConnection.CreateMasterConnection();
        await conn.OpenAsync().ConfigureAwait(false);

        string sql = $@"
DECLARE @disk NVARCHAR(500) = @DestinationFile;
DECLARE @bName NVARCHAR(200) = @BackupName;
BACKUP DATABASE [{dbName}]
TO DISK = @disk
WITH FORMAT, INIT, NAME = @bName, SKIP, NOREWIND, NOUNLOAD;";

        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 120;
        cmd.Parameters.Add("@DestinationFile", SqlDbType.NVarChar, 500).Value = destinationFile;
        cmd.Parameters.Add("@BackupName", SqlDbType.NVarChar, 200).Value = backupName;

        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

        return destinationFile;
    }

    public async Task<bool> VerifyBackupAsync(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException($"The specified backup file does not exist: {backupFilePath}");

        using var conn = DatabaseConnection.CreateMasterConnection();
        await conn.OpenAsync().ConfigureAwait(false);

        const string sql = "RESTORE VERIFYONLY FROM DISK = @BackupFilePath;";
        using var cmd = new SqlCommand(sql, conn);
        cmd.CommandTimeout = 120;
        cmd.Parameters.Add("@BackupFilePath", SqlDbType.NVarChar, 500).Value = backupFilePath;

        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        return true;
    }

    public async Task RestoreDatabaseAsync(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException($"The specified backup file does not exist: {backupFilePath}");

        string dbName = DatabaseConnection.DatabaseName;

        using var conn = DatabaseConnection.CreateMasterConnection();
        await conn.OpenAsync().ConfigureAwait(false);

        try
        {
            // 1. Force single user to disconnect active sessions
            string singleUserSql = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
            using (var cmdSingle = new SqlCommand(singleUserSql, conn))
            {
                cmdSingle.CommandTimeout = 60;
                await cmdSingle.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // 2. Execute Restore
            string restoreSql = $"RESTORE DATABASE [{dbName}] FROM DISK = @BackupFilePath WITH REPLACE;";
            using (var cmdRestore = new SqlCommand(restoreSql, conn))
            {
                cmdRestore.CommandTimeout = 300;
                cmdRestore.Parameters.Add("@BackupFilePath", SqlDbType.NVarChar, 500).Value = backupFilePath;
                await cmdRestore.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            // 3. Always bring database back to MULTI_USER
            try
            {
                string multiUserSql = $"ALTER DATABASE [{dbName}] SET MULTI_USER;";
                using var cmdMulti = new SqlCommand(multiUserSql, conn);
                cmdMulti.CommandTimeout = 60;
                await cmdMulti.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch
            {
                // Suppress if already in multi-user
            }
        }
    }
}
