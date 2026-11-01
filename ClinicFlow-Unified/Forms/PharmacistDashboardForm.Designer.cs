#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms;

partial class PharmacistDashboardForm
{
    private System.ComponentModel.IContainer components = null!;

    // 1. Top Header
    private Panel _headerPanel = null!;
    private Label _lblModuleTitle = null!;
    private Label _lblModuleSubtitle = null!;
    private FlowLayoutPanel _headerRightFlow = null!;
    private Label _lblPharmacistBadge = null!;
    private Button _btnRefresh = null!;
    private Button _btnLogout = null!;

    // 2. KPI Metrics Strip
    private Panel _kpiContainerPanel = null!;
    private Label _lblKpiQueueValue = null!;
    private Label _lblKpiLowStockValue = null!;
    private Label _lblKpiCatalogValue = null!;
    private Label _lblKpiAuditValue = null!;

    // 3. Tab Switcher Panel (Uniform with BillingForm)
    private Panel _tabSwitchPanel = null!;
    private Button _btnTabDispensing = null!;
    private Button _btnTabInventory = null!;
    private Button _btnTabAudit = null!;

    // 4. Main Body Container
    private Panel _mainContentPanel = null!;

    // ==========================================
    // VIEW 1: DISPENSING STATION
    // ==========================================
    private Panel _viewDispensing = null!;
    private SplitContainer _splitDispensing = null!;

    // Left: Paid Queue
    private Panel _pnlQueueLeft = null!;
    private Panel _pnlQueueHeader = null!;
    private Label _lblQueueHeader = null!;
    private Panel _pnlQueueSearch = null!;
    private TextBox _txtQueueSearch = null!;
    private DataGridView _dgvQueue = null!;
    private Label _lblEmptyQueue = null!;

    // Right: Prescription Ledger & Actions
    private Panel _pnlDetailsRight = null!;
    private Panel _pnlPatientBanner = null!;
    private Label _lblPatientName = null!;
    private Label _lblRxBadge = null!;
    private Label _lblPatientMeta = null!;
    private Label _lblReceiptTag = null!;
    private DataGridView _dgvItems = null!;
    private TableLayoutPanel _tlpActions = null!;
    private Button _btnDispenseAll = null!;
    private Button _btnDispenseSingle = null!;
    private Button _btnTagOutOfStock = null!;
    private Button _btnPrintOfficialRx = null!;

    // ==========================================
    // VIEW 2: INVENTORY & RESTOCKING STATION
    // ==========================================
    private Panel _viewInventory = null!;
    private Panel _pnlInventoryFilter = null!;
    private Label _lblCategoryTitle = null!;
    private ComboBox _cmbCategoryFilter = null!;
    private Label _lblSearchTitle = null!;
    private TextBox _txtInventorySearch = null!;
    private FlowLayoutPanel _flowInventoryActions = null!;
    private Button _btnRestockMedicine = null!;
    private Button _btnDisposeMedicine = null!;
    private Button _btnAddMedicine = null!;
    private DataGridView _dgvInventory = null!;

    // ==========================================
    // VIEW 3: DISPENSED AUDIT HISTORY
    // ==========================================
    private Panel _viewAudit = null!;
    private Panel _pnlAuditSearch = null!;
    private TextBox _txtAuditSearch = null!;
    private DataGridView _dgvAudit = null!;

    // ==========================================
    // VIEW 4: WASTE & DISPOSAL AUDIT
    // ==========================================
    private Button _btnTabDisposal = null!;
    private Panel _viewDisposal = null!;
    private Panel _pnlDisposalToolbar = null!;
    private TextBox _txtDisposalSearch = null!;
    private Button _btnNewDisposal = null!;
    private DataGridView _dgvDisposal = null!;

