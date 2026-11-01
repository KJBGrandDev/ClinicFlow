#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class PatientForm
    {
        private System.ComponentModel.IContainer? components = null;

        // Left Panel (Fixed 475px Dock)
        private Panel _leftPanel = null!;
        private Panel _leftHeaderPanel = null!;
        private Label _lblDirectoryTitle = null!;
        private TableLayoutPanel _searchTable = null!;
        private TextBox _searchTextBox = null!;
        private Button _btnSearch = null!;
        private CheckBox _chkShowArchived = null!;
        private DataGridView _patientsGrid = null!;
        private Label _lblDirectoryCount = null!;

        // Right Panel
        private Panel _rightPanel = null!;
        private Panel _bannerPanel = null!;
        private Label _lblBannerTitle = null!;
        private Label _lblBannerSubtitle = null!;

        // Demographics Group
        private GroupBox _grpDemographics = null!;
        private TableLayoutPanel _tlpDemographics = null!;
        private Label _lblFirstName = null!;
        private Label _lblMiddleName = null!;
        private Label _lblLastName = null!;
        private Label _lblSuffix = null!;
        private TextBox _txtFirstName = null!;
        private TextBox _txtMiddleName = null!;
        private TextBox _txtLastName = null!;
        private TextBox _txtSuffix = null!;
        private Label _lblSex = null!;
        private Label _lblBirthDate = null!;
        private Label _lblAgeHeader = null!;
        private ComboBox _cboSex = null!;
        private DateTimePicker _dtpBirthDate = null!;
        private Label _lblCalculatedAge = null!;

        // Contact & Address Group
        private GroupBox _grpContact = null!;
        private TableLayoutPanel _tlpContact = null!;
        private Label _lblContactNumber = null!;
        private Label _lblStreet = null!;
        private Label _lblCity = null!;
        private Label _lblMunicipality = null!;
        private TextBox _txtContactNumber = null!;
        private TextBox _txtStreetAddress = null!;
        private TextBox _txtCity = null!;
        private TextBox _txtMunicipality = null!;

        // Emergency Group
        private GroupBox _grpEmergency = null!;
        private TableLayoutPanel _tlpEmergency = null!;
        private Label _lblEmergencyPerson = null!;
        private Label _lblEmergencyPhone = null!;
        private TextBox _txtEmergencyPerson = null!;
        private TextBox _txtEmergencyPhone = null!;

        // Action Bar
        private FlowLayoutPanel _actionFlow = null!;
        private Button _btnRegister = null!;
        private Button _btnUpdate = null!;
        private Button _btnArchive = null!;
        private Button _btnRestore = null!;
        private Button _btnClear = null!;

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
            this._leftPanel = new Panel();
            this._leftHeaderPanel = new Panel();
            this._lblDirectoryTitle = new Label();
            this._searchTable = new TableLayoutPanel();
            this._searchTextBox = new TextBox();
            this._btnSearch = new Button();
            this._chkShowArchived = new CheckBox();
            this._patientsGrid = new DataGridView();
            this._lblDirectoryCount = new Label();

            this._rightPanel = new Panel();
            this._bannerPanel = new Panel();
            this._lblBannerTitle = new Label();
            this._lblBannerSubtitle = new Label();

            this._grpDemographics = new GroupBox();
            this._tlpDemographics = new TableLayoutPanel();
            this._lblFirstName = new Label();
            this._lblMiddleName = new Label();
            this._lblLastName = new Label();
            this._lblSuffix = new Label();
            this._txtFirstName = new TextBox();
            this._txtMiddleName = new TextBox();
            this._txtLastName = new TextBox();
            this._txtSuffix = new TextBox();
            this._lblSex = new Label();
            this._lblBirthDate = new Label();
            this._lblAgeHeader = new Label();
            this._cboSex = new ComboBox();
            this._dtpBirthDate = new DateTimePicker();
            this._lblCalculatedAge = new Label();

            this._grpContact = new GroupBox();
            this._tlpContact = new TableLayoutPanel();
            this._lblContactNumber = new Label();
            this._lblStreet = new Label();
            this._lblCity = new Label();
            this._lblMunicipality = new Label();
            this._txtContactNumber = new TextBox();
            this._txtStreetAddress = new TextBox();
            this._txtCity = new TextBox();
            this._txtMunicipality = new TextBox();

            this._grpEmergency = new GroupBox();
            this._tlpEmergency = new TableLayoutPanel();
            this._lblEmergencyPerson = new Label();
            this._lblEmergencyPhone = new Label();
            this._txtEmergencyPerson = new TextBox();
            this._txtEmergencyPhone = new TextBox();

            this._actionFlow = new FlowLayoutPanel();
            this._btnRegister = new Button();
            this._btnUpdate = new Button();
            this._btnArchive = new Button();
            this._btnRestore = new Button();
            this._btnClear = new Button();

            this._leftPanel.SuspendLayout();
            this._leftHeaderPanel.SuspendLayout();
            this._searchTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._patientsGrid).BeginInit();
            this._rightPanel.SuspendLayout();
            this._bannerPanel.SuspendLayout();
            this._grpDemographics.SuspendLayout();
            this._tlpDemographics.SuspendLayout();
            this._grpContact.SuspendLayout();
            this._tlpContact.SuspendLayout();
            this._grpEmergency.SuspendLayout();
            this._tlpEmergency.SuspendLayout();
            this._actionFlow.SuspendLayout();
            this.SuspendLayout();

            // 1. Form Settings
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.ClientSize = new Size(1380, 820);
            this.MinimumSize = new Size(1100, 700);
            this.Name = "PatientForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "ClinicFlow - Patient Registration & Directory Management";

            // -------------------------------------------------------------
            // 2. Left Panel: Registered Directory (Fixed 475px Dock)
            // -------------------------------------------------------------
            this._leftPanel.Dock = DockStyle.Left;
            this._leftPanel.Width = 475;
            this._leftPanel.Padding = new Padding(12);
            this._leftPanel.BackColor = Color.FromArgb(248, 250, 252);
            this._leftPanel.BorderStyle = BorderStyle.FixedSingle;

            // Header Container (Title + Search Bar)
            this._leftHeaderPanel.Dock = DockStyle.Top;
            this._leftHeaderPanel.Height = 74;
            this._leftHeaderPanel.BackColor = Color.Transparent;
            this._leftHeaderPanel.Controls.Add(this._searchTable);
            this._leftHeaderPanel.Controls.Add(this._lblDirectoryTitle);

            this._lblDirectoryTitle.Dock = DockStyle.Top;
            this._lblDirectoryTitle.Height = 30;
            this._lblDirectoryTitle.Text = "Registered Patient Directory";
            this._lblDirectoryTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            this._lblDirectoryTitle.ForeColor = Color.FromArgb(15, 23, 42);

            // Aligned Search Bar
            this._searchTable.Dock = DockStyle.Bottom;
            this._searchTable.Height = 34;
            this._searchTable.ColumnCount = 3;
            this._searchTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._searchTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
            this._searchTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85F));
            this._searchTable.RowCount = 1;
            this._searchTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            this._searchTextBox.Dock = DockStyle.Fill;
            this._searchTextBox.Font = new Font("Segoe UI", 9.5F);
            this._searchTextBox.PlaceholderText = "Search by Name, PT#, Contact...";
            this._searchTextBox.Margin = new Padding(0, 2, 4, 2);

            this._btnSearch.Dock = DockStyle.Fill;
            this._btnSearch.Text = "🔍 Search";
            this._btnSearch.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._btnSearch.Cursor = Cursors.Hand;
            this._btnSearch.Margin = new Padding(0, 1, 4, 1);

            this._chkShowArchived.Dock = DockStyle.Fill;
            this._chkShowArchived.Text = "Archived";
            this._chkShowArchived.Font = new Font("Segoe UI", 8.5F);
            this._chkShowArchived.Cursor = Cursors.Hand;
            this._chkShowArchived.Margin = new Padding(0, 1, 0, 1);

            this._searchTable.Controls.Add(this._searchTextBox, 0, 0);
            this._searchTable.Controls.Add(this._btnSearch, 1, 0);
            this._searchTable.Controls.Add(this._chkShowArchived, 2, 0);

            // DataGridView
            this._patientsGrid.Dock = DockStyle.Fill;
            this._patientsGrid.BackgroundColor = Color.White;
            this._patientsGrid.BorderStyle = BorderStyle.FixedSingle;
            this._patientsGrid.RowHeadersVisible = false;
            this._patientsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._patientsGrid.MultiSelect = false;
            this._patientsGrid.AllowUserToAddRows = false;
            this._patientsGrid.AllowUserToDeleteRows = false;
            this._patientsGrid.ReadOnly = true;
            this._patientsGrid.RowTemplate.Height = 32;
            this._patientsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._patientsGrid.ColumnHeadersHeight = 34;
            this._patientsGrid.Font = new Font("Segoe UI", 9F);
            this._patientsGrid.Margin = new Padding(0, 8, 0, 8);

            this._lblDirectoryCount.Dock = DockStyle.Bottom;
            this._lblDirectoryCount.Height = 26;
            this._lblDirectoryCount.Text = "Total Registered: 0";
            this._lblDirectoryCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            this._lblDirectoryCount.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblDirectoryCount.TextAlign = ContentAlignment.MiddleLeft;

            // Docking order: Fill is added first so it never overlaps Top/Bottom controls
            this._leftPanel.Controls.Add(this._patientsGrid);
            this._leftPanel.Controls.Add(this._lblDirectoryCount);
            this._leftPanel.Controls.Add(this._leftHeaderPanel);

            // -------------------------------------------------------------
            // 3. Right Panel: Patient Registration & Profile Workspace
            // -------------------------------------------------------------
            this._rightPanel.Dock = DockStyle.Fill;
            this._rightPanel.Padding = new Padding(18, 14, 18, 14);
            this._rightPanel.AutoScroll = true;

            // Header Banner
            this._bannerPanel.Dock = DockStyle.Top;
            this._bannerPanel.Height = 58;
            this._bannerPanel.BackColor = Color.FromArgb(241, 245, 249);
            this._bannerPanel.BorderStyle = BorderStyle.FixedSingle;
            this._bannerPanel.Padding = new Padding(12, 8, 12, 8);
            this._bannerPanel.Controls.Add(this._lblBannerTitle);
            this._bannerPanel.Controls.Add(this._lblBannerSubtitle);

            this._lblBannerTitle.Text = "➕ New Patient Registration";
            this._lblBannerTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            this._lblBannerTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblBannerTitle.Location = new Point(12, 6);
            this._lblBannerTitle.AutoSize = true;

            this._lblBannerSubtitle.Text = "Fill in the demographic and contact information below to register a new clinical profile.";
            this._lblBannerSubtitle.Font = new Font("Segoe UI", 8.5F);
            this._lblBannerSubtitle.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblBannerSubtitle.Location = new Point(13, 30);
            this._lblBannerSubtitle.AutoSize = true;

            // Section 1: Demographics
            this._grpDemographics.Dock = DockStyle.Top;
            this._grpDemographics.Height = 155;
            this._grpDemographics.Text = "Personal Demographic Information";
            this._grpDemographics.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._grpDemographics.ForeColor = Color.FromArgb(30, 41, 59);
            this._grpDemographics.Padding = new Padding(12);
            this._grpDemographics.Margin = new Padding(0, 12, 0, 10);
            this._grpDemographics.Controls.Add(this._tlpDemographics);

            this._tlpDemographics.Dock = DockStyle.Fill;
            this._tlpDemographics.ColumnCount = 4;
            this._tlpDemographics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            this._tlpDemographics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            this._tlpDemographics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            this._tlpDemographics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
            this._tlpDemographics.RowCount = 4;
            this._tlpDemographics.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            this._tlpDemographics.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this._tlpDemographics.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            this._tlpDemographics.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            this._lblFirstName.Text = "First Name *";
            this._lblFirstName.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblFirstName.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblFirstName.AutoSize = true;

            this._lblMiddleName.Text = "Middle Name";
            this._lblMiddleName.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblMiddleName.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblMiddleName.AutoSize = true;

            this._lblLastName.Text = "Last Name *";
            this._lblLastName.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblLastName.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblLastName.AutoSize = true;

            this._lblSuffix.Text = "Suffix";
            this._lblSuffix.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblSuffix.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblSuffix.AutoSize = true;

            this._txtFirstName.Dock = DockStyle.Fill;
            this._txtFirstName.Font = new Font("Segoe UI", 9.5F);
            this._txtFirstName.Margin = new Padding(0, 1, 8, 1);

            this._txtMiddleName.Dock = DockStyle.Fill;
            this._txtMiddleName.Font = new Font("Segoe UI", 9.5F);
            this._txtMiddleName.Margin = new Padding(0, 1, 8, 1);

            this._txtLastName.Dock = DockStyle.Fill;
            this._txtLastName.Font = new Font("Segoe UI", 9.5F);
            this._txtLastName.Margin = new Padding(0, 1, 8, 1);

            this._txtSuffix.Dock = DockStyle.Fill;
            this._txtSuffix.Font = new Font("Segoe UI", 9.5F);
            this._txtSuffix.PlaceholderText = "Jr, III";
            this._txtSuffix.Margin = new Padding(0, 1, 0, 1);

            this._lblSex.Text = "Sex *";
            this._lblSex.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblSex.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblSex.AutoSize = true;

            this._lblBirthDate.Text = "Birth Date *";
            this._lblBirthDate.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblBirthDate.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblBirthDate.AutoSize = true;

            this._lblAgeHeader.Text = "Calculated Age";
            this._lblAgeHeader.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblAgeHeader.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblAgeHeader.AutoSize = true;

            this._cboSex.Dock = DockStyle.Fill;
            this._cboSex.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cboSex.Font = new Font("Segoe UI", 9.5F);
            this._cboSex.Items.AddRange(new object[] { "Male", "Female" });
            this._cboSex.SelectedIndex = 0;
            this._cboSex.Margin = new Padding(0, 1, 8, 1);

            this._dtpBirthDate.Dock = DockStyle.Fill;
            this._dtpBirthDate.Format = DateTimePickerFormat.Short;
            this._dtpBirthDate.Font = new Font("Segoe UI", 9.5F);
            this._dtpBirthDate.MinDate = new DateTime(1800, 1, 1);
            this._dtpBirthDate.MaxDate = DateTime.Today;
            this._dtpBirthDate.Value = new DateTime(1800, 1, 1);
            this._dtpBirthDate.Margin = new Padding(0, 1, 8, 1);

            this._lblCalculatedAge.Dock = DockStyle.Fill;
            this._lblCalculatedAge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblCalculatedAge.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblCalculatedAge.BackColor = Color.FromArgb(241, 245, 249);
            this._lblCalculatedAge.TextAlign = ContentAlignment.MiddleCenter;
            this._lblCalculatedAge.Text = "Not Set";
            this._lblCalculatedAge.Margin = new Padding(0, 1, 8, 1);

            this._tlpDemographics.Controls.Add(this._lblFirstName, 0, 0);
            this._tlpDemographics.Controls.Add(this._lblMiddleName, 1, 0);
            this._tlpDemographics.Controls.Add(this._lblLastName, 2, 0);
            this._tlpDemographics.Controls.Add(this._lblSuffix, 3, 0);
            this._tlpDemographics.Controls.Add(this._txtFirstName, 0, 1);
            this._tlpDemographics.Controls.Add(this._txtMiddleName, 1, 1);
            this._tlpDemographics.Controls.Add(this._txtLastName, 2, 1);
            this._tlpDemographics.Controls.Add(this._txtSuffix, 3, 1);
            this._tlpDemographics.Controls.Add(this._lblSex, 0, 2);
            this._tlpDemographics.Controls.Add(this._lblBirthDate, 1, 2);
            this._tlpDemographics.Controls.Add(this._lblAgeHeader, 2, 2);
            this._tlpDemographics.Controls.Add(this._cboSex, 0, 3);
            this._tlpDemographics.Controls.Add(this._dtpBirthDate, 1, 3);
            this._tlpDemographics.Controls.Add(this._lblCalculatedAge, 2, 3);

            // Section 2: Contact & Address
            this._grpContact.Dock = DockStyle.Top;
            this._grpContact.Height = 110;
            this._grpContact.Text = "Contact & Residential Address Details";
            this._grpContact.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._grpContact.ForeColor = Color.FromArgb(30, 41, 59);
            this._grpContact.Padding = new Padding(12);
            this._grpContact.Margin = new Padding(0, 10, 0, 10);
            this._grpContact.Controls.Add(this._tlpContact);

            this._tlpContact.Dock = DockStyle.Fill;
            this._tlpContact.ColumnCount = 4;
            this._tlpContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            this._tlpContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            this._tlpContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            this._tlpContact.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            this._tlpContact.RowCount = 2;
            this._tlpContact.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            this._tlpContact.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            this._lblContactNumber.Text = "Contact Number *";
            this._lblContactNumber.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblContactNumber.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblContactNumber.AutoSize = true;

            this._lblStreet.Text = "Street / Barangay Address *";
            this._lblStreet.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblStreet.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblStreet.AutoSize = true;

            this._lblCity.Text = "City *";
            this._lblCity.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblCity.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblCity.AutoSize = true;

            this._lblMunicipality.Text = "Municipality / Region";
            this._lblMunicipality.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblMunicipality.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblMunicipality.AutoSize = true;

            this._txtContactNumber.Dock = DockStyle.Fill;
            this._txtContactNumber.Font = new Font("Segoe UI", 9.5F);
            this._txtContactNumber.PlaceholderText = "e.g., 09171234567";
            this._txtContactNumber.MaxLength = 11;
            this._txtContactNumber.Margin = new Padding(0, 1, 8, 1);

            this._txtStreetAddress.Dock = DockStyle.Fill;
            this._txtStreetAddress.Font = new Font("Segoe UI", 9.5F);
            this._txtStreetAddress.PlaceholderText = "Street, subdivision, or barangay";
            this._txtStreetAddress.Margin = new Padding(0, 1, 8, 1);

            this._txtCity.Dock = DockStyle.Fill;
            this._txtCity.Font = new Font("Segoe UI", 9.5F);
            this._txtCity.Text = "Davao City";
            this._txtCity.Margin = new Padding(0, 1, 8, 1);

            this._txtMunicipality.Dock = DockStyle.Fill;
            this._txtMunicipality.Font = new Font("Segoe UI", 9.5F);
            this._txtMunicipality.Text = "Davao";
            this._txtMunicipality.Margin = new Padding(0, 1, 0, 1);

            this._tlpContact.Controls.Add(this._lblContactNumber, 0, 0);
            this._tlpContact.Controls.Add(this._lblStreet, 1, 0);
            this._tlpContact.Controls.Add(this._lblCity, 2, 0);
            this._tlpContact.Controls.Add(this._lblMunicipality, 3, 0);
            this._tlpContact.Controls.Add(this._txtContactNumber, 0, 1);
            this._tlpContact.Controls.Add(this._txtStreetAddress, 1, 1);
            this._tlpContact.Controls.Add(this._txtCity, 2, 1);
            this._tlpContact.Controls.Add(this._txtMunicipality, 3, 1);

            // Section 3: Emergency
            this._grpEmergency.Dock = DockStyle.Top;
            this._grpEmergency.Height = 100;
            this._grpEmergency.Text = "Emergency Contact Person";
            this._grpEmergency.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._grpEmergency.ForeColor = Color.FromArgb(30, 41, 59);
            this._grpEmergency.Padding = new Padding(12);
            this._grpEmergency.Margin = new Padding(0, 10, 0, 10);
            this._grpEmergency.Controls.Add(this._tlpEmergency);

            this._tlpEmergency.Dock = DockStyle.Fill;
            this._tlpEmergency.ColumnCount = 2;
            this._tlpEmergency.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._tlpEmergency.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._tlpEmergency.RowCount = 2;
            this._tlpEmergency.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            this._tlpEmergency.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            this._lblEmergencyPerson.Text = "Contact Person Full Name";
            this._lblEmergencyPerson.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblEmergencyPerson.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblEmergencyPerson.AutoSize = true;

            this._lblEmergencyPhone.Text = "Emergency Contact Phone #";
            this._lblEmergencyPhone.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblEmergencyPhone.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblEmergencyPhone.AutoSize = true;

            this._txtEmergencyPerson.Dock = DockStyle.Fill;
            this._txtEmergencyPerson.Font = new Font("Segoe UI", 9.5F);
            this._txtEmergencyPerson.PlaceholderText = "Name of spouse, parent, or guardian";
            this._txtEmergencyPerson.Margin = new Padding(0, 1, 8, 1);

            this._txtEmergencyPhone.Dock = DockStyle.Fill;
            this._txtEmergencyPhone.Font = new Font("Segoe UI", 9.5F);
            this._txtEmergencyPhone.PlaceholderText = "e.g., 09171234567";
            this._txtEmergencyPhone.MaxLength = 11;
            this._txtEmergencyPhone.Margin = new Padding(0, 1, 0, 1);

            this._tlpEmergency.Controls.Add(this._lblEmergencyPerson, 0, 0);
            this._tlpEmergency.Controls.Add(this._lblEmergencyPhone, 1, 0);
            this._tlpEmergency.Controls.Add(this._txtEmergencyPerson, 0, 1);
            this._tlpEmergency.Controls.Add(this._txtEmergencyPhone, 1, 1);

            // Action Buttons Bar
            this._actionFlow.Dock = DockStyle.Top;
            this._actionFlow.Height = 52;
            this._actionFlow.Padding = new Padding(0, 8, 0, 0);
            this._actionFlow.Controls.Add(this._btnRegister);
            this._actionFlow.Controls.Add(this._btnUpdate);
            this._actionFlow.Controls.Add(this._btnArchive);
            this._actionFlow.Controls.Add(this._btnRestore);
            this._actionFlow.Controls.Add(this._btnClear);

            this._btnRegister.Size = new Size(160, 38);
            this._btnRegister.Text = "➕ Register Patient";
            this._btnRegister.Cursor = Cursors.Hand;

            this._btnUpdate.Size = new Size(150, 38);
            this._btnUpdate.Text = "✏ Update Profile";
            this._btnUpdate.Enabled = false;
            this._btnUpdate.Cursor = Cursors.Default;

            this._btnArchive.Size = new Size(125, 38);
            this._btnArchive.Text = "🗑 Archive";
            this._btnArchive.Enabled = false;
            this._btnArchive.Cursor = Cursors.Default;

            this._btnRestore.Size = new Size(125, 38);
            this._btnRestore.Text = "♻ Restore";
            this._btnRestore.Visible = false;
            this._btnRestore.Cursor = Cursors.Hand;

            this._btnClear.Size = new Size(115, 38);
            this._btnClear.Text = "Clear Fields";
            this._btnClear.Cursor = Cursors.Hand;

            // Docking order: Controls docked to Top appear from top-to-bottom when added in reverse order
            this._rightPanel.Controls.Add(this._actionFlow);
            this._rightPanel.Controls.Add(this._grpEmergency);
            this._rightPanel.Controls.Add(this._grpContact);
            this._rightPanel.Controls.Add(this._grpDemographics);
            this._rightPanel.Controls.Add(this._bannerPanel);

            this.Controls.Add(this._rightPanel);
            this.Controls.Add(this._leftPanel);

            this._leftPanel.ResumeLayout(false);
            this._leftHeaderPanel.ResumeLayout(false);
            this._searchTable.ResumeLayout(false);
            this._searchTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this._patientsGrid).EndInit();
            this._rightPanel.ResumeLayout(false);
            this._bannerPanel.ResumeLayout(false);
            this._bannerPanel.PerformLayout();
            this._grpDemographics.ResumeLayout(false);
            this._tlpDemographics.ResumeLayout(false);
            this._tlpDemographics.PerformLayout();
            this._grpContact.ResumeLayout(false);
            this._tlpContact.ResumeLayout(false);
            this._tlpContact.PerformLayout();
            this._grpEmergency.ResumeLayout(false);
            this._tlpEmergency.ResumeLayout(false);
            this._tlpEmergency.PerformLayout();
            this._actionFlow.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}