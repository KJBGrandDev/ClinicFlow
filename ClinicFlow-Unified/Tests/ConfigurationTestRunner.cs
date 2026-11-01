#nullable enable

using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ClinicFlow_Unified.Data;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified.Tests;

public static class ConfigurationTestRunner
{
    public static async Task<int> RunAllTestsAsync()
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("CLINICFLOW CONFIGURATION STANDARDIZATION TEST RUNNER");
        Console.WriteLine("===============================================================");

        int passed = 0;
        int failed = 0;

        void Report(string testName, bool success, string detail = "")
        {
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASS] {testName}");
                Console.ResetColor();
                passed++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName} - {detail}");
                Console.ResetColor();
                failed++;
            }
        }

        // Test 1: App.config or AppName.dll.config exists in runtime base directory
        string baseDir = AppContext.BaseDirectory;
        string appConfigPath = Path.Combine(baseDir, "App.config");
        string exeConfigPath = Path.Combine(baseDir, $"{AppDomain.CurrentDomain.FriendlyName}.dll.config");
        string altExeConfigPath = Path.Combine(baseDir, $"{AppDomain.CurrentDomain.FriendlyName}.config");

        bool configFileExists = File.Exists(appConfigPath) || File.Exists(exeConfigPath) || File.Exists(altExeConfigPath);
        Report("Test 1: App.config exists in output directory", configFileExists,
            $"Looked for: {appConfigPath}");

        // Test 2: ConfigurationManager resolves ConnectionStrings section
        string? configManagerCs = ConfigurationManager.ConnectionStrings["ClinicFlowDatabase"]?.ConnectionString;
        bool csResolvedFromConfig = !string.IsNullOrWhiteSpace(configManagerCs);
        Report("Test 2: ConfigurationManager resolves 'ClinicFlowDatabase' from App.config", csResolvedFromConfig,
            $"Found: {configManagerCs ?? "(null)"}");

        // Test 3: DatabaseConnection reports configuration source as App.config
        string csSource = DatabaseConnection.ConfigurationSource;
        bool sourceIsAppConfig = csSource.Equals("App.config", StringComparison.OrdinalIgnoreCase);
        Report("Test 3: DatabaseConnection prioritizes App.config over appsettings.json", sourceIsAppConfig,
            $"Active Source: {csSource}");

        // Test 4: AppConfiguration delegates to DatabaseConnection seamlessly
        string appConfigCs = AppConfiguration.ConnectionString;
        string dbConnCs = DatabaseConnection.ConnectionString;
        bool connectionStringsMatch = !string.IsNullOrWhiteSpace(appConfigCs) && appConfigCs == dbConnCs;
        Report("Test 4: AppConfiguration delegates to DatabaseConnection seamlessly", connectionStringsMatch,
            $"appConfigCs == dbConnCs: {connectionStringsMatch}");

        // Test 5: Live database connection test succeeds using resolved connection string
        bool canConnect = false;
        string connectError = string.Empty;
        try
        {
            await using var conn = DatabaseConnection.CreateConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("SELECT 1;", conn);
            var res = await cmd.ExecuteScalarAsync();
            canConnect = Convert.ToInt32(res) == 1;
        }
        catch (Exception ex)
        {
            connectError = ex.Message;
        }
        Report("Test 5: Live database connection succeeds using resolved App.config connection", canConnect, connectError);

        Console.WriteLine();
        Console.WriteLine($"CONFIGURATION TESTS COMPLETE: {passed} Passed, {failed} Failed.");
        Console.WriteLine("===============================================================");

        return failed == 0 ? 0 : 1;
    }
}
