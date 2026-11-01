#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class SecretaryOverviewForm
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel _headerPanel = null!;
        private Label _lblTitle = null!;
        private Label _lblSubtitle = null!;
        private Button _btnRefresh = null!;
        private Label _lblLastUpdated = null!;

        private TableLayoutPanel _tlpKpis = null!;
        private Panel _pnlKpiBookings = null!;
        private Label _lblKpiBookingsTitle = null!;
        private Label _lblKpiBookingsVal = null!;
        private Label _lblKpiBookingsSub = null!;

        private Panel _pnlKpiWaiting = null!;
        private Label _lblKpiWaitingTitle = null!;
        private Label _lblKpiWaitingVal = null!;
        private Label _lblKpiWaitingSub = null!;

        private Panel _pnlKpiInService = null!;
        private Label _lblKpiInServiceTitle = null!;
        private Label _lblKpiInServiceVal = null!;
        private Label _lblKpiInServiceSub = null!;

        private Panel _pnlKpiPatients = null!;
        private Label _lblKpiPatientsTitle = null!;
        private Label _lblKpiPatientsVal = null!;
        private Label _lblKpiPatientsSub = null!;

        private Panel _pnlQuickActions = null!;
        private FlowLayoutPanel _flowQuickActions = null!;
        private Label _lblQuickTitle = null!;
        private Button _btnQuickPatient = null!;
        private Button _btnQuickAppointment = null!;
        private Button _btnQuickBilling = null!;
        private Button _btnQuickRefresh = null!;

        private TableLayoutPanel _mainSplitTable = null!;
        private Panel _pnlScheduleCard = null!;
        private Panel _pnlScheduleHeader = null!;
        private Label _lblScheduleTitle = null!;
        private Label _lblScheduleBadge = null!;
        private DataGridView _gridSchedule = null!;
        private Label _lblScheduleEmpty = null!;

        private Panel _pnlStationsCard = null!;
        private Panel _pnlStationsHeader = null!;
        private Label _lblStationsTitle = null!;
        private Label _lblStationsBadge = null!;
        private DataGridView _gridStations = null!;
        private Label _lblStationsEmpty = null!;

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
            this._btnRefresh = new Button();
            this._lblLastUpdated = new Label();

            this._tlpKpis = new TableLayoutPanel();
            this._pnlKpiBookings = new Panel();
            this._lblKpiBookingsTitle = new Label();
            this._lblKpiBookingsVal = new Label();
            this._lblKpiBookingsSub = new Label();

            this._pnlKpiWaiting = new Panel();
            this._lblKpiWaitingTitle = new Label();
            this._lblKpiWaitingVal = new Label();
            this._lblKpiWaitingSub = new Label();

            this._pnlKpiInService = new Panel();
            this._lblKpiInServiceTitle = new Label();
            this._lblKpiInServiceVal = new Label();
            this._lblKpiInServiceSub = new Label();

            this._pnlKpiPatients = new Panel();
            this._lblKpiPatientsTitle = new Label();
            this._lblKpiPatientsVal = new Label();
            this._lblKpiPatientsSub = new Label();

            this._pnlQuickActions = new Panel();
            this._flowQuickActions = new FlowLayoutPanel();
            this._lblQuickTitle = new Label();
            this._btnQuickPatient = new Button();
            this._btnQuickAppointment = new Button();
            this._btnQuickBilling = new Button();
            this._btnQuickRefresh = new Button();

            this._mainSplitTable = new TableLayoutPanel();
            this._pnlScheduleCard = new Panel();
            this._pnlScheduleHeader = new Panel();
            this._lblScheduleTitle = new Label();
            this._lblScheduleBadge = new Label();
            this._gridSchedule = new DataGridView();
            this._lblScheduleEmpty = new Label();

            this._pnlStationsCard = new Panel();
            this._pnlStationsHeader = new Panel();
            this._lblStationsTitle = new Label();
            this._lblStationsBadge = new Label();
            this._gridStations = new DataGridView();
            this._lblStationsEmpty = new Label();

            this.SuspendLayout();

            // 1. Root Form Settings
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.ClientSize = new Size(1250, 780);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(24, 14, 24, 16);

            // 2. Header Panel
            this._headerPanel.Dock = DockStyle.Top;
            this._headerPanel.Height = 58;
            this._headerPanel.BackColor = Color.Transparent;

            this._lblTitle.Text = "Front-Desk & Reception Overview";
            this._lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            this._lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblTitle.AutoSize = true;
            this._lblTitle.Location = new Point(0, 2);

            this._lblSubtitle.Text = "Live lobby waiting queue, today's appointments, doctor station availability, and quick front-desk actions";
            this._lblSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this._lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblSubtitle.AutoSize = true;
            this._lblSubtitle.Location = new Point(0, 34);

            this._btnRefresh.Text = "🔄 Refresh Overview";
            this._btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnRefresh.Size = new Size(160, 34);
            this._btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._btnRefresh.Location = new Point(this._headerPanel.Width - 165, 8);
            this._btnRefresh.FlatStyle = FlatStyle.Flat;
            this._btnRefresh.BackColor = Color.White;
            this._btnRefresh.ForeColor = Color.FromArgb(15, 118, 110);
            this._btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(15, 118, 110);
            this._btnRefresh.FlatAppearance.BorderSize = 1;
            this._btnRefresh.Cursor = Cursors.Hand;

            this._lblLastUpdated.Text = "Live Reception Data";
            this._lblLastUpdated.Font = new Font("Segoe UI", 8F);
            this._lblLastUpdated.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblLastUpdated.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._lblLastUpdated.Location = new Point(this._headerPanel.Width - 325, 16);
            this._lblLastUpdated.AutoSize = true;

            this._headerPanel.Controls.Add(this._lblTitle);
            this._headerPanel.Controls.Add(this._lblSubtitle);
            this._headerPanel.Controls.Add(this._btnRefresh);
            this._headerPanel.Controls.Add(this._lblLastUpdated);

            // 3. KPI Metrics Table Panel
            this._tlpKpis.Dock = DockStyle.Top;
            this._tlpKpis.Height = 90;
            this._tlpKpis.ColumnCount = 4;
            this._tlpKpis.RowCount = 1;
            this._tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            this._tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            this._tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            this._tlpKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            this._tlpKpis.Padding = new Padding(0, 6, 0, 6);

            // Setup KPI Card Helper
            SetupKpiCard(this._pnlKpiBookings, this._lblKpiBookingsTitle, this._lblKpiBookingsVal, this._lblKpiBookingsSub,
                "TODAY'S BOOKINGS", "0", "Scheduled appointments today", Color.FromArgb(37, 99, 235));

            SetupKpiCard(this._pnlKpiWaiting, this._lblKpiWaitingTitle, this._lblKpiWaitingVal, this._lblKpiWaitingSub,
                "LOBBY WAITING QUEUE", "0", "Checked-in & awaiting call", Color.FromArgb(217, 119, 6));

            SetupKpiCard(this._pnlKpiInService, this._lblKpiInServiceTitle, this._lblKpiInServiceVal, this._lblKpiInServiceSub,
                "IN CONSULTATION", "0", "Active in doctor stations", Color.FromArgb(5, 150, 105));

            SetupKpiCard(this._pnlKpiPatients, this._lblKpiPatientsTitle, this._lblKpiPatientsVal, this._lblKpiPatientsSub,
                "REGISTERED PATIENTS", "0", "Total patient directory records", Color.FromArgb(15, 118, 110));

            this._tlpKpis.Controls.Add(this._pnlKpiBookings, 0, 0);
            this._tlpKpis.Controls.Add(this._pnlKpiWaiting, 1, 0);
            this._tlpKpis.Controls.Add(this._pnlKpiInService, 2, 0);
            this._tlpKpis.Controls.Add(this._pnlKpiPatients, 3, 0);

            // 4. Quick Actions Bar
            this._pnlQuickActions.Dock = DockStyle.Top;
            this._pnlQuickActions.Height = 44;
            this._pnlQuickActions.Padding = new Padding(0, 4, 0, 4);

            this._flowQuickActions.Dock = DockStyle.Fill;
            this._flowQuickActions.FlowDirection = FlowDirection.LeftToRight;
            this._flowQuickActions.WrapContents = false;

            this._lblQuickTitle.Text = "FRONT-DESK SHORTCUTS:";
            this._lblQuickTitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this._lblQuickTitle.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblQuickTitle.Margin = new Padding(2, 8, 10, 0);
            this._lblQuickTitle.AutoSize = true;

            this._btnQuickPatient.Text = "➕ Register New Patient";
            this._btnQuickPatient.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnQuickPatient.Size = new Size(185, 32);
            this._btnQuickPatient.FlatStyle = FlatStyle.Flat;
            this._btnQuickPatient.BackColor = Color.FromArgb(15, 118, 110);
            this._btnQuickPatient.ForeColor = Color.White;
            this._btnQuickPatient.FlatAppearance.BorderSize = 0;
            this._btnQuickPatient.Cursor = Cursors.Hand;
            this._btnQuickPatient.Margin = new Padding(0, 0, 10, 0);

            this._btnQuickAppointment.Text = "📅 Schedule Appointment";
            this._btnQuickAppointment.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnQuickAppointment.Size = new Size(200, 32);
            this._btnQuickAppointment.FlatStyle = FlatStyle.Flat;
            this._btnQuickAppointment.BackColor = Color.FromArgb(37, 99, 235);
            this._btnQuickAppointment.ForeColor = Color.White;
            this._btnQuickAppointment.FlatAppearance.BorderSize = 0;
            this._btnQuickAppointment.Cursor = Cursors.Hand;
            this._btnQuickAppointment.Margin = new Padding(0, 0, 10, 0);

            this._btnQuickBilling.Text = "💳 Settle Bills & Checkout";
            this._btnQuickBilling.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnQuickBilling.Size = new Size(195, 32);
            this._btnQuickBilling.FlatStyle = FlatStyle.Flat;
            this._btnQuickBilling.BackColor = Color.FromArgb(15, 118, 110);
            this._btnQuickBilling.ForeColor = Color.White;
            this._btnQuickBilling.FlatAppearance.BorderSize = 0;
            this._btnQuickBilling.Cursor = Cursors.Hand;
            this._btnQuickBilling.Margin = new Padding(0, 0, 10, 0);

            this._btnQuickRefresh.Text = "👥 Patient Directory";
            this._btnQuickRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnQuickRefresh.Size = new Size(160, 32);
            this._btnQuickRefresh.FlatStyle = FlatStyle.Flat;
            this._btnQuickRefresh.BackColor = Color.White;
            this._btnQuickRefresh.ForeColor = Color.FromArgb(71, 85, 105);
            this._btnQuickRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            this._btnQuickRefresh.FlatAppearance.BorderSize = 1;
            this._btnQuickRefresh.Cursor = Cursors.Hand;

            this._flowQuickActions.Controls.Add(this._lblQuickTitle);
            this._flowQuickActions.Controls.Add(this._btnQuickPatient);
            this._flowQuickActions.Controls.Add(this._btnQuickAppointment);
            this._flowQuickActions.Controls.Add(this._btnQuickBilling);
            this._flowQuickActions.Controls.Add(this._btnQuickRefresh);
            this._pnlQuickActions.Controls.Add(this._flowQuickActions);

            // 5. Main Split Grid Table (60% Schedule / 40% Stations)
            this._mainSplitTable.Dock = DockStyle.Fill;
            this._mainSplitTable.ColumnCount = 2;
            this._mainSplitTable.RowCount = 1;
            this._mainSplitTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            this._mainSplitTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            this._mainSplitTable.Padding = new Padding(0, 6, 0, 0);

            // Left Card: Schedule & Queue
            this._pnlScheduleCard.Dock = DockStyle.Fill;
            this._pnlScheduleCard.BackColor = Color.White;
            this._pnlScheduleCard.Padding = new Padding(14);
            this._pnlScheduleCard.Margin = new Padding(0, 0, 8, 0);
            this._pnlScheduleCard.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240), 1), 0, 0, this._pnlScheduleCard.Width - 1, this._pnlScheduleCard.Height - 1);

            this._pnlScheduleHeader.Dock = DockStyle.Top;
            this._pnlScheduleHeader.Height = 36;

            this._lblScheduleTitle.Text = "Today's Patient Schedule & Lobby Queue";
            this._lblScheduleTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this._lblScheduleTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblScheduleTitle.AutoSize = true;
            this._lblScheduleTitle.Location = new Point(0, 4);

            this._lblScheduleBadge.Text = "0 Appointments";
            this._lblScheduleBadge.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this._lblScheduleBadge.ForeColor = Color.FromArgb(37, 99, 235);
            this._lblScheduleBadge.BackColor = Color.FromArgb(239, 246, 255);
            this._lblScheduleBadge.Padding = new Padding(6, 2, 6, 2);
            this._lblScheduleBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._lblScheduleBadge.Location = new Point(this._pnlScheduleHeader.Width - 140, 4);
            this._lblScheduleBadge.AutoSize = true;

            this._pnlScheduleHeader.Controls.Add(this._lblScheduleTitle);
            this._pnlScheduleHeader.Controls.Add(this._lblScheduleBadge);

            this._gridSchedule.Dock = DockStyle.Fill;
            this._gridSchedule.AllowUserToAddRows = false;
            this._gridSchedule.AllowUserToDeleteRows = false;
            this._gridSchedule.AllowUserToResizeRows = false;
            this._gridSchedule.ReadOnly = true;
            this._gridSchedule.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._gridSchedule.MultiSelect = false;
            this._gridSchedule.RowHeadersVisible = false;
            this._gridSchedule.BackgroundColor = Color.White;
            this._gridSchedule.BorderStyle = BorderStyle.None;

            this._lblScheduleEmpty.Text = "No patient appointments or queue tickets found for today.\nUse 'Schedule Appointment' above to book a visit.";
            this._lblScheduleEmpty.Font = new Font("Segoe UI", 9.5F);
            this._lblScheduleEmpty.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblScheduleEmpty.TextAlign = ContentAlignment.MiddleCenter;
            this._lblScheduleEmpty.Dock = DockStyle.Fill;
            this._lblScheduleEmpty.Visible = false;

            this._pnlScheduleCard.Controls.Add(this._lblScheduleEmpty);
            this._pnlScheduleCard.Controls.Add(this._gridSchedule);
            this._pnlScheduleCard.Controls.Add(this._pnlScheduleHeader);

            // Right Card: Attending Doctors & Station Status
            this._pnlStationsCard.Dock = DockStyle.Fill;
            this._pnlStationsCard.BackColor = Color.White;
            this._pnlStationsCard.Padding = new Padding(14);
            this._pnlStationsCard.Margin = new Padding(8, 0, 0, 0);
            this._pnlStationsCard.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240), 1), 0, 0, this._pnlStationsCard.Width - 1, this._pnlStationsCard.Height - 1);

            this._pnlStationsHeader.Dock = DockStyle.Top;
            this._pnlStationsHeader.Height = 36;

            this._lblStationsTitle.Text = "Attending Physicians & Station Status";
            this._lblStationsTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this._lblStationsTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblStationsTitle.AutoSize = true;
            this._lblStationsTitle.Location = new Point(0, 4);

            this._lblStationsBadge.Text = "Active Roster";
            this._lblStationsBadge.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this._lblStationsBadge.ForeColor = Color.FromArgb(15, 118, 110);
            this._lblStationsBadge.BackColor = Color.FromArgb(236, 253, 245);
            this._lblStationsBadge.Padding = new Padding(6, 2, 6, 2);
            this._lblStationsBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this._lblStationsBadge.Location = new Point(this._pnlStationsHeader.Width - 110, 4);
            this._lblStationsBadge.AutoSize = true;

            this._pnlStationsHeader.Controls.Add(this._lblStationsTitle);
            this._pnlStationsHeader.Controls.Add(this._lblStationsBadge);

            this._gridStations.Dock = DockStyle.Fill;
            this._gridStations.AllowUserToAddRows = false;
            this._gridStations.AllowUserToDeleteRows = false;
            this._gridStations.AllowUserToResizeRows = false;
            this._gridStations.ReadOnly = true;
            this._gridStations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._gridStations.MultiSelect = false;
            this._gridStations.RowHeadersVisible = false;
            this._gridStations.BackgroundColor = Color.White;
            this._gridStations.BorderStyle = BorderStyle.None;

            this._lblStationsEmpty.Text = "No active doctor practice records available.";
            this._lblStationsEmpty.Font = new Font("Segoe UI", 9.5F);
            this._lblStationsEmpty.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblStationsEmpty.TextAlign = ContentAlignment.MiddleCenter;
            this._lblStationsEmpty.Dock = DockStyle.Fill;
            this._lblStationsEmpty.Visible = false;

            this._pnlStationsCard.Controls.Add(this._lblStationsEmpty);
            this._pnlStationsCard.Controls.Add(this._gridStations);
            this._pnlStationsCard.Controls.Add(this._pnlStationsHeader);

            this._mainSplitTable.Controls.Add(this._pnlScheduleCard, 0, 0);
            this._mainSplitTable.Controls.Add(this._pnlStationsCard, 1, 0);

            // Add all major panels in reverse-dock order
            this.Controls.Add(this._mainSplitTable);
            this.Controls.Add(this._pnlQuickActions);
            this.Controls.Add(this._tlpKpis);
            this.Controls.Add(this._headerPanel);

            this.ResumeLayout(false);
        }

        private void SetupKpiCard(Panel pnl, Label lblTitle, Label lblVal, Label lblSub, string title, string val, string sub, Color accentColor)
        {
            pnl.Dock = DockStyle.Fill;
            pnl.BackColor = Color.White;
            pnl.Padding = new Padding(14, 10, 14, 10);
            pnl.Margin = new Padding(4);
            pnl.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(226, 232, 240), 1), 0, 0, pnl.Width - 1, pnl.Height - 1);
                // Top accent strip
                e.Graphics.FillRectangle(new SolidBrush(accentColor), 0, 0, pnl.Width, 3);
            };

            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 16;

            lblVal.Text = val;
            lblVal.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblVal.ForeColor = Color.FromArgb(15, 23, 42);
            lblVal.Dock = DockStyle.Top;
            lblVal.Height = 34;

            lblSub.Text = sub;
            lblSub.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            lblSub.ForeColor = Color.FromArgb(148, 163, 184);
            lblSub.Dock = DockStyle.Top;
            lblSub.Height = 16;

            pnl.Controls.Add(lblSub);
            pnl.Controls.Add(lblVal);
            pnl.Controls.Add(lblTitle);
        }

        #endregion
    }
}

