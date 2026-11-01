#nullable enable

using System;
using System.Data;
using System.IO;
using ClinicFlow_Unified.Data;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Utilities;

/// <summary>
/// Centralized Error Handling & Exception Logging System (REQ-003, REQ-024).
/// Captures exception details to dbo.ErrorLogs and app_error.log while returning
/// friendly, non-technical messages to end users.
/// </summary>
public static class ErrorLogger
{
    private static readonly object _fileLock = new();
    public const string LogFileName = "app_error.log";

    /// <summary>
    /// Logs an exception to both the database and file log, and returns a sanitized user-friendly message.
    /// </summary>
    public static string Log(Exception ex, string source, string? customUserMessage = null)
    {
        string friendlyMessage = customUserMessage ?? GetFriendlyMessage(ex);

        // 1. Log to disk file (guaranteed even if database is down)
        LogToFile(ex, source);

        // 2. Log to database table dbo.ErrorLogs
        LogToDatabase(ex, source);

        return friendlyMessage;
    }

    /// <summary>
    /// Converts raw exceptions (especially SqlException) into polite, non-technical notices.
    /// </summary>
    public static string GetFriendlyMessage(Exception ex)
    {
        if (ex is SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                2627 or 2601 => "A record with this information already exists in the system. Duplicate records are not permitted.",
                547 => "The requested action cannot be completed because this record is linked to other critical data in the system.",
                -2 => "The database operation timed out. Please verify database connectivity and try again.",
                4060 or 18456 or 53 => "Unable to connect to the database server. Please check your network and database configuration.",
                _ => "A database error occurred while processing your request. The technical details have been logged for administrator review."
            };
        }

        if (ex is DBConcurrencyException)
        {
            return "This record has been modified by another user since you loaded it. Please reload the latest data and try again.";
        }

        if (ex is UnauthorizedAccessException)
        {
            return "You do not have permission to perform this action.";
        }

        if (ex is InvalidOperationException or ArgumentException)
        {
            return ex.Message;
        }

        return "An unexpected system error occurred. The incident has been recorded in the system error log.";
    }

    private static void LogToFile(Exception ex, string source)
    {
        try
        {
            lock (_fileLock)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);
                string entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}] [USER: {UserSession.Username ?? "Anonymous"}] [SOURCE: {source}]{Environment.NewLine}" +
                               $"Type: {ex.GetType().FullName}{Environment.NewLine}" +
                               $"Message: {ex.Message}{Environment.NewLine}" +
                               $"StackTrace: {ex.StackTrace}{Environment.NewLine}" +
                               new string('-', 80) + Environment.NewLine;

                File.AppendAllText(logPath, entry);
            }
        }
        catch
        {
            // Logging to file failure should not crash application
        }
    }

    private static void LogToDatabase(Exception ex, string source)
    {
        try
        {
            const string sql = @"
INSERT INTO dbo.ErrorLogs (UserId, MachineName, Source, ErrorMessage, StackTrace, LoggedAt)
VALUES (@UserId, @MachineName, @Source, @ErrorMessage, @StackTrace, SYSUTCDATETIME());";

            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);

            object userIdVal = UserSession.IsAuthenticated && UserSession.UserId > 0
                ? UserSession.UserId
                : DBNull.Value;

            string machine = Environment.MachineName;
            if (machine.Length > 100) machine = machine[..100];
            if (source.Length > 200) source = source[..200];

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userIdVal;
            cmd.Parameters.Add("@MachineName", SqlDbType.NVarChar, 100).Value = machine;
            cmd.Parameters.Add("@Source", SqlDbType.NVarChar, 200).Value = source;
            cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, -1).Value = ex.Message;
            cmd.Parameters.Add("@StackTrace", SqlDbType.NVarChar, -1).Value = (object?)ex.StackTrace ?? DBNull.Value;

            conn.Open();
            cmd.ExecuteNonQuery();
        }
        catch
        {
            // DB logging failure should never crash application
        }
    }
}