    // ==========================================
    // VIEW 5: FINANCIAL FLOW & INTELLIGENCE
    // ==========================================
    private Button _btnTabFinancial = null!;
    private Panel _viewFinancial = null!;
    private ComboBox _cmbFinancialPeriod = null!;
    private Button _btnRefreshFinancial = null!;
    private Label _lblFinancialSummaryBadge = null!;
    private Label _lblFinGrossSalesVal = null!;
    private Label _lblFinGrossSalesSub = null!;
    private Label _lblFinGrossProfitVal = null!;
    private Label _lblFinGrossProfitSub = null!;
    private Label _lblFinShrinkageVal = null!;
    private Label _lblFinShrinkageSub = null!;
    private Label _lblFinMissedSalesVal = null!;
    private Label _lblFinMissedSalesSub = null!;
    private Label _lblFinActiveCapitalVal = null!;
    private Label _lblFinActiveCapitalSub = null!;
    private DataGridView _dgvTopRevenue = null!;
    private DataGridView _dgvFinancialRisk = null!;
    private DataGridView _dgvLossBreakdown = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Central Pharmacy & Dispensing Station — ClinicFlow";
        this.Size = new Size(1320, 860);
        this.MinimumSize = new Size(1100, 720);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(248, 250, 252); // Slate-50
        this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

