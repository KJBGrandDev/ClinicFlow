#nullable enable

namespace ClinicFlow_Unified.Data;

public static class AppConfiguration
{
    public static string ConnectionString => DatabaseConnection.ConnectionString;
}