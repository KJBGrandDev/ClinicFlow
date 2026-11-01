using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Data;

public sealed class SqlConnectionFactory
{
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(AppConfiguration.ConnectionString);
    }
}