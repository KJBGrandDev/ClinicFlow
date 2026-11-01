#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class DoctorConsultationForm
    {
        private System.ComponentModel.IContainer? components = null;

        // Controls
        private Panel _leftPanel = null!;
        private Panel _activePatientPanel = null!;
        private Label _lblCurrentPatientTitle = null!;
        private Label _lblActivePatientName = null!;
        private Label _lblActivePatientMeta = null!;
        private Label _lblActiveConcern = null!;
        private TableLayoutPanel _topActionFlow = null!;
        private Button _btnDefer = null!;
        private Button _btnRefreshQueue = null!;
        private Label _lblWaitingQueueHeader = null!;
        private DataGridView _queueGrid = null!;
        private Button _btnEmergency = null!;

        private Panel _rightPanel = null!;
        private Panel _patientBanner = null!;
        private Label _lblBannerPatientName = null!;
        private Label _lblBannerPatientDetails = null!;
        private Button _viewPatientHistoryButton = null!;

        private Panel _workspaceScrollPanel = null!;
        private Panel _clinicalCard = null!;
        private Label _lblClinicalCardTitle = null!;
        private TableLayoutPanel _clinicalTable = null!;
        private Label _lblChief = null!;
        private TextBox _chiefComplaintTextBox = null!;
        private Label _lblNotes = null!;
        private TextBox _clinicalNotesTextBox = null!;
        private Label _lblDiag = null!;
        private TextBox _diagnosisTextBox = null!;
        private Label _lblRecs = null!;
        private TextBox _recommendationTextBox = null!;

        private Panel _rxPanel = null!;
        private Label _lblRxTitle = null!;
        private TableLayoutPanel _rxInputsTable = null!;
        private Label _lblMed = null!;
        private ComboBox _medicineComboBox = null!;
        private Label _lblQty = null!;
        private NumericUpDown _quantityNumeric = null!;
        private Label _lblDosage = null!;
        private TextBox _instructionsTextBox = null!;
        private Label _lblFulfill = null!;
        private ComboBox _fulfillmentComboBox = null!;
        private FlowLayoutPanel _rxButtonFlow = null!;
        private Button _addRxButton = null!;
        private Button _updateRxButton = null!;
        private Button _removeRxButton = null!;
        private Button _clearRxInputsButton = null!;
        private Label _lblStockBadge = null!;
        private DataGridView _rxGrid = null!;

        private Panel _bottomPanel = null!;
        private Button _completeConsultationButton = null!;
        private Button _clearButton = null!;

        // Dummy fields to satisfy legacy references if any
        private TableLayoutPanel _workspaceTable = null!;
        private TabControl _ordersTabControl = null!;
        private TabPage _rxTab = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._leftPanel = new Panel();
            this._activePatientPanel = new Panel();
            this._lblCurrentPatientTitle = new Label();
            this._lblActivePatientName = new Label();
            this._lblActivePatientMeta = new Label();
            this._lblActiveConcern = new Label();
            this._topActionFlow = new TableLayoutPanel();
            this._btnDefer = new Button();
            this._btnRefreshQueue = new Button();
            this._lblWaitingQueueHeader = new Label();
            this._queueGrid = new DataGridView();
            this._btnEmergency = new Button();

            this._rightPanel = new Panel();
            this._patientBanner = new Panel();
            this._lblBannerPatientName = new Label();
            this._lblBannerPatientDetails = new Label();
            this._viewPatientHistoryButton = new Button();

            this._workspaceScrollPanel = new Panel();
            this._clinicalCard = new Panel();
            this._lblClinicalCardTitle = new Label();
            this._clinicalTable = new TableLayoutPanel();
            this._lblChief = new Label();
            this._chiefComplaintTextBox = new TextBox();
            this._lblNotes = new Label();
            this._clinicalNotesTextBox = new TextBox();
            this._lblDiag = new Label();
            this._diagnosisTextBox = new TextBox();
            this._lblRecs = new Label();
            this._recommendationTextBox = new TextBox();

            this._rxPanel = new Panel();
            this._lblRxTitle = new Label();
            this._rxInputsTable = new TableLayoutPanel();
            this._lblMed = new Label();
            this._medicineComboBox = new ComboBox();
            this._lblQty = new Label();
            this._quantityNumeric = new NumericUpDown();
            this._lblDosage = new Label();
            this._instructionsTextBox = new TextBox();
            this._lblFulfill = new Label();
            this._fulfillmentComboBox = new ComboBox();
            this._rxButtonFlow = new FlowLayoutPanel();
            this._addRxButton = new Button();
            this._updateRxButton = new Button();
            this._removeRxButton = new Button();
            this._clearRxInputsButton = new Button();
            this._lblStockBadge = new Label();
            this._rxGrid = new DataGridView();

            this._bottomPanel = new Panel();
            this._completeConsultationButton = new Button();
            this._clearButton = new Button();

            this._workspaceTable = new TableLayoutPanel();
            this._ordersTabControl = new TabControl();
            this._rxTab = new TabPage();

            this.SuspendLayout();

            // =========================================================================
            // LEFT PANEL: WAITING QUEUE & ACTIVE PATIENT
            // =========================================================================
            this._leftPanel.Dock = DockStyle.Left;
            this._leftPanel.Width = 370;
            this._leftPanel.BackColor = Color.White;
            this._leftPanel.Padding = new Padding(16);
            this._leftPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), this._leftPanel.Width - 1, 0, this._leftPanel.Width - 1, this._leftPanel.Height);

            // Active Patient Card (Top)
            this._activePatientPanel.Dock = DockStyle.Top;
            this._activePatientPanel.Height = 160;
            this._activePatientPanel.BackColor = Color.FromArgb(240, 253, 244);
            this._activePatientPanel.Padding = new Padding(14);
            this._activePatientPanel.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, this._activePatientPanel.ClientRectangle, Color.FromArgb(187, 247, 208), ButtonBorderStyle.Solid);

            this._lblCurrentPatientTitle.Text = "ACTIVE CONSULTATION";
            this._lblCurrentPatientTitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this._lblCurrentPatientTitle.ForeColor = Color.FromArgb(4, 120, 87);
            this._lblCurrentPatientTitle.Dock = DockStyle.Top;
            this._lblCurrentPatientTitle.Height = 18;

            this._lblActivePatientName.Text = "No Patient Selected";
            this._lblActivePatientName.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
            this._lblActivePatientName.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblActivePatientName.Dock = DockStyle.Top;
            this._lblActivePatientName.Height = 26;

            this._lblActivePatientMeta.Text = "Select a patient from queue below";
            this._lblActivePatientMeta.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this._lblActivePatientMeta.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblActivePatientMeta.Dock = DockStyle.Top;
            this._lblActivePatientMeta.Height = 18;

            this._lblActiveConcern.Text = "";
            this._lblActiveConcern.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            this._lblActiveConcern.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblActiveConcern.Dock = DockStyle.Top;
            this._lblActiveConcern.Height = 18;

            this._topActionFlow.Dock = DockStyle.Bottom;
            this._topActionFlow.Height = 34;
            this._topActionFlow.ColumnCount = 2;
            this._topActionFlow.RowCount = 1;
            this._topActionFlow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._topActionFlow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._topActionFlow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this._topActionFlow.Margin = Padding.Empty;
            this._topActionFlow.Padding = Padding.Empty;

            this._btnRefreshQueue.Text = "Refresh Queue";
            this._btnRefreshQueue.Dock = DockStyle.Fill;
            this._btnRefreshQueue.BackColor = Color.White;
            this._btnRefreshQueue.FlatStyle = FlatStyle.Flat;
            this._btnRefreshQueue.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._btnRefreshQueue.ForeColor = Color.FromArgb(51, 65, 85);
            this._btnRefreshQueue.Cursor = Cursors.Hand;
            this._btnRefreshQueue.Margin = new Padding(0, 0, 4, 0);
            this._btnRefreshQueue.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            this._btnDefer.Text = "Skip Patient";
            this._btnDefer.Dock = DockStyle.Fill;
            this._btnDefer.BackColor = Color.White;
            this._btnDefer.FlatStyle = FlatStyle.Flat;
            this._btnDefer.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._btnDefer.ForeColor = Color.FromArgb(51, 65, 85);
            this._btnDefer.Cursor = Cursors.Hand;
            this._btnDefer.Margin = new Padding(4, 0, 0, 0);
            this._btnDefer.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            this._topActionFlow.Controls.Add(this._btnRefreshQueue, 0, 0);
            this._topActionFlow.Controls.Add(this._btnDefer, 1, 0);

            this._activePatientPanel.Controls.Add(this._topActionFlow);
            this._activePatientPanel.Controls.Add(this._lblActiveConcern);
            this._activePatientPanel.Controls.Add(this._lblActivePatientMeta);
            this._activePatientPanel.Controls.Add(this._lblActivePatientName);
            this._activePatientPanel.Controls.Add(this._lblCurrentPatientTitle);

            // Queue List Header
            this._lblWaitingQueueHeader.Text = "WAITING PATIENT QUEUE";
            this._lblWaitingQueueHeader.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblWaitingQueueHeader.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblWaitingQueueHeader.Dock = DockStyle.Top;
            this._lblWaitingQueueHeader.Height = 36;
            this._lblWaitingQueueHeader.TextAlign = ContentAlignment.BottomLeft;
            this._lblWaitingQueueHeader.Padding = new Padding(2, 0, 0, 6);

            // Queue Table
            this._queueGrid.Dock = DockStyle.Fill;
            this._queueGrid.BackgroundColor = Color.White;
            this._queueGrid.BorderStyle = BorderStyle.None;
            this._queueGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            this._queueGrid.GridColor = Color.FromArgb(241, 245, 249);
            this._queueGrid.RowHeadersVisible = false;
            this._queueGrid.RowTemplate.Height = 38;
            this._queueGrid.AllowUserToAddRows = false;
            this._queueGrid.AllowUserToDeleteRows = false;
            this._queueGrid.ReadOnly = true;
            this._queueGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._queueGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._queueGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            this._queueGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            // Emergency Button (Bottom)
            this._btnEmergency.Text = "Emergency Leave / Pause Queue";
            this._btnEmergency.Dock = DockStyle.Bottom;
            this._btnEmergency.Height = 40;
            this._btnEmergency.BackColor = Color.FromArgb(254, 242, 242);
            this._btnEmergency.ForeColor = Color.FromArgb(185, 28, 28);
            this._btnEmergency.FlatStyle = FlatStyle.Flat;
            this._btnEmergency.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnEmergency.Cursor = Cursors.Hand;
            this._btnEmergency.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);

            var pnlQueueWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 10) };
            pnlQueueWrapper.Controls.Add(this._queueGrid);

            // Add in proper docking order: Top & Bottom first, Fill LAST!
            this._leftPanel.Controls.Add(pnlQueueWrapper);
            this._leftPanel.Controls.Add(this._btnEmergency);
            this._leftPanel.Controls.Add(this._lblWaitingQueueHeader);
            this._leftPanel.Controls.Add(this._activePatientPanel);

            // =========================================================================
            // RIGHT PANEL: WORKSPACE (BANNER, CLINICAL NOTES, RX, FOOTER)
            // =========================================================================
            this._rightPanel.Dock = DockStyle.Fill;
            this._rightPanel.BackColor = Color.FromArgb(248, 250, 252);

            // -- Top Patient Banner --
            this._patientBanner.Dock = DockStyle.Top;
            this._patientBanner.Height = 88;
            this._patientBanner.BackColor = Color.White;
            this._patientBanner.Padding = new Padding(24, 14, 24, 14);
            this._patientBanner.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, this._patientBanner.Height - 1, this._patientBanner.Width, this._patientBanner.Height - 1);

            var pnlBannerLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 600,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            this._lblBannerPatientName.Text = "No Consultation Active";
            this._lblBannerPatientName.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this._lblBannerPatientName.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblBannerPatientName.AutoSize = true;
            this._lblBannerPatientName.Margin = new Padding(0, 0, 0, 4);

            this._lblBannerPatientDetails.Text = "Select a patient from the waiting queue to begin encounter.";
            this._lblBannerPatientDetails.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this._lblBannerPatientDetails.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblBannerPatientDetails.AutoSize = true;
            this._lblBannerPatientDetails.Margin = new Padding(0);

            pnlBannerLeft.Controls.Add(this._lblBannerPatientName);
            pnlBannerLeft.Controls.Add(this._lblBannerPatientDetails);

            this._viewPatientHistoryButton.Text = "View Patient EHR";
            this._viewPatientHistoryButton.Dock = DockStyle.Right;
            this._viewPatientHistoryButton.Width = 180;
            this._viewPatientHistoryButton.Height = 42;
            this._viewPatientHistoryButton.BackColor = Color.White;
            this._viewPatientHistoryButton.FlatStyle = FlatStyle.Flat;
            this._viewPatientHistoryButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this._viewPatientHistoryButton.ForeColor = Color.FromArgb(30, 41, 59);
            this._viewPatientHistoryButton.Cursor = Cursors.Hand;
            this._viewPatientHistoryButton.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            this._patientBanner.Controls.Add(pnlBannerLeft);
            this._patientBanner.Controls.Add(this._viewPatientHistoryButton);

            // -- Bottom Action Footer Bar --
            this._bottomPanel.Dock = DockStyle.Bottom;
            this._bottomPanel.Height = 68;
            this._bottomPanel.BackColor = Color.White;
            this._bottomPanel.Padding = new Padding(24, 12, 24, 12);
            this._bottomPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 0, this._bottomPanel.Width, 0);

            this._clearButton.Text = "Clear Workspace";
            this._clearButton.Dock = DockStyle.Left;
            this._clearButton.Width = 140;
            this._clearButton.Height = 44;
            this._clearButton.BackColor = Color.White;
            this._clearButton.FlatStyle = FlatStyle.Flat;
            this._clearButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this._clearButton.ForeColor = Color.FromArgb(71, 85, 105);
            this._clearButton.Cursor = Cursors.Hand;
            this._clearButton.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            this._completeConsultationButton.Text = "Complete Consultation";
            this._completeConsultationButton.Dock = DockStyle.Right;
            this._completeConsultationButton.Width = 280;
            this._completeConsultationButton.Height = 44;
            this._completeConsultationButton.BackColor = Color.FromArgb(16, 185, 129);
            this._completeConsultationButton.ForeColor = Color.White;
            this._completeConsultationButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._completeConsultationButton.FlatStyle = FlatStyle.Flat;
            this._completeConsultationButton.FlatAppearance.BorderSize = 0;
            this._completeConsultationButton.Cursor = Cursors.Hand;

            this._bottomPanel.Controls.Add(this._clearButton);
            this._bottomPanel.Controls.Add(this._completeConsultationButton);

            // -- Center Workspace Scroll Panel --
            this._workspaceScrollPanel.Dock = DockStyle.Fill;
            this._workspaceScrollPanel.AutoScroll = true;
            this._workspaceScrollPanel.BackColor = Color.FromArgb(248, 250, 252);
            this._workspaceScrollPanel.Padding = new Padding(24, 16, 24, 16);

            // SECTION 1: CLINICAL DOCUMENTATION CARD
            this._clinicalCard.Dock = DockStyle.Top;
            this._clinicalCard.Height = 420;
            this._clinicalCard.BackColor = Color.White;
            this._clinicalCard.Padding = new Padding(20);
            this._clinicalCard.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, this._clinicalCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            this._lblClinicalCardTitle.Text = "CLINICAL DOCUMENTATION & NOTES";
            this._lblClinicalCardTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblClinicalCardTitle.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblClinicalCardTitle.Dock = DockStyle.Top;
            this._lblClinicalCardTitle.Height = 26;
            this._lblClinicalCardTitle.UseMnemonic = false;

            this._clinicalTable.Dock = DockStyle.Fill;
            this._clinicalTable.ColumnCount = 2;
            this._clinicalTable.RowCount = 2;
            this._clinicalTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._clinicalTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this._clinicalTable.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            this._clinicalTable.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));

            Font headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            Font textFont = new Font("Segoe UI", 9.5F);

            Panel MakeClinicalBox(string title, TextBox tb, ref Label lbl, bool isRequired)
            {
                Panel p = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 2, 10, 8) };
                lbl = new Label
                {
                    Text = isRequired ? title + " *" : title,
                    Dock = DockStyle.Top,
                    Font = headerFont,
                    ForeColor = isRequired ? Color.FromArgb(15, 23, 42) : Color.FromArgb(71, 85, 105),
                    Height = 22,
                    UseMnemonic = false
                };
                tb.Dock = DockStyle.Fill;
                tb.Multiline = true;
                tb.ScrollBars = ScrollBars.Vertical;
                tb.Font = textFont;
                tb.BorderStyle = BorderStyle.FixedSingle;
                p.Controls.Add(tb);
                p.Controls.Add(lbl);
                return p;
            }

            this._clinicalTable.Controls.Add(MakeClinicalBox("Chief Complaint", this._chiefComplaintTextBox, ref this._lblChief, true), 0, 0);
            this._clinicalTable.Controls.Add(MakeClinicalBox("Diagnosis & Clinical Impression", this._diagnosisTextBox, ref this._lblDiag, true), 1, 0);
            this._clinicalTable.Controls.Add(MakeClinicalBox("Clinical Examination Notes & History", this._clinicalNotesTextBox, ref this._lblNotes, false), 0, 1);
            this._clinicalTable.Controls.Add(MakeClinicalBox("Orders & Recommendations", this._recommendationTextBox, ref this._lblRecs, false), 1, 1);

            this._clinicalCard.Controls.Add(this._clinicalTable);
            this._clinicalCard.Controls.Add(this._lblClinicalCardTitle);

            // Spacing panel between Clinical Card and Rx Card
            var spacerBetweenCards = new Panel { Dock = DockStyle.Top, Height = 16, BackColor = Color.Transparent };

            // SECTION 2: PRESCRIPTION ORDERS CARD
            this._rxPanel.Dock = DockStyle.Top;
            this._rxPanel.Height = 490;
            this._rxPanel.BackColor = Color.White;
            this._rxPanel.Padding = new Padding(20);
            this._rxPanel.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, this._rxPanel.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            this._lblRxTitle.Text = "PRESCRIPTIONS & MEDICATION ORDERS (Rx PAD)";
            this._lblRxTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblRxTitle.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblRxTitle.Dock = DockStyle.Top;
            this._lblRxTitle.Height = 26;
            this._lblRxTitle.UseMnemonic = false;

            // Input Grid
            this._rxInputsTable.Dock = DockStyle.Top;
            this._rxInputsTable.Height = 64;
            this._rxInputsTable.ColumnCount = 4;
            this._rxInputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            this._rxInputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            this._rxInputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            this._rxInputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            this._rxInputsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            this._rxInputsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

            this._lblMed.Text = "Medication Type / Category *"; this._lblMed.Font = headerFont; this._lblMed.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblQty.Text = "Quantity *"; this._lblQty.Font = headerFont; this._lblQty.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblDosage.Text = "Dosage & Instructions *"; this._lblDosage.Font = headerFont; this._lblDosage.ForeColor = Color.FromArgb(71, 85, 105); this._lblDosage.UseMnemonic = false;
            this._lblFulfill.Text = "Dispense Location"; this._lblFulfill.Font = headerFont; this._lblFulfill.ForeColor = Color.FromArgb(71, 85, 105); this._lblFulfill.UseMnemonic = false;

            this._medicineComboBox.Dock = DockStyle.Fill; this._medicineComboBox.Font = textFont; this._medicineComboBox.DropDownStyle = ComboBoxStyle.DropDownList; this._medicineComboBox.DropDownWidth = 460;
            this._quantityNumeric.Dock = DockStyle.Fill; this._quantityNumeric.Font = textFont; this._quantityNumeric.Minimum = 1; this._quantityNumeric.Maximum = 999; this._quantityNumeric.Value = 1;
            this._instructionsTextBox.Dock = DockStyle.Fill; this._instructionsTextBox.Font = textFont;
            this._fulfillmentComboBox.Dock = DockStyle.Fill; this._fulfillmentComboBox.Font = textFont; this._fulfillmentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._fulfillmentComboBox.Items.AddRange(new object[] { "Clinic Pharmacy", "External Pharmacy" });
            this._fulfillmentComboBox.SelectedIndex = 0;

            this._rxInputsTable.Controls.Add(this._lblMed, 0, 0);
            this._rxInputsTable.Controls.Add(this._lblQty, 1, 0);
            this._rxInputsTable.Controls.Add(this._lblDosage, 2, 0);
            this._rxInputsTable.Controls.Add(this._lblFulfill, 3, 0);
            this._rxInputsTable.Controls.Add(this._medicineComboBox, 0, 1);
            this._rxInputsTable.Controls.Add(this._quantityNumeric, 1, 1);
            this._rxInputsTable.Controls.Add(this._instructionsTextBox, 2, 1);
            this._rxInputsTable.Controls.Add(this._fulfillmentComboBox, 3, 1);

            // Rx Buttons
            this._rxButtonFlow.Dock = DockStyle.Top;
            this._rxButtonFlow.Height = 44;
            this._rxButtonFlow.Padding = new Padding(0, 6, 0, 0);
            this._rxButtonFlow.FlowDirection = FlowDirection.LeftToRight;

            this._addRxButton.Text = "+ Add to Rx";
            this._addRxButton.Size = new Size(115, 34);
            this._addRxButton.BackColor = Color.FromArgb(16, 185, 129);
            this._addRxButton.ForeColor = Color.White;
            this._addRxButton.FlatStyle = FlatStyle.Flat;
            this._addRxButton.FlatAppearance.BorderSize = 0;
            this._addRxButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._addRxButton.Cursor = Cursors.Hand;
            this._addRxButton.Margin = new Padding(0, 0, 8, 0);

            this._updateRxButton.Text = "Update";
            this._updateRxButton.Size = new Size(85, 34);
            this._updateRxButton.BackColor = Color.White;
            this._updateRxButton.FlatStyle = FlatStyle.Flat;
            this._updateRxButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._updateRxButton.Cursor = Cursors.Hand;
            this._updateRxButton.Margin = new Padding(0, 0, 8, 0);

            this._removeRxButton.Text = "🗑";
            this._removeRxButton.Size = new Size(44, 34);
            this._removeRxButton.BackColor = Color.White;
            this._removeRxButton.ForeColor = Color.FromArgb(220, 38, 38);
            this._removeRxButton.FlatStyle = FlatStyle.Flat;
            this._removeRxButton.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this._removeRxButton.Cursor = Cursors.Hand;
            this._removeRxButton.Margin = new Padding(0, 0, 8, 0);

            this._clearRxInputsButton.Text = "Reset to Default";
            this._clearRxInputsButton.Size = new Size(120, 34);
            this._clearRxInputsButton.BackColor = Color.White;
            this._clearRxInputsButton.FlatStyle = FlatStyle.Flat;
            this._clearRxInputsButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this._clearRxInputsButton.Cursor = Cursors.Hand;

            this._lblStockBadge.AutoSize = true;
            this._lblStockBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblStockBadge.Margin = new Padding(12, 8, 0, 0);
            this._lblStockBadge.Text = "🟢 Stock: Ready";

            this._rxButtonFlow.Controls.AddRange(new Control[] { this._addRxButton, this._updateRxButton, this._removeRxButton, this._clearRxInputsButton, this._lblStockBadge });

            // Rx Grid
            this._rxGrid.Dock = DockStyle.Fill;
            this._rxGrid.BackgroundColor = Color.White;
            this._rxGrid.BorderStyle = BorderStyle.None;
            this._rxGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            this._rxGrid.GridColor = Color.FromArgb(241, 245, 249);
            this._rxGrid.RowHeadersVisible = false;
            this._rxGrid.RowTemplate.Height = 36;
            this._rxGrid.AllowUserToAddRows = false;
            this._rxGrid.AllowUserToDeleteRows = false;
            this._rxGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._rxGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this._rxGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            this._rxGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            var pnlRxGridWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            pnlRxGridWrapper.Controls.Add(this._rxGrid);

            this._rxPanel.Controls.Add(pnlRxGridWrapper);
            this._rxPanel.Controls.Add(this._rxButtonFlow);
            this._rxPanel.Controls.Add(this._rxInputsTable);
            this._rxPanel.Controls.Add(this._lblRxTitle);

            // Add cards to workspace scroll panel
            this._workspaceScrollPanel.Controls.Add(this._rxPanel);
            this._workspaceScrollPanel.Controls.Add(spacerBetweenCards);
            this._workspaceScrollPanel.Controls.Add(this._clinicalCard);

            // Add to right panel in correct docking order: Top & Bottom first, Fill LAST!
            this._rightPanel.Controls.Add(this._workspaceScrollPanel);
            this._rightPanel.Controls.Add(this._bottomPanel);
            this._rightPanel.Controls.Add(this._patientBanner);

            // Add left & right panels to form: Left first, Fill LAST!
            this.Controls.Add(this._rightPanel);
            this.Controls.Add(this._leftPanel);

            this.ClientSize = new Size(1280, 800);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(248, 250, 252);

            this.ResumeLayout(false);
        }
    }
}

