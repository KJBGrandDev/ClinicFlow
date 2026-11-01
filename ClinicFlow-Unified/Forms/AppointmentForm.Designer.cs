#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class AppointmentForm
    {
        private System.ComponentModel.IContainer? components = null;

        // Left Panel
        private Panel _leftPanel = null!;
        private Label _lblScheduleTitle = null!;
        private Label _lblSelectPatient = null!;
        private TableLayoutPanel _patientSearchTable = null!;
        private TextBox _patientSearchTextBox = null!;
        private Button _btnFindPatient = null!;
        private Label _lblSearchFeedback = null!;
        private ComboBox _patientComboBox = null!;
        private Panel _pnlPatientWrapper = null!;
        private Panel _pnlConflictAlert = null!;
        private Label _lblConflictTitle = null!;
        private Label _lblConflictMessage = null!;
        private Label _lblDoctor = null!;
        private ComboBox _doctorComboBox = null!;
        private Label _lblAppointmentDate = null!;
        private DateTimePicker _appointmentDatePicker = null!;
        private Label _lblTimeSlot = null!;
        private ComboBox _timeSlotComboBox = null!;
        private Label _lblReason = null!;
        private TextBox _reasonTextBox = null!;
        private Button _btnBookAppointment = null!;
        private Button _btnClearBooking = null!;

        // Right Panel
        private Panel _rightPanel = null!;
        private Panel _headerPanel = null!;
        private Label _lblMainTitle = null!;
        private Label _lblSubtitle = null!;
        private FlowLayoutPanel _flpKpiStats = null!;
        private Label _lblKpiTotal = null!;
        private Label _lblKpiInService = null!;
        private Label _lblKpiCalled = null!;
        private Label _lblKpiWaiting = null!;

        private Label _lblRoomSectionTitle = null!;
        private FlowLayoutPanel _flpRoomStations = null!;
        private FlowLayoutPanel _flpDoctorFilter = null!;
        private Button _btnFilterAll = null!;
        private Label _lblLobbyQueueHeader = null!;
        private Panel _lobbyActionBar = null!;
        private Button _btnCheckIn = null!;
        private Button _btnCallPatient = null!;
        private Button _btnSkipNoShow = null!;
        private Button _btnRefreshQueue = null!;
        private Panel _pnlEmergencyAlertBanner = null!;
        private Label _lblEmergencyAlertText = null!;
        private Button _btnOpenEmergencyTriage = null!;
        private DataGridView _lobbyQueueGrid = null!;

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
            this._lblScheduleTitle = new Label();
            this._lblSelectPatient = new Label();
            this._patientSearchTable = new TableLayoutPanel();
            this._patientSearchTextBox = new TextBox();
            this._btnFindPatient = new Button();
            this._lblSearchFeedback = new Label();
            this._patientComboBox = new ComboBox();
            this._pnlPatientWrapper = new Panel();
            this._pnlConflictAlert = new Panel();
            this._lblConflictTitle = new Label();
            this._lblConflictMessage = new Label();
            this._lblDoctor = new Label();
            this._doctorComboBox = new ComboBox();
            this._lblAppointmentDate = new Label();
            this._appointmentDatePicker = new DateTimePicker();
            this._lblTimeSlot = new Label();
            this._timeSlotComboBox = new ComboBox();
            this._lblReason = new Label();
            this._reasonTextBox = new TextBox();
            this._btnBookAppointment = new Button();
            this._btnClearBooking = new Button();

            this._rightPanel = new Panel();
            this._headerPanel = new Panel();
            this._lblMainTitle = new Label();
            this._lblSubtitle = new Label();
            this._flpKpiStats = new FlowLayoutPanel();
            this._lblKpiTotal = new Label();
            this._lblKpiInService = new Label();
            this._lblKpiCalled = new Label();
            this._lblKpiWaiting = new Label();

            this._lblRoomSectionTitle = new Label();
            this._flpRoomStations = new FlowLayoutPanel();
            this._flpDoctorFilter = new FlowLayoutPanel();
            this._btnFilterAll = new Button();
            this._lblLobbyQueueHeader = new Label();
            this._lobbyActionBar = new Panel();
            this._btnCheckIn = new Button();
            this._btnCallPatient = new Button();
            this._btnSkipNoShow = new Button();
            this._btnRefreshQueue = new Button();
            this._pnlEmergencyAlertBanner = new Panel();
            this._lblEmergencyAlertText = new Label();
            this._btnOpenEmergencyTriage = new Button();
            this._lobbyQueueGrid = new DataGridView();

            this._leftPanel.SuspendLayout();
            this._patientSearchTable.SuspendLayout();
            this._pnlPatientWrapper.SuspendLayout();
            this._pnlConflictAlert.SuspendLayout();
            this._rightPanel.SuspendLayout();
            this._headerPanel.SuspendLayout();
            this._flpKpiStats.SuspendLayout();
            this._flpDoctorFilter.SuspendLayout();
            this._lobbyActionBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._lobbyQueueGrid).BeginInit();
            this.SuspendLayout();

            // 1. Form Level Settings
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.ClientSize = new Size(1380, 820);
            this.MinimumSize = new Size(1100, 700);
            this.Name = "AppointmentForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "ClinicFlow - Clinical Operations & Queue Command Board";
            this.UseWaitCursor = false;
            this.Cursor = Cursors.Default;

            // 2. Left Panel: Fixed 410px Dock
            this._leftPanel.Dock = DockStyle.Left;
            this._leftPanel.Width = 410;
            this._leftPanel.Padding = new Padding(16);
            this._leftPanel.AutoScroll = true;
            this._leftPanel.BackColor = Color.FromArgb(248, 250, 252);
            this._leftPanel.BorderStyle = BorderStyle.FixedSingle;
            this._leftPanel.UseWaitCursor = false;
            this._leftPanel.Cursor = Cursors.Default;

            this._lblScheduleTitle.Text = "Schedule Appointment";
            this._lblScheduleTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this._lblScheduleTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblScheduleTitle.Location = new Point(16, 16);
            this._lblScheduleTitle.Size = new Size(375, 28);

            this._lblSelectPatient.Text = "Search & Select Patient *";
            this._lblSelectPatient.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblSelectPatient.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblSelectPatient.Location = new Point(16, 50);
            this._lblSelectPatient.Size = new Size(375, 20);
            this._lblSelectPatient.UseMnemonic = false;

            // Aligned Find Bar
            this._patientSearchTable.Location = new Point(16, 72);
            this._patientSearchTable.Size = new Size(375, 28);
            this._patientSearchTable.ColumnCount = 2;
            this._patientSearchTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._patientSearchTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85F));
            this._patientSearchTable.RowCount = 1;
            this._patientSearchTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            this._patientSearchTextBox.Dock = DockStyle.Fill;
            this._patientSearchTextBox.Font = new Font("Segoe UI", 9.5F);
            this._patientSearchTextBox.PlaceholderText = "Search by Name, PT#, or Contact...";
            this._patientSearchTextBox.Margin = new Padding(0, 0, 4, 0);

            this._btnFindPatient.Dock = DockStyle.Fill;
            this._btnFindPatient.Text = "🔍 Find";
            this._btnFindPatient.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._btnFindPatient.Cursor = Cursors.Hand;
            this._btnFindPatient.Margin = new Padding(0);

            this._patientSearchTable.Controls.Add(this._patientSearchTextBox, 0, 0);
            this._patientSearchTable.Controls.Add(this._btnFindPatient, 1, 0);

            // Search Feedback Helper
            this._lblSearchFeedback.Text = "Showing top 20 registered patients • Type to live filter";
            this._lblSearchFeedback.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            this._lblSearchFeedback.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblSearchFeedback.Location = new Point(16, 103);
            this._lblSearchFeedback.Size = new Size(375, 16);

            // Themed Patient Dropdown with Border Wrapper
            this._pnlPatientWrapper.Location = new Point(16, 122);
            this._pnlPatientWrapper.Size = new Size(375, 30);
            this._pnlPatientWrapper.BorderStyle = BorderStyle.FixedSingle;
            this._pnlPatientWrapper.BackColor = Color.FromArgb(203, 213, 225);
            this._pnlPatientWrapper.Padding = new Padding(1);
            this._pnlPatientWrapper.Controls.Add(this._patientComboBox);

            this._patientComboBox.Dock = DockStyle.Fill;
            this._patientComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._patientComboBox.FlatStyle = FlatStyle.Flat;
            this._patientComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this._patientComboBox.ItemHeight = 22;
            this._patientComboBox.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Contextual Booking Status Alert Card
            this._pnlConflictAlert.Location = new Point(16, 158);
            this._pnlConflictAlert.Size = new Size(375, 72);
            this._pnlConflictAlert.BorderStyle = BorderStyle.FixedSingle;
            this._pnlConflictAlert.BackColor = Color.FromArgb(240, 253, 244);
            this._pnlConflictAlert.Padding = new Padding(10, 6, 10, 6);
            this._pnlConflictAlert.Controls.Add(this._lblConflictMessage);
            this._pnlConflictAlert.Controls.Add(this._lblConflictTitle);

            this._lblConflictTitle.Dock = DockStyle.Top;
            this._lblConflictTitle.Height = 20;
            this._lblConflictTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblConflictTitle.ForeColor = Color.FromArgb(22, 101, 52);
            this._lblConflictTitle.TextAlign = ContentAlignment.MiddleLeft;
            this._lblConflictTitle.Text = "✓ SCHEDULE CLEAR — ELIGIBLE TO BOOK";
            this._lblConflictTitle.UseMnemonic = false;

            this._lblConflictMessage.Dock = DockStyle.Fill;
            this._lblConflictMessage.AutoSize = false;
            this._lblConflictMessage.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular);
            this._lblConflictMessage.ForeColor = Color.FromArgb(21, 128, 61);
            this._lblConflictMessage.TextAlign = ContentAlignment.TopLeft;
            this._lblConflictMessage.Text = "No conflicting appointments found on this date. Patient is eligible for scheduling.";
            this._lblConflictMessage.UseMnemonic = false;

            this._lblDoctor.Text = "Attending Doctor *";
            this._lblDoctor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblDoctor.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblDoctor.Location = new Point(16, 236);
            this._lblDoctor.Size = new Size(375, 20);

            this._doctorComboBox.Location = new Point(16, 258);
            this._doctorComboBox.Size = new Size(375, 28);
            this._doctorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._doctorComboBox.Font = new Font("Segoe UI", 9.5F);

            this._lblAppointmentDate.Text = "Appointment Date *";
            this._lblAppointmentDate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblAppointmentDate.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblAppointmentDate.Location = new Point(16, 294);
            this._lblAppointmentDate.Size = new Size(375, 20);

            this._appointmentDatePicker.Location = new Point(16, 316);
            this._appointmentDatePicker.Size = new Size(375, 26);
            this._appointmentDatePicker.Font = new Font("Segoe UI", 9.5F);
            this._appointmentDatePicker.Format = DateTimePickerFormat.Short;
            this._appointmentDatePicker.MinDate = DateTime.Today;

            this._lblTimeSlot.Text = "Available 30-Min Time Slot *";
            this._lblTimeSlot.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblTimeSlot.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblTimeSlot.Location = new Point(16, 350);
            this._lblTimeSlot.Size = new Size(375, 20);

            // OwnerDraw TimeSlot Dropdown
            this._timeSlotComboBox.Location = new Point(16, 372);
            this._timeSlotComboBox.Size = new Size(375, 28);
            this._timeSlotComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._timeSlotComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this._timeSlotComboBox.ItemHeight = 24;
            this._timeSlotComboBox.Font = new Font("Segoe UI", 9F);

            this._lblReason.Text = "Chief Concern / Reason for Visit";
            this._lblReason.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblReason.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblReason.Location = new Point(16, 408);
            this._lblReason.Size = new Size(375, 20);

            this._reasonTextBox.Location = new Point(16, 430);
            this._reasonTextBox.Size = new Size(375, 68);
            this._reasonTextBox.Multiline = true;
            this._reasonTextBox.Font = new Font("Segoe UI", 9F);
            this._reasonTextBox.PlaceholderText = "e.g., Persistent dry cough, BP check, routine follow-up...";

            this._btnBookAppointment.Location = new Point(16, 508);
            this._btnBookAppointment.Size = new Size(265, 38);
            this._btnBookAppointment.Text = "✓ Book Appointment";
            this._btnBookAppointment.Cursor = Cursors.Hand;

            this._btnClearBooking.Location = new Point(288, 508);
            this._btnClearBooking.Size = new Size(103, 38);
            this._btnClearBooking.Text = "Clear";
            this._btnClearBooking.Cursor = Cursors.Hand;

            this._leftPanel.Controls.Add(this._lblScheduleTitle);
            this._leftPanel.Controls.Add(this._lblSelectPatient);
            this._leftPanel.Controls.Add(this._patientSearchTable);
            this._leftPanel.Controls.Add(this._lblSearchFeedback);
            this._leftPanel.Controls.Add(this._pnlPatientWrapper);
            this._leftPanel.Controls.Add(this._pnlConflictAlert);
            this._leftPanel.Controls.Add(this._lblDoctor);
            this._leftPanel.Controls.Add(this._doctorComboBox);
            this._leftPanel.Controls.Add(this._lblAppointmentDate);
            this._leftPanel.Controls.Add(this._appointmentDatePicker);
            this._leftPanel.Controls.Add(this._lblTimeSlot);
            this._leftPanel.Controls.Add(this._timeSlotComboBox);
            this._leftPanel.Controls.Add(this._lblReason);
            this._leftPanel.Controls.Add(this._reasonTextBox);
            this._leftPanel.Controls.Add(this._btnBookAppointment);
            this._leftPanel.Controls.Add(this._btnClearBooking);

            // 3. Right Panel: Queue Command Board
            this._rightPanel.Dock = DockStyle.Fill;
            this._rightPanel.Padding = new Padding(18, 14, 18, 14);
            this._rightPanel.UseWaitCursor = false;
            this._rightPanel.Cursor = Cursors.Default;

            // Header Banner
            this._headerPanel.Dock = DockStyle.Top;
            this._headerPanel.Height = 56;
            this._headerPanel.Controls.Add(this._flpKpiStats);
            this._headerPanel.Controls.Add(this._lblMainTitle);
            this._headerPanel.Controls.Add(this._lblSubtitle);

            this._lblMainTitle.Text = "CLINICAL QUEUE && CONSULTATION ROOM STATIONS";
            this._lblMainTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this._lblMainTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblMainTitle.Location = new Point(0, 2);
            this._lblMainTitle.AutoSize = true;

            this._lblSubtitle.Text = "Real-time room occupancy, active doctor stations, and lobby waiting queue.";
            this._lblSubtitle.Font = new Font("Segoe UI", 8.5F);
            this._lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblSubtitle.Location = new Point(1, 28);
            this._lblSubtitle.AutoSize = true;

            // Top Right KPI Stats (Single horizontal row, no wrapping)
            this._flpKpiStats.Dock = DockStyle.Right;
            this._flpKpiStats.WrapContents = false;
            this._flpKpiStats.AutoSize = true;
            this._flpKpiStats.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this._flpKpiStats.Height = 46;
            this._flpKpiStats.FlowDirection = FlowDirection.LeftToRight;
            this._flpKpiStats.Padding = new Padding(0, 4, 0, 4);
            this._flpKpiStats.Controls.Add(this._lblKpiTotal);
            this._flpKpiStats.Controls.Add(this._lblKpiInService);
            this._flpKpiStats.Controls.Add(this._lblKpiCalled);
            this._flpKpiStats.Controls.Add(this._lblKpiWaiting);

            this._lblKpiTotal.Text = "👥 Total: 0";
            this._lblKpiTotal.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblKpiTotal.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblKpiTotal.BackColor = Color.FromArgb(226, 232, 240);
            this._lblKpiTotal.Padding = new Padding(8, 6, 8, 6);
            this._lblKpiTotal.Margin = new Padding(4, 2, 4, 2);
            this._lblKpiTotal.AutoSize = true;

            this._lblKpiInService.Text = "🩺 In Service: 0";
            this._lblKpiInService.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblKpiInService.ForeColor = Color.FromArgb(4, 120, 87);
            this._lblKpiInService.BackColor = Color.FromArgb(236, 253, 245);
            this._lblKpiInService.Padding = new Padding(8, 6, 8, 6);
            this._lblKpiInService.Margin = new Padding(4, 2, 4, 2);
            this._lblKpiInService.AutoSize = true;

            this._lblKpiCalled.Text = "📢 Called: 0";
            this._lblKpiCalled.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblKpiCalled.ForeColor = Color.FromArgb(180, 83, 9);
            this._lblKpiCalled.BackColor = Color.FromArgb(254, 243, 199);
            this._lblKpiCalled.Padding = new Padding(8, 6, 8, 6);
            this._lblKpiCalled.Margin = new Padding(4, 2, 4, 2);
            this._lblKpiCalled.AutoSize = true;

            this._lblKpiWaiting.Text = "⏳ Waiting: 0";
            this._lblKpiWaiting.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblKpiWaiting.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblKpiWaiting.BackColor = Color.FromArgb(241, 245, 249);
            this._lblKpiWaiting.Padding = new Padding(8, 6, 8, 6);
            this._lblKpiWaiting.Margin = new Padding(4, 2, 4, 2);
            this._lblKpiWaiting.AutoSize = true;

            // Room Stations Section
            this._lblRoomSectionTitle.Dock = DockStyle.Top;
            this._lblRoomSectionTitle.Height = 28;
            this._lblRoomSectionTitle.Text = "Active Consultation Rooms && Attending Stations";
            this._lblRoomSectionTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._lblRoomSectionTitle.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblRoomSectionTitle.TextAlign = ContentAlignment.BottomLeft;
            this._lblRoomSectionTitle.UseMnemonic = false;

            this._flpRoomStations.Dock = DockStyle.Top;
            this._flpRoomStations.Height = 110;
            this._flpRoomStations.Padding = new Padding(0, 4, 0, 4);
            this._flpRoomStations.AutoScroll = true;
            this._flpRoomStations.WrapContents = false;

            // Doctor Filter Bar
            this._flpDoctorFilter.Dock = DockStyle.Top;
            this._flpDoctorFilter.Height = 42;
            this._flpDoctorFilter.Padding = new Padding(0, 6, 0, 4);
            this._flpDoctorFilter.Controls.Add(this._btnFilterAll);

            this._btnFilterAll.Text = "🌐 All Doctors";
            this._btnFilterAll.Size = new Size(115, 30);
            this._btnFilterAll.Cursor = Cursors.Hand;

            // Lobby Header
            this._lblLobbyQueueHeader.Dock = DockStyle.Top;
            this._lblLobbyQueueHeader.Height = 28;
            this._lblLobbyQueueHeader.Text = "Lobby Waiting && Upcoming Queue";
            this._lblLobbyQueueHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblLobbyQueueHeader.ForeColor = Color.FromArgb(30, 41, 59);
            this._lblLobbyQueueHeader.TextAlign = ContentAlignment.BottomLeft;
            this._lblLobbyQueueHeader.UseMnemonic = false;

            // Action Bar (Unified Horizontal Group)
            this._lobbyActionBar.Dock = DockStyle.Top;
            this._lobbyActionBar.Height = 44;
            this._lobbyActionBar.Padding = new Padding(0, 4, 0, 6);
            this._lobbyActionBar.Controls.Add(this._btnCheckIn);
            this._lobbyActionBar.Controls.Add(this._btnCallPatient);
            this._lobbyActionBar.Controls.Add(this._btnSkipNoShow);
            this._lobbyActionBar.Controls.Add(this._btnRefreshQueue);

            this._btnCheckIn.Location = new Point(0, 4);
            this._btnCheckIn.Size = new Size(130, 34);
            this._btnCheckIn.Text = "📥 Check-In";
            this._btnCheckIn.Cursor = Cursors.Hand;

            this._btnCallPatient.Location = new Point(138, 4);
            this._btnCallPatient.Size = new Size(130, 34);
            this._btnCallPatient.Text = "📢 Call Patient";
            this._btnCallPatient.Cursor = Cursors.Hand;

            this._btnSkipNoShow.Location = new Point(276, 4);
            this._btnSkipNoShow.Size = new Size(125, 34);
            this._btnSkipNoShow.Text = "⏭ No-Show";
            this._btnSkipNoShow.Cursor = Cursors.Hand;

            // Refresh Button now sits neatly next to Skip button
            this._btnRefreshQueue.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this._btnRefreshQueue.Location = new Point(409, 4);
            this._btnRefreshQueue.Size = new Size(105, 34);
            this._btnRefreshQueue.Text = "⟳ Refresh";
            this._btnRefreshQueue.Cursor = Cursors.Hand;

            // Emergency Alert Banner
            this._pnlEmergencyAlertBanner.Dock = DockStyle.Top;
            this._pnlEmergencyAlertBanner.Height = 44;
            this._pnlEmergencyAlertBanner.BackColor = Color.FromArgb(254, 242, 242);
            this._pnlEmergencyAlertBanner.Padding = new Padding(12, 4, 12, 4);
            this._pnlEmergencyAlertBanner.Visible = false;

            this._lblEmergencyAlertText.Text = "🚨 EMERGENCY SCHEDULE ALERT: Patient(s) displaced due to doctor emergency blockout!";
            this._lblEmergencyAlertText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblEmergencyAlertText.ForeColor = Color.FromArgb(185, 28, 28);
            this._lblEmergencyAlertText.Dock = DockStyle.Left;
            this._lblEmergencyAlertText.AutoSize = false;
            this._lblEmergencyAlertText.Width = 600;
            this._lblEmergencyAlertText.TextAlign = ContentAlignment.MiddleLeft;

            this._btnOpenEmergencyTriage.Text = "⚡ Open Emergency Triage Center";
            this._btnOpenEmergencyTriage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnOpenEmergencyTriage.BackColor = Color.FromArgb(220, 38, 38);
            this._btnOpenEmergencyTriage.ForeColor = Color.White;
            this._btnOpenEmergencyTriage.FlatStyle = FlatStyle.Flat;
            this._btnOpenEmergencyTriage.FlatAppearance.BorderSize = 0;
            this._btnOpenEmergencyTriage.Dock = DockStyle.Right;
            this._btnOpenEmergencyTriage.Width = 240;
            this._btnOpenEmergencyTriage.Cursor = Cursors.Hand;

            this._pnlEmergencyAlertBanner.Controls.Add(this._lblEmergencyAlertText);
            this._pnlEmergencyAlertBanner.Controls.Add(this._btnOpenEmergencyTriage);

            // Queue DataGridView
            this._lobbyQueueGrid.Dock = DockStyle.Fill;
            this._lobbyQueueGrid.BackgroundColor = Color.White;
            this._lobbyQueueGrid.BorderStyle = BorderStyle.FixedSingle;
            this._lobbyQueueGrid.RowHeadersVisible = false;
            this._lobbyQueueGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._lobbyQueueGrid.MultiSelect = false;
            this._lobbyQueueGrid.AllowUserToAddRows = false;
            this._lobbyQueueGrid.AllowUserToDeleteRows = false;
            this._lobbyQueueGrid.ReadOnly = true;
            this._lobbyQueueGrid.RowTemplate.Height = 34;
            this._lobbyQueueGrid.Font = new Font("Segoe UI", 9F);
            this._lobbyQueueGrid.Cursor = Cursors.Default;
            this._lobbyQueueGrid.UseWaitCursor = false;

            // Right Panel: Add controls in reverse order for correct top-to-bottom docking
            this._rightPanel.Controls.Add(this._lobbyQueueGrid);
            this._rightPanel.Controls.Add(this._pnlEmergencyAlertBanner);
            this._rightPanel.Controls.Add(this._lblLobbyQueueHeader);
            this._rightPanel.Controls.Add(this._flpDoctorFilter);
            this._rightPanel.Controls.Add(this._lobbyActionBar);
            this._rightPanel.Controls.Add(this._flpRoomStations);
            this._rightPanel.Controls.Add(this._lblRoomSectionTitle);
            this._rightPanel.Controls.Add(this._headerPanel);

            // Unified Dashboard: Direct Left & Right Docking without tabs
            this.Controls.Add(this._rightPanel);
            this.Controls.Add(this._leftPanel);

            this._leftPanel.ResumeLayout(false);
            this._leftPanel.PerformLayout();
            this._patientSearchTable.ResumeLayout(false);
            this._patientSearchTable.PerformLayout();
            this._pnlPatientWrapper.ResumeLayout(false);
            this._pnlConflictAlert.ResumeLayout(false);
            this._rightPanel.ResumeLayout(false);
            this._headerPanel.ResumeLayout(false);
            this._headerPanel.PerformLayout();
            this._flpKpiStats.ResumeLayout(false);
            this._flpKpiStats.PerformLayout();
            this._flpDoctorFilter.ResumeLayout(false);
            this._lobbyActionBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this._lobbyQueueGrid).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}