        // ---------------------------------------------------------------------
        // 1. TOP HEADER PANEL (Matches BillingForm and MainDashboard)
        // ---------------------------------------------------------------------
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            BackColor = Color.White,
            Padding = new Padding(20, 10, 20, 10)
        };
        _headerPanel.Paint += (s, e) =>
            e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), 0, _headerPanel.Height - 1, _headerPanel.Width, _headerPanel.Height - 1);

        var pnlHeaderLeft = new Panel
        {
            Dock = DockStyle.Left,
            Width = 650,
            BackColor = Color.Transparent
        };

        _lblModuleTitle = new Label
        {
            Text = "💊 Central Pharmacy & Dispensing Station",
            Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42), // Slate-900
            Dock = DockStyle.Top,
            Height = 26
        };

        _lblModuleSubtitle = new Label
        {
            Text = "Strict paid-only claim fulfillment, 1-click FIFO inventory deduction, and official doctor Rx issuance",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139), // Slate-500
            Dock = DockStyle.Top,
            Height = 20
        };

        pnlHeaderLeft.Controls.Add(_lblModuleSubtitle);
        pnlHeaderLeft.Controls.Add(_lblModuleTitle);

        _headerRightFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 4, 0, 0)
        };

        _btnLogout = new Button
        {
            Text = "🚪 Logout",
            Size = new Size(95, 34),
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(220, 38, 38), // Red-600
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnLogout.FlatAppearance.BorderSize = 0;
        _btnLogout.MouseEnter += (s, e) => { _btnLogout.BackColor = Color.FromArgb(254, 242, 242); };
        _btnLogout.MouseLeave += (s, e) => { _btnLogout.BackColor = Color.Transparent; };

        _btnRefresh = new Button
        {
            Text = "🔄 Refresh",
            Size = new Size(100, 34),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };
        _btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        _lblPharmacistBadge = new Label
        {
            Text = "Maria Santos, RPh  |  Davao Central Pharmacy",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            AutoSize = true,
            Padding = new Padding(0, 7, 12, 0)
        };

        _headerRightFlow.Controls.Add(_btnLogout);
        _headerRightFlow.Controls.Add(_btnRefresh);
        _headerRightFlow.Controls.Add(_lblPharmacistBadge);

        _headerPanel.Controls.Add(pnlHeaderLeft);
        _headerPanel.Controls.Add(_headerRightFlow);

        // ---------------------------------------------------------------------
        // 2. KPI METRICS STRIP (Generous Height, No Clipping, Slate Borders)
        // ---------------------------------------------------------------------
        _kpiContainerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(20, 8, 20, 4)
        };

        var flowKpi = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        _lblKpiQueueValue = new Label { Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), AutoSize = false, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
        _lblKpiLowStockValue = new Label { Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), AutoSize = false, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
        _lblKpiCatalogValue = new Label { Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), AutoSize = false, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
        _lblKpiAuditValue = new Label { Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = false, Height = 26, TextAlign = ContentAlignment.MiddleLeft };

        flowKpi.Controls.Add(CreateKpiCard("AWAITING DISPENSE (PAID)", _lblKpiQueueValue, Color.FromArgb(239, 246, 255)));
        flowKpi.Controls.Add(CreateKpiCard("LOW STOCK WARNINGS", _lblKpiLowStockValue, Color.FromArgb(254, 242, 242)));
        flowKpi.Controls.Add(CreateKpiCard("ACTIVE FORMULATIONS", _lblKpiCatalogValue, Color.FromArgb(240, 253, 244)));
        flowKpi.Controls.Add(CreateKpiCard("DISPENSED RECORDS", _lblKpiAuditValue, Color.FromArgb(248, 250, 252)));

        _kpiContainerPanel.Controls.Add(flowKpi);

        // ---------------------------------------------------------------------
        // 3. TAB SWITCHER PANEL (Styled Uniform with BillingForm TabSwitcher)
        // ---------------------------------------------------------------------
        _tabSwitchPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.Transparent,
            Padding = new Padding(20, 4, 20, 4)
        };

        var flowTabs = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };

        _btnTabDispensing = new Button
        {
            Text = "⚡ Dispensing Station",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowOnly,
            Padding = new Padding(16, 0, 16, 0),
            Height = 34,
            MinimumSize = new Size(180, 34),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };

        _btnTabInventory = new Button
        {
            Text = "📦 Inventory & Restocking",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowOnly,
            Padding = new Padding(16, 0, 16, 0),
            Height = 34,
            MinimumSize = new Size(190, 34),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };

        _btnTabAudit = new Button
        {
            Text = "📜 Dispensed Audit History",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowOnly,
            Padding = new Padding(16, 0, 16, 0),
            Height = 34,
            MinimumSize = new Size(190, 34),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };

        _btnTabDisposal = new Button
        {
            Text = "🗑️ Waste & Disposal Audit",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowOnly,
            Padding = new Padding(16, 0, 16, 0),
            Height = 34,
            MinimumSize = new Size(190, 34),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };

        _btnTabFinancial = new Button
        {
            Text = "📊 Financial Flow & Intelligence",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowOnly,
            Padding = new Padding(16, 0, 16, 0),
            Height = 34,
            MinimumSize = new Size(220, 34),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };

        flowTabs.Controls.AddRange(new Control[] { _btnTabDispensing, _btnTabInventory, _btnTabAudit, _btnTabDisposal, _btnTabFinancial });
        _tabSwitchPanel.Controls.Add(flowTabs);

        // ---------------------------------------------------------------------
        // 4. MAIN CONTENT CONTAINER (Houses the 5 module views)
        // ---------------------------------------------------------------------
        _mainContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 4, 20, 16),
            BackColor = Color.FromArgb(248, 250, 252)
        };

        SetupDispensingView();
        SetupInventoryView();
        SetupAuditView();
        SetupDisposalView();
        SetupFinancialView();

        _mainContentPanel.Controls.Add(_viewDispensing);
        _mainContentPanel.Controls.Add(_viewInventory);
        _mainContentPanel.Controls.Add(_viewAudit);
        _mainContentPanel.Controls.Add(_viewDisposal);
        _mainContentPanel.Controls.Add(_viewFinancial);

        Controls.Add(_mainContentPanel);
        Controls.Add(_tabSwitchPanel);
        Controls.Add(_kpiContainerPanel);
        Controls.Add(_headerPanel);
    }

    private Panel CreateKpiCard(string header, Label valueLabel, Color iconTone)
    {
        var card = new Panel
        {
            Width = 260,
            Height = 56,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 14, 0),
            Padding = new Padding(14, 8, 14, 6)
        };
        card.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        var lblH = new Label
        {
            Text = header,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Top,
            Height = 16
        };

        valueLabel.Dock = DockStyle.Fill;
        valueLabel.Text = "0";

        card.Controls.Add(valueLabel);
        card.Controls.Add(lblH);
        return card;
    }

    // =========================================================================
    // VIEW 1: DISPENSING STATION (Balanced Splitter 50/50, Responsive)
    // =========================================================================
    private void SetupDispensingView()
    {
        _viewDispensing = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252)
        };

        _splitDispensing = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterWidth = 8,
            BackColor = Color.FromArgb(226, 232, 240) // Slate-200 divider
        };

        // ------------------ LEFT: QUEUE ------------------
        _pnlQueueLeft = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14)
        };
        _pnlQueueLeft.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, _pnlQueueLeft.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        _pnlQueueHeader = new Panel { Dock = DockStyle.Top, Height = 28 };
        _lblQueueHeader = new Label
        {
            Text = "CONFIRMED PAID PRESCRIPTIONS",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _pnlQueueHeader.Controls.Add(_lblQueueHeader);

        _pnlQueueSearch = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(0, 2, 0, 6) };
        _txtQueueSearch = new TextBox
        {
            PlaceholderText = "🔍 Search paid queue by patient, OR#, Rx#...",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9.5F)
        };
        _pnlQueueSearch.Controls.Add(_txtQueueSearch);

        _dgvQueue = CreateUniformGrid();
        _dgvQueue.Dock = DockStyle.Fill;

        _lblEmptyQueue = new Label
        {
            Text = "No paid prescriptions currently awaiting dispensing.\nPrescriptions appear here automatically once cashier payment is settled.",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(148, 163, 184),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Visible = false
        };

        var pnlQueueGridWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
        pnlQueueGridWrap.Controls.Add(_lblEmptyQueue);
        pnlQueueGridWrap.Controls.Add(_dgvQueue);

        _pnlQueueLeft.Controls.Add(pnlQueueGridWrap);
        _pnlQueueLeft.Controls.Add(_pnlQueueSearch);
        _pnlQueueLeft.Controls.Add(_pnlQueueHeader);

        // ------------------ RIGHT: DETAILS LEDGER & ACTION BUTTONS ------------------
        _pnlDetailsRight = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16)
        };
        _pnlDetailsRight.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, _pnlDetailsRight.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        // Patient Metadata Banner (96px, Structured, Non-Overlapping)
        _pnlPatientBanner = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(16, 10, 16, 10)
        };
        _pnlPatientBanner.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, _pnlPatientBanner.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        // Row 1: Patient Name & Rx Number
        var pnlRow1 = new Panel { Dock = DockStyle.Top, Height = 26 };
        _lblPatientName = new Label
        {
            Text = "No Paid Prescription Selected",
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Dock = DockStyle.Left,
            AutoSize = true
        };
        _lblRxBadge = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 99, 235),
            Dock = DockStyle.Right,
            AutoSize = true
        };
        pnlRow1.Controls.Add(_lblRxBadge);
        pnlRow1.Controls.Add(_lblPatientName);

        // Row 2: Metadata (ID, Age, Sex, Attending Physician)
        _lblPatientMeta = new Label
        {
            Text = "Select an item from the confirmed paid queue on the left to inspect formulation stock and dispense.",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Top,
            Height = 20
        };

        // Row 3: Cashier Confirmation Tag
        _lblReceiptTag = new Label
        {
            Text = "Waiting for cashier settlements...",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 185, 129),
            Dock = DockStyle.Bottom,
            Height = 22
        };

        _pnlPatientBanner.Controls.Add(_lblReceiptTag);
        _pnlPatientBanner.Controls.Add(_lblPatientMeta);
        _pnlPatientBanner.Controls.Add(pnlRow1);

        // Medication Items Table
        _dgvItems = CreateUniformGrid();
        _dgvItems.Dock = DockStyle.Fill;
        var pnlItemsWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 10) };
        pnlItemsWrap.Controls.Add(_dgvItems);

        // Bottom Action Controls (TableLayoutPanel for Clean Non-Clipping Layout)
        _tlpActions = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 84,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(0, 4, 0, 0)
        };
        _tlpActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        _tlpActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        _tlpActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        _tlpActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        _btnDispenseAll = new Button
        {
            Text = "✅ Confirm Dispense (Deduct Stock)",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 6, 4),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnDispenseAll.FlatAppearance.BorderSize = 0;

        _btnDispenseSingle = new Button
        {
            Text = "💊 Dispense Selected Item",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 4),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        _btnTagOutOfStock = new Button
        {
            Text = "⚠️ Tag Out of Stock & Issue Rx",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 6, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        _btnPrintOfficialRx = new Button
        {
            Text = "🖨️ View Doctor Rx Slip",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        _tlpActions.Controls.Add(_btnDispenseAll, 0, 0);
        _tlpActions.Controls.Add(_btnDispenseSingle, 1, 0);
        _tlpActions.Controls.Add(_btnTagOutOfStock, 0, 1);
        _tlpActions.Controls.Add(_btnPrintOfficialRx, 1, 1);

        _pnlDetailsRight.Controls.Add(pnlItemsWrap);
        _pnlDetailsRight.Controls.Add(_tlpActions);
        _pnlDetailsRight.Controls.Add(_pnlPatientBanner);

        _splitDispensing.Panel1.Controls.Add(_pnlQueueLeft);
        _splitDispensing.Panel2.Controls.Add(_pnlDetailsRight);
        _viewDispensing.Controls.Add(_splitDispensing);
    }

    // =========================================================================
    // VIEW 2: INVENTORY & RESTOCKING STATION
    // =========================================================================
    private void SetupInventoryView()
    {
        _viewInventory = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16),
            Visible = false
        };
        _viewInventory.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, _viewInventory.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        // Top Filter & Action Toolbar
        _pnlInventoryFilter = new Panel { Dock = DockStyle.Top, Height = 52 };

        _flowInventoryActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Width = 570,
            Height = 48,
            Padding = new Padding(0, 4, 0, 0)
        };

        _btnAddMedicine = new Button
        {
            Text = "➕ Add New Medicine",
            Size = new Size(175, 36),
            BackColor = Color.FromArgb(15, 118, 110), // Medical Teal
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 0)
        };
        _btnAddMedicine.FlatAppearance.BorderSize = 0;

        _btnRestockMedicine = new Button
        {
            Text = "📥 Restock Medicine...",
            Size = new Size(175, 36),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0)
        };
        _btnRestockMedicine.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        _btnDisposeMedicine = new Button
        {
            Text = "🗑️ Dispose / Write-Off...",
            Size = new Size(185, 36),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(185, 28, 28), // Red-700
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0)
        };
        _btnDisposeMedicine.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);

        _flowInventoryActions.Controls.Add(_btnAddMedicine);
        _flowInventoryActions.Controls.Add(_btnRestockMedicine);
        _flowInventoryActions.Controls.Add(_btnDisposeMedicine);

        var flowFilterLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        _lblCategoryTitle = new Label { Text = "Category:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Margin = new Padding(0, 4, 4, 0), AutoSize = true };
        _cmbCategoryFilter = new ComboBox
        {
            Width = 210,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9.5F),
            Margin = new Padding(0, 0, 16, 0)
        };

        _lblSearchTitle = new Label { Text = "Search:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Margin = new Padding(0, 4, 4, 0), AutoSize = true };
        _txtInventorySearch = new TextBox
        {
            PlaceholderText = "🔍 Search generic name, brand name, category...",
            Width = 280,
            Font = new Font("Segoe UI", 9.5F),
            Margin = new Padding(0, 0, 0, 0)
        };

        flowFilterLeft.Controls.AddRange(new Control[] { _lblCategoryTitle, _cmbCategoryFilter, _lblSearchTitle, _txtInventorySearch });

        _pnlInventoryFilter.Controls.Add(flowFilterLeft);
        _pnlInventoryFilter.Controls.Add(_flowInventoryActions);

        // Inventory Grid
        _dgvInventory = CreateUniformGrid();
        _dgvInventory.Dock = DockStyle.Fill;

        var pnlInvGridWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 0) };
        pnlInvGridWrap.Controls.Add(_dgvInventory);

        _viewInventory.Controls.Add(pnlInvGridWrap);
        _viewInventory.Controls.Add(_pnlInventoryFilter);
    }

    // =========================================================================
    // VIEW 3: DISPENSED AUDIT HISTORY
    // =========================================================================
    private void SetupAuditView()
    {
        _viewAudit = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16),
            Visible = false
        };
        _viewAudit.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, _viewAudit.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        _pnlAuditSearch = new Panel { Dock = DockStyle.Top, Height = 44 };
        _txtAuditSearch = new TextBox
        {
            PlaceholderText = "🔍 Search by patient, medicine name, receipt#, Rx#...",
            Location = new Point(0, 6),
            Width = 500,
            Font = new Font("Segoe UI", 9.5F)
        };
        _pnlAuditSearch.Controls.Add(_txtAuditSearch);

        _dgvAudit = CreateUniformGrid();
        _dgvAudit.Dock = DockStyle.Fill;

        var pnlAuditWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
        pnlAuditWrap.Controls.Add(_dgvAudit);

        _viewAudit.Controls.Add(pnlAuditWrap);
        _viewAudit.Controls.Add(_pnlAuditSearch);
    }

    // =========================================================================
    // VIEW 4: WASTE & DISPOSAL AUDIT
    // =========================================================================
    private void SetupDisposalView()
    {
        _viewDisposal = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16),
            Visible = false
        };
        _viewDisposal.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, _viewDisposal.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        _pnlDisposalToolbar = new Panel { Dock = DockStyle.Top, Height = 48 };

        _txtDisposalSearch = new TextBox
        {
            PlaceholderText = "🔍 Search waste audit by medicine, lot#, reason, pharmacist...",
            Location = new Point(0, 6),
            Width = 480,
            Font = new Font("Segoe UI", 9.5F)
        };

        _btnNewDisposal = new Button
        {
            Text = "🗑️ New Disposal / Destruction Run...",
            Size = new Size(260, 36),
            BackColor = Color.FromArgb(185, 28, 28), // Red-700
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Dock = DockStyle.Right
        };
        _btnNewDisposal.FlatAppearance.BorderSize = 0;

        _pnlDisposalToolbar.Controls.Add(_txtDisposalSearch);
        _pnlDisposalToolbar.Controls.Add(_btnNewDisposal);

        _dgvDisposal = CreateUniformGrid();
        _dgvDisposal.Dock = DockStyle.Fill;

        var pnlDisposalWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
        pnlDisposalWrap.Controls.Add(_dgvDisposal);

        _viewDisposal.Controls.Add(pnlDisposalWrap);
        _viewDisposal.Controls.Add(_pnlDisposalToolbar);
    }

    // =========================================================================
    // VIEW 5: FINANCIAL FLOW & INTELLIGENCE
    // =========================================================================
    private void SetupFinancialView()
    {
        _viewFinancial = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Visible = false
        };

        // 1. Top Filter & Toolbar Strip
        var pnlToolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = Color.Transparent,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 6),
            Margin = new Padding(0)
        };

        var lblPeriod = new Label
        {
            Text = "Financial Period:",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            AutoSize = true,
            Margin = new Padding(0, 7, 8, 0)
        };

        _cmbFinancialPeriod = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9F),
            Width = 150,
            Height = 28,
            Margin = new Padding(0, 4, 12, 0)
        };
        _cmbFinancialPeriod.Items.AddRange(new object[] { "Today", "This Week", "This Month", "Year-to-Date", "All Time" });
        _cmbFinancialPeriod.SelectedIndex = 2; // "This Month"

        _btnRefreshFinancial = new Button
        {
            Text = "🔄 Refresh",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 30),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 2, 16, 0)
        };
        _btnRefreshFinancial.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        _lblFinancialSummaryBadge = new Label
        {
            Text = "Shrinkage Rate: 0.00% (Healthy)",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 185, 129),
            BackColor = Color.FromArgb(240, 253, 244),
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            Margin = new Padding(0, 2, 0, 0)
        };

        pnlToolbar.Controls.Add(lblPeriod);
        pnlToolbar.Controls.Add(_cmbFinancialPeriod);
        pnlToolbar.Controls.Add(_btnRefreshFinancial);
        pnlToolbar.Controls.Add(_lblFinancialSummaryBadge);

        // 2. Financial KPI Cards Strip (5 Equal Columns - Spans 100% of dashboard width)
        var tblKpis = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 84,
            ColumnCount = 5,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 10),
            Margin = new Padding(0)
        };
        for (int i = 0; i < 5; i++)
        {
            tblKpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
        }

        _lblFinGrossSalesVal = new Label { Text = "₱0.00" };
        _lblFinGrossSalesSub = new Label { Text = "COGS: ₱0.00" };

        _lblFinGrossProfitVal = new Label { Text = "₱0.00" };
        _lblFinGrossProfitSub = new Label { Text = "Margin: 0.0%" };

        _lblFinShrinkageVal = new Label { Text = "₱0.00 (0.0%)" };
        _lblFinShrinkageSub = new Label { Text = "Target: < 2.0%" };

        _lblFinMissedSalesVal = new Label { Text = "₱0.00" };
        _lblFinMissedSalesSub = new Label { Text = "External / Stockouts" };

        _lblFinActiveCapitalVal = new Label { Text = "₱0.00" };
        _lblFinActiveCapitalSub = new Label { Text = "At Expiry Risk: ₱0.00" };

        tblKpis.Controls.Add(CreateFinancialKpiCard("GROSS SALES REVENUE", _lblFinGrossSalesVal, _lblFinGrossSalesSub, Color.FromArgb(37, 99, 235), 0), 0, 0);
        tblKpis.Controls.Add(CreateFinancialKpiCard("GROSS PROFIT & MARGIN", _lblFinGrossProfitVal, _lblFinGrossProfitSub, Color.FromArgb(16, 185, 129), 1), 1, 0);
        tblKpis.Controls.Add(CreateFinancialKpiCard("DISPOSAL WASTE / SHRINKAGE", _lblFinShrinkageVal, _lblFinShrinkageSub, Color.FromArgb(220, 38, 38), 2), 2, 0);
        tblKpis.Controls.Add(CreateFinancialKpiCard("MISSED SALES (STOCKOUTS)", _lblFinMissedSalesVal, _lblFinMissedSalesSub, Color.FromArgb(217, 119, 6), 3), 3, 0);
        tblKpis.Controls.Add(CreateFinancialKpiCard("WORKING CAPITAL ON SHELVES", _lblFinActiveCapitalVal, _lblFinActiveCapitalSub, Color.FromArgb(71, 85, 105), 4), 4, 0);

        // 3. Grid Panels Layout (TableLayoutPanel)
        var tblGridContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        tblGridContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49f));
        tblGridContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51f));

        // Left: Top Revenue Medications
        var pnlLeft = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14),
            Margin = new Padding(0, 0, 10, 0)
        };
        pnlLeft.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, pnlLeft.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        var lblTopRevTitle = new Label
        {
            Text = "🏆 Top Revenue Generating Medications & Fast Movers",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Dock = DockStyle.Top,
            Height = 28
        };

        _dgvTopRevenue = CreateUniformGrid();
        _dgvTopRevenue.Dock = DockStyle.Fill;

        pnlLeft.Controls.Add(_dgvTopRevenue);
        pnlLeft.Controls.Add(lblTopRevTitle);

        // Right: Expiry Risk (Top) and Loss Breakdown (Bottom)
        var tblRight = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        tblRight.RowStyles.Add(new RowStyle(SizeType.Percent, 52f));
        tblRight.RowStyles.Add(new RowStyle(SizeType.Percent, 48f));

        var pnlRisk = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14),
            Margin = new Padding(0, 0, 0, 6)
        };
        pnlRisk.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, pnlRisk.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        var lblRiskTitle = new Label
        {
            Text = "⚠️ Imminent Expiry Capital at Risk (< 30 Days)",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(185, 28, 28),
            Dock = DockStyle.Top,
            Height = 28
        };

        _dgvFinancialRisk = CreateUniformGrid();
        _dgvFinancialRisk.Dock = DockStyle.Fill;

        pnlRisk.Controls.Add(_dgvFinancialRisk);
        pnlRisk.Controls.Add(lblRiskTitle);

        var pnlLoss = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14),
            Margin = new Padding(0, 6, 0, 0)
        };
        pnlLoss.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, pnlLoss.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        var lblLossTitle = new Label
        {
            Text = "📉 Disposal Loss & Shrinkage Breakdown by Reason",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            Dock = DockStyle.Top,
            Height = 28
        };

        _dgvLossBreakdown = CreateUniformGrid();
        _dgvLossBreakdown.Dock = DockStyle.Fill;

        pnlLoss.Controls.Add(_dgvLossBreakdown);
        pnlLoss.Controls.Add(lblLossTitle);

        tblRight.Controls.Add(pnlRisk, 0, 0);
        tblRight.Controls.Add(pnlLoss, 0, 1);

        tblGridContainer.Controls.Add(pnlLeft, 0, 0);
        tblGridContainer.Controls.Add(tblRight, 1, 0);

        var pnlBodyWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 4, 0, 0) };
        pnlBodyWrap.Controls.Add(tblGridContainer);

        _viewFinancial.Controls.Add(pnlBodyWrap);
        _viewFinancial.Controls.Add(tblKpis);
        _viewFinancial.Controls.Add(pnlToolbar);
    }

    private Panel CreateFinancialKpiCard(string header, Label valLabel, Label subLabel, Color accentColor, int colIndex)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(0, 0, colIndex < 4 ? 10 : 0, 0),
            Padding = new Padding(12, 8, 12, 8)
        };
        card.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        var lblTitle = new Label
        {
            Text = header,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Top,
            Height = 16,
            AutoEllipsis = true
        };

        valLabel.Dock = DockStyle.Top;
        valLabel.Height = 26;
        valLabel.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
        valLabel.ForeColor = accentColor;
        valLabel.AutoEllipsis = true;

        subLabel.Dock = DockStyle.Top;
        subLabel.Height = 17;
        subLabel.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
        subLabel.ForeColor = Color.FromArgb(100, 116, 139);
        subLabel.AutoEllipsis = true;

        card.Controls.Add(subLabel);
        card.Controls.Add(valLabel);
        card.Controls.Add(lblTitle);

        return card;
    }

    private Button CreateActionButton(string text, Color bg, Color fg, int width)
    {
        var b = new Button
        {
            Text = text,
            Size = new Size(width, 34),
            BackColor = bg,
            ForeColor = fg,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 6, 0)
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private Button CreateOutlinedButton(string text, int width)
    {
        var b = new Button
        {
            Text = text,
            Size = new Size(width, 34),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        return b;
    }

    private DataGridView CreateUniformGrid()
    {
        var grid = new DataGridView
        {
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(241, 245, 249),
            RowHeadersVisible = false,
            RowTemplate = { Height = 36 },
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            DefaultCellStyle =
            {
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(30, 41, 59),
                SelectionBackColor = Color.FromArgb(239, 246, 255), // Soft Blue
                SelectionForeColor = Color.FromArgb(15, 23, 42)
            },
            ColumnHeadersDefaultCellStyle =
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(71, 85, 105),
                SelectionBackColor = Color.FromArgb(248, 250, 252),
                SelectionForeColor = Color.FromArgb(71, 85, 105)
            },
            EnableHeadersVisualStyles = false
        };
        return grid;
    }
}

