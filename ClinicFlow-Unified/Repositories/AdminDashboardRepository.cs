#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;

namespace ClinicFlow_Unified.Repositories;

public sealed class AdminDashboardRepository
{
    public AdminOverviewStats GetOverviewStats()
    {
        const string sql = @"
SELECT 
    (SELECT COUNT(1) FROM dbo.Patients WHERE IsActive = 1) AS TotalPatients,
    (SELECT COUNT(1) FROM dbo.Appointments WHERE AppointmentDate = CAST(GETDATE() AS DATE) AND Status NOT IN (N'Cancelled')) AS TodayAppointments,
    (SELECT COUNT(1) FROM dbo.Doctors WHERE IsActive = 1) AS ActiveDoctors,
    (SELECT COUNT(1) 
     FROM dbo.Appointments a
     LEFT JOIN dbo.AppointmentQueue q ON q.AppointmentId = a.AppointmentId
     WHERE a.AppointmentDate = CAST(GETDATE() AS DATE) 
       AND ISNULL(q.QueueStatus, a.Status) IN (N'Waiting', N'Called', N'In Service')) AS WaitingQueueCount;";

        var stats = new AdminOverviewStats();
        try
        {
            using var connection = DatabaseConnection.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                stats.TotalPatients = reader.GetInt32(reader.GetOrdinal("TotalPatients"));
                stats.TodayAppointments = reader.GetInt32(reader.GetOrdinal("TodayAppointments"));
                stats.ActiveDoctors = reader.GetInt32(reader.GetOrdinal("ActiveDoctors"));
                stats.WaitingQueueCount = reader.GetInt32(reader.GetOrdinal("WaitingQueueCount"));
            }
        }
        catch
        {
            // Fail gracefully if database or tables are unavailable
        }

        return stats;
    }

    public List<AdminAuditItem> GetRecentAuditLogs(int top = 15)
    {
        const string sql = @"
IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NOT NULL
BEGIN
    SELECT TOP (@Top)
        a.AuditLogId,
        ISNULL(u.FullName, ISNULL(u.Username, N'System')) AS PerformedBy,
        a.ActionType,
        a.EntityName,
        a.Description,
        a.LoggedAt
    FROM dbo.AuditLogs a
    LEFT JOIN dbo.Users u ON u.UserId = a.UserId
    ORDER BY a.LoggedAt DESC;
END;";

        var list = new List<AdminAuditItem>();
        try
        {
            using var connection = DatabaseConnection.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Top", SqlDbType.Int).Value = top;
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AdminAuditItem
                {
                    AuditLogId = reader.GetInt64(reader.GetOrdinal("AuditLogId")),
                    PerformedBy = reader.GetString(reader.GetOrdinal("PerformedBy")),
                    ActionType = reader.GetString(reader.GetOrdinal("ActionType")),
                    EntityName = reader.GetString(reader.GetOrdinal("EntityName")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    LoggedAt = reader.GetDateTime(reader.GetOrdinal("LoggedAt"))
                });
            }
        }
        catch
        {
            // Fail gracefully if table or connection is not ready
        }

        return list;
    }
}

