#nullable enable

using System.Drawing;
using System.Windows.Forms;
using Font = System.Drawing.Font;
using Color = System.Drawing.Color;

namespace ClinicFlow_Unified.Forms;

public partial class PatientHistoryForm : Form
{
    private System.ComponentModel.IContainer? components = null;

    // Top Summary Banner
    private Panel _headerPanel = null!;
    private Label _lblPatientTitle = null!;
    private Label _lblPatientDetails = null!;

    // Main Master-Detail SplitContainer
    private SplitContainer _mainSplit = null!;

    // Left: Timeline List
    private GroupBox _timelineGroup = null!;
    private DataGridView _timelineGrid = null!;

    // Right: Unified Encounter Sheet
    private Panel _encounterPanel = null!;
    private Panel _encounterHeaderPanel = null!;
    private Label _lblEncounterInfo = null!;

    // 1. Clinical Diagnosis Group
    private GroupBox _clinicalGroup = null!;
    private TableLayoutPanel _clinicalTable = null!;
    private Label _lblChiefComplaint = null!;
    private TextBox _txtChiefComplaint = null!;
    private Label _lblDiagnosis = null!;
    private TextBox _txtDiagnosis = null!;
    private Label _lblClinicalNotes = null!;
    private TextBox _txtClinicalNotes = null!;
    private Label _lblRecommendation = null!;
    private TextBox _txtRecommendation = null!;

    // 2. Rx Group for this Encounter
    private GroupBox _rxGroup = null!;
    private DataGridView _rxGrid = null!;

    // 3. Lab Group for this Encounter
    private GroupBox _labGroup = null!;
    private DataGridView _labGrid = null!;

