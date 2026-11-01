#nullable enable

using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class DatabaseBackupRestoreDialog : Form
{
    private readonly DatabaseMaintenanceService _maintenanceService = new();

    public DatabaseBackupRestoreDialog()
    {
        // Strictly Admin only (REQ-022)
        if (LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
        {
            UserSession.RequireRole("Admin");
        }

        InitializeComponent();
        ClinicTheme.ApplyForm(this);
    }

    private void DatabaseBackupRestoreDialog_Load(object? sender, EventArgs e)
    {
        string defaultDir = _maintenanceService.GetDefaultBackupDirectory();
        string defaultName = $"{DatabaseConnection.DatabaseName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        _txtBackupPath.Text = Path.Combine(defaultDir, defaultName);

        _btnBrowseBackup.Click += BtnBrowseBackup_Click;
        _btnExecuteBackup.Click += async (_, _) => await ExecuteBackupAsync();

        _btnBrowseRestore.Click += BtnBrowseRestore_Click;
        _btnVerifyRestore.Click += async (_, _) => await VerifyRestoreFileAsync();
        _btnExecuteRestore.Click += async (_, _) => await ExecuteRestoreAsync();
    }

    private void BtnBrowseBackup_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "SQL Server Backup Files (*.bak)|*.bak|All Files (*.*)|*.*",
            DefaultExt = "bak",
            FileName = Path.GetFileName(_txtBackupPath.Text),
            InitialDirectory = Path.GetDirectoryName(_txtBackupPath.Text) ?? _maintenanceService.GetDefaultBackupDirectory(),
            Title = "Select Destination for Database Backup"
        };

        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            _txtBackupPath.Text = sfd.FileName;
        }
    }

    private async Task ExecuteBackupAsync()
    {
        string path = _txtBackupPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show("Please specify a valid backup destination path.", "Validation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnExecuteBackup.Enabled = false;
        _btnBrowseBackup.Enabled = false;
        Cursor = Cursors.WaitCursor;
        _lblBackupStatus.Text = "Generating backup... Please wait.";
        _lblBackupStatus.ForeColor = Color.FromArgb(37, 99, 235);

        try
        {
            var (success, message, backupFile) = await _maintenanceService.BackupDatabaseAsync(path);
            if (success)
            {
                _lblBackupStatus.Text = $"✅ Backup completed: {Path.GetFileName(backupFile)}";
                _lblBackupStatus.ForeColor = Color.FromArgb(16, 185, 129);
                MessageBox.Show(message, "Backup Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _lblBackupStatus.Text = "❌ Backup failed. See error notice.";
                _lblBackupStatus.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show(message, "Backup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _btnExecuteBackup.Enabled = true;
            _btnBrowseBackup.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private void BtnBrowseRestore_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "SQL Server Backup Files (*.bak)|*.bak|All Files (*.*)|*.*",
            InitialDirectory = _maintenanceService.GetDefaultBackupDirectory(),
            Title = "Select Database Backup File to Restore"
        };

        if (ofd.ShowDialog(this) == DialogResult.OK)
        {
            _txtRestorePath.Text = ofd.FileName;
            _lblRestoreStatus.Text = $"Selected: {Path.GetFileName(ofd.FileName)}";
        }
    }

    private async Task VerifyRestoreFileAsync()
    {
        string path = _txtRestorePath.Text.Trim();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show("Please select an existing SQL Server backup file (.bak) to verify.", "Validation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnVerifyRestore.Enabled = false;
        Cursor = Cursors.WaitCursor;
        _lblRestoreStatus.Text = "Verifying backup headers...";

        try
        {
            var (success, message) = await _maintenanceService.VerifyBackupAsync(path);
            if (success)
            {
                _lblRestoreStatus.Text = "✅ File verified: Header is intact and restorable.";
                _lblRestoreStatus.ForeColor = Color.FromArgb(16, 185, 129);
                MessageBox.Show(message, "Verification Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _lblRestoreStatus.Text = "❌ File verification failed.";
                _lblRestoreStatus.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show(message, "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        finally
        {
            _btnVerifyRestore.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private async Task ExecuteRestoreAsync()
    {
        string path = _txtRestorePath.Text.Trim();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            MessageBox.Show("Please select a valid, existing backup file (.bak) to restore.", "Validation Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Critical Confirmation Dialog (REQ-022)
        var confirmResult = MessageBox.Show(
            "⚠️ CRITICAL DATABASE OVERWRITE WARNING!\n\n" +
            "Restoring the database will terminate all active connections and permanently overwrite " +
            "all existing tables, appointments, consultations, and invoices with the contents of the backup file.\n\n" +
            $"Target Backup File:\n{path}\n\n" +
            "Are you absolutely certain you want to proceed with the database restoration?",
            "Confirm Database Restore",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Stop,
            MessageBoxDefaultButton.Button2);

        if (confirmResult != DialogResult.Yes)
        {
            return;
        }

        _btnExecuteRestore.Enabled = false;
        _btnVerifyRestore.Enabled = false;
        _btnBrowseRestore.Enabled = false;
        Cursor = Cursors.WaitCursor;
        _lblRestoreStatus.Text = "Restoring database... Please do not close ClinicFlow.";
        _lblRestoreStatus.ForeColor = Color.FromArgb(220, 38, 38);

        try
        {
            var (success, message) = await _maintenanceService.RestoreDatabaseAsync(path);
            if (success)
            {
                _lblRestoreStatus.Text = "✅ Database restored successfully.";
                _lblRestoreStatus.ForeColor = Color.FromArgb(16, 185, 129);
                MessageBox.Show(
                    message + "\n\nClinicFlow will now return to the main dashboard.",
                    "Restore Completed Successfully",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                _lblRestoreStatus.Text = "❌ Database restore failed. See error log.";
                _lblRestoreStatus.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show(message, "Restore Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _btnExecuteRestore.Enabled = true;
            _btnVerifyRestore.Enabled = true;
            _btnBrowseRestore.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}
