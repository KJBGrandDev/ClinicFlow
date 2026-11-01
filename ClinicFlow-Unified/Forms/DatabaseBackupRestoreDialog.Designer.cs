namespace ClinicFlow_Unified.Forms;

partial class DatabaseBackupRestoreDialog
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Panel _headerPanel;
    private System.Windows.Forms.Label _lblHeaderTitle;
    private System.Windows.Forms.Label _lblHeaderSubtitle;

    private System.Windows.Forms.Panel _backupCard;
    private System.Windows.Forms.Label _lblBackupSectionTitle;
    private System.Windows.Forms.Label _lblBackupSectionSubtitle;
    private System.Windows.Forms.Label _lblBackupPath;
    private System.Windows.Forms.TextBox _txtBackupPath;
    private System.Windows.Forms.Button _btnBrowseBackup;
    private System.Windows.Forms.Button _btnExecuteBackup;
    private System.Windows.Forms.Label _lblBackupStatus;

    private System.Windows.Forms.Panel _restoreCard;
    private System.Windows.Forms.Label _lblRestoreSectionTitle;
    private System.Windows.Forms.Panel _warningBox;
    private System.Windows.Forms.Label _lblWarningText;
    private System.Windows.Forms.Label _lblRestorePath;
    private System.Windows.Forms.TextBox _txtRestorePath;
    private System.Windows.Forms.Button _btnBrowseRestore;
    private System.Windows.Forms.Button _btnVerifyRestore;
    private System.Windows.Forms.Button _btnExecuteRestore;
    private System.Windows.Forms.Label _lblRestoreStatus;

    private System.Windows.Forms.Panel _footerPanel;
    private System.Windows.Forms.Button _btnClose;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this._headerPanel = new System.Windows.Forms.Panel();
        this._lblHeaderTitle = new System.Windows.Forms.Label();
        this._lblHeaderSubtitle = new System.Windows.Forms.Label();

        this._backupCard = new System.Windows.Forms.Panel();
        this._lblBackupSectionTitle = new System.Windows.Forms.Label();
        this._lblBackupSectionSubtitle = new System.Windows.Forms.Label();
        this._lblBackupPath = new System.Windows.Forms.Label();
        this._txtBackupPath = new System.Windows.Forms.TextBox();
        this._btnBrowseBackup = new System.Windows.Forms.Button();
        this._btnExecuteBackup = new System.Windows.Forms.Button();
        this._lblBackupStatus = new System.Windows.Forms.Label();

        this._restoreCard = new System.Windows.Forms.Panel();
        this._lblRestoreSectionTitle = new System.Windows.Forms.Label();
        this._warningBox = new System.Windows.Forms.Panel();
        this._lblWarningText = new System.Windows.Forms.Label();
        this._lblRestorePath = new System.Windows.Forms.Label();
        this._txtRestorePath = new System.Windows.Forms.TextBox();
        this._btnBrowseRestore = new System.Windows.Forms.Button();
        this._btnVerifyRestore = new System.Windows.Forms.Button();
        this._btnExecuteRestore = new System.Windows.Forms.Button();
        this._lblRestoreStatus = new System.Windows.Forms.Label();

        this._footerPanel = new System.Windows.Forms.Panel();
        this._btnClose = new System.Windows.Forms.Button();

        this.SuspendLayout();

        // _headerPanel
        this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this._headerPanel.Height = 70;
        this._headerPanel.BackColor = System.Drawing.Color.White;
        this._headerPanel.Padding = new System.Windows.Forms.Padding(24, 16, 24, 8);

        this._lblHeaderTitle.Text = "Database Maintenance — Backup & Restore";
        this._lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        this._lblHeaderTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        this._lblHeaderTitle.Dock = System.Windows.Forms.DockStyle.Top;

        this._lblHeaderSubtitle.Text = "Administrative utility to back up ClinicFlowUnifiedDb to disk and restore from an existing .bak file";
        this._lblHeaderSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular);
        this._lblHeaderSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        this._lblHeaderSubtitle.Dock = System.Windows.Forms.DockStyle.Bottom;

        this._headerPanel.Controls.Add(this._lblHeaderTitle);
        this._headerPanel.Controls.Add(this._lblHeaderSubtitle);

        // _backupCard
        this._backupCard.Dock = System.Windows.Forms.DockStyle.Top;
        this._backupCard.Height = 160;
        this._backupCard.BackColor = System.Drawing.Color.White;
        this._backupCard.Padding = new System.Windows.Forms.Padding(24, 12, 24, 12);
        this._backupCard.Margin = new System.Windows.Forms.Padding(24, 12, 24, 12);
        this._backupCard.Paint += (s, e) => {
            e.Graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Color.FromArgb(226, 232, 240)), 24, this._backupCard.Height - 1, this._backupCard.Width - 24, this._backupCard.Height - 1);
        };

        this._lblBackupSectionTitle.Text = "💾 Full Database Backup (.BAK)";
        this._lblBackupSectionTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        this._lblBackupSectionTitle.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
        this._lblBackupSectionTitle.Location = new System.Drawing.Point(24, 12);
        this._lblBackupSectionTitle.Size = new System.Drawing.Size(350, 24);

        this._lblBackupSectionSubtitle.Text = "Creates an immediate point-in-time full snapshot of all 24 normalized tables and security logs.";
        this._lblBackupSectionSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this._lblBackupSectionSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        this._lblBackupSectionSubtitle.Location = new System.Drawing.Point(24, 38);
        this._lblBackupSectionSubtitle.Size = new System.Drawing.Size(620, 20);

        this._lblBackupPath.Text = "Destination File Path:";
        this._lblBackupPath.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this._lblBackupPath.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblBackupPath.Location = new System.Drawing.Point(24, 66);
        this._lblBackupPath.Size = new System.Drawing.Size(200, 18);

        this._txtBackupPath.Location = new System.Drawing.Point(24, 86);
        this._txtBackupPath.Size = new System.Drawing.Size(460, 27);
        this._txtBackupPath.Font = new System.Drawing.Font("Segoe UI", 9F);

        this._btnBrowseBackup.Text = "📂 Browse...";
        this._btnBrowseBackup.Location = new System.Drawing.Point(492, 85);
        this._btnBrowseBackup.Size = new System.Drawing.Size(90, 29);
        this._btnBrowseBackup.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this._btnBrowseBackup.Cursor = System.Windows.Forms.Cursors.Hand;

        this._btnExecuteBackup.Text = "⚡ Create Backup";
        this._btnExecuteBackup.Location = new System.Drawing.Point(590, 85);
        this._btnExecuteBackup.Size = new System.Drawing.Size(120, 29);
        this._btnExecuteBackup.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this._btnExecuteBackup.BackColor = System.Drawing.Color.FromArgb(16, 185, 129); // Emerald
        this._btnExecuteBackup.ForeColor = System.Drawing.Color.White;
        this._btnExecuteBackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnExecuteBackup.FlatAppearance.BorderSize = 0;
        this._btnExecuteBackup.Cursor = System.Windows.Forms.Cursors.Hand;

        this._lblBackupStatus.Text = "Ready to back up.";
        this._lblBackupStatus.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
        this._lblBackupStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        this._lblBackupStatus.Location = new System.Drawing.Point(24, 122);
        this._lblBackupStatus.Size = new System.Drawing.Size(680, 20);

        this._backupCard.Controls.Add(this._lblBackupSectionTitle);
        this._backupCard.Controls.Add(this._lblBackupSectionSubtitle);
        this._backupCard.Controls.Add(this._lblBackupPath);
        this._backupCard.Controls.Add(this._txtBackupPath);
        this._backupCard.Controls.Add(this._btnBrowseBackup);
        this._backupCard.Controls.Add(this._btnExecuteBackup);
        this._backupCard.Controls.Add(this._lblBackupStatus);

        // _restoreCard
        this._restoreCard.Dock = System.Windows.Forms.DockStyle.Fill;
        this._restoreCard.BackColor = System.Drawing.Color.White;
        this._restoreCard.Padding = new System.Windows.Forms.Padding(24, 12, 24, 12);

        this._lblRestoreSectionTitle.Text = "♻️ Restore Database from Backup";
        this._lblRestoreSectionTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        this._lblRestoreSectionTitle.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
        this._lblRestoreSectionTitle.Location = new System.Drawing.Point(24, 12);
        this._lblRestoreSectionTitle.Size = new System.Drawing.Size(350, 24);

        // _warningBox
        this._warningBox.BackColor = System.Drawing.Color.FromArgb(254, 242, 242); // Red 50
        this._warningBox.Location = new System.Drawing.Point(24, 38);
        this._warningBox.Size = new System.Drawing.Size(686, 44);
        this._warningBox.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
        this._warningBox.Paint += (s, e) => {
            e.Graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.FromArgb(254, 202, 202)), 0, 0, this._warningBox.Width - 1, this._warningBox.Height - 1);
        };

        this._lblWarningText.Text = "⚠️ CRITICAL NOTICE: Restoring will disconnect active users and overwrite all existing patient records, visits, and payments with the data in the backup file.";
        this._lblWarningText.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
        this._lblWarningText.ForeColor = System.Drawing.Color.FromArgb(185, 28, 28); // Red 700
        this._lblWarningText.Dock = System.Windows.Forms.DockStyle.Fill;

        this._warningBox.Controls.Add(this._lblWarningText);

        this._lblRestorePath.Text = "Source Backup File (.BAK):";
        this._lblRestorePath.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this._lblRestorePath.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblRestorePath.Location = new System.Drawing.Point(24, 94);
        this._lblRestorePath.Size = new System.Drawing.Size(200, 18);

        this._txtRestorePath.Location = new System.Drawing.Point(24, 114);
        this._txtRestorePath.Size = new System.Drawing.Size(460, 27);
        this._txtRestorePath.Font = new System.Drawing.Font("Segoe UI", 9F);

        this._btnBrowseRestore.Text = "📂 Select...";
        this._btnBrowseRestore.Location = new System.Drawing.Point(492, 113);
        this._btnBrowseRestore.Size = new System.Drawing.Size(90, 29);
        this._btnBrowseRestore.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this._btnBrowseRestore.Cursor = System.Windows.Forms.Cursors.Hand;

        this._btnVerifyRestore.Text = "🔍 Verify File";
        this._btnVerifyRestore.Location = new System.Drawing.Point(492, 150);
        this._btnVerifyRestore.Size = new System.Drawing.Size(90, 29);
        this._btnVerifyRestore.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this._btnVerifyRestore.Cursor = System.Windows.Forms.Cursors.Hand;

        this._btnExecuteRestore.Text = "⚠️ Restore Database";
        this._btnExecuteRestore.Location = new System.Drawing.Point(590, 113);
        this._btnExecuteRestore.Size = new System.Drawing.Size(120, 29);
        this._btnExecuteRestore.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this._btnExecuteRestore.BackColor = System.Drawing.Color.FromArgb(220, 38, 38); // Red 600
        this._btnExecuteRestore.ForeColor = System.Drawing.Color.White;
        this._btnExecuteRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnExecuteRestore.FlatAppearance.BorderSize = 0;
        this._btnExecuteRestore.Cursor = System.Windows.Forms.Cursors.Hand;

        this._lblRestoreStatus.Text = "Select a valid SQL Server backup file (.bak) to restore.";
        this._lblRestoreStatus.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
        this._lblRestoreStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        this._lblRestoreStatus.Location = new System.Drawing.Point(24, 154);
        this._lblRestoreStatus.Size = new System.Drawing.Size(460, 20);

        this._restoreCard.Controls.Add(this._lblRestoreSectionTitle);
        this._restoreCard.Controls.Add(this._warningBox);
        this._restoreCard.Controls.Add(this._lblRestorePath);
        this._restoreCard.Controls.Add(this._txtRestorePath);
        this._restoreCard.Controls.Add(this._btnBrowseRestore);
        this._restoreCard.Controls.Add(this._btnVerifyRestore);
        this._restoreCard.Controls.Add(this._btnExecuteRestore);
        this._restoreCard.Controls.Add(this._lblRestoreStatus);

        // _footerPanel
        this._footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this._footerPanel.Height = 50;
        this._footerPanel.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        this._footerPanel.Padding = new System.Windows.Forms.Padding(24, 10, 24, 10);

        this._btnClose.Text = "Close";
        this._btnClose.Dock = System.Windows.Forms.DockStyle.Right;
        this._btnClose.Width = 90;
        this._btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this._btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
        this._btnClose.Click += (_, _) => this.Close();

        this._footerPanel.Controls.Add(this._btnClose);

        // DatabaseBackupRestoreDialog
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(740, 480);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Database Maintenance — Backup & Restore";
        this.Controls.Add(this._restoreCard);
        this.Controls.Add(this._backupCard);
        this.Controls.Add(this._headerPanel);
        this.Controls.Add(this._footerPanel);
        this.Load += DatabaseBackupRestoreDialog_Load;

        this.ResumeLayout(false);
    }

    #endregion
}
