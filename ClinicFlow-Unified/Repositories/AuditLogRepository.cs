using System.Data;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Repositories;

public sealed class AuditLogRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public AuditLogRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> InsertAsync(AuditLogEntry auditLogEntry)
    {
        const string sql = """
            INSERT INTO dbo.AuditLogs
            (
                UserId,
                ActionType,
                EntityName,
                RecordId,
                Description,
                MachineName,
                BeforeJson,
                AfterJson
            )
            VALUES
            (
                @UserId,
                @ActionType,
                @EntityName,
                @RecordId,
                @Description,
                @MachineName,
                @BeforeJson,
                @AfterJson
            );

            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value =
            (object?)auditLogEntry.UserId ?? DBNull.Value;

        command.Parameters.Add(
            "@ActionType",
            SqlDbType.NVarChar,
            50).Value = auditLogEntry.ActionType;

        command.Parameters.Add(
            "@EntityName",
            SqlDbType.NVarChar,
            100).Value = auditLogEntry.EntityName;

        command.Parameters.Add(
            "@RecordId",
            SqlDbType.NVarChar,
            50).Value =
            (object?)auditLogEntry.RecordId ?? DBNull.Value;

        command.Parameters.Add(
            "@Description",
            SqlDbType.NVarChar,
            500).Value = auditLogEntry.Description;

        command.Parameters.Add(
            "@MachineName",
            SqlDbType.NVarChar,
            100).Value =
            (object?)auditLogEntry.MachineName ?? DBNull.Value;

        command.Parameters.Add(
            "@BeforeJson",
            SqlDbType.NVarChar,
            -1).Value =
            (object?)auditLogEntry.BeforeJson ?? DBNull.Value;

        command.Parameters.Add(
            "@AfterJson",
            SqlDbType.NVarChar,
            -1).Value =
            (object?)auditLogEntry.AfterJson ?? DBNull.Value;

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt64(result);
    }

    public async Task<List<AuditLogViewItem>> SearchLogsAsync(
        string? actionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        int top = 250)
    {
        const string sql = """
            SELECT TOP (@Top)
                a.AuditLogId,
                a.UserId,
                ISNULL(u.Username, N'System') AS Username,
                ISNULL(u.FullName, N'System Service') AS UserFullName,
                a.ActionType,
                a.EntityName,
                a.RecordId,
                a.Description,
                a.MachineName,
                a.LoggedAt
            FROM dbo.AuditLogs a
            LEFT JOIN dbo.Users u ON u.UserId = a.UserId
            WHERE (@ActionType IS NULL OR @ActionType = '' OR a.ActionType = @ActionType)
              AND (@FromDate IS NULL OR a.LoggedAt >= @FromDate)
              AND (@ToDate IS NULL OR a.LoggedAt <= @ToDate)
              AND (@Search IS NULL OR @Search = '' OR a.Description LIKE @SearchPattern OR a.EntityName LIKE @SearchPattern OR u.Username LIKE @SearchPattern)
            ORDER BY a.LoggedAt DESC;
            """;

        var list = new List<AuditLogViewItem>();
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Top", SqlDbType.Int).Value = top;
        command.Parameters.Add("@ActionType", SqlDbType.NVarChar, 50).Value = (object?)actionType ?? DBNull.Value;
        command.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = (object?)fromDate ?? DBNull.Value;
        command.Parameters.Add("@ToDate", SqlDbType.DateTime2).Value = (object?)toDate ?? DBNull.Value;
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = (object?)search ?? DBNull.Value;
        command.Parameters.Add("@SearchPattern", SqlDbType.NVarChar, 105).Value = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : $"%{search.Trim()}%";

        await using SqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new AuditLogViewItem
            {
                AuditLogId = reader.GetInt64(0),
                UserId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Username = reader.GetString(2),
                UserFullName = reader.GetString(3),
                ActionType = reader.GetString(4),
                EntityName = reader.GetString(5),
                RecordId = reader.IsDBNull(6) ? null : reader.GetString(6),
                Description = reader.GetString(7),
                MachineName = reader.IsDBNull(8) ? null : reader.GetString(8),
                LoggedAt = reader.GetDateTime(9)
            });
        }

        return list;
    }

    public async Task<List<string>> GetDistinctActionTypesAsync()
    {
        const string sql = """
            SELECT DISTINCT ActionType
            FROM dbo.AuditLogs
            WHERE ActionType IS NOT NULL
            ORDER BY ActionType ASC;
            """;

        var list = new List<string>();
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using SqlCommand command = new SqlCommand(sql, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(reader.GetString(0));
        }

        return list;
    }
}