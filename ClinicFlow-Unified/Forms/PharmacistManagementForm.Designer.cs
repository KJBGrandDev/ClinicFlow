#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class PharmacistManagementForm
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel _headerPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;

        private Panel _leftGridPanel = null!;
        private Panel _rosterHeaderPanel = null!;
        private Label _lblRosterHeader = null!;
        private Button _btnAddNew = null!;
        private DataGridView _gridPharmacists = null!;

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
        private TextBox _txtLicenseNumber = null!;
        private Label _lblContact = null!;
        private TextBox _txtContactNumber = null!;
        private Label _lblEmail = null!;
        private TextBox _txtEmailAddress = null!;

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
        private Button _btnArchivePharmacist = null!;
        private Button _btnDeletePharmacist = null!;
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
            this._btnAddNew = new Button();
            this._gridPharmacists = new DataGridView();

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
            this._txtLicenseNumber = new TextBox();
            this._lblContact = new Label();
            this._txtContactNumber = new TextBox();
            this._lblEmail = new Label();
            this._txtEmailAddress = new TextBox();

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
            this._btnArchivePharmacist = new Button();
            this._btnDeletePharmacist = new Button();
            this._btnTestPortal = new Button();

            this._headerPanel.SuspendLayout();
            this._leftGridPanel.SuspendLayout();
            this._rosterHeaderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._gridPharmacists).BeginInit();
            this._rightPanel.SuspendLayout();
            this._modeBannerPanel.SuspendLayout();
            this._grpProfile.SuspendLayout();
            this._grpCredentials.SuspendLayout();
            this._actionPanel.SuspendLayout();
            this.SuspendLayout();

            // 1. Form Level Settings
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1300, 800);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "PharmacistManagementForm";
            this.Text = "Pharmacist Staff Management";

            // 2. Header Panel
            this._headerPanel.Dock = DockStyle.Top;
            this._headerPanel.Height = 68;
            this._headerPanel.Padding = new Padding(24, 12, 24, 12);
            this._headerPanel.Controls.Add(this._lblTitle);
            this._headerPanel.Controls.Add(this._lblSubtitle);

            this._lblTitle.Text = "Pharmacist Staff Management";
            this._lblTitle.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            this._lblTitle.Location = new Point(24, 10);
            this._lblTitle.AutoSize = true;

            this._lblSubtitle.Text = "Configure licensed clinic pharmacists, professional credentials, and dispensary portal access.";
            this._lblSubtitle.Font = new Font("Segoe UI", 9F);
            this._lblSubtitle.Location = new Point(25, 38);
            this._lblSubtitle.AutoSize = true;

            // 3. Left Panel (Grid & Top Bar)
            this._leftGridPanel.Dock = DockStyle.Fill;
            this._leftGridPanel.Padding = new Padding(20, 14, 12, 16);

            this._rosterHeaderPanel.Dock = DockStyle.Top;
            this._rosterHeaderPanel.Height = 48;
            this._rosterHeaderPanel.Controls.Add(this._lblRosterHeader);
            this._rosterHeaderPanel.Controls.Add(this._btnAddNew);

            this._lblRosterHeader.Text = "Registered Pharmacists Roster";
            this._lblRosterHeader.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._lblRosterHeader.Location = new Point(4, 10);
            this._lblRosterHeader.AutoSize = true;

            this._btnAddNew.Text = "+ Add New Pharmacist";
            this._btnAddNew.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnAddNew.Size = new Size(185, 34);
            this._btnAddNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._btnAddNew.Location = new Point(this._rosterHeaderPanel.Width - 190, 7);

            this._gridPharmacists.Dock = DockStyle.Fill;
            this._gridPharmacists.AllowUserToAddRows = false;
            this._gridPharmacists.AllowUserToDeleteRows = false;
            this._gridPharmacists.AllowUserToResizeRows = false;
            this._gridPharmacists.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._gridPharmacists.MultiSelect = false;
            this._gridPharmacists.ReadOnly = true;
            this._gridPharmacists.RowHeadersVisible = false;
            this._gridPharmacists.BackgroundColor = Color.White;
            this._gridPharmacists.BorderStyle = BorderStyle.FixedSingle;

            this._leftGridPanel.Controls.Add(this._gridPharmacists);
            this._leftGridPanel.Controls.Add(this._rosterHeaderPanel);

            // 4. Right Panel (Form Card)
            this._rightPanel.Dock = DockStyle.Right;
            this._rightPanel.Width = 430;
            this._rightPanel.Padding = new Padding(12, 14, 20, 16);
            this._rightPanel.BackColor = Color.White;

            // Mode Banner
            this._modeBannerPanel.Dock = DockStyle.Top;
            this._modeBannerPanel.Height = 56;
            this._modeBannerPanel.Padding = new Padding(12, 8, 12, 8);
            this._modeBannerPanel.Controls.Add(this._lblModeTitle);
            this._modeBannerPanel.Controls.Add(this._lblModeSubtitle);

            this._lblModeTitle.Text = "➕ Register New Pharmacist";
            this._lblModeTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this._lblModeTitle.Location = new Point(10, 6);
            this._lblModeTitle.AutoSize = true;

            this._lblModeSubtitle.Text = "Fill in information to register a licensed pharmacist and create portal credentials.";
            this._lblModeSubtitle.Font = new Font("Segoe UI", 8.25F);
            this._lblModeSubtitle.Location = new Point(11, 28);
            this._lblModeSubtitle.AutoSize = true;

            // Profile GroupBox
            this._grpProfile.Dock = DockStyle.Top;
            this._grpProfile.Height = 290;
            this._grpProfile.Text = "Pharmacist Staff Profile";
            this._grpProfile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._grpProfile.Padding = new Padding(14);

            this._lblFirstName.Text = "First Name *";
            this._lblFirstName.Font = new Font("Segoe UI", 8.5F);
            this._lblFirstName.Location = new Point(14, 26);
            this._lblFirstName.AutoSize = true;

            this._txtFirstName.Font = new Font("Segoe UI", 9.5F);
            this._txtFirstName.Location = new Point(16, 46);
            this._txtFirstName.Size = new Size(175, 26);

            this._lblLastName.Text = "Last Name *";
            this._lblLastName.Font = new Font("Segoe UI", 8.5F);
            this._lblLastName.Location = new Point(204, 26);
            this._lblLastName.AutoSize = true;

            this._txtLastName.Font = new Font("Segoe UI", 9.5F);
            this._txtLastName.Location = new Point(206, 46);
            this._txtLastName.Size = new Size(175, 26);

            this._lblLicense.Text = "PRC Professional License # *";
            this._lblLicense.Font = new Font("Segoe UI", 8.5F);
            this._lblLicense.Location = new Point(14, 82);
            this._lblLicense.AutoSize = true;

            this._txtLicenseNumber.Font = new Font("Segoe UI", 9.5F);
            this._txtLicenseNumber.Location = new Point(16, 102);
            this._txtLicenseNumber.Size = new Size(365, 26);

            this._lblContact.Text = "Contact / Phone Number * (09XXXXXXXXX)";
            this._lblContact.Font = new Font("Segoe UI", 8.5F);
            this._lblContact.Location = new Point(14, 138);
            this._lblContact.AutoSize = true;

            this._txtContactNumber.Font = new Font("Segoe UI", 9.5F);
            this._txtContactNumber.Location = new Point(16, 158);
            this._txtContactNumber.Size = new Size(365, 26);

            this._lblEmail.Text = "Email Address (Optional)";
            this._lblEmail.Font = new Font("Segoe UI", 8.5F);
            this._lblEmail.Location = new Point(14, 196);
            this._lblEmail.AutoSize = true;

            this._txtEmailAddress.Font = new Font("Segoe UI", 9.5F);
            this._txtEmailAddress.Location = new Point(16, 216);
            this._txtEmailAddress.Size = new Size(365, 26);

            this._grpProfile.Controls.Add(this._lblFirstName);
            this._grpProfile.Controls.Add(this._txtFirstName);
            this._grpProfile.Controls.Add(this._lblLastName);
            this._grpProfile.Controls.Add(this._txtLastName);
            this._grpProfile.Controls.Add(this._lblLicense);
            this._grpProfile.Controls.Add(this._txtLicenseNumber);
            this._grpProfile.Controls.Add(this._lblContact);
            this._grpProfile.Controls.Add(this._txtContactNumber);
            this._grpProfile.Controls.Add(this._lblEmail);
            this._grpProfile.Controls.Add(this._txtEmailAddress);

            // Credentials GroupBox
            this._grpCredentials.Dock = DockStyle.Top;
            this._grpCredentials.Height = 150;
            this._grpCredentials.Text = "Pharmacist Portal Login Account";
            this._grpCredentials.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._grpCredentials.Padding = new Padding(14);

            this._lblUsername.Text = "Username (Login ID) *";
            this._lblUsername.Font = new Font("Segoe UI", 8.5F);
            this._lblUsername.Location = new Point(14, 25);
            this._lblUsername.AutoSize = true;

            this._txtUsername.Font = new Font("Segoe UI", 9.5F);
            this._txtUsername.Location = new Point(16, 45);
            this._txtUsername.Size = new Size(365, 26);

            this._lblPassword.Text = "Password *";
            this._lblPassword.Font = new Font("Segoe UI", 8.5F);
            this._lblPassword.Location = new Point(14, 77);
            this._lblPassword.AutoSize = true;

            this._txtPassword.Font = new Font("Segoe UI", 9.5F);
            this._txtPassword.Location = new Point(16, 97);
            this._txtPassword.Size = new Size(215, 26);
            this._txtPassword.UseSystemPasswordChar = true;

            this._btnResetOrProvision.Text = "🔑 Update Login";
            this._btnResetOrProvision.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._btnResetOrProvision.Location = new Point(238, 96);
            this._btnResetOrProvision.Size = new Size(143, 28);

            this._lblCredentialsNote.Text = "Edit the username or enter a new password above, then save or click 'Update Login'.";
            this._lblCredentialsNote.Font = new Font("Segoe UI", 7.5F, FontStyle.Italic);
            this._lblCredentialsNote.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblCredentialsNote.Location = new Point(16, 126);
            this._lblCredentialsNote.Size = new Size(365, 18);

            this._grpCredentials.Controls.Add(this._lblUsername);
            this._grpCredentials.Controls.Add(this._txtUsername);
            this._grpCredentials.Controls.Add(this._lblPassword);
            this._grpCredentials.Controls.Add(this._txtPassword);
            this._grpCredentials.Controls.Add(this._btnResetOrProvision);
            this._grpCredentials.Controls.Add(this._lblCredentialsNote);

            // Action Panel (Docked at Bottom)
            this._actionPanel.Dock = DockStyle.Bottom;
            this._actionPanel.Height = 195;
            this._actionPanel.Padding = new Padding(4);

            this._btnPrimaryAction.Text = "Register Pharmacist";
            this._btnPrimaryAction.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this._btnPrimaryAction.Location = new Point(4, 6);
            this._btnPrimaryAction.Size = new Size(250, 40);

            this._btnClearOrCancel.Text = "Clear Fields";
            this._btnClearOrCancel.Font = new Font("Segoe UI", 9F);
            this._btnClearOrCancel.Location = new Point(260, 6);
            this._btnClearOrCancel.Size = new Size(130, 40);

            this._btnArchivePharmacist.Text = "🚫 Archive Pharmacist";
            this._btnArchivePharmacist.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnArchivePharmacist.Location = new Point(4, 54);
            this._btnArchivePharmacist.Size = new Size(185, 36);

            this._btnDeletePharmacist.Text = "🗑️ Delete Record";
            this._btnDeletePharmacist.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnDeletePharmacist.Location = new Point(195, 54);
            this._btnDeletePharmacist.Size = new Size(195, 36);

            this._btnTestPortal.Text = "💊 Test Pharmacist Portal As Selected Pharmacist";
            this._btnTestPortal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnTestPortal.Location = new Point(4, 98);
            this._btnTestPortal.Size = new Size(386, 38);

            this._actionPanel.Controls.Add(this._btnPrimaryAction);
            this._actionPanel.Controls.Add(this._btnClearOrCancel);
            this._actionPanel.Controls.Add(this._btnArchivePharmacist);
            this._actionPanel.Controls.Add(this._btnDeletePharmacist);
            this._actionPanel.Controls.Add(this._btnTestPortal);

            // Add to Right Panel in strict order
            this._rightPanel.Controls.Add(this._actionPanel);
            this._rightPanel.Controls.Add(this._grpCredentials);
            this._rightPanel.Controls.Add(this._grpProfile);
            this._rightPanel.Controls.Add(this._modeBannerPanel);

            // Add to Form
            this.Controls.Add(this._leftGridPanel);
            this.Controls.Add(this._rightPanel);
            this.Controls.Add(this._headerPanel);

            this._headerPanel.ResumeLayout(false);
            this._headerPanel.PerformLayout();
            this._rosterHeaderPanel.ResumeLayout(false);
            this._rosterHeaderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this._gridPharmacists).EndInit();
            this._leftGridPanel.ResumeLayout(false);
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

