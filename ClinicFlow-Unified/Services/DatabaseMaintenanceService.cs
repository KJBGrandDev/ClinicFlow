#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Services;

public sealed class DatabaseMaintenanceService
{
    private readonly DatabaseMaintenanceRepository _repo = new();

    public string GetDefaultBackupDirectory() => _repo.GetDefaultBackupDirectory();

    public async Task<(bool Success, string Message, string? BackupPath)> BackupDatabaseAsync(string? targetPath = null)
    {
        UserSession.RequireRole("Admin");

        try
        {
            string backupFile = await _repo.BackupDatabaseAsync(targetPath);
            int userId = UserSession.UserId ?? 1;

            AuditLogger.Log(
                "BACKUP",
                "Database",
                0,
                $"Database backup successfully generated to: {Path.GetFileName(backupFile)}");

            return (true, $"Database successfully backed up to:\n{backupFile}", backupFile);
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "DatabaseMaintenanceService.BackupDatabaseAsync");
            return (false, $"Database backup failed: {ErrorLogger.GetFriendlyMessage(ex)}", null);
        }
    }

    public async Task<(bool Success, string Message)> VerifyBackupAsync(string backupFilePath)
    {
        UserSession.RequireRole("Admin");

        try
        {
            await _repo.VerifyBackupAsync(backupFilePath);
            return (true, "Backup file verification succeeded. The backup file is healthy and restorable.");
        }
        catch (FileNotFoundException fnfEx)
        {
            return (false, $"Backup file verification failed: {fnfEx.Message}");
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "DatabaseMaintenanceService.VerifyBackupAsync");
            return (false, $"Backup file verification failed: {ErrorLogger.GetFriendlyMessage(ex)}");
        }
    }

    public async Task<(bool Success, string Message)> RestoreDatabaseAsync(string backupFilePath)
    {
        UserSession.RequireRole("Admin");

        try
        {
            // Verify file first
            await _repo.VerifyBackupAsync(backupFilePath);

            // Execute restore
            await _repo.RestoreDatabaseAsync(backupFilePath);

            int userId = UserSession.UserId ?? 1;
            AuditLogger.Log(
                "RESTORE",
                "Database",
                0,
                $"Database restored successfully from backup file: {Path.GetFileName(backupFilePath)}");

            return (true, "Database restored successfully. All tables and transaction data have been restored.");
        }
        catch (FileNotFoundException fnfEx)
        {
            return (false, $"Database restore failed: {fnfEx.Message}");
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "DatabaseMaintenanceService.RestoreDatabaseAsync");
            return (false, $"Database restore failed: {ErrorLogger.GetFriendlyMessage(ex)}");
        }
    }
}
