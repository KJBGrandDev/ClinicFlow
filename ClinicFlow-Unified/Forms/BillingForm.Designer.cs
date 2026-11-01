#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class BillingForm
    {
        private System.ComponentModel.IContainer? components = null;

        // Main layout containers
        private SplitContainer _mainSplitContainer = null!;
        
        // Left Side: Queue / History Shell
        private Panel _leftHeaderPanel = null!;
        private Label _lblModuleTitle = null!;
        private Label _lblModuleSubtitle = null!;
        private Panel _tabSwitchPanel = null!;
        private Button _btnTabPending = null!;
        private Button _btnTabHistory = null!;
        
        private Panel _searchFilterPanel = null!;
        private TextBox _txtSearch = null!;
        private Button _btnSearch = null!;
        private Button _btnRefresh = null!;

        private Panel _gridContainerPanel = null!;
        private DataGridView _gridQueue = null!;
        private DataGridView _gridHistory = null!;
        private Label _lblEmptyQueue = null!;
        private Label _lblEmptyHistory = null!;

        // Right Side: Checkout Ledger & Settlement
        private Panel _rightLedgerPanel = null!;
        private Panel _ledgerHeaderPanel = null!;
        private Label _lblLedgerTitle = null!;
        private Label _lblLedgerBadge = null!;

        private Panel _patientCardPanel = null!;
        private Label _lblPatientCardName = null!;
        private Label _lblPatientCardMeta = null!;
        private Label _lblPatientCardDiagnosis = null!;

        private DataGridView _gridBillItems = null!;

        private Panel _totalsBreakdownPanel = null!;
        private TableLayoutPanel _tlpTotals = null!;
        private Label _lblConsultFeeTitle = null!;
        private Label _lblConsultFeeValue = null!;
        private Label _lblPharmacyFeeTitle = null!;
        private Label _lblPharmacyFeeValue = null!;
        private Label _lblSubtotalTitle = null!;
        private Label _lblSubtotalValue = null!;
        
        private Label _lblDiscountTypeTitle = null!;
        private ComboBox _cboDiscountType = null!;
        private Label _lblDiscountAmountValue = null!;
        
        private Label _lblTotalDueTitle = null!;
        private Label _lblTotalDueValue = null!;

        private Panel _paymentActionPanel = null!;
        private Label _lblPaymentMethodTitle = null!;
        private ComboBox _cboPaymentMethod = null!;
        
        private Label _lblTenderedTitle = null!;
        private TextBox _txtAmountTendered = null!;
        private FlowLayoutPanel _quickTenderPanel = null!;
        private Button _btnQuickExact = null!;
        private Button _btnQuick500 = null!;
        private Button _btnQuick1000 = null!;
        private Button _btnQuick2000 = null!;

        private Label _lblChangeTitle = null!;
        private Label _lblChangeValue = null!;

        private Label _lblNotesTitle = null!;
        private TextBox _txtPaymentNotes = null!;

        private Panel _bottomButtonPanel = null!;
        private Button _btnProcessPayment = null!;
        private Button _btnPrintReceipt = null!;
        private Button _btnClearLedger = null!;

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
            this._mainSplitContainer = new SplitContainer();
            
            // Left components
            this._leftHeaderPanel = new Panel();
            this._lblModuleTitle = new Label();
            this._lblModuleSubtitle = new Label();
            this._tabSwitchPanel = new Panel();
            this._btnTabPending = new Button();
            this._btnTabHistory = new Button();
            this._searchFilterPanel = new Panel();
            this._txtSearch = new TextBox();
            this._btnSearch = new Button();
            this._btnRefresh = new Button();
            this._gridContainerPanel = new Panel();
            this._gridQueue = new DataGridView();
            this._gridHistory = new DataGridView();
            this._lblEmptyQueue = new Label();
            this._lblEmptyHistory = new Label();

            // Right components
            this._rightLedgerPanel = new Panel();
            this._ledgerHeaderPanel = new Panel();
            this._lblLedgerTitle = new Label();
            this._lblLedgerBadge = new Label();
            this._patientCardPanel = new Panel();
            this._lblPatientCardName = new Label();
            this._lblPatientCardMeta = new Label();
            this._lblPatientCardDiagnosis = new Label();
            this._gridBillItems = new DataGridView();
            this._totalsBreakdownPanel = new Panel();
            this._tlpTotals = new TableLayoutPanel();
            this._lblConsultFeeTitle = new Label();
            this._lblConsultFeeValue = new Label();
            this._lblPharmacyFeeTitle = new Label();
            this._lblPharmacyFeeValue = new Label();
            this._lblSubtotalTitle = new Label();
            this._lblSubtotalValue = new Label();
            this._lblDiscountTypeTitle = new Label();
            this._cboDiscountType = new ComboBox();
            this._lblDiscountAmountValue = new Label();
            this._lblTotalDueTitle = new Label();
            this._lblTotalDueValue = new Label();

            this._paymentActionPanel = new Panel();
            this._lblPaymentMethodTitle = new Label();
            this._cboPaymentMethod = new ComboBox();
            this._lblTenderedTitle = new Label();
            this._txtAmountTendered = new TextBox();
            this._quickTenderPanel = new FlowLayoutPanel();
            this._btnQuickExact = new Button();
            this._btnQuick500 = new Button();
            this._btnQuick1000 = new Button();
            this._btnQuick2000 = new Button();
            this._lblChangeTitle = new Label();
            this._lblChangeValue = new Label();
            this._lblNotesTitle = new Label();
            this._txtPaymentNotes = new TextBox();

            this._bottomButtonPanel = new Panel();
            this._btnProcessPayment = new Button();
            this._btnPrintReceipt = new Button();
            this._btnClearLedger = new Button();

            ((System.ComponentModel.ISupportInitialize)(this._mainSplitContainer)).BeginInit();
            this._mainSplitContainer.Panel1.SuspendLayout();
            this._mainSplitContainer.Panel2.SuspendLayout();
            this.SuspendLayout();

            // =========================================================================
            // MAIN SPLIT CONTAINER
            // =========================================================================
            this._mainSplitContainer.Dock = DockStyle.Fill;
            this._mainSplitContainer.SplitterDistance = 600;
            this._mainSplitContainer.SplitterWidth = 6;
            this._mainSplitContainer.BackColor = Color.FromArgb(226, 232, 240); // Slate 200 splitter

            // -------------------------------------------------------------------------
            // LEFT PANEL: QUEUE & HISTORY
            // -------------------------------------------------------------------------
            this._mainSplitContainer.Panel1.BackColor = Color.FromArgb(248, 250, 252);
            this._mainSplitContainer.Panel1.Padding = new Padding(16, 16, 10, 16);

            // Left Header Panel
            this._leftHeaderPanel.Dock = DockStyle.Top;
            this._leftHeaderPanel.Height = 65;
            this._leftHeaderPanel.BackColor = Color.Transparent;

            this._lblModuleTitle.Text = "💳 Front-Desk Billing & Cashier";
            this._lblModuleTitle.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
            this._lblModuleTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblModuleTitle.Dock = DockStyle.Top;
            this._lblModuleTitle.Height = 32;

            this._lblModuleSubtitle.Text = "Settle consultation and clinic pharmacy charges, apply statutory discounts, and issue Official Receipts (OR)";
            this._lblModuleSubtitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this._lblModuleSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblModuleSubtitle.Dock = DockStyle.Top;
            this._lblModuleSubtitle.Height = 26;

            this._leftHeaderPanel.Controls.Add(this._lblModuleSubtitle);
            this._leftHeaderPanel.Controls.Add(this._lblModuleTitle);

            // Tab Switcher Panel (Pending Queue vs Settled History)
            this._tabSwitchPanel.Dock = DockStyle.Top;
            this._tabSwitchPanel.Height = 44;
            this._tabSwitchPanel.BackColor = Color.Transparent;
            this._tabSwitchPanel.Padding = new Padding(0, 4, 0, 6);

            this._btnTabPending.Text = "⚡ Pending Checkout Queue";
            this._btnTabPending.Size = new Size(210, 34);
            this._btnTabPending.Location = new Point(0, 4);
            this._btnTabPending.FlatStyle = FlatStyle.Flat;
            this._btnTabPending.FlatAppearance.BorderSize = 0;
            this._btnTabPending.BackColor = Color.FromArgb(15, 118, 110); // Medical Teal
            this._btnTabPending.ForeColor = Color.White;
            this._btnTabPending.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnTabPending.Cursor = Cursors.Hand;

            this._btnTabHistory.Text = "📜 Settled Receipts History";
            this._btnTabHistory.Size = new Size(190, 34);
            this._btnTabHistory.Location = new Point(216, 4);
            this._btnTabHistory.FlatStyle = FlatStyle.Flat;
            this._btnTabHistory.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            this._btnTabHistory.FlatAppearance.BorderSize = 1;
            this._btnTabHistory.BackColor = Color.White;
            this._btnTabHistory.ForeColor = Color.FromArgb(71, 85, 105);
            this._btnTabHistory.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this._btnTabHistory.Cursor = Cursors.Hand;

            this._tabSwitchPanel.Controls.Add(this._btnTabHistory);
            this._tabSwitchPanel.Controls.Add(this._btnTabPending);

            // Search & Filter Panel
            this._searchFilterPanel.Dock = DockStyle.Top;
            this._searchFilterPanel.Height = 46;
            this._searchFilterPanel.BackColor = Color.Transparent;
            this._searchFilterPanel.Padding = new Padding(0, 4, 0, 6);

            this._btnRefresh.Text = "↻ Refresh";
            this._btnRefresh.Size = new Size(88, 32);
            this._btnRefresh.Dock = DockStyle.Right;
            this._btnRefresh.FlatStyle = FlatStyle.Flat;
            this._btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            this._btnRefresh.FlatAppearance.BorderSize = 1;
            this._btnRefresh.BackColor = Color.White;
            this._btnRefresh.ForeColor = Color.FromArgb(71, 85, 105);
            this._btnRefresh.Font = new Font("Segoe UI", 8.5F);
            this._btnRefresh.Cursor = Cursors.Hand;

            var pnlSearchSep = new Panel { Dock = DockStyle.Right, Width = 6 };

            this._btnSearch.Text = "🔍 Search";
            this._btnSearch.Size = new Size(88, 32);
            this._btnSearch.Dock = DockStyle.Right;
            this._btnSearch.FlatStyle = FlatStyle.Flat;
            this._btnSearch.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            this._btnSearch.FlatAppearance.BorderSize = 1;
            this._btnSearch.BackColor = Color.White;
            this._btnSearch.ForeColor = Color.FromArgb(15, 23, 42);
            this._btnSearch.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this._btnSearch.Cursor = Cursors.Hand;

            var pnlSearchInput = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 1, 6, 0) };
            this._txtSearch.Dock = DockStyle.Fill;
            this._txtSearch.Font = new Font("Segoe UI", 9.5F);
            this._txtSearch.PlaceholderText = "Search patient name, PT#, or ticket...";
            pnlSearchInput.Controls.Add(this._txtSearch);

            this._searchFilterPanel.Controls.Add(pnlSearchInput);
            this._searchFilterPanel.Controls.Add(this._btnSearch);
            this._searchFilterPanel.Controls.Add(pnlSearchSep);
            this._searchFilterPanel.Controls.Add(this._btnRefresh);

            // Grid Container Panel
            this._gridContainerPanel.Dock = DockStyle.Fill;
            this._gridContainerPanel.BackColor = Color.White;
            this._gridContainerPanel.Padding = new Padding(1);
            this._gridContainerPanel.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawRectangle(p, 0, 0, this._gridContainerPanel.Width - 1, this._gridContainerPanel.Height - 1);
            };

            this._gridQueue.Dock = DockStyle.Fill;
            this._gridQueue.BackgroundColor = Color.White;
            this._gridQueue.BorderStyle = BorderStyle.None;
            this._gridQueue.RowHeadersVisible = false;
            this._gridQueue.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._gridQueue.MultiSelect = false;
            this._gridQueue.AllowUserToAddRows = false;
            this._gridQueue.AllowUserToDeleteRows = false;
            this._gridQueue.ReadOnly = true;
            this._gridQueue.RowTemplate.Height = 36;

            this._gridHistory.Dock = DockStyle.Fill;
            this._gridHistory.BackgroundColor = Color.White;
            this._gridHistory.BorderStyle = BorderStyle.None;
            this._gridHistory.RowHeadersVisible = false;
            this._gridHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._gridHistory.MultiSelect = false;
            this._gridHistory.AllowUserToAddRows = false;
            this._gridHistory.AllowUserToDeleteRows = false;
            this._gridHistory.ReadOnly = true;
            this._gridHistory.RowTemplate.Height = 36;
            this._gridHistory.Visible = false;

            this._lblEmptyQueue.Dock = DockStyle.Fill;
            this._lblEmptyQueue.BackColor = Color.White;
            this._lblEmptyQueue.TextAlign = ContentAlignment.MiddleCenter;
            this._lblEmptyQueue.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this._lblEmptyQueue.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblEmptyQueue.Text = "🎉 No pending checkouts at the moment.\nAll consultations are paid or queue is clear.";
            this._lblEmptyQueue.Visible = false;

            this._lblEmptyHistory.Dock = DockStyle.Fill;
            this._lblEmptyHistory.BackColor = Color.White;
            this._lblEmptyHistory.TextAlign = ContentAlignment.MiddleCenter;
            this._lblEmptyHistory.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this._lblEmptyHistory.ForeColor = Color.FromArgb(148, 163, 184);
            this._lblEmptyHistory.Text = "No settled receipts found.";
            this._lblEmptyHistory.Visible = false;

            this._gridContainerPanel.Controls.Add(this._lblEmptyHistory);
            this._gridContainerPanel.Controls.Add(this._lblEmptyQueue);
            this._gridContainerPanel.Controls.Add(this._gridHistory);
            this._gridContainerPanel.Controls.Add(this._gridQueue);

            this._mainSplitContainer.Panel1.Controls.Add(this._gridContainerPanel);
            this._mainSplitContainer.Panel1.Controls.Add(this._searchFilterPanel);
            this._mainSplitContainer.Panel1.Controls.Add(this._tabSwitchPanel);
            this._mainSplitContainer.Panel1.Controls.Add(this._leftHeaderPanel);

            // -------------------------------------------------------------------------
            // RIGHT PANEL: CHECKOUT LEDGER CARD
            // -------------------------------------------------------------------------
            this._mainSplitContainer.Panel2.BackColor = Color.FromArgb(248, 250, 252);
            this._mainSplitContainer.Panel2.Padding = new Padding(10, 16, 16, 16);

            this._rightLedgerPanel.Dock = DockStyle.Fill;
            this._rightLedgerPanel.BackColor = Color.White;
            this._rightLedgerPanel.Padding = new Padding(20, 16, 20, 16);
            this._rightLedgerPanel.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawRectangle(p, 0, 0, this._rightLedgerPanel.Width - 1, this._rightLedgerPanel.Height - 1);
            };

            // Ledger Card Header
            this._ledgerHeaderPanel.Dock = DockStyle.Top;
            this._ledgerHeaderPanel.Height = 38;
            this._ledgerHeaderPanel.BackColor = Color.Transparent;

            this._lblLedgerTitle.Text = "📋 Checkout Ledger";
            this._lblLedgerTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this._lblLedgerTitle.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblLedgerTitle.Dock = DockStyle.Left;
            this._lblLedgerTitle.AutoSize = true;

            this._lblLedgerBadge.Text = "Awaiting Patient Selection";
            this._lblLedgerBadge.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this._lblLedgerBadge.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblLedgerBadge.BackColor = Color.FromArgb(241, 245, 249);
            this._lblLedgerBadge.Padding = new Padding(8, 4, 8, 4);
            this._lblLedgerBadge.Dock = DockStyle.Right;
            this._lblLedgerBadge.AutoSize = true;

            this._ledgerHeaderPanel.Controls.Add(this._lblLedgerBadge);
            this._ledgerHeaderPanel.Controls.Add(this._lblLedgerTitle);

            // Patient Summary Box
            this._patientCardPanel.Dock = DockStyle.Top;
            this._patientCardPanel.Height = 74;
            this._patientCardPanel.BackColor = Color.FromArgb(248, 250, 252);
            this._patientCardPanel.Padding = new Padding(12, 10, 12, 10);
            this._patientCardPanel.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawRectangle(p, 0, 0, this._patientCardPanel.Width - 1, this._patientCardPanel.Height - 1);
            };

            this._lblPatientCardName.Text = "No patient selected";
            this._lblPatientCardName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this._lblPatientCardName.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblPatientCardName.Dock = DockStyle.Top;
            this._lblPatientCardName.Height = 22;

            this._lblPatientCardMeta.Text = "Select a pending patient from the queue to view itemized fees.";
            this._lblPatientCardMeta.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this._lblPatientCardMeta.ForeColor = Color.FromArgb(100, 116, 139);
            this._lblPatientCardMeta.Dock = DockStyle.Top;
            this._lblPatientCardMeta.Height = 18;

            this._lblPatientCardDiagnosis.Text = "";
            this._lblPatientCardDiagnosis.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            this._lblPatientCardDiagnosis.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblPatientCardDiagnosis.Dock = DockStyle.Top;
            this._lblPatientCardDiagnosis.Height = 18;

            this._patientCardPanel.Controls.Add(this._lblPatientCardDiagnosis);
            this._patientCardPanel.Controls.Add(this._lblPatientCardMeta);
            this._patientCardPanel.Controls.Add(this._lblPatientCardName);

            // Bill Items DataGridView
            this._gridBillItems.Dock = DockStyle.Fill;
            this._gridBillItems.BackgroundColor = Color.White;
            this._gridBillItems.BorderStyle = BorderStyle.FixedSingle;
            this._gridBillItems.RowHeadersVisible = false;
            this._gridBillItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this._gridBillItems.MultiSelect = false;
            this._gridBillItems.AllowUserToAddRows = false;
            this._gridBillItems.AllowUserToDeleteRows = false;
            this._gridBillItems.ReadOnly = true;
            this._gridBillItems.RowTemplate.Height = 28;

            // Totals Breakdown Panel
            this._totalsBreakdownPanel.Dock = DockStyle.Bottom;
            this._totalsBreakdownPanel.Height = 150;
            this._totalsBreakdownPanel.BackColor = Color.FromArgb(248, 250, 252);
            this._totalsBreakdownPanel.Padding = new Padding(14, 10, 14, 10);
            this._totalsBreakdownPanel.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawRectangle(p, 0, 0, this._totalsBreakdownPanel.Width - 1, this._totalsBreakdownPanel.Height - 1);
            };

            this._tlpTotals.Dock = DockStyle.Fill;
            this._tlpTotals.ColumnCount = 4;
            this._tlpTotals.RowCount = 4;
            this._tlpTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            this._tlpTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            this._tlpTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            this._tlpTotals.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));

            this._tlpTotals.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            this._tlpTotals.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            this._tlpTotals.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            this._tlpTotals.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

            // Row 0: Consultation Fee & Clinic Medications
            this._lblConsultFeeTitle.Text = "Consultation Fee:";
            this._lblConsultFeeTitle.Font = new Font("Segoe UI", 9F);
            this._lblConsultFeeTitle.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblConsultFeeTitle.Dock = DockStyle.Fill;
            this._lblConsultFeeTitle.TextAlign = ContentAlignment.MiddleLeft;

            this._lblConsultFeeValue.Text = "₱0.00";
            this._lblConsultFeeValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblConsultFeeValue.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblConsultFeeValue.Dock = DockStyle.Fill;
            this._lblConsultFeeValue.TextAlign = ContentAlignment.MiddleRight;

            this._lblPharmacyFeeTitle.Text = "Clinic Medications:";
            this._lblPharmacyFeeTitle.Font = new Font("Segoe UI", 9F);
            this._lblPharmacyFeeTitle.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblPharmacyFeeTitle.Dock = DockStyle.Fill;
            this._lblPharmacyFeeTitle.TextAlign = ContentAlignment.MiddleLeft;

            this._lblPharmacyFeeValue.Text = "₱0.00";
            this._lblPharmacyFeeValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblPharmacyFeeValue.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblPharmacyFeeValue.Dock = DockStyle.Fill;
            this._lblPharmacyFeeValue.TextAlign = ContentAlignment.MiddleRight;

            this._tlpTotals.Controls.Add(this._lblConsultFeeTitle, 0, 0);
            this._tlpTotals.Controls.Add(this._lblConsultFeeValue, 1, 0);
            this._tlpTotals.Controls.Add(this._lblPharmacyFeeTitle, 2, 0);
            this._tlpTotals.Controls.Add(this._lblPharmacyFeeValue, 3, 0);

            // Row 1: Subtotal Gross & Statutory Discount Amount
            this._lblSubtotalTitle.Text = "Subtotal Gross:";
            this._lblSubtotalTitle.Font = new Font("Segoe UI", 9F);
            this._lblSubtotalTitle.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblSubtotalTitle.Dock = DockStyle.Fill;
            this._lblSubtotalTitle.TextAlign = ContentAlignment.MiddleLeft;

            this._lblSubtotalValue.Text = "₱0.00";
            this._lblSubtotalValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblSubtotalValue.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblSubtotalValue.Dock = DockStyle.Fill;
            this._lblSubtotalValue.TextAlign = ContentAlignment.MiddleRight;

            this._lblDiscountTypeTitle.Text = "Discount Deduction:";
            this._lblDiscountTypeTitle.Font = new Font("Segoe UI", 9F);
            this._lblDiscountTypeTitle.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblDiscountTypeTitle.Dock = DockStyle.Fill;
            this._lblDiscountTypeTitle.TextAlign = ContentAlignment.MiddleLeft;

            this._lblDiscountAmountValue.Text = "-₱0.00";
            this._lblDiscountAmountValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._lblDiscountAmountValue.ForeColor = Color.FromArgb(220, 38, 38);
            this._lblDiscountAmountValue.Dock = DockStyle.Fill;
            this._lblDiscountAmountValue.TextAlign = ContentAlignment.MiddleRight;

            this._tlpTotals.Controls.Add(this._lblSubtotalTitle, 0, 1);
            this._tlpTotals.Controls.Add(this._lblSubtotalValue, 1, 1);
            this._tlpTotals.Controls.Add(this._lblDiscountTypeTitle, 2, 1);
            this._tlpTotals.Controls.Add(this._lblDiscountAmountValue, 3, 1);

            // Row 2: Statutory Discount Category Dropdown
            var lblCategoryTitle = new Label
            {
                Text = "Statutory Discount:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            this._cboDiscountType.Dock = DockStyle.Fill;
            this._cboDiscountType.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cboDiscountType.Font = new Font("Segoe UI", 9F);

            this._tlpTotals.Controls.Add(lblCategoryTitle, 0, 2);
            this._tlpTotals.Controls.Add(this._cboDiscountType, 1, 2);
            this._tlpTotals.SetColumnSpan(this._cboDiscountType, 2);

            // Row 3: NET TOTAL DUE
            this._lblTotalDueTitle.Text = "NET TOTAL DUE:";
            this._lblTotalDueTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this._lblTotalDueTitle.ForeColor = Color.FromArgb(15, 118, 110);
            this._lblTotalDueTitle.Dock = DockStyle.Fill;
            this._lblTotalDueTitle.TextAlign = ContentAlignment.MiddleLeft;

            this._lblTotalDueValue.Text = "₱0.00";
            this._lblTotalDueValue.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this._lblTotalDueValue.ForeColor = Color.FromArgb(15, 118, 110);
            this._lblTotalDueValue.Dock = DockStyle.Fill;
            this._lblTotalDueValue.TextAlign = ContentAlignment.MiddleRight;

            this._tlpTotals.Controls.Add(this._lblTotalDueTitle, 0, 3);
            this._tlpTotals.SetColumnSpan(this._lblTotalDueTitle, 2);
            this._tlpTotals.Controls.Add(this._lblTotalDueValue, 2, 3);
            this._tlpTotals.SetColumnSpan(this._lblTotalDueValue, 2);

            this._totalsBreakdownPanel.Controls.Add(this._tlpTotals);

            // Payment Action & Cashier Tender Panel
            this._paymentActionPanel.Dock = DockStyle.Bottom;
            this._paymentActionPanel.Height = 150;
            this._paymentActionPanel.BackColor = Color.White;
            this._paymentActionPanel.Padding = new Padding(12, 10, 12, 8);

            this._lblPaymentMethodTitle.Text = "Payment Method:";
            this._lblPaymentMethodTitle.Location = new Point(12, 10);
            this._lblPaymentMethodTitle.Size = new Size(118, 22);
            this._lblPaymentMethodTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            this._cboPaymentMethod.Location = new Point(134, 8);
            this._cboPaymentMethod.Size = new Size(180, 26);
            this._cboPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cboPaymentMethod.Font = new Font("Segoe UI", 9F);

            this._lblTenderedTitle.Text = "Amount Tendered:";
            this._lblTenderedTitle.Location = new Point(12, 44);
            this._lblTenderedTitle.Size = new Size(118, 22);
            this._lblTenderedTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            this._txtAmountTendered.Location = new Point(134, 40);
            this._txtAmountTendered.Size = new Size(130, 29);
            this._txtAmountTendered.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this._txtAmountTendered.Text = "0.00";

            this._quickTenderPanel.Location = new Point(270, 40);
            this._quickTenderPanel.Size = new Size(240, 32);
            this._quickTenderPanel.WrapContents = false;

            this._btnQuickExact.Text = "Exact";
            this._btnQuickExact.Size = new Size(54, 28);
            this._btnQuickExact.FlatStyle = FlatStyle.Flat;
            this._btnQuickExact.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            this._btnQuickExact.Cursor = Cursors.Hand;

            this._btnQuick500.Text = "₱500";
            this._btnQuick500.Size = new Size(54, 28);
            this._btnQuick500.FlatStyle = FlatStyle.Flat;
            this._btnQuick500.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            this._btnQuick500.Cursor = Cursors.Hand;

            this._btnQuick1000.Text = "₱1k";
            this._btnQuick1000.Size = new Size(54, 28);
            this._btnQuick1000.FlatStyle = FlatStyle.Flat;
            this._btnQuick1000.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            this._btnQuick1000.Cursor = Cursors.Hand;

            this._btnQuick2000.Text = "₱2k";
            this._btnQuick2000.Size = new Size(54, 28);
            this._btnQuick2000.FlatStyle = FlatStyle.Flat;
            this._btnQuick2000.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            this._btnQuick2000.Cursor = Cursors.Hand;

            this._quickTenderPanel.Controls.Add(this._btnQuickExact);
            this._quickTenderPanel.Controls.Add(this._btnQuick500);
            this._quickTenderPanel.Controls.Add(this._btnQuick1000);
            this._quickTenderPanel.Controls.Add(this._btnQuick2000);

            this._lblChangeTitle.Text = "Change Due:";
            this._lblChangeTitle.Location = new Point(12, 78);
            this._lblChangeTitle.Size = new Size(118, 24);
            this._lblChangeTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this._lblChangeValue.Text = "₱0.00";
            this._lblChangeValue.Location = new Point(134, 76);
            this._lblChangeValue.Size = new Size(180, 24);
            this._lblChangeValue.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._lblChangeValue.ForeColor = Color.FromArgb(5, 150, 105);

            this._lblNotesTitle.Text = "Notes / Ref #:";
            this._lblNotesTitle.Location = new Point(12, 110);
            this._lblNotesTitle.Size = new Size(118, 22);
            this._lblNotesTitle.Font = new Font("Segoe UI", 8.5F);
            this._lblNotesTitle.ForeColor = Color.FromArgb(100, 116, 139);

            this._txtPaymentNotes.Location = new Point(134, 108);
            this._txtPaymentNotes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this._txtPaymentNotes.Width = this._paymentActionPanel.Width - 150;
            this._txtPaymentNotes.Font = new Font("Segoe UI", 9F);
            this._txtPaymentNotes.PlaceholderText = "Optional GCash reference # or cashier remarks...";

            this._paymentActionPanel.Controls.Add(this._txtPaymentNotes);
            this._paymentActionPanel.Controls.Add(this._lblNotesTitle);
            this._paymentActionPanel.Controls.Add(this._lblChangeValue);
            this._paymentActionPanel.Controls.Add(this._lblChangeTitle);
            this._paymentActionPanel.Controls.Add(this._quickTenderPanel);
            this._paymentActionPanel.Controls.Add(this._txtAmountTendered);
            this._paymentActionPanel.Controls.Add(this._lblTenderedTitle);
            this._paymentActionPanel.Controls.Add(this._cboPaymentMethod);
            this._paymentActionPanel.Controls.Add(this._lblPaymentMethodTitle);

            // Bottom Buttons Panel
            this._bottomButtonPanel.Dock = DockStyle.Bottom;
            this._bottomButtonPanel.Height = 56;
            this._bottomButtonPanel.BackColor = Color.White;
            this._bottomButtonPanel.Padding = new Padding(0, 8, 0, 0);

            this._btnProcessPayment.Text = "💳 Settle & Issue Official Receipt (OR)";
            this._btnProcessPayment.UseMnemonic = false;
            this._btnProcessPayment.Dock = DockStyle.Fill;
            this._btnProcessPayment.Height = 44;
            this._btnProcessPayment.BackColor = Color.FromArgb(5, 150, 105); // Emerald Green
            this._btnProcessPayment.ForeColor = Color.White;
            this._btnProcessPayment.FlatStyle = FlatStyle.Flat;
            this._btnProcessPayment.FlatAppearance.BorderSize = 0;
            this._btnProcessPayment.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this._btnProcessPayment.Cursor = Cursors.Hand;
            this._btnProcessPayment.Enabled = false;

            this._btnClearLedger.Text = "✕ Clear";
            this._btnClearLedger.Dock = DockStyle.Right;
            this._btnClearLedger.Width = 90;
            this._btnClearLedger.FlatStyle = FlatStyle.Flat;
            this._btnClearLedger.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            this._btnClearLedger.FlatAppearance.BorderSize = 1;
            this._btnClearLedger.BackColor = Color.White;
            this._btnClearLedger.ForeColor = Color.FromArgb(100, 116, 139);
            this._btnClearLedger.Font = new Font("Segoe UI", 9F);
            this._btnClearLedger.Cursor = Cursors.Hand;

            this._bottomButtonPanel.Controls.Add(this._btnProcessPayment);
            this._bottomButtonPanel.Controls.Add(this._btnClearLedger);

            // Assemble Right Panel
            this._rightLedgerPanel.Controls.Add(this._gridBillItems);
            this._rightLedgerPanel.Controls.Add(this._totalsBreakdownPanel);
            this._rightLedgerPanel.Controls.Add(this._paymentActionPanel);
            this._rightLedgerPanel.Controls.Add(this._bottomButtonPanel);
            this._rightLedgerPanel.Controls.Add(this._patientCardPanel);
            this._rightLedgerPanel.Controls.Add(this._ledgerHeaderPanel);

            this._mainSplitContainer.Panel2.Controls.Add(this._rightLedgerPanel);

            // BillingForm
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1240, 780);
            this.Controls.Add(this._mainSplitContainer);
            this.Name = "BillingForm";
            this.Text = "Billing & Cashier";

            this._mainSplitContainer.Panel1.ResumeLayout(false);
            this._mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._mainSplitContainer)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}

