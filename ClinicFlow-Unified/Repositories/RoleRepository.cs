using System.Data;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Repositories;

public sealed class RoleRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public RoleRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Role?> GetActiveByNameAsync(string roleName)
    {
        const string sql = """
            SELECT
                RoleId,
                RoleName,
                Description,
                IsActive,
                CreatedAt
            FROM dbo.Roles
            WHERE RoleName = @RoleName
              AND IsActive = 1;
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@RoleName",
            SqlDbType.NVarChar,
            50).Value = roleName;

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Role
        {
            RoleId = reader.GetInt32(0),
            RoleName = reader.GetString(1),
            Description = reader.IsDBNull(2)
                ? null
                : reader.GetString(2),
            IsActive = reader.GetBoolean(3),
            CreatedAt = reader.GetDateTime(4)
        };
    }

    public async Task<List<Role>> GetAllActiveRolesAsync()
    {
        const string sql = """
            SELECT RoleId, RoleName, Description, IsActive, CreatedAt
            FROM dbo.Roles
            WHERE IsActive = 1
            ORDER BY RoleId ASC;
            """;

        var list = new List<Role>();
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using SqlCommand command = new SqlCommand(sql, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Role
            {
                RoleId = reader.GetInt32(0),
                RoleName = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsActive = reader.GetBoolean(3),
                CreatedAt = reader.GetDateTime(4)
            });
        }

        return list;
    }
}