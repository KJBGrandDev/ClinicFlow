namespace ClinicFlow_Unified.Forms;

partial class AuditLogViewerForm
{
    private System.ComponentModel.IContainer components = null!;
    private Panel _topBannerPanel = null!;
    private Label _lblTitle = null!;
    private Label _lblSubtitle = null!;
    private Panel _filterPanel = null!;
    private FlowLayoutPanel _filterFlow = null!;
    private Label _lblSearch = null!;
    private TextBox _txtSearch = null!;
    private Label _lblAction = null!;
    private ComboBox _cboActionType = null!;
    private Label _lblFrom = null!;
    private DateTimePicker _dtpFromDate = null!;
    private Label _lblTo = null!;
    private DateTimePicker _dtpToDate = null!;
    private Button _btnFilter = null!;
    private Button _btnReset = null!;
    private Button _btnExportCsv = null!;
    private DataGridView _gridAuditLogs = null!;
    private Panel _bottomStatusPanel = null!;
    private Label _lblStatusCount = null!;

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
        _topBannerPanel = new Panel();
        _lblTitle = new Label();
        _lblSubtitle = new Label();
        _filterPanel = new Panel();
        _filterFlow = new FlowLayoutPanel();
        _lblSearch = new Label();
        _txtSearch = new TextBox();
        _lblAction = new Label();
        _cboActionType = new ComboBox();
        _lblFrom = new Label();
        _dtpFromDate = new DateTimePicker();
        _lblTo = new Label();
        _dtpToDate = new DateTimePicker();
        _btnFilter = new Button();
        _btnReset = new Button();
        _btnExportCsv = new Button();
        _gridAuditLogs = new DataGridView();
        _bottomStatusPanel = new Panel();
        _lblStatusCount = new Label();

