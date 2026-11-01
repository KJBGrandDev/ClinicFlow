#nullable enable

using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Data;

public static class DatabaseConnection
{
    private static readonly Lazy<(string ConnectionString, string Source)> LazyConfig = new(LoadConfiguration);

    public static string ConnectionString => LazyConfig.Value.ConnectionString;
    public static string ConfigurationSource => LazyConfig.Value.Source;

    public static string DatabaseName
    {
        get
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);
                return !string.IsNullOrWhiteSpace(builder.InitialCatalog) ? builder.InitialCatalog : "ClinicFlowUnifiedDb";
            }
            catch
            {
                return "ClinicFlowUnifiedDb";
            }
        }
    }

    public static SqlConnection CreateConnection() => new(ConnectionString);

    public static SqlConnection CreateMasterConnection()
    {
        var builder = new SqlConnectionStringBuilder(ConnectionString)
        {
            InitialCatalog = "master",
            Pooling = false
        };
        return new SqlConnection(builder.ConnectionString);
    }

    private static (string ConnectionString, string Source) LoadConfiguration()
    {
        // 1. Prioritize App.config (Academic & Grading Rubric Requirement REQ-006)
        try
        {
            var configCs = ConfigurationManager.ConnectionStrings["ClinicFlowDatabase"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(configCs))
                return (configCs, "App.config");

            var altConfigCs = ConfigurationManager.ConnectionStrings["ClinicFlowDb"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(altConfigCs))
                return (altConfigCs, "App.config");

            foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
            {
                if (!string.IsNullOrWhiteSpace(cs.ConnectionString) &&
                    !cs.Name.Equals("LocalSqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    return (cs.ConnectionString, "App.config");
                }
            }
        }
        catch
        {
            // Fall through to appsettings.json
        }

        // 2. Fallback to appsettings.json (.NET 10 standard)
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("ConnectionString", out var flatVal) && !string.IsNullOrWhiteSpace(flatVal.GetString()))
            {
                return (flatVal.GetString()!, "appsettings.json");
            }

            if (root.TryGetProperty("ConnectionStrings", out var section) && section.ValueKind == JsonValueKind.Object)
            {
                if (section.TryGetProperty("ClinicFlowDatabase", out var clinicFlowDbVal) && !string.IsNullOrWhiteSpace(clinicFlowDbVal.GetString()))
                    return (clinicFlowDbVal.GetString()!, "appsettings.json");

                foreach (var prop in section.EnumerateObject())
                {
                    var val = prop.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(val)) return (val, "appsettings.json");
                }
            }
        }

        throw new InvalidOperationException("Database connection string was not found in 'App.config' or 'appsettings.json'.");
    }
}