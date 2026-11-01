#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public sealed class DailyTimelineSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeDisplay { get; set; } = string.Empty;
        public string SlotStatus { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;
        public string AppointmentNumber { get; set; } = string.Empty;
        public string ChiefConcern { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public bool IsBooked { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsOutsideHours { get; set; }
    }

    public sealed class DoctorAppointmentRow
    {
        public string ScheduledTime { get; set; } = string.Empty;
        public string AppointmentNumber { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public partial class DoctorScheduleForm : Form
    {
        private readonly DoctorScheduleRepository _scheduleRepo = new();
        private List<DailyTimelineSlot> _timelineSlots = new();
        private List<DoctorAppointmentRow> _appointmentRows = new();
        private int _currentSlotMinutes = 30;
        private int _doctorId = 1;
        private bool _isUpdatingDate = false;
        private bool _isTimelineView = true;

        // UI Controls
        private Panel _headerPanel = null!;
        private DateTimePicker _dtpDate = null!;
        private MonthCalendar _monthCalendar = null!;
        private CheckBox _chkMon = null!, _chkTue = null!, _chkWed = null!, _chkThu = null!, _chkFri = null!, _chkSat = null!, _chkSun = null!;
        private ComboBox _cboStart = null!, _cboEnd = null!;
        private Label _lblDuration = null!;
        private Button _btnSaveWeekly = null!;
        private Button _btnToggleDayOff = null!;
        private Label _lblDayBanner = null!;
        private Label _lblDayStatusBadge = null!;
        private Label _lblKpiBooked = null!;
        private Label _lblKpiAvailable = null!;
        private Label _lblKpiBlocked = null!;
        private Panel _collisionAlertBar = null!;
        private Label _lblCollisionText = null!;
        private Button _btnViewTimeline = null!;
        private Button _btnViewRoster = null!;
        private DataGridView _timelineGrid = null!;
        private DataGridView _rosterGrid = null!;

        public DoctorScheduleForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            BuildProfessionalLayout();
            WireEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _doctorId = (UserSession.DoctorId.HasValue && UserSession.DoctorId.Value > 0) ? UserSession.DoctorId.Value : 1;
            await LoadWeeklyRoutineAsync();
            await RefreshActiveDateAsync(_dtpDate.Value.Date);
        }

                                private Panel _viewDaily = null!;
        private Panel _viewWeekly = null!;
        private Button _btnPillDaily = null!;
        private Button _btnPillWeekly = null!;

        private void BuildProfessionalLayout()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // - TOP APP HEADER BAR (NO OVERLAPS GUARANTEED) -
            _headerPanel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White };
            _headerPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);

            // Left Title Group
            var pnlTitleGroup = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                Width = 450,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(28, 12, 0, 0)
            };
            var lblTitle = new Label
            {
                Text = "Practice Schedule",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2)
            };
            var lblSubtitle = new Label
            {
                Text = "Daily Consultation Roster & Operating Hours",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Margin = new Padding(0)
            };
            pnlTitleGroup.Controls.Add(lblTitle);
            pnlTitleGroup.Controls.Add(lblSubtitle);
            _headerPanel.Controls.Add(pnlTitleGroup);

            // Right Segmented Pill Switcher (Anchored to Right, never collides with Left)
            var pnlPillHost = new Panel { Dock = DockStyle.Right, Width = 410, BackColor = Color.Transparent };
            var pnlPillBg = new Panel
            {
                Size = new Size(380, 42),
                Location = new Point(10, 14),
                BackColor = Color.FromArgb(241, 245, 249)
            };
            pnlPillBg.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = RoundedRect(pnlPillBg.ClientRectangle, 8);
                g.FillPath(new SolidBrush(Color.FromArgb(241, 245, 249)), path);
            };

            _btnPillDaily = new Button
            {
                Text = "Daily Operations",
                Size = new Size(185, 34),
                Location = new Point(4, 4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            _btnPillDaily.FlatAppearance.BorderSize = 0;

            _btnPillWeekly = new Button
            {
                Text = "Weekly Routine",
                Size = new Size(185, 34),
                Location = new Point(191, 4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            _btnPillWeekly.FlatAppearance.BorderSize = 0;

            pnlPillBg.Controls.AddRange(new Control[] { _btnPillDaily, _btnPillWeekly });
            pnlPillHost.Controls.Add(pnlPillBg);
            _headerPanel.Controls.Add(pnlPillHost);

            // Main container for the 2 views
            var mainContainer = new Panel { Dock = DockStyle.Fill };
            this.Controls.Add(mainContainer);
            this.Controls.Add(_headerPanel);

            // -
            // VIEW 1: DAILY OPERATIONS
            // -
            _viewDaily = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };

            // - Left Sidebar (Calendar & Quick Actions) -
            var leftDailyBar = new Panel { Dock = DockStyle.Left, Width = 280, BackColor = Color.White };
            leftDailyBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), leftDailyBar.Width - 1, 0, leftDailyBar.Width - 1, leftDailyBar.Height);

            var pnlSidebarContent = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(18, 14, 18, 14),
                BackColor = Color.White
            };

            var lblCalHeader = new Label
            {
                Text = "CALENDAR NAVIGATOR",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlSidebarContent.Controls.Add(lblCalHeader);

            _monthCalendar = new MonthCalendar
            {
                MaxSelectionCount = 1,
                Margin = new Padding(0, 0, 0, 16)
            };
            pnlSidebarContent.Controls.Add(_monthCalendar);

            var sepSidebar = new Panel
            {
                Size = new Size(244, 1),
                BackColor = Color.FromArgb(226, 232, 240),
                Margin = new Padding(0, 0, 0, 14)
            };
            pnlSidebarContent.Controls.Add(sepSidebar);

            var lblActionsHeader = new Label
            {
                Text = "SCHEDULE OVERRIDES",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlSidebarContent.Controls.Add(lblActionsHeader);

            var btnBlockLunch = new Button
            {
                Text = "Block Lunch (12:00 - 1:00)",
                Size = new Size(244, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(30, 41, 59),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            btnBlockLunch.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            pnlSidebarContent.Controls.Add(btnBlockLunch);

            var btnUnblockAll = new Button
            {
                Text = "Clear Daily Blockouts",
                Size = new Size(244, 40),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(30, 41, 59),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            btnUnblockAll.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            pnlSidebarContent.Controls.Add(btnUnblockAll);

            _btnToggleDayOff = new Button
            {
                Text = "Mark Date as Day Off",
                Size = new Size(244, 44),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(254, 242, 242),
                ForeColor = Color.FromArgb(220, 38, 38),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            _btnToggleDayOff.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            pnlSidebarContent.Controls.Add(_btnToggleDayOff);

            leftDailyBar.Controls.Add(pnlSidebarContent);

            // - Right Main Workspace -
            var rightMain = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(24, 18, 24, 18) };

            // 1. Date Navigation Bar
            var pnlDateNav = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White, Padding = new Padding(16, 8, 16, 8) };
            pnlDateNav.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, pnlDateNav.Height - 1, pnlDateNav.Width, pnlDateNav.Height - 1);

            var flowNav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            var btnPrevDay = new Button
            {
                Text = "- Prev",
                Size = new Size(80, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnPrevDay.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            _dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Long,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(330, 38),
                Margin = new Padding(0, 0, 8, 0)
            };

            var btnNextDay = new Button
            {
                Text = "Next -",
                Size = new Size(80, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnNextDay.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            var btnToday = new Button
            {
                Text = "Today",
                Size = new Size(80, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(239, 246, 255),
                ForeColor = Color.FromArgb(37, 99, 235),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnToday.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);

            flowNav.Controls.AddRange(new Control[] { btnPrevDay, _dtpDate, btnNextDay, btnToday });
            pnlDateNav.Controls.Add(flowNav);

            // 2. Status & KPI Strip
            var pnlStatusKpi = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White, Padding = new Padding(16, 0, 16, 0) };
            pnlStatusKpi.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, pnlStatusKpi.Height - 1, pnlStatusKpi.Width, pnlStatusKpi.Height - 1);

            _lblDayBanner = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(14, 10),
                AutoSize = true
            };
            _lblDayStatusBadge = new Label
            {
                Text = "- Active Working Day",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(4, 120, 87),
                BackColor = Color.FromArgb(209, 250, 229),
                Location = new Point(15, 36),
                AutoSize = true,
                Padding = new Padding(8, 2, 8, 2)
            };

            var pnlKpis = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlowDirection = FlowDirection.LeftToRight,
                Size = new Size(420, 56),
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 12, 0, 0)
            };
            _lblKpiBooked = new Label
            {
                Text = "0 Booked",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                BackColor = Color.FromArgb(239, 246, 255),
                AutoSize = true,
                Padding = new Padding(10, 4, 10, 4),
                Margin = new Padding(0, 0, 10, 0)
            };
            _lblKpiAvailable = new Label
            {
                Text = "0 Available",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(4, 120, 87),
                BackColor = Color.FromArgb(209, 250, 229),
                AutoSize = true,
                Padding = new Padding(10, 4, 10, 4),
                Margin = new Padding(0, 0, 10, 0)
            };
            _lblKpiBlocked = new Label
            {
                Text = "0 Blocked",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(185, 28, 28),
                BackColor = Color.FromArgb(254, 226, 226),
                AutoSize = true,
                Padding = new Padding(10, 4, 10, 4),
                Margin = new Padding(0, 0, 10, 0)
            };
            pnlKpis.Controls.AddRange(new Control[] { _lblKpiBooked, _lblKpiAvailable, _lblKpiBlocked });

            pnlStatusKpi.Controls.Add(_lblDayBanner);
            pnlStatusKpi.Controls.Add(_lblDayStatusBadge);
            pnlStatusKpi.Controls.Add(pnlKpis);
            pnlStatusKpi.Resize += (s, e) => pnlKpis.Location = new Point(pnlStatusKpi.Width - pnlKpis.Width - 12, 0);

            // 3. Conflict Alert Bar
            _collisionAlertBar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.FromArgb(255, 243, 205), Visible = false, Padding = new Padding(16, 8, 16, 8) };
            _lblCollisionText = new Label
            {
                Text = "Conflict Guard: Patients are already booked on this date. Existing appointments remain active.",
                ForeColor = Color.FromArgb(146, 64, 14),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _collisionAlertBar.Controls.Add(_lblCollisionText);

            // 4. View Switcher Tabs
            var pnlSubToggle = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(0, 8, 0, 0) };
            _btnViewTimeline = new Button
            {
                Text = "Daily Slot Timeline",
                Size = new Size(160, 34),
                Location = new Point(0, 7),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White
            };
            _btnViewTimeline.FlatAppearance.BorderSize = 0;

            _btnViewRoster = new Button
            {
                Text = "Booked Patients",
                Size = new Size(150, 34),
                Location = new Point(168, 7),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            _btnViewRoster.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            pnlSubToggle.Controls.AddRange(new Control[] { _btnViewTimeline, _btnViewRoster });

            // 5. Grid Host
            var gridWrapper = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            gridWrapper.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, gridWrapper.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            _timelineGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(241, 245, 249),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 46 }
            };
            ClinicTheme.ApplyGrid(_timelineGrid);
            ConfigureTimelineGrid();

            _rosterGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(241, 245, 249),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 46 }
            };
            ClinicTheme.ApplyGrid(_rosterGrid);
            ConfigureRosterGrid();

            gridWrapper.Controls.Add(_timelineGrid);
            gridWrapper.Controls.Add(_rosterGrid);

            rightMain.Controls.Add(gridWrapper);
            rightMain.Controls.Add(pnlSubToggle);
            rightMain.Controls.Add(_collisionAlertBar);
            rightMain.Controls.Add(pnlStatusKpi);
            rightMain.Controls.Add(pnlDateNav);

            _viewDaily.Controls.Add(rightMain);
            _viewDaily.Controls.Add(leftDailyBar);

            // -
            // VIEW 2: WEEKLY ROUTINE SETTINGS (BEAUTIFUL SPACIOUS CARD)
            // -
            _viewWeekly = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };

            var pnlScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(248, 250, 252) };

            var card = new Panel
            {
                Size = new Size(820, 680),
                BackColor = Color.White,
                Padding = new Padding(36)
            };
            card.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            Action resizeWeeklyCard = () =>
            {
                int cardW = Math.Min(820, pnlScroll.ClientSize.Width - 40);
                card.Size = new Size(cardW, 680);
                card.Location = new Point(Math.Max(20, (pnlScroll.ClientSize.Width - cardW) / 2), 24);
            };
            pnlScroll.Resize += (s, e) => resizeWeeklyCard();

            // Card Header
            var lblWkTitle = new Label
            {
                Text = "Standard Weekly Routine",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(36, 28),
                AutoSize = true
            };
            var lblWkDesc = new Label
            {
                Text = "Configure your recurring weekly practice schedule, operating hours, and consultation durations.",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(37, 64),
                AutoSize = true
            };
            var divWk1 = new Panel
            {
                Location = new Point(36, 100),
                Size = new Size(748, 1),
                BackColor = Color.FromArgb(226, 232, 240)
            };
            card.Controls.AddRange(new Control[] { lblWkTitle, lblWkDesc, divWk1 });

            // Section 1: Active Practice Days
            var lblDaysTitle = new Label
            {
                Text = "Active Practice Days",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(36, 120),
                AutoSize = true
            };
            var lblDaysHint = new Label
            {
                Text = "Select the days of the week when you are available to see patients:",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(37, 144),
                AutoSize = true
            };
            card.Controls.Add(lblDaysTitle);
            card.Controls.Add(lblDaysHint);

            var pnlDays = new FlowLayoutPanel
            {
                Location = new Point(36, 170),
                Size = new Size(748, 54),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            _chkMon = MakeDayButton("Mon");
            _chkTue = MakeDayButton("Tue");
            _chkWed = MakeDayButton("Wed");
            _chkThu = MakeDayButton("Thu");
            _chkFri = MakeDayButton("Fri");
            _chkSat = MakeDayButton("Sat");
            _chkSun = MakeDayButton("Sun");

            pnlDays.Controls.AddRange(new Control[] { _chkMon, _chkTue, _chkWed, _chkThu, _chkFri, _chkSat, _chkSun });
            card.Controls.Add(pnlDays);

            var divWk2 = new Panel
            {
                Location = new Point(36, 242),
                Size = new Size(748, 1),
                BackColor = Color.FromArgb(241, 245, 249)
            };
            card.Controls.Add(divWk2);

            // Section 2: Shift Hours
            var lblHoursTitle = new Label
            {
                Text = "Practice Shift Hours",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(36, 260),
                AutoSize = true
            };
            var lblHoursHint = new Label
            {
                Text = "Default consultation start and end times for your working shifts:",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(37, 284),
                AutoSize = true
            };
            card.Controls.Add(lblHoursTitle);
            card.Controls.Add(lblHoursHint);

            var lblStartHeader = new Label { Text = "Shift Starts:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(36, 312), AutoSize = true };
            _cboStart = new ComboBox { Location = new Point(36, 334), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F) };

            var lblTo = new Label { Text = "to", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(236, 338), AutoSize = true };

            var lblEndHeader = new Label { Text = "Shift Ends:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(276, 312), AutoSize = true };
            _cboEnd = new ComboBox { Location = new Point(276, 334), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F) };

            PopulateTimes();
            card.Controls.AddRange(new Control[] { lblStartHeader, _cboStart, lblTo, lblEndHeader, _cboEnd });

            var divWk3 = new Panel
            {
                Location = new Point(36, 390),
                Size = new Size(748, 1),
                BackColor = Color.FromArgb(241, 245, 249)
            };
            card.Controls.Add(divWk3);

            // Section 3: Slot Duration
            var lblDurTitle = new Label
            {
                Text = "Appointment Slot Duration",
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(36, 408),
                AutoSize = true
            };
            var lblDurHint = new Label
            {
                Text = "Standard time allocated per patient appointment:",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(37, 432),
                AutoSize = true
            };
            card.Controls.Add(lblDurTitle);
            card.Controls.Add(lblDurHint);

            var pnlStepper = new FlowLayoutPanel
            {
                Location = new Point(36, 458),
                Size = new Size(748, 48),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var btnMinus = new Button
            {
                Text = "-",
                Size = new Size(44, 44),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                Cursor = Cursors.Hand
            };
            btnMinus.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            _lblDuration = new Label
            {
                Text = "30 mins",
                Size = new Size(110, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                BackColor = Color.FromArgb(241, 245, 249),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(8, 0, 8, 0)
            };

            var btnPlus = new Button
            {
                Text = "+",
                Size = new Size(44, 44),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                Cursor = Cursors.Hand
            };
            btnPlus.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

            var pnlPresets = new FlowLayoutPanel
            {
                Size = new Size(420, 44),
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(24, 0, 0, 0)
            };
            foreach (var mins in new[] { 15, 20, 30, 45, 60 })
            {
                var m = mins;
                var btnPreset = new Button
                {
                    Text = $"{m}m",
                    Size = new Size(54, 44),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(71, 85, 105),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 6, 0)
                };
                btnPreset.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
                btnPreset.Click += (s, e) =>
                {
                    _currentSlotMinutes = m;
                    _lblDuration.Text = $"{_currentSlotMinutes} mins";
                };
                pnlPresets.Controls.Add(btnPreset);
            }

            pnlStepper.Controls.AddRange(new Control[] { btnMinus, _lblDuration, btnPlus, pnlPresets });
            card.Controls.Add(pnlStepper);

            var divWk4 = new Panel
            {
                Location = new Point(36, 526),
                Size = new Size(748, 1),
                BackColor = Color.FromArgb(241, 245, 249)
            };
            card.Controls.Add(divWk4);

            // Primary Action Button (Fully visible, 40px margin from bottom!)
            _btnSaveWeekly = new Button
            {
                Text = "Save Weekly Practice Routine",
                Location = new Point(36, 548),
                Size = new Size(748, 52),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnSaveWeekly.FlatAppearance.BorderSize = 0;
            card.Controls.Add(_btnSaveWeekly);

            var lblSaveHint = new Label
            {
                Text = "Saved preferences will instantly update your practice profile and future calendar slots.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(37, 610),
                AutoSize = true
            };
            card.Controls.Add(lblSaveHint);

            pnlScroll.Controls.Add(card);
            _viewWeekly.Controls.Add(pnlScroll);

            mainContainer.Controls.Add(_viewDaily);
            mainContainer.Controls.Add(_viewWeekly);

            // - WIRE ALL EVENTS -
            _btnPillDaily.Click += (s, e) => SwitchMainTab(true);
            _btnPillWeekly.Click += (s, e) =>
            {
                SwitchMainTab(false);
                resizeWeeklyCard();
            };

            btnMinus.Click += (s, e) =>
            {
                if (_currentSlotMinutes > 15)
                {
                    _currentSlotMinutes -= 15;
                    _lblDuration.Text = $"{_currentSlotMinutes} mins";
                }
            };
            btnPlus.Click += (s, e) =>
            {
                if (_currentSlotMinutes < 120)
                {
                    _currentSlotMinutes += 15;
                    _lblDuration.Text = $"{_currentSlotMinutes} mins";
                }
            };

            btnPrevDay.Click += async (s, e) => await ChangeDateAsync(_dtpDate.Value.AddDays(-1));
            btnNextDay.Click += async (s, e) => await ChangeDateAsync(_dtpDate.Value.AddDays(1));
            btnToday.Click += async (s, e) => await ChangeDateAsync(DateTime.Today);
            _dtpDate.ValueChanged += async (s, e) => { if (!_isUpdatingDate) await ChangeDateAsync(_dtpDate.Value); };
            _monthCalendar.DateSelected += async (s, e) => { if (!_isUpdatingDate) await ChangeDateAsync(e.Start); };

            btnBlockLunch.Click += async (s, e) => await BlockLunchAsync();
            btnUnblockAll.Click += async (s, e) => await UnblockAllTodayAsync();

            _btnViewTimeline.Click += (s, e) => SwitchMainView(true);
            _btnViewRoster.Click += (s, e) => SwitchMainView(false);

            _timelineGrid.CellContentClick += async (s, e) => await HandleTimelineGridActionAsync(e.RowIndex, e.ColumnIndex);

            this.Load += (s, e) => resizeWeeklyCard();
            SwitchMainTab(true);
        }

        private static CheckBox MakeDayButton(string dayText)
        {
            var chk = new CheckBox
            {
                Text = dayText,
                Size = new Size(96, 44),
                Appearance = Appearance.Button,
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = new Padding(0, 0, 8, 0)
            };
            chk.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            chk.FlatAppearance.BorderSize = 1;

            chk.CheckedChanged += (s, e) =>
            {
                if (chk.Checked)
                {
                    chk.BackColor = Color.FromArgb(37, 99, 235);
                    chk.ForeColor = Color.White;
                    chk.FlatAppearance.BorderColor = Color.FromArgb(29, 78, 216);
                }
                else
                {
                    chk.BackColor = Color.FromArgb(248, 250, 252);
                    chk.ForeColor = Color.FromArgb(100, 116, 139);
                    chk.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
                }
            };
            return chk;
        }

        private void SwitchMainTab(bool isDaily)
        {
            _viewDaily.Visible = isDaily;
            _viewWeekly.Visible = !isDaily;

            if (isDaily)
            {
                _btnPillDaily.BackColor = Color.White;
                _btnPillDaily.ForeColor = Color.FromArgb(15, 23, 42);
                _btnPillWeekly.BackColor = Color.Transparent;
                _btnPillWeekly.ForeColor = Color.FromArgb(100, 116, 139);
            }
            else
            {
                _btnPillWeekly.BackColor = Color.White;
                _btnPillWeekly.ForeColor = Color.FromArgb(15, 23, 42);
                _btnPillDaily.BackColor = Color.Transparent;
                _btnPillDaily.ForeColor = Color.FromArgb(100, 116, 139);
            }
        }

        private void SwitchMainView(bool showTimeline)
        {
            _isTimelineView = showTimeline;
            if (showTimeline)
            {
                _btnViewTimeline.BackColor = Color.FromArgb(37, 99, 235);
                _btnViewTimeline.ForeColor = Color.White;
                _btnViewTimeline.FlatAppearance.BorderSize = 0;

                _btnViewRoster.BackColor = Color.White;
                _btnViewRoster.ForeColor = Color.FromArgb(71, 85, 105);
                _btnViewRoster.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                _btnViewRoster.FlatAppearance.BorderSize = 1;
            }
            else
            {
                _btnViewRoster.BackColor = Color.FromArgb(37, 99, 235);
                _btnViewRoster.ForeColor = Color.White;
                _btnViewRoster.FlatAppearance.BorderSize = 0;

                _btnViewTimeline.BackColor = Color.White;
                _btnViewTimeline.ForeColor = Color.FromArgb(71, 85, 105);
                _btnViewTimeline.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                _btnViewTimeline.FlatAppearance.BorderSize = 1;
            }
            _timelineGrid.Visible = showTimeline;
            _rosterGrid.Visible = !showTimeline;
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void PopulateTimes()
        {
            _cboStart.Items.Clear();
            _cboEnd.Items.Clear();
            for (int h = 6; h <= 21; h++)
            {
                string t1 = DateTime.Today.AddHours(h).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                string t2 = DateTime.Today.AddHours(h).AddMinutes(30).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                _cboStart.Items.AddRange(new object[] { t1, t2 });
                _cboEnd.Items.AddRange(new object[] { t1, t2 });
            }
            SelectTimeInCombo(_cboStart, new TimeSpan(9, 0, 0));
            SelectTimeInCombo(_cboEnd, new TimeSpan(14, 0, 0));
        }

        private static void SelectTimeInCombo(ComboBox cbo, TimeSpan time)
        {
            string target = DateTime.Today.Add(time).ToString("hh:mm tt", CultureInfo.InvariantCulture);
            int idx = cbo.Items.IndexOf(target);
            if (idx >= 0) cbo.SelectedIndex = idx;
            else if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
        }

        private static TimeSpan GetTimeSpan(ComboBox cbo, TimeSpan fallback)
        {
            if (cbo.SelectedItem != null && DateTime.TryParse(cbo.SelectedItem.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt.TimeOfDay;
            }
            if (!string.IsNullOrWhiteSpace(cbo.Text) && DateTime.TryParse(cbo.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtText))
            {
                return dtText.TimeOfDay;
            }
            return fallback;
        }

        private void ApplyPreset(int[] days, TimeSpan start, TimeSpan end)
        {
            _chkMon.Checked = days.Contains(1); _chkTue.Checked = days.Contains(2);
            _chkWed.Checked = days.Contains(3); _chkThu.Checked = days.Contains(4);
            _chkFri.Checked = days.Contains(5); _chkSat.Checked = days.Contains(6); _chkSun.Checked = days.Contains(0);
            SelectTimeInCombo(_cboStart, start);
            SelectTimeInCombo(_cboEnd, end);
        }

        private void WireEvents()
        {
            _btnSaveWeekly.Click += async (s, e) => await SaveWeeklyScheduleAsync();
            _btnToggleDayOff.Click += async (s, e) => await HandleToggleDayOffAsync();
        }

        private void ConfigureTimelineGrid()
        {
            _timelineGrid.AutoGenerateColumns = false;
            _timelineGrid.Columns.Clear();
            _timelineGrid.RowTemplate.Height = 46;

            _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DailyTimelineSlot.TimeDisplay),
                HeaderText = "Time Slot",
                Width = 200,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }
            });

            _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DailyTimelineSlot.SlotStatus),
                HeaderText = "Availability",
                Width = 140,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }
            });

            _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DailyTimelineSlot.PatientName),
                HeaderText = "Booked Patient Name",
                Width = 240,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) }
            });

            _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DailyTimelineSlot.PatientNumber),
                HeaderText = "Patient #",
                Width = 110,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DailyTimelineSlot.ChiefConcern),
                HeaderText = "Reason / Notes",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            var btnCol = new DataGridViewButtonColumn
            {
                DataPropertyName = nameof(DailyTimelineSlot.ActionText),
                HeaderText = "Action",
                Width = 135,
                FlatStyle = FlatStyle.Flat
            };
            _timelineGrid.Columns.Add(btnCol);

            _timelineGrid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _timelineSlots.Count) return;
                var item = _timelineSlots[e.RowIndex];

                if (e.ColumnIndex == 1)
                {
                    if (item.IsOutsideHours) { e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9); e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); }
                    else if (item.IsBooked) { e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216); e.CellStyle.BackColor = Color.FromArgb(239, 246, 255); }
                    else if (item.IsBlocked) { e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38); e.CellStyle.BackColor = Color.FromArgb(254, 242, 242); }
                    else { e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52); e.CellStyle.BackColor = Color.FromArgb(240, 253, 244); }
                }
            };
        }

        private void ConfigureRosterGrid()
        {
            _rosterGrid.AutoGenerateColumns = false;
            _rosterGrid.Columns.Clear();
            _rosterGrid.RowTemplate.Height = 44;
            _rosterGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DoctorAppointmentRow.ScheduledTime), HeaderText = "Time", Width = 130, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) } });
            _rosterGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DoctorAppointmentRow.AppointmentNumber), HeaderText = "Appt #", Width = 140, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _rosterGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DoctorAppointmentRow.PatientNumber), HeaderText = "Patient #", Width = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _rosterGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DoctorAppointmentRow.PatientName), HeaderText = "Patient Full Name", Width = 260, DefaultCellStyle = { Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) } });
            _rosterGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DoctorAppointmentRow.Status), HeaderText = "Status", Width = 120, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _rosterGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DoctorAppointmentRow.Reason), HeaderText = "Chief Concern", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private async Task ChangeDateAsync(DateTime newDate)
        {
            _isUpdatingDate = true;
            _dtpDate.Value = newDate.Date;
            _monthCalendar.SetDate(newDate.Date);
            _isUpdatingDate = false;

            await RefreshActiveDateAsync(newDate.Date);
        }

        private async Task RefreshActiveDateAsync(DateTime date)
        {
            _lblDayBanner.Text = date.ToString("dddd, MMMM dd, yyyy");

            bool isDayOff = await Task.Run(() => _scheduleRepo.IsDayOff(_doctorId, date));
            _appointmentRows = await Task.Run(() => FetchAppointments(date));
            int bookedCount = _appointmentRows.Count;

            if (isDayOff)
            {
                _lblDayStatusBadge.Text = "- Full Day Off / Leave";
                _lblDayStatusBadge.ForeColor = Color.FromArgb(220, 38, 38);
                _lblDayStatusBadge.BackColor = Color.FromArgb(254, 242, 242);

                _btnToggleDayOff.Text = "Restore Practice Day";
                _btnToggleDayOff.BackColor = Color.FromArgb(236, 253, 245);
                _btnToggleDayOff.ForeColor = Color.FromArgb(4, 120, 87);
                _btnToggleDayOff.FlatAppearance.BorderColor = Color.FromArgb(167, 243, 208);

                if (bookedCount > 0)
                {
                    _collisionAlertBar.Visible = true;
                    _lblCollisionText.Text = $"Conflict Guard: {bookedCount} patient(s) are already booked on this date. Existing appointments remain active.";
                }
                else
                {
                    _collisionAlertBar.Visible = false;
                }
            }
            else
            {
                _lblDayStatusBadge.Text = "- Active Working Day";
                _lblDayStatusBadge.ForeColor = Color.FromArgb(4, 120, 87);
                _lblDayStatusBadge.BackColor = Color.FromArgb(209, 250, 229);

                _btnToggleDayOff.Text = "Mark Date as Day Off";
                _btnToggleDayOff.BackColor = Color.FromArgb(254, 242, 242);
                _btnToggleDayOff.ForeColor = Color.FromArgb(220, 38, 38);
                _btnToggleDayOff.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);

                _collisionAlertBar.Visible = false;
            }

            TimeSpan start = GetTimeSpan(_cboStart, new TimeSpan(9, 0, 0));
            TimeSpan end = GetTimeSpan(_cboEnd, new TimeSpan(14, 0, 0));
            if (end <= start) end = start.Add(TimeSpan.FromHours(5));
            int step = _currentSlotMinutes > 0 ? _currentSlotMinutes : 30;

            _timelineSlots = await Task.Run(() => BuildTimeline(date, isDayOff, _appointmentRows, start, end, step));
            _timelineGrid.DataSource = null;
            _timelineGrid.DataSource = _timelineSlots;

            _rosterGrid.DataSource = null;
            _rosterGrid.DataSource = _appointmentRows;

            int availableCount = _timelineSlots.Count(s => !s.IsBooked && !s.IsBlocked && !s.IsOutsideHours);
            int blockedCountSlots = _timelineSlots.Count(s => s.IsBlocked);

            _lblKpiBooked.Text = $"{bookedCount} Booked";
            _lblKpiAvailable.Text = $"{availableCount} Available";
            _lblKpiBlocked.Text = $"{blockedCountSlots} Blocked";
        }

        private List<DailyTimelineSlot> BuildTimeline(DateTime date, bool isDayOff, List<DoctorAppointmentRow> appointments, TimeSpan start, TimeSpan end, int step)
        {
            var slots = new List<DailyTimelineSlot>();
            var blockedSet = _scheduleRepo.GetBlockedSlots(_doctorId, date);

            var earlyAppts = appointments.Where(a =>
            {
                if (TimeSpan.TryParse(a.ScheduledTime.Split('-')[0].Trim(), out TimeSpan t))
                {
                    return t < start;
                }
                return false;
            }).ToList();

            foreach (var appt in earlyAppts)
            {
                slots.Add(new DailyTimelineSlot
                {
                    TimeDisplay = appt.ScheduledTime,
                    SlotStatus = "OUTSIDE SHIFT",
                    PatientName = appt.PatientName,
                    PatientNumber = appt.PatientNumber,
                    AppointmentNumber = appt.AppointmentNumber,
                    ChiefConcern = $"{appt.Reason} (Booked before shift start)",
                    ActionText = "Move to Slot",
                    IsOutsideHours = true,
                    IsBooked = true
                });
            }

            TimeSpan cur = start;
            while (cur < end)
            {
                TimeSpan next = cur.Add(TimeSpan.FromMinutes(step));
                if (next > end) break;

                string curStr = cur.ToString(@"hh\:mm");
                var appt = appointments.FirstOrDefault(a => a.ScheduledTime.StartsWith(curStr));

                var slot = new DailyTimelineSlot
                {
                    StartTime = cur,
                    EndTime = next,
                    TimeDisplay = $"{DateTime.Today.Add(cur):hh:mm tt} - {DateTime.Today.Add(next):hh:mm tt}"
                };

                if (appt != null)
                {
                    slot.IsBooked = true;
                    slot.SlotStatus = "BOOKED";
                    slot.PatientName = appt.PatientName;
                    slot.PatientNumber = appt.PatientNumber;
                    slot.AppointmentNumber = appt.AppointmentNumber;
                    slot.ChiefConcern = $"{appt.Reason} [{appt.Status}]";
                    slot.ActionText = "Emergency Block";
                }
                else if (isDayOff || blockedSet.Contains(cur))
                {
                    slot.IsBlocked = true;
                    slot.SlotStatus = "BLOCKED";
                    slot.PatientName = "-";
                    slot.PatientNumber = "-";
                    slot.AppointmentNumber = "-";
                    slot.ChiefConcern = isDayOff ? "Doctor Leave / Day Off" : "Manual Blockout";
                    slot.ActionText = isDayOff ? "Day Off" : "Unblock";
                }
                else
                {
                    slot.SlotStatus = "AVAILABLE";
                    slot.PatientName = "-";
                    slot.PatientNumber = "-";
                    slot.AppointmentNumber = "-";
                    slot.ChiefConcern = "Open for appointments";
                    slot.ActionText = "Block Slot";
                }

                slots.Add(slot);
                cur = next;
            }
            return slots;
        }

        private async Task HandleTimelineGridActionAsync(int rowIndex, int colIndex)
        {
            if (rowIndex < 0 || rowIndex >= _timelineSlots.Count) return;
            if (colIndex != 5) return;

            var slot = _timelineSlots[rowIndex];
            DateTime date = _dtpDate.Value.Date;

            if (slot.IsOutsideHours)
            {
                TimeSpan start = GetTimeSpan(_cboStart, new TimeSpan(9, 0, 0));
                TimeSpan end = GetTimeSpan(_cboEnd, new TimeSpan(14, 0, 0));
                var res = await Task.Run(() => _scheduleRepo.AutoTransportAffectedPatients(_doctorId, date, start, end, _currentSlotMinutes));
                if (res.TransportedCount > 0)
                {
                    MessageBox.Show($"Patient successfully moved to your earliest available slot!", "Patient Shifted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await RefreshActiveDateAsync(date);
                }
                else
                {
                    MessageBox.Show("No open slots found within today's working hours.", "Full Schedule", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            if (slot.IsBooked)
            {
                var bookedList = await Task.Run(() => _scheduleRepo.GetBookedAppointments(_doctorId, date, slot.StartTime));
                var previewItems = bookedList.Select(b => new AffectedPatientPreviewItem
                {
                    AppointmentId = b.AppointmentId,
                    AppointmentNumber = b.AppointmentNumber,
                    PatientName = b.PatientName,
                    PatientNumber = b.PatientNumber,
                    ContactNumber = b.ContactNumber,
                    TimeDisplay = $"{DateTime.Today.Add(b.Start):hh:mm tt} - {DateTime.Today.Add(b.End):hh:mm tt}"
                }).ToList();

                if (previewItems.Count == 0)
                {
                    MessageBox.Show("No active booking found for this slot.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var dlg = new EmergencyBlockoutDialog(previewItems, $"Slot {slot.TimeDisplay}");
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var apptService = new AppointmentService();
                    await Task.Run(() =>
                    {
                        _scheduleRepo.ToggleSlotBlock(_doctorId, date, slot.StartTime, slot.EndTime, true);
                        var ids = previewItems.Select(p => p.AppointmentId).ToList();
                        apptService.DisplaceAppointments(ids, dlg.SelectedReason, UserSession.UserId ?? 1);
                    });

                    MessageBox.Show($"Emergency blockout applied to slot {slot.TimeDisplay}.\nAffected patient(s) have been dispatched to the Front-Desk Emergency Triage Center.", "Emergency Blockout Declared", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await RefreshActiveDateAsync(date);
                }
                return;
            }

            bool isCurrentlyDayOff = await Task.Run(() => _scheduleRepo.IsDayOff(_doctorId, date));
            if (isCurrentlyDayOff)
            {
                MessageBox.Show("This entire day is marked as Day Off. Click 'Restore Practice' on the left sidebar to open individual slots.", "Day Off Active", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool willBlock = !slot.IsBlocked;
            await Task.Run(() => _scheduleRepo.ToggleSlotBlock(_doctorId, date, slot.StartTime, slot.EndTime, willBlock));
            await RefreshActiveDateAsync(date);
        }

        private async Task HandleToggleDayOffAsync()
        {
            DateTime date = _dtpDate.Value.Date;
            bool isCurrentlyOff = await Task.Run(() => _scheduleRepo.IsDayOff(_doctorId, date));
            int booked = await Task.Run(() => _scheduleRepo.GetBookedPatientCount(_doctorId, date));

            if (!isCurrentlyOff && booked > 0)
            {
                var bookedList = await Task.Run(() => _scheduleRepo.GetBookedAppointments(_doctorId, date));
                var previewItems = bookedList.Select(b => new AffectedPatientPreviewItem
                {
                    AppointmentId = b.AppointmentId,
                    AppointmentNumber = b.AppointmentNumber,
                    PatientName = b.PatientName,
                    PatientNumber = b.PatientNumber,
                    ContactNumber = b.ContactNumber,
                    TimeDisplay = $"{DateTime.Today.Add(b.Start):hh:mm tt} - {DateTime.Today.Add(b.End):hh:mm tt}"
                }).ToList();

                using var dlg = new EmergencyBlockoutDialog(previewItems, $"Full Day Leave on {date:MMMM dd, yyyy}");
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                var apptService = new AppointmentService();
                await Task.Run(() =>
                {
                    _scheduleRepo.ToggleDayOff(_doctorId, date, true);
                    var ids = previewItems.Select(p => p.AppointmentId).ToList();
                    apptService.DisplaceAppointments(ids, dlg.SelectedReason, UserSession.UserId ?? 1);
                });

                MessageBox.Show($"Full day marked as Emergency Leave.\nAll {booked} patient(s) have been safely dispatched to the Front-Desk Emergency Triage Center for peer transfer or priority rescheduling.", "Emergency Leave Declared", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshActiveDateAsync(date);
                return;
            }

            await Task.Run(() => _scheduleRepo.ToggleDayOff(_doctorId, date, !isCurrentlyOff));
            await RefreshActiveDateAsync(date);
        }

        private async Task BlockLunchAsync()
        {
            DateTime date = _dtpDate.Value.Date;
            TimeSpan t1 = new TimeSpan(12, 0, 0);
            TimeSpan t2 = new TimeSpan(12, 30, 0);
            TimeSpan t3 = new TimeSpan(13, 0, 0);

            await Task.Run(() =>
            {
                _scheduleRepo.ToggleSlotBlock(_doctorId, date, t1, t2, true);
                _scheduleRepo.ToggleSlotBlock(_doctorId, date, t2, t3, true);
            });
            await RefreshActiveDateAsync(date);
        }

        private async Task UnblockAllTodayAsync()
        {
            DateTime date = _dtpDate.Value.Date;
            await Task.Run(() =>
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = "DELETE FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = _doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.ExecuteNonQuery();
            });
            await RefreshActiveDateAsync(date);
        }

        private async Task LoadWeeklyRoutineAsync()
        {
            try
            {
                var routine = await Task.Run(() => _scheduleRepo.GetWeeklySchedule(_doctorId));
                if (routine.Count > 0)
                {
                    var activeDays = routine.Where(r => r.IsActive).Select(r => r.DayOfWeek).ToHashSet();
                    _chkMon.Checked = activeDays.Contains(1); _chkTue.Checked = activeDays.Contains(2);
                    _chkWed.Checked = activeDays.Contains(3); _chkThu.Checked = activeDays.Contains(4);
                    _chkFri.Checked = activeDays.Contains(5); _chkSat.Checked = activeDays.Contains(6); _chkSun.Checked = activeDays.Contains(0);

                    var first = routine[0];
                    _currentSlotMinutes = first.SlotDurationMinutes > 0 ? first.SlotDurationMinutes : 30;
                    _lblDuration.Text = $"{_currentSlotMinutes} mins";

                    SelectTimeInCombo(_cboStart, first.StartTime);
                    SelectTimeInCombo(_cboEnd, first.EndTime);
                }
                else
                {
                    ApplyPreset(new[] { 1, 3, 5 }, new TimeSpan(9, 0, 0), new TimeSpan(14, 0, 0));
                }
            }
            catch { }
        }

        private async Task SaveWeeklyScheduleAsync()
        {
            var activeDays = new List<int>();
            if (_chkMon.Checked) activeDays.Add(1); if (_chkTue.Checked) activeDays.Add(2);
            if (_chkWed.Checked) activeDays.Add(3); if (_chkThu.Checked) activeDays.Add(4);
            if (_chkFri.Checked) activeDays.Add(5); if (_chkSat.Checked) activeDays.Add(6); if (_chkSun.Checked) activeDays.Add(0);

            if (activeDays.Count == 0)
            {
                MessageBox.Show("Please select at least one active working day.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TimeSpan startTime = GetTimeSpan(_cboStart, new TimeSpan(9, 0, 0));
            TimeSpan endTime = GetTimeSpan(_cboEnd, new TimeSpan(14, 0, 0));

            if (endTime <= startTime)
            {
                MessageBox.Show("End time must be later than start time.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                await Task.Run(() => _scheduleRepo.SaveWeeklySchedule(_doctorId, activeDays, startTime, endTime, _currentSlotMinutes));

                var transport = await Task.Run(() => _scheduleRepo.AutoTransportAffectedPatients(_doctorId, _dtpDate.Value.Date, startTime, endTime, _currentSlotMinutes));

                string msg = "Weekly practice routine saved successfully!";
                if (transport.TransportedCount > 0)
                {
                    msg += $"\n\n Auto-Transport Notice: {transport.TransportedCount} earlier patient appointment(s) were automatically moved to your new open slots in First-Come, First-Served order!";
                }

                MessageBox.Show(msg, "Schedule Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await RefreshActiveDateAsync(_dtpDate.Value.Date);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving schedule: {ex.Message}", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private List<DoctorAppointmentRow> FetchAppointments(DateTime targetDate)
        {
            const string sql = @"
SELECT 
    CONVERT(VARCHAR(5), a.ScheduledStart, 108) + ' - ' + CONVERT(VARCHAR(5), a.ScheduledEnd, 108) AS ScheduledTime,
    a.AppointmentNumber,
    p.PatientNumber,
    (p.LastName + ', ' + p.FirstName) AS PatientName,
    a.Status,
    ISNULL(a.Reason, 'Routine Follow-up') AS Reason
FROM dbo.Appointments a
INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
WHERE a.DoctorId = @DoctorId AND a.AppointmentDate = @TargetDate AND a.Status NOT IN ('Cancelled', 'Displaced')
ORDER BY a.ScheduledStart ASC;";

            var list = new List<DoctorAppointmentRow>();
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = _doctorId;
                cmd.Parameters.Add("@TargetDate", SqlDbType.Date).Value = targetDate.Date;
                conn.Open();

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new DoctorAppointmentRow
                    {
                        ScheduledTime = rdr.GetString(0),
                        AppointmentNumber = rdr.GetString(1),
                        PatientNumber = rdr.GetString(2),
                        PatientName = rdr.GetString(3),
                        Status = rdr.GetString(4),
                        Reason = rdr.GetString(5)
                    });
                }
            }
            catch { }
            return list;
        }
    }
}





