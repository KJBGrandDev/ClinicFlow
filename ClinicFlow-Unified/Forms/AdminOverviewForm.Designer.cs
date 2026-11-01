namespace ClinicFlow_Unified.Forms;

partial class AdminOverviewForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Panel _headerPanel = null!;
    private System.Windows.Forms.Label _lblTitle = null!;
    private System.Windows.Forms.Label _lblSubtitle = null!;
    private System.Windows.Forms.Button _btnRefresh = null!;
    private System.Windows.Forms.Label _lblLastUpdated = null!;

    private System.Windows.Forms.TableLayoutPanel _tlpKpis = null!;
    private System.Windows.Forms.Panel _pnlKpiPatients = null!;
    private System.Windows.Forms.Label _lblKpiPatientsTitle = null!;
    private System.Windows.Forms.Label _lblKpiPatientsVal = null!;
    private System.Windows.Forms.Label _lblKpiPatientsSub = null!;

    private System.Windows.Forms.Panel _pnlKpiAppts = null!;
    private System.Windows.Forms.Label _lblKpiApptsTitle = null!;
    private System.Windows.Forms.Label _lblKpiApptsVal = null!;
    private System.Windows.Forms.Label _lblKpiApptsSub = null!;

    private System.Windows.Forms.Panel _pnlKpiDoctors = null!;
    private System.Windows.Forms.Label _lblKpiDoctorsTitle = null!;
    private System.Windows.Forms.Label _lblKpiDoctorsVal = null!;
    private System.Windows.Forms.Label _lblKpiDoctorsSub = null!;

    private System.Windows.Forms.Panel _pnlKpiQueue = null!;
    private System.Windows.Forms.Label _lblKpiQueueTitle = null!;
    private System.Windows.Forms.Label _lblKpiQueueVal = null!;
    private System.Windows.Forms.Label _lblKpiQueueSub = null!;

    private System.Windows.Forms.Panel _pnlQuickActions = null!;
    private System.Windows.Forms.FlowLayoutPanel _flowQuickActions = null!;
    private System.Windows.Forms.Label _lblQuickTitle = null!;
    private System.Windows.Forms.Button _btnQuickPatient = null!;
    private System.Windows.Forms.Button _btnQuickAppointment = null!;
    private System.Windows.Forms.Button _btnQuickDoctor = null!;

    private System.Windows.Forms.TableLayoutPanel _mainSplitTable = null!;
    private System.Windows.Forms.Panel _pnlScheduleCard = null!;
    private System.Windows.Forms.Panel _pnlScheduleHeader = null!;
    private System.Windows.Forms.Label _lblScheduleTitle = null!;
    private System.Windows.Forms.Label _lblScheduleCountBadge = null!;
    private System.Windows.Forms.DataGridView _gridSchedule = null!;
    private System.Windows.Forms.Label _lblScheduleEmpty = null!;

    private System.Windows.Forms.Panel _pnlAuditCard = null!;
    private System.Windows.Forms.Panel _pnlAuditHeader = null!;
    private System.Windows.Forms.Label _lblAuditTitle = null!;
    private System.Windows.Forms.Label _lblAuditBadge = null!;
    private System.Windows.Forms.DataGridView _gridAudit = null!;
    private System.Windows.Forms.Label _lblAuditEmpty = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        // 1. Root Form Settings
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        this.ClientSize = new System.Drawing.Size(1200, 750);
        this.Padding = new System.Windows.Forms.Padding(24, 16, 24, 16);

        // 2. Header Panel
        this._headerPanel = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 60,
            BackColor = System.Drawing.Color.Transparent
        };

        this._lblTitle = new System.Windows.Forms.Label
        {
            Text = "Executive Overview",
            Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
            AutoSize = true,
            Location = new System.Drawing.Point(0, 4)
        };

        this._lblSubtitle = new System.Windows.Forms.Label
        {
            Text = "Real-time clinical metrics, queue status, and daily administrative operations",
            Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular),
            ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Location = new System.Drawing.Point(0, 36)
        };

        var pnlHeaderRight = new System.Windows.Forms.FlowLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Right,
            FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
            BackColor = System.Drawing.Color.Transparent,
            Padding = new System.Windows.Forms.Padding(0, 10, 0, 0)
        };

        this._lblLastUpdated = new System.Windows.Forms.Label
        {
            Text = "Updated: Just now",
            Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular),
            ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Margin = new System.Windows.Forms.Padding(0, 9, 12, 0)
        };

        this._btnRefresh = new System.Windows.Forms.Button
        {
            Text = "🔄 Refresh",
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
            BackColor = System.Drawing.Color.White,
            FlatStyle = System.Windows.Forms.FlatStyle.Flat,
            Size = new System.Drawing.Size(110, 36),
            Cursor = System.Windows.Forms.Cursors.Hand,
            Margin = new System.Windows.Forms.Padding(0, 0, 0, 0)
        };
        this._btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        this._btnRefresh.FlatAppearance.BorderSize = 1;

        pnlHeaderRight.Controls.Add(this._lblLastUpdated);
        pnlHeaderRight.Controls.Add(this._btnRefresh);

        this._headerPanel.Controls.Add(this._lblTitle);
        this._headerPanel.Controls.Add(this._lblSubtitle);
        this._headerPanel.Controls.Add(pnlHeaderRight);

        // Spacer 1
        var spacer1 = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 12, BackColor = System.Drawing.Color.Transparent };

        // 3. Top KPI Row (4 Cards)
        this._tlpKpis = new System.Windows.Forms.TableLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 104,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = System.Drawing.Color.Transparent
        };
        this._tlpKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this._tlpKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this._tlpKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this._tlpKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this._tlpKpis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

        // KPI Card 1: Patients
        this._pnlKpiPatients = CreateKpiCard(out this._lblKpiPatientsTitle, out this._lblKpiPatientsVal, out this._lblKpiPatientsSub,
            "👥 TOTAL REGISTERED PATIENTS", "0", "Active medical records", System.Drawing.Color.FromArgb(16, 185, 129));
        this._pnlKpiPatients.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);

        // KPI Card 2: Today's Appointments
        this._pnlKpiAppts = CreateKpiCard(out this._lblKpiApptsTitle, out this._lblKpiApptsVal, out this._lblKpiApptsSub,
            "📅 TODAY'S APPOINTMENTS", "0", "Scheduled visits today", System.Drawing.Color.FromArgb(37, 99, 235));
        this._pnlKpiAppts.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);

        // KPI Card 3: Active Doctors
        this._pnlKpiDoctors = CreateKpiCard(out this._lblKpiDoctorsTitle, out this._lblKpiDoctorsVal, out this._lblKpiDoctorsSub,
            "👨‍⚕️ ATTENDING DOCTORS", "0", "Licensed physicians", System.Drawing.Color.FromArgb(15, 23, 42));
        this._pnlKpiDoctors.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);

        // KPI Card 4: Waiting in Lobby
        this._pnlKpiQueue = CreateKpiCard(out this._lblKpiQueueTitle, out this._lblKpiQueueVal, out this._lblKpiQueueSub,
            "⏳ WAITING IN LOBBY", "0", "Patients awaiting service", System.Drawing.Color.FromArgb(217, 119, 6));
        this._pnlKpiQueue.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);

        this._tlpKpis.Controls.Add(this._pnlKpiPatients, 0, 0);
        this._tlpKpis.Controls.Add(this._pnlKpiAppts, 1, 0);
        this._tlpKpis.Controls.Add(this._pnlKpiDoctors, 2, 0);
        this._tlpKpis.Controls.Add(this._pnlKpiQueue, 3, 0);

        // Spacer 2
        var spacer2 = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 12, BackColor = System.Drawing.Color.Transparent };

        // 4. Quick Action Bar
        this._pnlQuickActions = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 52,
            BackColor = System.Drawing.Color.White,
            Padding = new System.Windows.Forms.Padding(16, 8, 16, 8)
        };
        this._pnlQuickActions.Paint += (s, e) =>
            System.Windows.Forms.ControlPaint.DrawBorder(e.Graphics, this._pnlQuickActions.ClientRectangle, System.Drawing.Color.FromArgb(226, 232, 240), System.Windows.Forms.ButtonBorderStyle.Solid);

        this._flowQuickActions = new System.Windows.Forms.FlowLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = System.Drawing.Color.Transparent
        };

        this._lblQuickTitle = new System.Windows.Forms.Label
        {
            Text = "QUICK ACTIONS:",
            Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Margin = new System.Windows.Forms.Padding(0, 8, 14, 0)
        };

        this._btnQuickPatient = new System.Windows.Forms.Button
        {
            Text = "+ Register New Patient",
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
            FlatStyle = System.Windows.Forms.FlatStyle.Flat,
            Size = new System.Drawing.Size(165, 34),
            Cursor = System.Windows.Forms.Cursors.Hand,
            Margin = new System.Windows.Forms.Padding(0, 0, 10, 0)
        };
        this._btnQuickPatient.FlatAppearance.BorderSize = 0;

        this._btnQuickAppointment = new System.Windows.Forms.Button
        {
            Text = "📅 Book Appointment",
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.White,
            BackColor = System.Drawing.Color.FromArgb(37, 99, 235),
            FlatStyle = System.Windows.Forms.FlatStyle.Flat,
            Size = new System.Drawing.Size(165, 34),
            Cursor = System.Windows.Forms.Cursors.Hand,
            Margin = new System.Windows.Forms.Padding(0, 0, 10, 0)
        };
        this._btnQuickAppointment.FlatAppearance.BorderSize = 0;

        this._btnQuickDoctor = new System.Windows.Forms.Button
        {
            Text = "👨‍⚕️ Manage Doctors",
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(30, 41, 59),
            BackColor = System.Drawing.Color.White,
            FlatStyle = System.Windows.Forms.FlatStyle.Flat,
            Size = new System.Drawing.Size(155, 34),
            Cursor = System.Windows.Forms.Cursors.Hand,
            Margin = new System.Windows.Forms.Padding(0, 0, 10, 0)
        };
        this._btnQuickDoctor.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        this._btnQuickDoctor.FlatAppearance.BorderSize = 1;

        this._flowQuickActions.Controls.Add(this._lblQuickTitle);
        this._flowQuickActions.Controls.Add(this._btnQuickPatient);
        this._flowQuickActions.Controls.Add(this._btnQuickAppointment);
        this._flowQuickActions.Controls.Add(this._btnQuickDoctor);
        this._pnlQuickActions.Controls.Add(this._flowQuickActions);

        // Spacer 3
        var spacer3 = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Top, Height = 12, BackColor = System.Drawing.Color.Transparent };

        // 5. Main Split Section (Schedule + Audit Log)
        this._mainSplitTable = new System.Windows.Forms.TableLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = System.Drawing.Color.Transparent
        };
        this._mainSplitTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47F));
        this._mainSplitTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53F));
        this._mainSplitTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

        // LEFT CARD: Today's Schedule
        this._pnlScheduleCard = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackColor = System.Drawing.Color.White,
            Padding = new System.Windows.Forms.Padding(16),
            Margin = new System.Windows.Forms.Padding(0, 0, 8, 0)
        };
        this._pnlScheduleCard.Paint += (s, e) =>
            System.Windows.Forms.ControlPaint.DrawBorder(e.Graphics, this._pnlScheduleCard.ClientRectangle, System.Drawing.Color.FromArgb(226, 232, 240), System.Windows.Forms.ButtonBorderStyle.Solid);

        this._pnlScheduleHeader = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 32,
            BackColor = System.Drawing.Color.Transparent
        };

        this._lblScheduleTitle = new System.Windows.Forms.Label
        {
            Text = "Today's Live Schedule",
            Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
            AutoSize = true,
            Location = new System.Drawing.Point(0, 2)
        };

        this._lblScheduleCountBadge = new System.Windows.Forms.Label
        {
            Text = "0 scheduled",
            Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(37, 99, 235),
            BackColor = System.Drawing.Color.FromArgb(239, 246, 255),
            Padding = new System.Windows.Forms.Padding(6, 2, 6, 2),
            AutoSize = true,
            Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right
        };
        this._lblScheduleCountBadge.Location = new System.Drawing.Point(this._pnlScheduleHeader.Width - 95, 4);

        this._pnlScheduleHeader.Controls.Add(this._lblScheduleTitle);
        this._pnlScheduleHeader.Controls.Add(this._lblScheduleCountBadge);

        this._gridSchedule = new System.Windows.Forms.DataGridView
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackgroundColor = System.Drawing.Color.White,
            BorderStyle = System.Windows.Forms.BorderStyle.None,
            CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = System.Drawing.Color.FromArgb(241, 245, 249),
            RowHeadersVisible = false,
            RowTemplate = { Height = 36 },
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 36
        };

        this._lblScheduleEmpty = new System.Windows.Forms.Label
        {
            Text = "✨ No appointments scheduled for today.",
            Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular),
            ForeColor = System.Drawing.Color.FromArgb(148, 163, 184),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Dock = System.Windows.Forms.DockStyle.Fill,
            Visible = false
        };

        this._pnlScheduleCard.Controls.Add(this._gridSchedule);
        this._pnlScheduleCard.Controls.Add(this._lblScheduleEmpty);
        this._pnlScheduleCard.Controls.Add(this._pnlScheduleHeader);

        // RIGHT CARD: Recent System Activity
        this._pnlAuditCard = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackColor = System.Drawing.Color.White,
            Padding = new System.Windows.Forms.Padding(16),
            Margin = new System.Windows.Forms.Padding(8, 0, 0, 0)
        };
        this._pnlAuditCard.Paint += (s, e) =>
            System.Windows.Forms.ControlPaint.DrawBorder(e.Graphics, this._pnlAuditCard.ClientRectangle, System.Drawing.Color.FromArgb(226, 232, 240), System.Windows.Forms.ButtonBorderStyle.Solid);

        this._pnlAuditHeader = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 32,
            BackColor = System.Drawing.Color.Transparent
        };

        this._lblAuditTitle = new System.Windows.Forms.Label
        {
            Text = "Security & Audit Activity",
            Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(15, 23, 42),
            AutoSize = true,
            Location = new System.Drawing.Point(0, 2)
        };

        this._lblAuditBadge = new System.Windows.Forms.Label
        {
            Text = "Live Trail",
            Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249),
            Padding = new System.Windows.Forms.Padding(6, 2, 6, 2),
            AutoSize = true,
            Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right
        };
        this._lblAuditBadge.Location = new System.Drawing.Point(this._pnlAuditHeader.Width - 80, 4);

        this._pnlAuditHeader.Controls.Add(this._lblAuditTitle);
        this._pnlAuditHeader.Controls.Add(this._lblAuditBadge);

        this._gridAudit = new System.Windows.Forms.DataGridView
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackgroundColor = System.Drawing.Color.White,
            BorderStyle = System.Windows.Forms.BorderStyle.None,
            CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = System.Drawing.Color.FromArgb(241, 245, 249),
            RowHeadersVisible = false,
            RowTemplate = { Height = 34 },
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 36
        };

        this._lblAuditEmpty = new System.Windows.Forms.Label
        {
            Text = "No recent activity recorded.",
            Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular),
            ForeColor = System.Drawing.Color.FromArgb(148, 163, 184),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Dock = System.Windows.Forms.DockStyle.Fill,
            Visible = false
        };

        this._pnlAuditCard.Controls.Add(this._gridAudit);
        this._pnlAuditCard.Controls.Add(this._lblAuditEmpty);
        this._pnlAuditCard.Controls.Add(this._pnlAuditHeader);

        this._mainSplitTable.Controls.Add(this._pnlScheduleCard, 0, 0);
        this._mainSplitTable.Controls.Add(this._pnlAuditCard, 1, 0);

        // Assemble all sections into form (Dock from bottom to top)
        this.Controls.Add(this._mainSplitTable);
        this.Controls.Add(spacer3);
        this.Controls.Add(this._pnlQuickActions);
        this.Controls.Add(spacer2);
        this.Controls.Add(this._tlpKpis);
        this.Controls.Add(spacer1);
        this.Controls.Add(this._headerPanel);

        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel CreateKpiCard(
        out System.Windows.Forms.Label lblHeader,
        out System.Windows.Forms.Label lblValue,
        out System.Windows.Forms.Label lblSubtitle,
        string headerText,
        string defaultValue,
        string subText,
        System.Drawing.Color valueColor)
    {
        var panel = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackColor = System.Drawing.Color.White,
            Padding = new System.Windows.Forms.Padding(16, 12, 16, 12)
        };
        panel.Paint += (s, e) =>
            System.Windows.Forms.ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle, System.Drawing.Color.FromArgb(226, 232, 240), System.Windows.Forms.ButtonBorderStyle.Solid);

        lblHeader = new System.Windows.Forms.Label
        {
            Text = headerText,
            Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 18
        };

        lblValue = new System.Windows.Forms.Label
        {
            Text = defaultValue,
            Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold),
            ForeColor = valueColor,
            Dock = System.Windows.Forms.DockStyle.Top,
            Height = 38
        };

        lblSubtitle = new System.Windows.Forms.Label
        {
            Text = subText,
            Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular),
            ForeColor = System.Drawing.Color.FromArgb(100, 116, 139),
            Dock = System.Windows.Forms.DockStyle.Bottom,
            Height = 18
        };

        panel.Controls.Add(lblSubtitle);
        panel.Controls.Add(lblValue);
        panel.Controls.Add(lblHeader);

        return panel;
    }
}