        _topBannerPanel.SuspendLayout();
        _filterPanel.SuspendLayout();
        _filterFlow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridAuditLogs).BeginInit();
        _bottomStatusPanel.SuspendLayout();
        SuspendLayout();

        // Banner
        _topBannerPanel.Dock = DockStyle.Top;
        _topBannerPanel.Height = 70;
        _topBannerPanel.Padding = new Padding(20, 12, 20, 10);
        _topBannerPanel.BackColor = Color.FromArgb(248, 250, 252);
        _topBannerPanel.Controls.Add(_lblSubtitle);
        _topBannerPanel.Controls.Add(_lblTitle);

        _lblTitle.Text = "📋 System Audit Trail & Compliance Log";
        _lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        _lblTitle.Dock = DockStyle.Top;
        _lblTitle.Height = 28;

        _lblSubtitle.Text = "Tamper-evident chronological audit of all clinical operations, logins, cancellations, and administrative events.";
        _lblSubtitle.Font = new Font("Segoe UI", 9F);
        _lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        _lblSubtitle.Dock = DockStyle.Top;
        _lblSubtitle.Height = 20;

        // Filter Panel
        _filterPanel.Dock = DockStyle.Top;
        _filterPanel.Height = 84;
        _filterPanel.Padding = new Padding(16, 8, 16, 8);
        _filterPanel.BackColor = Color.White;
        _filterPanel.Controls.Add(_filterFlow);

        _filterFlow.Dock = DockStyle.Fill;
        _filterFlow.AutoScroll = true;

        _lblSearch.Text = "Search:";
        _lblSearch.AutoSize = true;
        _lblSearch.Margin = new Padding(4, 8, 2, 4);
        _lblSearch.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        _txtSearch.Width = 180;
        _txtSearch.PlaceholderText = "User, entity, description...";
        _txtSearch.Margin = new Padding(2, 4, 12, 4);
        _txtSearch.Font = new Font("Segoe UI", 9F);

        _lblAction.Text = "Action:";
        _lblAction.AutoSize = true;
        _lblAction.Margin = new Padding(4, 8, 2, 4);
        _lblAction.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        _cboActionType.Width = 140;
        _cboActionType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboActionType.Margin = new Padding(2, 4, 12, 4);
        _cboActionType.Font = new Font("Segoe UI", 9F);

        _lblFrom.Text = "From:";
        _lblFrom.AutoSize = true;
        _lblFrom.Margin = new Padding(4, 8, 2, 4);
        _lblFrom.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        _dtpFromDate.Width = 120;
        _dtpFromDate.Format = DateTimePickerFormat.Short;
        _dtpFromDate.Margin = new Padding(2, 4, 12, 4);
        _dtpFromDate.Font = new Font("Segoe UI", 9F);

        _lblTo.Text = "To:";
        _lblTo.AutoSize = true;
        _lblTo.Margin = new Padding(4, 8, 2, 4);
        _lblTo.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        _dtpToDate.Width = 120;
        _dtpToDate.Format = DateTimePickerFormat.Short;
        _dtpToDate.Margin = new Padding(2, 4, 16, 4);
        _dtpToDate.Font = new Font("Segoe UI", 9F);

        _btnFilter.Text = "🔍 Filter";
        _btnFilter.Size = new Size(85, 32);
        _btnFilter.Cursor = Cursors.Hand;
        _btnFilter.Margin = new Padding(2, 2, 6, 2);

        _btnReset.Text = "Reset";
        _btnReset.Size = new Size(75, 32);
        _btnReset.Cursor = Cursors.Hand;
        _btnReset.Margin = new Padding(2, 2, 12, 2);

        _btnExportCsv.Text = "📥 Export CSV";
        _btnExportCsv.Size = new Size(115, 32);
        _btnExportCsv.Cursor = Cursors.Hand;
        _btnExportCsv.Margin = new Padding(2, 2, 2, 2);

        _filterFlow.Controls.Add(_lblSearch);
        _filterFlow.Controls.Add(_txtSearch);
        _filterFlow.Controls.Add(_lblAction);
        _filterFlow.Controls.Add(_cboActionType);
        _filterFlow.Controls.Add(_lblFrom);
        _filterFlow.Controls.Add(_dtpFromDate);
        _filterFlow.Controls.Add(_lblTo);
        _filterFlow.Controls.Add(_dtpToDate);
        _filterFlow.Controls.Add(_btnFilter);
        _filterFlow.Controls.Add(_btnReset);
        _filterFlow.Controls.Add(_btnExportCsv);

        // DataGridView
        _gridAuditLogs.Dock = DockStyle.Fill;
        _gridAuditLogs.BackgroundColor = Color.White;
        _gridAuditLogs.BorderStyle = BorderStyle.None;
        _gridAuditLogs.RowHeadersVisible = false;
        _gridAuditLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridAuditLogs.MultiSelect = false;
        _gridAuditLogs.ReadOnly = true;
        _gridAuditLogs.AllowUserToAddRows = false;
        _gridAuditLogs.AllowUserToDeleteRows = false;

        // Bottom Status
        _bottomStatusPanel.Dock = DockStyle.Bottom;
        _bottomStatusPanel.Height = 32;
        _bottomStatusPanel.Padding = new Padding(16, 6, 16, 6);
        _bottomStatusPanel.BackColor = Color.FromArgb(248, 250, 252);
        _bottomStatusPanel.Controls.Add(_lblStatusCount);

        _lblStatusCount.Dock = DockStyle.Fill;
        _lblStatusCount.Text = "Loaded 0 audit log records.";
        _lblStatusCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        _lblStatusCount.ForeColor = Color.FromArgb(100, 116, 139);

        // Form
        ClientSize = new Size(1100, 680);
        Controls.Add(_gridAuditLogs);
        Controls.Add(_bottomStatusPanel);
        Controls.Add(_filterPanel);
        Controls.Add(_topBannerPanel);
        Text = "System Audit Log Viewer";

        _topBannerPanel.ResumeLayout(false);
        _filterPanel.ResumeLayout(false);
        _filterFlow.ResumeLayout(false);
        _filterFlow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_gridAuditLogs).EndInit();
        _bottomStatusPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
