using ClinicFlow_Unified.Utilities;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using Microsoft.Data.SqlClient;
namespace ClinicFlow_Unified.Forms;

public partial class InitialSetupForm : Form
{
    private readonly UserManagementService _userManagementService;

    public InitialSetupForm()
    {
        InitializeComponent();
            ClinicTheme.ApplyForm(this);

        SqlConnectionFactory connectionFactory = new();

        AuditLogRepository auditLogRepository =
        new AuditLogRepository(connectionFactory);

        AuditService auditService =
        new AuditService(auditLogRepository);

        _userManagementService = new UserManagementService(
            new UserRepository(connectionFactory),
            new RoleRepository(connectionFactory),
            auditService);
    }

    private async void btnCreateAdmin_Click(
        object? sender,
        EventArgs e)
    {
        btnCreateAdmin.Enabled = false;

        try
        {
            await _userManagementService.CreateInitialAdminAsync(
                txtUsername.Text,
                txtPassword.Text,
                txtFullName.Text);

            MessageBox.Show(
                "The initial Admin account was created successfully.",
                "ClinicFlow Setup Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Close();
        }
        catch (ArgumentException exception)
        {
            MessageBox.Show(
                exception.Message,
                "Check the Entered Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(
                exception.Message,
                "Initial Setup Cannot Continue",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (SqlException)
        {
            MessageBox.Show(
                "ClinicFlow could not save the Admin account. " +
                "Please check the database connection and try again.",
                "Database Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnCreateAdmin.Enabled = true;
        }
    }
}