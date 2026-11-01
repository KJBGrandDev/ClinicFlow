using System.Data;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Repositories;

public sealed class UserRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public UserRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = """
        SELECT
            u.UserId,
            u.Username,
            u.PasswordHash,
            u.FullName,
            u.RoleId,
            u.IsActive,
            u.CreatedAt,
            u.UpdatedAt,
            r.RoleName,
            u.PasswordSalt,
            u.FailedLoginAttempts,
            u.LockoutUntil
        FROM dbo.Users AS u
        INNER JOIN dbo.Roles AS r
            ON r.RoleId = u.RoleId
        WHERE u.Username = @Username;
        """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Username",
            SqlDbType.NVarChar,
            50).Value = username;

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new User
        {
            UserId = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            FullName = reader.GetString(3),
            RoleId = reader.GetInt32(4),
            RoleName = reader.GetString(8),
            IsActive = reader.GetBoolean(5),
            CreatedAt = reader.GetDateTime(6),
            UpdatedAt = reader.IsDBNull(7)
                ? null
                : reader.GetDateTime(7),
            PasswordSalt = reader.IsDBNull(9)
                ? null
                : reader.GetString(9),
            FailedLoginAttempts = reader.IsDBNull(10)
                ? 0
                : reader.GetInt32(10),
            LockoutUntil = reader.IsDBNull(11)
                ? null
                : reader.GetDateTime(11)
        };
    }

    public async Task<int> CreateAsync(User user)
    {
        const string sql = """
            INSERT INTO dbo.Users
            (
                Username,
                PasswordHash,
                PasswordSalt,
                FullName,
                RoleId
            )
            VALUES
            (
                @Username,
                @PasswordHash,
                @PasswordSalt,
                @FullName,
                @RoleId
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Username",
            SqlDbType.NVarChar,
            50).Value = user.Username;

        command.Parameters.Add(
            "@PasswordHash",
            SqlDbType.NVarChar,
            512).Value = user.PasswordHash;

        command.Parameters.Add(
            "@PasswordSalt",
            SqlDbType.NVarChar,
            128).Value = user.PasswordSalt ?? string.Empty;

        command.Parameters.Add(
            "@FullName",
            SqlDbType.NVarChar,
            150).Value = user.FullName;

        command.Parameters.Add(
            "@RoleId",
            SqlDbType.Int).Value = user.RoleId;

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
    public async Task<bool> AnyUsersExistAsync()
    {
        const string sql = """
            SELECT CASE
                WHEN EXISTS (SELECT 1 FROM dbo.Users)
                THEN CAST(1 AS BIT)
                ELSE CAST(0 AS BIT)
            END;
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        object? result = await command.ExecuteScalarAsync();

        return Convert.ToBoolean(result);
    }

    public async Task RecordFailedLoginAsync(int userId)
    {
        const string sql = """
            UPDATE dbo.Users
            SET FailedLoginAttempts = FailedLoginAttempts + 1,
                LockoutUntil = CASE
                    WHEN FailedLoginAttempts + 1 >= 3 THEN DATEADD(MINUTE, 15, SYSUTCDATETIME())
                    ELSE LockoutUntil
                END,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId;
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value = userId;

        await command.ExecuteNonQueryAsync();
    }

    public async Task ResetFailedLoginsAsync(int userId)
    {
        const string sql = """
            UPDATE dbo.Users
            SET FailedLoginAttempts = 0,
                LockoutUntil = NULL,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId;
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value = userId;

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateUserStatusAsync(int userId, bool isActive)
    {
        const string sql = """
            UPDATE dbo.Users
            SET IsActive = @IsActive,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId;
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt)
    {
        const string sql = """
            UPDATE dbo.Users
            SET PasswordHash = @PasswordHash,
                PasswordSalt = @PasswordSalt,
                FailedLoginAttempts = 0,
                LockoutUntil = NULL,
                UpdatedAt = SYSUTCDATETIME()
            WHERE UserId = @UserId;
            """;

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = passwordHash;
        command.Parameters.Add("@PasswordSalt", SqlDbType.NVarChar, 128).Value = passwordSalt ?? string.Empty;

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        const string sql = """
            SELECT
                u.UserId,
                u.Username,
                u.PasswordHash,
                u.FullName,
                u.RoleId,
                r.RoleName,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt,
                u.PasswordSalt,
                u.FailedLoginAttempts,
                u.LockoutUntil
            FROM dbo.Users u
            INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
            ORDER BY u.UserId ASC;
            """;

        var list = new List<User>();

        await using SqlConnection connection =
            _connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new User
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.GetString(3),
                RoleId = reader.GetInt32(4),
                RoleName = reader.GetString(5),
                IsActive = reader.GetBoolean(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                PasswordSalt = reader.IsDBNull(9) ? null : reader.GetString(9),
                FailedLoginAttempts = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                LockoutUntil = reader.IsDBNull(11) ? null : reader.GetDateTime(11)
            });
        }

            return list;
        }

        public async Task UpdateUserDetailsAsync(int userId, string fullName, int roleId)
        {
            const string sql = """
                UPDATE dbo.Users
                SET FullName = @FullName,
                    RoleId = @RoleId,
                    UpdatedAt = SYSUTCDATETIME()
                WHERE UserId = @UserId;
                """;

            await using SqlConnection connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            await using SqlCommand command =
                new SqlCommand(sql, connection);

            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName.Trim();
            command.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;

            await command.ExecuteNonQueryAsync();
        }
    }