    // Bottom Action Panel
    private Panel _bottomPanel = null!;
    private Button _closeButton = null!;
    private Button _refreshButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _headerPanel = new Panel();
        _lblPatientDetails = new Label();
        _lblPatientTitle = new Label();
        _mainSplit = new SplitContainer();
        _timelineGroup = new GroupBox();
        _timelineGrid = new DataGridView();
        _encounterPanel = new Panel();
        _labGroup = new GroupBox();
        _labGrid = new DataGridView();
        _rxGroup = new GroupBox();
        _rxGrid = new DataGridView();
        _clinicalGroup = new GroupBox();
        _clinicalTable = new TableLayoutPanel();
        _lblChiefComplaint = new Label();
        _lblDiagnosis = new Label();
        _txtChiefComplaint = new TextBox();
        _txtDiagnosis = new TextBox();
        _lblClinicalNotes = new Label();
        _lblRecommendation = new Label();
        _txtClinicalNotes = new TextBox();
        _txtRecommendation = new TextBox();
        _encounterHeaderPanel = new Panel();
        _lblEncounterInfo = new Label();
        _bottomPanel = new Panel();
        _closeButton = new Button();
        _refreshButton = new Button();
        _headerPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_mainSplit).BeginInit();
        _mainSplit.Panel1.SuspendLayout();
        _mainSplit.Panel2.SuspendLayout();
        _mainSplit.SuspendLayout();
        _timelineGroup.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_timelineGrid).BeginInit();
        _encounterPanel.SuspendLayout();
        _labGroup.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_labGrid).BeginInit();
        _rxGroup.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_rxGrid).BeginInit();
        _clinicalGroup.SuspendLayout();
        _clinicalTable.SuspendLayout();
        _encounterHeaderPanel.SuspendLayout();
        _bottomPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _headerPanel
        // 
        _headerPanel.BackColor = Color.FromArgb(28, 39, 51);
        _headerPanel.Controls.Add(_lblPatientDetails);
        _headerPanel.Controls.Add(_lblPatientTitle);
        _headerPanel.Dock = DockStyle.Top;
        _headerPanel.Location = new Point(0, 0);
        _headerPanel.Margin = new Padding(3, 4, 3, 4);
        _headerPanel.Name = "_headerPanel";
        _headerPanel.Padding = new Padding(23, 11, 23, 11);
        _headerPanel.Size = new Size(1561, 87);
        _headerPanel.TabIndex = 2;
        // 
        // _lblPatientDetails
        // 
        _lblPatientDetails.Dock = DockStyle.Bottom;
        _lblPatientDetails.Font = new Font("Segoe UI", 9.5F);
        _lblPatientDetails.ForeColor = Color.FromArgb(200, 214, 229);
        _lblPatientDetails.Location = new Point(23, 45);
        _lblPatientDetails.Name = "_lblPatientDetails";
        _lblPatientDetails.Size = new Size(1515, 31);
        _lblPatientDetails.TabIndex = 0;
        _lblPatientDetails.Text = "Loading patient profile...";
        // 
        // _lblPatientTitle
        // 
        _lblPatientTitle.Dock = DockStyle.Top;
        _lblPatientTitle.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
        _lblPatientTitle.ForeColor = Color.White;
        _lblPatientTitle.Location = new Point(23, 11);
        _lblPatientTitle.Name = "_lblPatientTitle";
        _lblPatientTitle.Size = new Size(1515, 31);
        _lblPatientTitle.TabIndex = 1;
        _lblPatientTitle.Text = "Patient Electronic Health Record";
        // 
        // _mainSplit
        // 
        _mainSplit.Dock = DockStyle.Fill;
        _mainSplit.FixedPanel = FixedPanel.Panel1;
        _mainSplit.Location = new Point(0, 87);
        _mainSplit.Margin = new Padding(3, 4, 3, 4);
        _mainSplit.Name = "_mainSplit";
        // 
        // _mainSplit.Panel1
        // 
        _mainSplit.Panel1.Controls.Add(_timelineGroup);
        // 
        // _mainSplit.Panel2
        // 
        _mainSplit.Panel2.Controls.Add(_encounterPanel);
        _mainSplit.Size = new Size(1561, 870);
        _mainSplit.SplitterDistance = 138;
        _mainSplit.SplitterWidth = 6;
        _mainSplit.TabIndex = 0;
        // 
        // _timelineGroup
        // 
        _timelineGroup.Controls.Add(_timelineGrid);
        _timelineGroup.Dock = DockStyle.Fill;
        _timelineGroup.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _timelineGroup.Location = new Point(0, 0);
        _timelineGroup.Margin = new Padding(3, 4, 3, 4);
        _timelineGroup.Name = "_timelineGroup";
        _timelineGroup.Padding = new Padding(11, 13, 11, 13);
        _timelineGroup.Size = new Size(138, 870);
        _timelineGroup.TabIndex = 0;
        _timelineGroup.TabStop = false;
        _timelineGroup.Text = "Encounter History Timeline";
        // 
        // _timelineGrid
        // 
        _timelineGrid.AllowUserToAddRows = false;
        _timelineGrid.AllowUserToDeleteRows = false;
        _timelineGrid.BackgroundColor = Color.White;
        _timelineGrid.BorderStyle = BorderStyle.Fixed3D;
        _timelineGrid.ColumnHeadersHeight = 29;
        _timelineGrid.Dock = DockStyle.Fill;
        _timelineGrid.Font = new Font("Segoe UI", 9F);
        _timelineGrid.Location = new Point(11, 35);
        _timelineGrid.Margin = new Padding(3, 4, 3, 4);
        _timelineGrid.MultiSelect = false;
        _timelineGrid.Name = "_timelineGrid";
        _timelineGrid.ReadOnly = true;
        _timelineGrid.RowHeadersVisible = false;
        _timelineGrid.RowHeadersWidth = 51;
        _timelineGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _timelineGrid.Size = new Size(116, 822);
        _timelineGrid.TabIndex = 0;
        // 
        // _encounterPanel
        // 
        _encounterPanel.AutoScroll = true;
        _encounterPanel.Controls.Add(_labGroup);
        _encounterPanel.Controls.Add(_rxGroup);
        _encounterPanel.Controls.Add(_clinicalGroup);
        _encounterPanel.Controls.Add(_encounterHeaderPanel);
        _encounterPanel.Dock = DockStyle.Fill;
        _encounterPanel.Location = new Point(0, 0);
        _encounterPanel.Margin = new Padding(3, 4, 3, 4);
        _encounterPanel.Name = "_encounterPanel";
        _encounterPanel.Padding = new Padding(14, 16, 14, 16);
        _encounterPanel.Size = new Size(1417, 870);
        _encounterPanel.TabIndex = 0;
        // 
        // _labGroup
        // 
        _labGroup.Controls.Add(_labGrid);
        _labGroup.Dock = DockStyle.Top;
        _labGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _labGroup.Location = new Point(14, 600);
        _labGroup.Margin = new Padding(3, 4, 3, 4);
        _labGroup.Name = "_labGroup";
        _labGroup.Padding = new Padding(9, 11, 9, 11);
        _labGroup.Size = new Size(1389, 233);
        _labGroup.TabIndex = 0;
        _labGroup.TabStop = false;
        _labGroup.Visible = false;
        _labGroup.Text = "\U0001f9ea  Laboratory Tests & Diagnostic Findings in THIS Encounter";
        // 
        // _labGrid
        // 
        _labGrid.AllowUserToAddRows = false;
        _labGrid.AllowUserToDeleteRows = false;
        _labGrid.BackgroundColor = Color.White;
        _labGrid.BorderStyle = BorderStyle.Fixed3D;
        _labGrid.ColumnHeadersHeight = 29;
        _labGrid.Dock = DockStyle.Fill;
        _labGrid.Font = new Font("Segoe UI", 9F);
        _labGrid.Location = new Point(9, 31);
        _labGrid.Margin = new Padding(3, 4, 3, 4);
        _labGrid.Name = "_labGrid";
        _labGrid.ReadOnly = true;
        _labGrid.RowHeadersVisible = false;
        _labGrid.RowHeadersWidth = 51;
        _labGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _labGrid.Size = new Size(1371, 191);
        _labGrid.TabIndex = 0;
        // 
        // _rxGroup
        // 
        _rxGroup.Controls.Add(_rxGrid);
        _rxGroup.Dock = DockStyle.Top;
        _rxGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _rxGroup.Location = new Point(14, 367);
        _rxGroup.Margin = new Padding(3, 4, 3, 4);
        _rxGroup.Name = "_rxGroup";
        _rxGroup.Padding = new Padding(9, 11, 9, 11);
        _rxGroup.Size = new Size(1389, 233);
        _rxGroup.TabIndex = 1;
        _rxGroup.TabStop = false;
        _rxGroup.Text = "💊  Prescriptions Issued in THIS Encounter (Rx Pad)";
        // 
        // _rxGrid
        // 
        _rxGrid.AllowUserToAddRows = false;
        _rxGrid.AllowUserToDeleteRows = false;
        _rxGrid.BackgroundColor = Color.White;
        _rxGrid.BorderStyle = BorderStyle.Fixed3D;
        _rxGrid.ColumnHeadersHeight = 29;
        _rxGrid.Dock = DockStyle.Fill;
        _rxGrid.Font = new Font("Segoe UI", 9F);
        _rxGrid.Location = new Point(9, 31);
        _rxGrid.Margin = new Padding(3, 4, 3, 4);
        _rxGrid.Name = "_rxGrid";
        _rxGrid.ReadOnly = true;
        _rxGrid.RowHeadersVisible = false;
        _rxGrid.RowHeadersWidth = 51;
        _rxGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _rxGrid.Size = new Size(1371, 191);
        _rxGrid.TabIndex = 0;
        // 
        // _clinicalGroup
        // 
        _clinicalGroup.Controls.Add(_clinicalTable);
        _clinicalGroup.Dock = DockStyle.Top;
        _clinicalGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _clinicalGroup.Location = new Point(14, 74);
        _clinicalGroup.Margin = new Padding(3, 4, 3, 4);
        _clinicalGroup.Name = "_clinicalGroup";
        _clinicalGroup.Padding = new Padding(9, 11, 9, 11);
        _clinicalGroup.Size = new Size(1389, 293);
        _clinicalGroup.TabIndex = 2;
        _clinicalGroup.TabStop = false;
        _clinicalGroup.Text = "\U0001fa7a  Clinical Diagnosis & Medical Findings";
        // 
        // _clinicalTable
        // 
        _clinicalTable.ColumnCount = 2;
        _clinicalTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _clinicalTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        _clinicalTable.Controls.Add(_lblChiefComplaint, 0, 0);
        _clinicalTable.Controls.Add(_lblDiagnosis, 1, 0);
        _clinicalTable.Controls.Add(_txtChiefComplaint, 0, 1);
        _clinicalTable.Controls.Add(_txtDiagnosis, 1, 1);
        _clinicalTable.Controls.Add(_lblClinicalNotes, 0, 2);
        _clinicalTable.Controls.Add(_lblRecommendation, 1, 2);
        _clinicalTable.Controls.Add(_txtClinicalNotes, 0, 3);
        _clinicalTable.Controls.Add(_txtRecommendation, 1, 3);
        _clinicalTable.Dock = DockStyle.Fill;
        _clinicalTable.Location = new Point(9, 31);
        _clinicalTable.Margin = new Padding(3, 4, 3, 4);
        _clinicalTable.Name = "_clinicalTable";
        _clinicalTable.RowCount = 4;
        _clinicalTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
        _clinicalTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _clinicalTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
        _clinicalTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _clinicalTable.Size = new Size(1371, 251);
        _clinicalTable.TabIndex = 0;
        // 
        // _lblChiefComplaint
        // 
        _lblChiefComplaint.Dock = DockStyle.Fill;
        _lblChiefComplaint.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        _lblChiefComplaint.Location = new Point(3, 0);
        _lblChiefComplaint.Name = "_lblChiefComplaint";
        _lblChiefComplaint.Size = new Size(679, 27);
        _lblChiefComplaint.TabIndex = 0;
        _lblChiefComplaint.Text = "Chief Complaint";
        // 
        // _lblDiagnosis
        // 
        _lblDiagnosis.Dock = DockStyle.Fill;
        _lblDiagnosis.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        _lblDiagnosis.Location = new Point(688, 0);
        _lblDiagnosis.Name = "_lblDiagnosis";
        _lblDiagnosis.Size = new Size(680, 27);
        _lblDiagnosis.TabIndex = 1;
        _lblDiagnosis.Text = "Diagnosis";
        // 
        // _txtChiefComplaint
        // 
        _txtChiefComplaint.BackColor = Color.White;
        _txtChiefComplaint.Dock = DockStyle.Fill;
        _txtChiefComplaint.Font = new Font("Segoe UI", 9F);
        _txtChiefComplaint.Location = new Point(3, 31);
        _txtChiefComplaint.Margin = new Padding(3, 4, 3, 4);
        _txtChiefComplaint.Name = "_txtChiefComplaint";
        _txtChiefComplaint.ReadOnly = true;
        _txtChiefComplaint.Size = new Size(679, 27);
        _txtChiefComplaint.TabIndex = 2;
        // 
        // _txtDiagnosis
        // 
        _txtDiagnosis.BackColor = Color.White;
        _txtDiagnosis.Dock = DockStyle.Fill;
        _txtDiagnosis.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _txtDiagnosis.ForeColor = Color.FromArgb(192, 57, 43);
        _txtDiagnosis.Location = new Point(688, 31);
        _txtDiagnosis.Margin = new Padding(3, 4, 3, 4);
        _txtDiagnosis.Name = "_txtDiagnosis";
        _txtDiagnosis.ReadOnly = true;
        _txtDiagnosis.Size = new Size(680, 27);
        _txtDiagnosis.TabIndex = 3;
        // 
        // _lblClinicalNotes
        // 
        _lblClinicalNotes.Dock = DockStyle.Fill;
        _lblClinicalNotes.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        _lblClinicalNotes.Location = new Point(3, 67);
        _lblClinicalNotes.Name = "_lblClinicalNotes";
        _lblClinicalNotes.Size = new Size(679, 27);
        _lblClinicalNotes.TabIndex = 4;
        _lblClinicalNotes.Text = "Clinical Examination Notes";
        // 
        // _lblRecommendation
        // 
        _lblRecommendation.Dock = DockStyle.Fill;
        _lblRecommendation.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        _lblRecommendation.Location = new Point(688, 67);
        _lblRecommendation.Name = "_lblRecommendation";
        _lblRecommendation.Size = new Size(680, 27);
        _lblRecommendation.TabIndex = 5;
        _lblRecommendation.Text = "Doctor Recommendations & Orders";
        // 
        // _txtClinicalNotes
        // 
        _txtClinicalNotes.BackColor = Color.White;
        _txtClinicalNotes.Dock = DockStyle.Fill;
        _txtClinicalNotes.Font = new Font("Segoe UI", 9F);
        _txtClinicalNotes.Location = new Point(3, 98);
        _txtClinicalNotes.Margin = new Padding(3, 4, 3, 4);
        _txtClinicalNotes.Multiline = true;
        _txtClinicalNotes.Name = "_txtClinicalNotes";
        _txtClinicalNotes.ReadOnly = true;
        _txtClinicalNotes.ScrollBars = ScrollBars.Vertical;
        _txtClinicalNotes.Size = new Size(679, 149);
        _txtClinicalNotes.TabIndex = 6;
        // 
        // _txtRecommendation
        // 
        _txtRecommendation.BackColor = Color.White;
        _txtRecommendation.Dock = DockStyle.Fill;
        _txtRecommendation.Font = new Font("Segoe UI", 9F);
        _txtRecommendation.Location = new Point(688, 98);
        _txtRecommendation.Margin = new Padding(3, 4, 3, 4);
        _txtRecommendation.Multiline = true;
        _txtRecommendation.Name = "_txtRecommendation";
        _txtRecommendation.ReadOnly = true;
        _txtRecommendation.ScrollBars = ScrollBars.Vertical;
        _txtRecommendation.Size = new Size(680, 149);
        _txtRecommendation.TabIndex = 7;
        // 
        // _encounterHeaderPanel
        // 
        _encounterHeaderPanel.BackColor = Color.FromArgb(235, 242, 250);
        _encounterHeaderPanel.BorderStyle = BorderStyle.FixedSingle;
        _encounterHeaderPanel.Controls.Add(_lblEncounterInfo);
        _encounterHeaderPanel.Dock = DockStyle.Top;
        _encounterHeaderPanel.Location = new Point(14, 16);
        _encounterHeaderPanel.Margin = new Padding(3, 4, 3, 4);
        _encounterHeaderPanel.Name = "_encounterHeaderPanel";
        _encounterHeaderPanel.Padding = new Padding(14, 0, 14, 0);
        _encounterHeaderPanel.Size = new Size(1389, 58);
        _encounterHeaderPanel.TabIndex = 3;
        // 
        // _lblEncounterInfo
        // 
        _lblEncounterInfo.Dock = DockStyle.Fill;
        _lblEncounterInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _lblEncounterInfo.ForeColor = Color.FromArgb(28, 39, 51);
        _lblEncounterInfo.Location = new Point(14, 0);
        _lblEncounterInfo.Name = "_lblEncounterInfo";
        _lblEncounterInfo.Size = new Size(1359, 56);
        _lblEncounterInfo.TabIndex = 0;
        _lblEncounterInfo.Text = "Select a visit from the left timeline to view its clinical record.";
        _lblEncounterInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _bottomPanel
        // 
        _bottomPanel.Controls.Add(_closeButton);
        _bottomPanel.Controls.Add(_refreshButton);
        _bottomPanel.Dock = DockStyle.Bottom;
        _bottomPanel.Location = new Point(0, 957);
        _bottomPanel.Margin = new Padding(3, 4, 3, 4);
        _bottomPanel.Name = "_bottomPanel";
        _bottomPanel.Padding = new Padding(17, 8, 17, 8);
        _bottomPanel.Size = new Size(1561, 67);
        _bottomPanel.TabIndex = 1;
        // 
        // _closeButton
        // 
        _closeButton.Cursor = Cursors.Hand;
        _closeButton.Dock = DockStyle.Right;
        _closeButton.Location = new Point(1418, 8);
        _closeButton.Margin = new Padding(3, 4, 3, 4);
        _closeButton.Name = "_closeButton";
        _closeButton.Size = new Size(126, 51);
        _closeButton.TabIndex = 0;
        _closeButton.Text = "Close";
        // 
        // _refreshButton
        // 
        _refreshButton.Cursor = Cursors.Hand;
        _refreshButton.Dock = DockStyle.Left;
        _refreshButton.Location = new Point(17, 8);
        _refreshButton.Margin = new Padding(3, 4, 3, 4);
        _refreshButton.Name = "_refreshButton";
        _refreshButton.Size = new Size(149, 51);
        _refreshButton.TabIndex = 1;
        _refreshButton.Text = "🔄  Refresh";
        // 
        // PatientHistoryForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 250);
        ClientSize = new Size(1561, 1024);
        Controls.Add(_mainSplit);
        Controls.Add(_bottomPanel);
        Controls.Add(_headerPanel);
        Margin = new Padding(3, 4, 3, 4);
        MinimumSize = new Size(1197, 891);
        Name = "PatientHistoryForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ClinicFlow - Patient Electronic Health Record (EHR Timeline)";
        WindowState = FormWindowState.Maximized;
        _headerPanel.ResumeLayout(false);
        _mainSplit.Panel1.ResumeLayout(false);
        _mainSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_mainSplit).EndInit();
        _mainSplit.ResumeLayout(false);
        _timelineGroup.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_timelineGrid).EndInit();
        _encounterPanel.ResumeLayout(false);
        _labGroup.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_labGrid).EndInit();
        _rxGroup.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_rxGrid).EndInit();
        _clinicalGroup.ResumeLayout(false);
        _clinicalTable.ResumeLayout(false);
        _clinicalTable.PerformLayout();
        _encounterHeaderPanel.ResumeLayout(false);
        _bottomPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}