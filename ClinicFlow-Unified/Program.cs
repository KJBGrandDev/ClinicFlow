using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Forms;
using ClinicFlow_Unified.Repositories;
using Microsoft.Data.SqlClient;

namespace ClinicFlow_Unified;

internal static class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test-auth")
        {
            int exitCode = await ClinicFlow_Unified.Tests.AuthSecurityTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-rbac")
        {
            int exitCode = await ClinicFlow_Unified.Tests.RbacSecurityTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-concurrency")
        {
            int exitCode = await ClinicFlow_Unified.Tests.ConcurrencySecurityTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-errors")
        {
            int exitCode = await ClinicFlow_Unified.Tests.ErrorLoggingTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-config")
        {
            int exitCode = await ClinicFlow_Unified.Tests.ConfigurationTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-users-audit")
        {
            int exitCode = await ClinicFlow_Unified.Tests.UserManagementAndAuditTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-dashboard")
        {
            int exitCode = await ClinicFlow_Unified.Tests.DashboardShellTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-reports")
        {
            int exitCode = await ClinicFlow_Unified.Tests.ReportsEngineTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-backup")
        {
            int exitCode = await ClinicFlow_Unified.Tests.DatabaseBackupRestoreTestRunner.RunAllTestsAsync();
            Environment.Exit(exitCode);
            return;
        }

        if (args.Length > 0 && args[0] == "--test-all")
        {
            int authExit = await ClinicFlow_Unified.Tests.AuthSecurityTestRunner.RunAllTestsAsync();
            int rbacExit = await ClinicFlow_Unified.Tests.RbacSecurityTestRunner.RunAllTestsAsync();
            int concExit = await ClinicFlow_Unified.Tests.ConcurrencySecurityTestRunner.RunAllTestsAsync();
            int errExit = await ClinicFlow_Unified.Tests.ErrorLoggingTestRunner.RunAllTestsAsync();
            int cfgExit = await ClinicFlow_Unified.Tests.ConfigurationTestRunner.RunAllTestsAsync();
            int usrExit = await ClinicFlow_Unified.Tests.UserManagementAndAuditTestRunner.RunAllTestsAsync();
            int dshExit = await ClinicFlow_Unified.Tests.DashboardShellTestRunner.RunAllTestsAsync();
            int repExit = await ClinicFlow_Unified.Tests.ReportsEngineTestRunner.RunAllTestsAsync();
            int bakExit = await ClinicFlow_Unified.Tests.DatabaseBackupRestoreTestRunner.RunAllTestsAsync();
            Environment.Exit(authExit == 0 && rbacExit == 0 && concExit == 0 && errExit == 0 && cfgExit == 0 && usrExit == 0 && dshExit == 0 && repExit == 0 && bakExit == 0 ? 0 : 1);
            return;
        }

        ApplicationConfiguration.Initialize();

        bool usersExist;

        try
        {
            SqlConnectionFactory connectionFactory = new();

            await using SqlConnection connection =
                connectionFactory.CreateConnection();

            await connection.OpenAsync();

            UserRepository userRepository =
                new UserRepository(connectionFactory);

            usersExist = await userRepository.AnyUsersExistAsync();
        }
        catch (SqlException)
        {
            MessageBox.Show(
                "ClinicFlow could not connect to the SQL Server database. " +
                "Please confirm that SQL Server is running and that the connection settings are correct.",
                "Database Connection Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }
        catch (InvalidOperationException)
        {
            MessageBox.Show(
                "ClinicFlow could not read its database configuration. " +
                "Please check appsettings.json.",
                "Configuration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        Application.Run(
            usersExist
                ? new LoginForm()
                : new InitialSetupForm());
    }
}