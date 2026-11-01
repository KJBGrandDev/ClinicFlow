#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class DoctorManagementForm
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel _headerPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;

        private Panel _leftGridPanel = null!;
        private Panel _rosterHeaderPanel = null!;
        private Label _lblRosterHeader = null!;
        private Button _btnAddNewDoctor = null!;
        private DataGridView _doctorsGrid = null!;

        private Panel _rightPanel = null!;
        private Panel _modeBannerPanel = null!;
        private Label _lblModeTitle = null!;
        private Label _lblModeSubtitle = null!;

        private GroupBox _grpProfile = null!;
        private Label _lblFirstName = null!;
        private TextBox _txtFirstName = null!;
        private Label _lblLastName = null!;
        private TextBox _txtLastName = null!;
        private Label _lblLicense = null!;
        private TextBox _txtLicense = null!;
        private Label _lblSpecialty = null!;
        private ComboBox _cboSpecialty = null!;
        private Label _lblContact = null!;
        private TextBox _txtContact = null!;

        private GroupBox _grpCredentials = null!;
        private Label _lblUsername = null!;
        private TextBox _txtUsername = null!;
        private Label _lblPassword = null!;
        private TextBox _txtPassword = null!;
        private Button _btnResetOrProvision = null!;
        private Label _lblCredentialsNote = null!;

        private Panel _actionPanel = null!;
        private Button _btnPrimaryAction = null!;
        private Button _btnClearOrCancel = null!;
        private Button _btnArchiveDoctor = null!;
        private Button _btnDeleteDoctor = null!;
        private Button _btnTestPortal = null!;

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
            this._headerPanel = new Panel();
            this._lblTitle = new Label();
            this._lblSubtitle = new Label();

            this._leftGridPanel = new Panel();
            this._rosterHeaderPanel = new Panel();
            this._lblRosterHeader = new Label();
            this._btnAddNewDoctor = new Button();
            this._doctorsGrid = new DataGridView();

            this._rightPanel = new Panel();
            this._modeBannerPanel = new Panel();
            this._lblModeTitle = new Label();
            this._lblModeSubtitle = new Label();

            this._grpProfile = new GroupBox();
            this._lblFirstName = new Label();
            this._txtFirstName = new TextBox();
            this._lblLastName = new Label();
            this._txtLastName = new TextBox();
            this._lblLicense = new Label();
            this._txtLicense = new TextBox();
            this._lblSpecialty = new Label();
            this._cboSpecialty = new ComboBox();
            this._lblContact = new Label();
            this._txtContact = new TextBox();

            this._grpCredentials = new GroupBox();
            this._lblUsername = new Label();
            this._txtUsername = new TextBox();
            this._lblPassword = new Label();
            this._txtPassword = new TextBox();
            this._btnResetOrProvision = new Button();
            this._lblCredentialsNote = new Label();

            this._actionPanel = new Panel();
            this._btnPrimaryAction = new Button();
            this._btnClearOrCancel = new Button();
            this._btnArchiveDoctor = new Button();
            this._btnDeleteDoctor = new Button();
            this._btnTestPortal = new Button();

            this._headerPanel.SuspendLayout();
            this._leftGridPanel.SuspendLayout();
            this._rosterHeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._doctorsGrid).BeginInit();
            this._rightPanel.SuspendLayout();
            this._modeBannerPanel.SuspendLayout();
            this._grpProfile.SuspendLayout();
            this._grpCredentials.SuspendLayout();
            this._actionPanel.SuspendLayout();
            this.SuspendLayout();

            // 1. Form Level Settings
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.ClientSize = new Size(1300, 800);
            this.MinimumSize = new Size(1000, 680);
            this.Name = "DoctorManagementForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "ClinicFlow - Attending Doctor Roster & Account Management";

            // 2. Header Panel
            this._headerPanel.Dock = DockStyle.Top;
            this._headerPanel.Height = 72;
            this._headerPanel.BackColor = Color.FromArgb(15, 23, 42);
            this._headerPanel.Padding = new Padding(24, 14, 24, 14);

            this._lblTitle.Text = "Attending Doctor Management & Portal Access";
            this._lblTitle.UseMnemonic = false;
            this._lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            this._lblTitle.ForeColor = Color.White;
            this._lblTitle.Location = new Point(24, 12);
            this._lblTitle.AutoSize = true;

            this._lblSubtitle.Text = "Register medical staff, provision portal credentials, and manage doctor practice access.";
            this._lblSubtitle.Font = new Font("Segoe UI", 9F);
            this._lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblSubtitle.Location = new Point(25, 40);
            this._lblSubtitle.AutoSize = true;

            this._headerPanel.Controls.Add(this._lblSubtitle);
            this._headerPanel.Controls.Add(this._lblTitle);

            // 3. Left Panel (Roster Grid + Header Bar)
            this._leftGridPanel.Dock = DockStyle.Fill;
            this._leftGridPanel.Padding = new Padding(20, 16, 20, 16);

            this._rosterHeaderPanel.Dock = DockStyle.Top;
            this._rosterHeaderPanel.Height = 48;
            this._rosterHeaderPanel.Padding = new Padding(0, 0, 0, 10);

            this._lblRosterHeader.Dock = DockStyle.Left;
            this._lblRosterHeader.Text = "Registered Doctors Roster";
            this._lblRosterHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._lblRosterHeader.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblRosterHeader.TextAlign = ContentAlignment.MiddleLeft;
            this._lblRosterHeader.AutoSize = true;

            this._btnAddNewDoctor.Dock = DockStyle.Right;
            this._btnAddNewDoctor.Width = 180;
            this._btnAddNewDoctor.Text = "➕ Add New Doctor";
            this._btnAddNewDoctor.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this._btnAddNewDoctor.Cursor = Cursors.Hand;

            this._rosterHeaderPanel.Controls.Add(this._btnAddNewDoctor);
            this._rosterHeaderPanel.Controls.Add(this._lblRosterHeader);

            this._doctorsGrid.Dock = DockStyle.Fill;
            this._doctorsGrid.BackgroundColor = Color.White;
            this._doctorsGrid.BorderStyle = BorderStyle.FixedSingle;
            this._doctorsGrid.RowHeadersVisible = false;
            this._doctorsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._doctorsGrid.MultiSelect = false;
            this._doctorsGrid.AllowUserToAddRows = false;
            this._doctorsGrid.AllowUserToDeleteRows = false;
            this._doctorsGrid.ReadOnly = true;
            this._doctorsGrid.RowTemplate.Height = 36;
            this._doctorsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._doctorsGrid.ColumnHeadersHeight = 36;
            this._doctorsGrid.EnableHeadersVisualStyles = false;
            this._doctorsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            this._doctorsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            this._doctorsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._doctorsGrid.Cursor = Cursors.Hand;

            // Add Fill control FIRST, Top control SECOND so Fill does not overlap underneath Top
            this._leftGridPanel.Controls.Add(this._doctorsGrid);
            this._leftGridPanel.Controls.Add(this._rosterHeaderPanel);

            // 4. Right Panel (Form Card - Fixed 460px)
            this._rightPanel.Dock = DockStyle.Right;
            this._rightPanel.Width = 460;
            this._rightPanel.Padding = new Padding(18);
            this._rightPanel.BackColor = Color.FromArgb(248, 250, 252);
            this._rightPanel.BorderStyle = BorderStyle.FixedSingle;
            this._rightPanel.AutoScroll = true;

            // Mode Banner
            this._modeBannerPanel.Dock = DockStyle.Top;
            this._modeBannerPanel.Height = 56;
            this._modeBannerPanel.BackColor = Color.FromArgb(241, 245, 249);
            this._modeBannerPanel.Padding = new Padding(12, 10, 12, 10);

            this._lblModeTitle.Text = "➕ Register New Doctor";
            this._lblModeTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this._lblModeTitle.ForeColor = Color.FromArgb(16, 185, 129);
            this._lblModeTitle.Location = new Point(12, 8);
            this._lblModeTitle.AutoSize = true;

            this._lblModeSubtitle.Text = "Fill in clinical details and login credentials below.";
            this._lblModeSubtitle.Font = new Font("Segoe UI", 8.5F);
            this._lblModeSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblModeSubtitle.Location = new Point(12, 30);
            this._lblModeSubtitle.AutoSize = true;

            this._modeBannerPanel.Controls.Add(this._lblModeSubtitle);
            this._modeBannerPanel.Controls.Add(this._lblModeTitle);

            // Profile Group
            this._grpProfile.Text = "Doctor Clinical Profile";
            this._grpProfile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._grpProfile.ForeColor = Color.FromArgb(30, 41, 59);
            this._grpProfile.Dock = DockStyle.Top;
            this._grpProfile.Height = 275;
            this._grpProfile.Padding = new Padding(14);

            this._lblFirstName.Text = "First Name *";
            this._lblFirstName.Font = new Font("Segoe UI", 8.5F);
            this._lblFirstName.Location = new Point(14, 25);
            this._lblFirstName.AutoSize = true;

            this._txtFirstName.Location = new Point(14, 44);
            this._txtFirstName.Size = new Size(190, 26);
            this._txtFirstName.Font = new Font("Segoe UI", 9F);

            this._lblLastName.Text = "Last Name *";
            this._lblLastName.Font = new Font("Segoe UI", 8.5F);
            this._lblLastName.Location = new Point(214, 25);
            this._lblLastName.AutoSize = true;

            this._txtLastName.Location = new Point(214, 44);
            this._txtLastName.Size = new Size(190, 26);
            this._txtLastName.Font = new Font("Segoe UI", 9F);

            this._lblLicense.Text = "PRC License # *";
            this._lblLicense.Font = new Font("Segoe UI", 8.5F);
            this._lblLicense.Location = new Point(14, 76);
            this._lblLicense.AutoSize = true;

            this._txtLicense.Location = new Point(14, 95);
            this._txtLicense.Size = new Size(390, 26);
            this._txtLicense.Font = new Font("Segoe UI", 9F);

            this._lblSpecialty.Text = "Clinical Specialization *";
            this._lblSpecialty.Font = new Font("Segoe UI", 8.5F);
            this._lblSpecialty.Location = new Point(14, 128);
            this._lblSpecialty.AutoSize = true;

            this._cboSpecialty.Location = new Point(14, 147);
            this._cboSpecialty.Size = new Size(390, 26);
            this._cboSpecialty.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cboSpecialty.Font = new Font("Segoe UI", 9F);
            this._cboSpecialty.Items.AddRange(new object[] {
                "Family Medicine", "General Practice", "Internal Medicine", "Pediatrics", "Obstetrics & Gynecology", "General Surgery", "Dermatology", "ENT / Otorhinolaryngology"
            });
            this._cboSpecialty.SelectedIndex = 0;

            this._lblContact.Text = "Contact / Phone Number";
            this._lblContact.Font = new Font("Segoe UI", 8.5F);
            this._lblContact.Location = new Point(14, 180);
            this._lblContact.AutoSize = true;

            this._txtContact.Location = new Point(14, 199);
            this._txtContact.Size = new Size(390, 26);
            this._txtContact.Font = new Font("Segoe UI", 9F);

            this._grpProfile.Controls.Add(this._txtContact);
            this._grpProfile.Controls.Add(this._lblContact);
            this._grpProfile.Controls.Add(this._cboSpecialty);
            this._grpProfile.Controls.Add(this._lblSpecialty);
            this._grpProfile.Controls.Add(this._txtLicense);
            this._grpProfile.Controls.Add(this._lblLicense);
            this._grpProfile.Controls.Add(this._txtLastName);
            this._grpProfile.Controls.Add(this._lblLastName);
            this._grpProfile.Controls.Add(this._txtFirstName);
            this._grpProfile.Controls.Add(this._lblFirstName);

            // Credentials Group
            this._grpCredentials.Text = "Doctor Portal Login Account";
            this._grpCredentials.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._grpCredentials.ForeColor = Color.FromArgb(30, 41, 59);
            this._grpCredentials.Dock = DockStyle.Top;
            this._grpCredentials.Height = 185;
            this._grpCredentials.Padding = new Padding(14);

            this._lblUsername.Text = "Username (Login ID) *";
            this._lblUsername.Font = new Font("Segoe UI", 8.5F);
            this._lblUsername.Location = new Point(14, 25);
            this._lblUsername.AutoSize = true;

            this._txtUsername.Location = new Point(14, 44);
            this._txtUsername.Size = new Size(390, 26);
            this._txtUsername.Font = new Font("Segoe UI", 9F);

            this._lblPassword.Text = "Password *";
            this._lblPassword.Font = new Font("Segoe UI", 8.5F);
            this._lblPassword.Location = new Point(14, 77);
            this._lblPassword.AutoSize = true;

            this._txtPassword.Location = new Point(14, 96);
            this._txtPassword.Size = new Size(235, 26);
            this._txtPassword.Font = new Font("Segoe UI", 9F);
            this._txtPassword.PasswordChar = '•';

            this._btnResetOrProvision.Location = new Point(258, 95);
            this._btnResetOrProvision.Size = new Size(146, 28);
            this._btnResetOrProvision.Text = "🔑 Reset Pwd";
            this._btnResetOrProvision.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._btnResetOrProvision.Cursor = Cursors.Hand;

            this._lblCredentialsNote.Text = "Auto-assigned Doctor role for direct access to Clinical Portal.";
            this._lblCredentialsNote.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            this._lblCredentialsNote.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblCredentialsNote.Location = new Point(14, 130);
            this._lblCredentialsNote.Size = new Size(390, 42);

            this._grpCredentials.Controls.Add(this._lblCredentialsNote);
            this._grpCredentials.Controls.Add(this._btnResetOrProvision);
            this._grpCredentials.Controls.Add(this._txtPassword);
            this._grpCredentials.Controls.Add(this._lblPassword);
            this._grpCredentials.Controls.Add(this._txtUsername);
            this._grpCredentials.Controls.Add(this._lblUsername);

            // Action Panel - Unified 3-Row Architecture with Fitts's Law Sizes
            this._actionPanel.Dock = DockStyle.Top;
            this._actionPanel.Height = 160;
            this._actionPanel.Padding = new Padding(0, 10, 0, 0);

            // Row 1: Primary Action & Cancel
            this._btnPrimaryAction.Location = new Point(0, 10);
            this._btnPrimaryAction.Size = new Size(275, 42);
            this._btnPrimaryAction.Text = "✓ Register Doctor & Login";
            this._btnPrimaryAction.UseMnemonic = false;
            this._btnPrimaryAction.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this._btnPrimaryAction.Cursor = Cursors.Hand;

            this._btnClearOrCancel.Location = new Point(285, 10);
            this._btnClearOrCancel.Size = new Size(135, 42);
            this._btnClearOrCancel.Text = "Clear Fields";
            this._btnClearOrCancel.UseMnemonic = false;
            this._btnClearOrCancel.Font = new Font("Segoe UI", 9F);
            this._btnClearOrCancel.Cursor = Cursors.Hand;

            // Row 2: Archive & Delete
            this._btnArchiveDoctor.Location = new Point(0, 60);
            this._btnArchiveDoctor.Size = new Size(205, 38);
            this._btnArchiveDoctor.Text = "🚫 Archive Doctor";
            this._btnArchiveDoctor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnArchiveDoctor.Cursor = Cursors.Hand;
            this._btnArchiveDoctor.Visible = false;

            this._btnDeleteDoctor.Location = new Point(215, 60);
            this._btnDeleteDoctor.Size = new Size(205, 38);
            this._btnDeleteDoctor.Text = "🗑 Delete Record";
            this._btnDeleteDoctor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnDeleteDoctor.Cursor = Cursors.Hand;
            this._btnDeleteDoctor.Visible = false;

            // Row 3: Test Portal
            this._btnTestPortal.Location = new Point(0, 108);
            this._btnTestPortal.Size = new Size(420, 38);
            this._btnTestPortal.Text = "🩺 Test Doctor Portal As Selected Doctor";
            this._btnTestPortal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this._btnTestPortal.Cursor = Cursors.Hand;
            this._btnTestPortal.Visible = false;

            this._actionPanel.Controls.Add(this._btnPrimaryAction);
            this._actionPanel.Controls.Add(this._btnClearOrCancel);
            this._actionPanel.Controls.Add(this._btnArchiveDoctor);
            this._actionPanel.Controls.Add(this._btnDeleteDoctor);
            this._actionPanel.Controls.Add(this._btnTestPortal);

            this._rightPanel.Controls.Add(this._actionPanel);
            this._rightPanel.Controls.Add(this._grpCredentials);
            this._rightPanel.Controls.Add(this._grpProfile);
            this._rightPanel.Controls.Add(this._modeBannerPanel);

            this.Controls.Add(this._leftGridPanel);
            this.Controls.Add(this._rightPanel);
            this.Controls.Add(this._headerPanel);

            this._headerPanel.ResumeLayout(false);
            this._headerPanel.PerformLayout();
            this._rosterHeaderPanel.ResumeLayout(false);
            this._rosterHeaderPanel.PerformLayout();
            this._leftGridPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this._doctorsGrid).EndInit();
            this._modeBannerPanel.ResumeLayout(false);
            this._modeBannerPanel.PerformLayout();
            this._grpProfile.ResumeLayout(false);
            this._grpProfile.PerformLayout();
            this._grpCredentials.ResumeLayout(false);
            this._grpCredentials.PerformLayout();
            this._actionPanel.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}