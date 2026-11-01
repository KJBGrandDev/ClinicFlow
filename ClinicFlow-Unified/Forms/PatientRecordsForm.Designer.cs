#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class PatientRecordsForm
    {
        private System.ComponentModel.IContainer? components = null;

        // Left Panel (Patient Search & Directory)
        private Panel _leftPanel = null!;
        private Label _lblDirectoryTitle = null!;
        private TableLayoutPanel _searchTable = null!;
        private TextBox _searchTextBox = null!;
        private Button _btnSearch = null!;
        private DataGridView _patientsGrid = null!;
        private Label _lblDirectoryCount = null!;

        // Right Panel (Longitudinal Medical Chart)
        private Panel _rightPanel = null!;
        private Panel _patientBanner = null!;
        private Label _lblPatientTitle = null!;
        private Label _lblPatientDetails = null!;
        private Button _btnRefresh = null!;

        // Middle Timeline Panel (Fixed 380px Dock)
        private Panel _timelinePanel = null!;
        private Label _lblTimelineHeader = null!;
        private DataGridView _timelineGrid = null!;

        // Encounter Detail Sheet Panel (Dock Fill)
        private Panel _encounterPanel = null!;
        private Label _lblEncounterInfo = null!;
        private TableLayoutPanel _notesTable = null!;
        private Label _lblChiefTitle = null!;
        private TextBox _txtChief = null!;
        private Label _lblDiagTitle = null!;
        private TextBox _txtDiag = null!;
        private Label _lblNotesTitle = null!;
        private TextBox _txtNotes = null!;
        private Label _lblRecsTitle = null!;
        private TextBox _txtRecs = null!;

        // Past Rx Section
        private Label _lblRxHeader = null!;
        private DataGridView _rxGrid = null!;

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
            _leftPanel = new Panel();
            _patientsGrid = new DataGridView();
            _lblDirectoryCount = new Label();
            _searchTable = new TableLayoutPanel();
            _searchTextBox = new TextBox();
            _btnSearch = new Button();
            _lblDirectoryTitle = new Label();
            _rightPanel = new Panel();
            _encounterPanel = new Panel();
            _rxGrid = new DataGridView();
            _lblRxHeader = new Label();
            _notesTable = new TableLayoutPanel();
            _lblChiefTitle = new Label();
            _lblDiagTitle = new Label();
            _txtChief = new TextBox();
            _txtDiag = new TextBox();
            _lblNotesTitle = new Label();
            _lblRecsTitle = new Label();
            _txtNotes = new TextBox();
            _txtRecs = new TextBox();
            _lblEncounterInfo = new Label();
            _timelinePanel = new Panel();
            _timelineGrid = new DataGridView();
            _lblTimelineHeader = new Label();
            _patientBanner = new Panel();
            _btnRefresh = new Button();
            _lblPatientDetails = new Label();
            _lblPatientTitle = new Label();
            _leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_patientsGrid).BeginInit();
            _searchTable.SuspendLayout();
            _rightPanel.SuspendLayout();
            _encounterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_rxGrid).BeginInit();
            _notesTable.SuspendLayout();
            _timelinePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_timelineGrid).BeginInit();
            _patientBanner.SuspendLayout();
            SuspendLayout();
            // 
            // _leftPanel
            // 
            _leftPanel.BackColor = Color.FromArgb(248, 250, 252);
            _leftPanel.BorderStyle = BorderStyle.FixedSingle;
            _leftPanel.Controls.Add(_patientsGrid);
            _leftPanel.Controls.Add(_lblDirectoryCount);
            _leftPanel.Controls.Add(_searchTable);
            _leftPanel.Controls.Add(_lblDirectoryTitle);
            _leftPanel.Dock = DockStyle.Left;
            _leftPanel.Location = new Point(0, 0);
            _leftPanel.Margin = new Padding(3, 4, 3, 4);
            _leftPanel.Name = "_leftPanel";
            _leftPanel.Padding = new Padding(11, 13, 11, 13);
            _leftPanel.Size = new Size(343, 1055);
            _leftPanel.TabIndex = 1;
            // 
            // _patientsGrid
            // 
            _patientsGrid.AllowUserToAddRows = false;
            _patientsGrid.AllowUserToDeleteRows = false;
            _patientsGrid.BackgroundColor = Color.White;
            _patientsGrid.ColumnHeadersHeight = 29;
            _patientsGrid.Cursor = Cursors.Hand;
            _patientsGrid.Dock = DockStyle.Fill;
            _patientsGrid.Location = new Point(11, 93);
            _patientsGrid.Margin = new Padding(3, 4, 3, 4);
            _patientsGrid.MultiSelect = false;
            _patientsGrid.Name = "_patientsGrid";
            _patientsGrid.ReadOnly = true;
            _patientsGrid.RowHeadersVisible = false;
            _patientsGrid.RowHeadersWidth = 51;
            _patientsGrid.RowTemplate.Height = 32;
            _patientsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _patientsGrid.Size = new Size(319, 915);
            _patientsGrid.TabIndex = 0;
            // 
            // _lblDirectoryCount
            // 
            _lblDirectoryCount.Dock = DockStyle.Bottom;
            _lblDirectoryCount.Font = new Font("Segoe UI", 8F);
            _lblDirectoryCount.ForeColor = Color.FromArgb(100, 116, 139);
            _lblDirectoryCount.Location = new Point(11, 1008);
            _lblDirectoryCount.Name = "_lblDirectoryCount";
            _lblDirectoryCount.Size = new Size(319, 32);
            _lblDirectoryCount.TabIndex = 1;
            _lblDirectoryCount.Text = "Found 0 registered patient(s).";
            _lblDirectoryCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _searchTable
            // 
            _searchTable.ColumnCount = 2;
            _searchTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _searchTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            _searchTable.Controls.Add(_searchTextBox, 0, 0);
            _searchTable.Controls.Add(_btnSearch, 1, 0);
            _searchTable.Dock = DockStyle.Top;
            _searchTable.Location = new Point(11, 50);
            _searchTable.Margin = new Padding(3, 4, 3, 4);
            _searchTable.Name = "_searchTable";
            _searchTable.RowCount = 1;
            _searchTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _searchTable.Size = new Size(319, 43);
            _searchTable.TabIndex = 2;
            // 
            // _searchTextBox
            // 
            _searchTextBox.Dock = DockStyle.Fill;
            _searchTextBox.Font = new Font("Segoe UI", 9.5F);
            _searchTextBox.Location = new Point(0, 0);
            _searchTextBox.Margin = new Padding(0, 0, 5, 0);
            _searchTextBox.Name = "_searchTextBox";
            _searchTextBox.PlaceholderText = "Search Name or PT #...";
            _searchTextBox.Size = new Size(234, 29);
            _searchTextBox.TabIndex = 0;
            // 
            // _btnSearch
            // 
            _btnSearch.Cursor = Cursors.Hand;
            _btnSearch.Dock = DockStyle.Fill;
            _btnSearch.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _btnSearch.Location = new Point(239, 0);
            _btnSearch.Margin = new Padding(0);
            _btnSearch.Name = "_btnSearch";
            _btnSearch.Size = new Size(80, 43);
            _btnSearch.TabIndex = 1;
            _btnSearch.Text = "🔍 Find";
            // 
            // _lblDirectoryTitle
            // 
            _lblDirectoryTitle.Dock = DockStyle.Top;
            _lblDirectoryTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _lblDirectoryTitle.ForeColor = Color.FromArgb(15, 23, 42);
            _lblDirectoryTitle.Location = new Point(11, 13);
            _lblDirectoryTitle.Name = "_lblDirectoryTitle";
            _lblDirectoryTitle.Size = new Size(319, 37);
            _lblDirectoryTitle.TabIndex = 3;
            _lblDirectoryTitle.Text = "Search Patient Records";
            // 
            // _rightPanel
            // 
            _rightPanel.Controls.Add(_encounterPanel);
            _rightPanel.Controls.Add(_timelinePanel);
            _rightPanel.Controls.Add(_patientBanner);
            _rightPanel.Dock = DockStyle.Fill;
            _rightPanel.Location = new Point(343, 0);
            _rightPanel.Margin = new Padding(3, 4, 3, 4);
            _rightPanel.Name = "_rightPanel";
            _rightPanel.Padding = new Padding(14, 16, 14, 16);
            _rightPanel.Size = new Size(1234, 1055);
            _rightPanel.TabIndex = 0;
            // 
            // _encounterPanel
            // 
            _encounterPanel.BackColor = Color.White;
            _encounterPanel.BorderStyle = BorderStyle.FixedSingle;
            _encounterPanel.Controls.Add(_rxGrid);
            _encounterPanel.Controls.Add(_lblRxHeader);
            _encounterPanel.Controls.Add(_notesTable);
            _encounterPanel.Controls.Add(_lblEncounterInfo);
            _encounterPanel.Dock = DockStyle.Fill;
            _encounterPanel.Location = new Point(448, 93);
            _encounterPanel.Margin = new Padding(3, 4, 3, 4);
            _encounterPanel.Name = "_encounterPanel";
            _encounterPanel.Padding = new Padding(16, 19, 16, 19);
            _encounterPanel.Size = new Size(772, 946);
            _encounterPanel.TabIndex = 0;
            // 
            // _rxGrid
            // 
            _rxGrid.AllowUserToAddRows = false;
            _rxGrid.AllowUserToDeleteRows = false;
            _rxGrid.BackgroundColor = Color.White;
            _rxGrid.ColumnHeadersHeight = 29;
            _rxGrid.Dock = DockStyle.Fill;
            _rxGrid.Location = new Point(16, 355);
            _rxGrid.Margin = new Padding(3, 4, 3, 4);
            _rxGrid.MultiSelect = false;
            _rxGrid.Name = "_rxGrid";
            _rxGrid.ReadOnly = true;
            _rxGrid.RowHeadersVisible = false;
            _rxGrid.RowHeadersWidth = 51;
            _rxGrid.RowTemplate.Height = 30;
            _rxGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _rxGrid.Size = new Size(738, 570);
            _rxGrid.TabIndex = 0;
            // 
            // _lblRxHeader
            // 
            _lblRxHeader.Dock = DockStyle.Top;
            _lblRxHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblRxHeader.ForeColor = Color.FromArgb(71, 85, 105);
            _lblRxHeader.Location = new Point(16, 312);
            _lblRxHeader.Name = "_lblRxHeader";
            _lblRxHeader.Size = new Size(738, 43);
            _lblRxHeader.TabIndex = 1;
            _lblRxHeader.Text = "Prescribed Medications";
            _lblRxHeader.TextAlign = ContentAlignment.BottomLeft;
            // 
            // _notesTable
            // 
            _notesTable.ColumnCount = 2;
            _notesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _notesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _notesTable.Controls.Add(_lblChiefTitle, 0, 0);
            _notesTable.Controls.Add(_lblDiagTitle, 1, 0);
            _notesTable.Controls.Add(_txtChief, 0, 1);
            _notesTable.Controls.Add(_txtDiag, 1, 1);
            _notesTable.Controls.Add(_lblNotesTitle, 0, 2);
            _notesTable.Controls.Add(_lblRecsTitle, 1, 2);
            _notesTable.Controls.Add(_txtNotes, 0, 3);
            _notesTable.Controls.Add(_txtRecs, 1, 3);
            _notesTable.Dock = DockStyle.Top;
            _notesTable.Location = new Point(16, 59);
            _notesTable.Margin = new Padding(3, 4, 3, 4);
            _notesTable.Name = "_notesTable";
            _notesTable.RowCount = 4;
            _notesTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            _notesTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 91F));
            _notesTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            _notesTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _notesTable.Size = new Size(738, 253);
            _notesTable.TabIndex = 2;
            // 
            // _lblChiefTitle
            // 
            _lblChiefTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblChiefTitle.ForeColor = Color.FromArgb(71, 85, 105);
            _lblChiefTitle.Location = new Point(3, 0);
            _lblChiefTitle.Name = "_lblChiefTitle";
            _lblChiefTitle.Size = new Size(91, 29);
            _lblChiefTitle.TabIndex = 0;
            _lblChiefTitle.Text = "Chief Complaint:";
            // 
            // _lblDiagTitle
            // 
            _lblDiagTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblDiagTitle.ForeColor = Color.FromArgb(71, 85, 105);
            _lblDiagTitle.Location = new Point(372, 0);
            _lblDiagTitle.Name = "_lblDiagTitle";
            _lblDiagTitle.Size = new Size(91, 29);
            _lblDiagTitle.TabIndex = 1;
            _lblDiagTitle.Text = "Formal Diagnosis:";
            // 
            // _txtChief
            // 
            _txtChief.BackColor = Color.FromArgb(248, 250, 252);
            _txtChief.Dock = DockStyle.Fill;
            _txtChief.Font = new Font("Segoe UI", 9F);
            _txtChief.Location = new Point(0, 29);
            _txtChief.Margin = new Padding(0, 0, 9, 0);
            _txtChief.Multiline = true;
            _txtChief.Name = "_txtChief";
            _txtChief.ReadOnly = true;
            _txtChief.ScrollBars = ScrollBars.Vertical;
            _txtChief.Size = new Size(360, 91);
            _txtChief.TabIndex = 2;
            // 
            // _txtDiag
            // 
            _txtDiag.BackColor = Color.FromArgb(248, 250, 252);
            _txtDiag.Dock = DockStyle.Fill;
            _txtDiag.Font = new Font("Segoe UI", 9F);
            _txtDiag.Location = new Point(378, 29);
            _txtDiag.Margin = new Padding(9, 0, 0, 0);
            _txtDiag.Multiline = true;
            _txtDiag.Name = "_txtDiag";
            _txtDiag.ReadOnly = true;
            _txtDiag.ScrollBars = ScrollBars.Vertical;
            _txtDiag.Size = new Size(360, 91);
            _txtDiag.TabIndex = 3;
            // 
            // _lblNotesTitle
            // 
            _lblNotesTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblNotesTitle.ForeColor = Color.FromArgb(71, 85, 105);
            _lblNotesTitle.Location = new Point(3, 120);
            _lblNotesTitle.Name = "_lblNotesTitle";
            _lblNotesTitle.Size = new Size(91, 29);
            _lblNotesTitle.TabIndex = 4;
            _lblNotesTitle.Text = "Clinical Examination Notes:";
            // 
            // _lblRecsTitle
            // 
            _lblRecsTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblRecsTitle.ForeColor = Color.FromArgb(71, 85, 105);
            _lblRecsTitle.Location = new Point(372, 120);
            _lblRecsTitle.Name = "_lblRecsTitle";
            _lblRecsTitle.Size = new Size(91, 29);
            _lblRecsTitle.TabIndex = 5;
            _lblRecsTitle.Text = "Doctor Recommendations / Orders:";
            // 
            // _txtNotes
            // 
            _txtNotes.BackColor = Color.FromArgb(248, 250, 252);
            _txtNotes.Dock = DockStyle.Fill;
            _txtNotes.Font = new Font("Segoe UI", 9F);
            _txtNotes.Location = new Point(0, 149);
            _txtNotes.Margin = new Padding(0, 0, 9, 0);
            _txtNotes.Multiline = true;
            _txtNotes.Name = "_txtNotes";
            _txtNotes.ReadOnly = true;
            _txtNotes.ScrollBars = ScrollBars.Vertical;
            _txtNotes.Size = new Size(360, 104);
            _txtNotes.TabIndex = 6;
            // 
            // _txtRecs
            // 
            _txtRecs.BackColor = Color.FromArgb(248, 250, 252);
            _txtRecs.Dock = DockStyle.Fill;
            _txtRecs.Font = new Font("Segoe UI", 9F);
            _txtRecs.Location = new Point(378, 149);
            _txtRecs.Margin = new Padding(9, 0, 0, 0);
            _txtRecs.Multiline = true;
            _txtRecs.Name = "_txtRecs";
            _txtRecs.ReadOnly = true;
            _txtRecs.ScrollBars = ScrollBars.Vertical;
            _txtRecs.Size = new Size(360, 104);
            _txtRecs.TabIndex = 7;
            // 
            // _lblEncounterInfo
            // 
            _lblEncounterInfo.Dock = DockStyle.Top;
            _lblEncounterInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _lblEncounterInfo.ForeColor = Color.FromArgb(15, 23, 42);
            _lblEncounterInfo.Location = new Point(16, 19);
            _lblEncounterInfo.Name = "_lblEncounterInfo";
            _lblEncounterInfo.Size = new Size(738, 40);
            _lblEncounterInfo.TabIndex = 3;
            _lblEncounterInfo.Text = "Encounter Summary";
            // 
            // _timelinePanel
            // 
            _timelinePanel.Controls.Add(_timelineGrid);
            _timelinePanel.Controls.Add(_lblTimelineHeader);
            _timelinePanel.Dock = DockStyle.Left;
            _timelinePanel.Location = new Point(14, 93);
            _timelinePanel.Margin = new Padding(3, 4, 3, 4);
            _timelinePanel.Name = "_timelinePanel";
            _timelinePanel.Padding = new Padding(0, 13, 11, 0);
            _timelinePanel.Size = new Size(434, 946);
            _timelinePanel.TabIndex = 1;
            // 
            // _timelineGrid
            // 
            _timelineGrid.AllowUserToAddRows = false;
            _timelineGrid.AllowUserToDeleteRows = false;
            _timelineGrid.BackgroundColor = Color.White;
            _timelineGrid.ColumnHeadersHeight = 29;
            _timelineGrid.Dock = DockStyle.Fill;
            _timelineGrid.Location = new Point(0, 48);
            _timelineGrid.Margin = new Padding(3, 4, 3, 4);
            _timelineGrid.MultiSelect = false;
            _timelineGrid.Name = "_timelineGrid";
            _timelineGrid.ReadOnly = true;
            _timelineGrid.RowHeadersVisible = false;
            _timelineGrid.RowHeadersWidth = 51;
            _timelineGrid.RowTemplate.Height = 32;
            _timelineGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _timelineGrid.Size = new Size(423, 898);
            _timelineGrid.TabIndex = 0;
            // 
            // _lblTimelineHeader
            // 
            _lblTimelineHeader.Dock = DockStyle.Top;
            _lblTimelineHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblTimelineHeader.ForeColor = Color.FromArgb(71, 85, 105);
            _lblTimelineHeader.Location = new Point(0, 13);
            _lblTimelineHeader.Name = "_lblTimelineHeader";
            _lblTimelineHeader.Size = new Size(423, 35);
            _lblTimelineHeader.TabIndex = 1;
            _lblTimelineHeader.Text = "Clinical Visit Timeline";
            // 
            // _patientBanner
            // 
            _patientBanner.BackColor = Color.White;
            _patientBanner.BorderStyle = BorderStyle.FixedSingle;
            _patientBanner.Controls.Add(_btnRefresh);
            _patientBanner.Controls.Add(_lblPatientDetails);
            _patientBanner.Controls.Add(_lblPatientTitle);
            _patientBanner.Dock = DockStyle.Top;
            _patientBanner.Location = new Point(14, 16);
            _patientBanner.Margin = new Padding(3, 4, 3, 4);
            _patientBanner.Name = "_patientBanner";
            _patientBanner.Padding = new Padding(14, 11, 14, 11);
            _patientBanner.Size = new Size(1206, 77);
            _patientBanner.TabIndex = 2;
            // 
            // _btnRefresh
            // 
            _btnRefresh.Cursor = Cursors.Hand;
            _btnRefresh.Dock = DockStyle.Right;
            _btnRefresh.Location = new Point(1087, 11);
            _btnRefresh.Margin = new Padding(3, 4, 3, 4);
            _btnRefresh.Name = "_btnRefresh";
            _btnRefresh.Size = new Size(103, 53);
            _btnRefresh.TabIndex = 0;
            _btnRefresh.Text = "⟳ Refresh";
            // 
            // _lblPatientDetails
            // 
            _lblPatientDetails.AutoSize = true;
            _lblPatientDetails.Font = new Font("Segoe UI", 8.5F);
            _lblPatientDetails.ForeColor = Color.FromArgb(100, 116, 139);
            _lblPatientDetails.Location = new Point(15, 40);
            _lblPatientDetails.Name = "_lblPatientDetails";
            _lblPatientDetails.Size = new Size(541, 20);
            _lblPatientDetails.TabIndex = 1;
            _lblPatientDetails.Text = "Select any patient from the directory on the left to review their longitudinal EHR.";
            // 
            // _lblPatientTitle
            // 
            _lblPatientTitle.AutoSize = true;
            _lblPatientTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            _lblPatientTitle.ForeColor = Color.FromArgb(15, 23, 42);
            _lblPatientTitle.Location = new Point(14, 8);
            _lblPatientTitle.Name = "_lblPatientTitle";
            _lblPatientTitle.Size = new Size(200, 28);
            _lblPatientTitle.TabIndex = 2;
            _lblPatientTitle.Text = "No Patient Selected";
            // 
            // PatientRecordsForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1577, 1055);
            Controls.Add(_rightPanel);
            Controls.Add(_leftPanel);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1255, 918);
            Name = "PatientRecordsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ClinicFlow - Patient Medical Records (EHR Archive)";
            WindowState = FormWindowState.Maximized;
            _leftPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_patientsGrid).EndInit();
            _searchTable.ResumeLayout(false);
            _searchTable.PerformLayout();
            _rightPanel.ResumeLayout(false);
            _encounterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_rxGrid).EndInit();
            _notesTable.ResumeLayout(false);
            _notesTable.PerformLayout();
            _timelinePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_timelineGrid).EndInit();
            _patientBanner.ResumeLayout(false);
            _patientBanner.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
