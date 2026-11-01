#nullable enable

using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;

namespace ClinicFlow_Unified.Utilities;

public static class AuditLogger
{
    public static void Log(string action, string entityName, int? recordId, string? description = null)
    {
        const string sql = @"
INSERT INTO dbo.AuditLogs (UserId, ActionType, EntityName, RecordId, Description, LoggedAt)
VALUES (@UserId, @ActionType, @EntityName, @RecordId, @Description, SYSUTCDATETIME());";

        try
        {
            using var connection = DatabaseConnection.CreateConnection();
            using var command = new SqlCommand(sql, connection);

            object userIdVal = UserSession.IsAuthenticated && UserSession.UserId > 0
                ? UserSession.UserId
                : DBNull.Value;

            string recId = recordId.HasValue ? recordId.Value.ToString() : "";
            string desc = string.IsNullOrWhiteSpace(description) ? $"{action} on {entityName}" : description;
            if (desc.Length > 500) desc = desc.Substring(0, 500);

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userIdVal;
            command.Parameters.Add("@ActionType", SqlDbType.NVarChar, 50).Value = action;
            command.Parameters.Add("@EntityName", SqlDbType.NVarChar, 100).Value = entityName;
            command.Parameters.Add("@RecordId", SqlDbType.NVarChar, 50).Value = string.IsNullOrEmpty(recId) ? DBNull.Value : recId;
            command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = desc;

            connection.Open();
            command.ExecuteNonQuery();
        }
        catch
        {
            // Logging errors are suppressed so they do not interrupt primary transactions
        }
    }
}