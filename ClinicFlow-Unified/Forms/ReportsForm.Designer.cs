namespace ClinicFlow_Unified.Forms;

partial class ReportsForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Panel _headerPanel;
    private System.Windows.Forms.Label _lblTitle;
    private System.Windows.Forms.Label _lblSubtitle;

    private System.Windows.Forms.Panel _filterCard;
    private System.Windows.Forms.Label _lblReportType;
    private System.Windows.Forms.ComboBox _cboReportType;
    private System.Windows.Forms.Label _lblDateFrom;
    private System.Windows.Forms.DateTimePicker _dtpFrom;
    private System.Windows.Forms.Label _lblDateTo;
    private System.Windows.Forms.DateTimePicker _dtpTo;
    private System.Windows.Forms.Label _lblSecondaryFilter;
    private System.Windows.Forms.ComboBox _cboSecondaryFilter;
    private System.Windows.Forms.Button _btnGenerate;
    private System.Windows.Forms.Button _btnExportCsv;
    private System.Windows.Forms.Button _btnPrint;

    private System.Windows.Forms.Panel _kpiContainer;
    private System.Windows.Forms.Panel _cardKpi1;
    private System.Windows.Forms.Label _lblKpi1Title;
    private System.Windows.Forms.Label _lblKpi1Value;
    private System.Windows.Forms.Panel _cardKpi2;
    private System.Windows.Forms.Label _lblKpi2Title;
    private System.Windows.Forms.Label _lblKpi2Value;
    private System.Windows.Forms.Panel _cardKpi3;
    private System.Windows.Forms.Label _lblKpi3Title;
    private System.Windows.Forms.Label _lblKpi3Value;
    private System.Windows.Forms.Panel _cardKpi4;
    private System.Windows.Forms.Label _lblKpi4Title;
    private System.Windows.Forms.Label _lblKpi4Value;

    private System.Windows.Forms.Panel _chartPanel;
    private System.Windows.Forms.Panel _gridPanel;
    private System.Windows.Forms.DataGridView _gridReport;
    private System.Windows.Forms.Label _lblEmptyState;

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
        this._headerPanel = new System.Windows.Forms.Panel();
        this._lblTitle = new System.Windows.Forms.Label();
        this._lblSubtitle = new System.Windows.Forms.Label();

        this._filterCard = new System.Windows.Forms.Panel();
        this._lblReportType = new System.Windows.Forms.Label();
        this._cboReportType = new System.Windows.Forms.ComboBox();
        this._lblDateFrom = new System.Windows.Forms.Label();
        this._dtpFrom = new System.Windows.Forms.DateTimePicker();
        this._lblDateTo = new System.Windows.Forms.Label();
        this._dtpTo = new System.Windows.Forms.DateTimePicker();
        this._lblSecondaryFilter = new System.Windows.Forms.Label();
        this._cboSecondaryFilter = new System.Windows.Forms.ComboBox();
        this._btnGenerate = new System.Windows.Forms.Button();
        this._btnExportCsv = new System.Windows.Forms.Button();
        this._btnPrint = new System.Windows.Forms.Button();

        this._kpiContainer = new System.Windows.Forms.Panel();
        this._cardKpi1 = new System.Windows.Forms.Panel();
        this._lblKpi1Title = new System.Windows.Forms.Label();
        this._lblKpi1Value = new System.Windows.Forms.Label();

        this._cardKpi2 = new System.Windows.Forms.Panel();
        this._lblKpi2Title = new System.Windows.Forms.Label();
        this._lblKpi2Value = new System.Windows.Forms.Label();

        this._cardKpi3 = new System.Windows.Forms.Panel();
        this._lblKpi3Title = new System.Windows.Forms.Label();
        this._lblKpi3Value = new System.Windows.Forms.Label();

        this._cardKpi4 = new System.Windows.Forms.Panel();
        this._lblKpi4Title = new System.Windows.Forms.Label();
        this._lblKpi4Value = new System.Windows.Forms.Label();

        this._chartPanel = new System.Windows.Forms.Panel();
        this._gridPanel = new System.Windows.Forms.Panel();
        this._gridReport = new System.Windows.Forms.DataGridView();
        this._lblEmptyState = new System.Windows.Forms.Label();

        this.SuspendLayout();

        // _headerPanel
        this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this._headerPanel.Height = 70;
        this._headerPanel.Padding = new System.Windows.Forms.Padding(24, 16, 24, 8);
        this._headerPanel.BackColor = System.Drawing.Color.White;

        this._lblTitle.Text = "ClinicFlow Analytics & Reporting Engine";
        this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        this._lblTitle.Dock = System.Windows.Forms.DockStyle.Top;

        this._lblSubtitle.Text = "Generate aggregated transaction reports with grouping, totals, date filters, and multi-format export";
        this._lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular);
        this._lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        this._lblSubtitle.Dock = System.Windows.Forms.DockStyle.Bottom;

        this._headerPanel.Controls.Add(this._lblTitle);
        this._headerPanel.Controls.Add(this._lblSubtitle);

        // _filterCard
        this._filterCard.Dock = System.Windows.Forms.DockStyle.Top;
        this._filterCard.Height = 80;
        this._filterCard.Padding = new System.Windows.Forms.Padding(24, 12, 24, 12);
        this._filterCard.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);

        // Controls inside _filterCard
        this._lblReportType.Text = "Report Type:";
        this._lblReportType.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this._lblReportType.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblReportType.Location = new System.Drawing.Point(24, 12);
        this._lblReportType.Size = new System.Drawing.Size(180, 16);

        this._cboReportType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cboReportType.Location = new System.Drawing.Point(24, 32);
        this._cboReportType.Size = new System.Drawing.Size(260, 28);
        this._cboReportType.Font = new System.Drawing.Font("Segoe UI", 9F);

        this._lblDateFrom.Text = "From Date:";
        this._lblDateFrom.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this._lblDateFrom.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblDateFrom.Location = new System.Drawing.Point(296, 12);
        this._lblDateFrom.Size = new System.Drawing.Size(110, 16);

        this._dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this._dtpFrom.Location = new System.Drawing.Point(296, 32);
        this._dtpFrom.Size = new System.Drawing.Size(120, 28);
        this._dtpFrom.Font = new System.Drawing.Font("Segoe UI", 9F);

        this._lblDateTo.Text = "To Date:";
        this._lblDateTo.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this._lblDateTo.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblDateTo.Location = new System.Drawing.Point(428, 12);
        this._lblDateTo.Size = new System.Drawing.Size(110, 16);

        this._dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this._dtpTo.Location = new System.Drawing.Point(428, 32);
        this._dtpTo.Size = new System.Drawing.Size(120, 28);
        this._dtpTo.Font = new System.Drawing.Font("Segoe UI", 9F);

        this._lblSecondaryFilter.Text = "Category / Method:";
        this._lblSecondaryFilter.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
        this._lblSecondaryFilter.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblSecondaryFilter.Location = new System.Drawing.Point(560, 12);
        this._lblSecondaryFilter.Size = new System.Drawing.Size(150, 16);

        this._cboSecondaryFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cboSecondaryFilter.Location = new System.Drawing.Point(560, 32);
        this._cboSecondaryFilter.Size = new System.Drawing.Size(170, 28);
        this._cboSecondaryFilter.Font = new System.Drawing.Font("Segoe UI", 9F);

        this._btnGenerate.Text = "⚡ Generate";
        this._btnGenerate.Location = new System.Drawing.Point(744, 30);
        this._btnGenerate.Size = new System.Drawing.Size(100, 32);
        this._btnGenerate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this._btnGenerate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
        this._btnGenerate.ForeColor = System.Drawing.Color.White;
        this._btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnGenerate.FlatAppearance.BorderSize = 0;
        this._btnGenerate.Cursor = System.Windows.Forms.Cursors.Hand;

        this._btnExportCsv.Text = "📥 CSV";
        this._btnExportCsv.Location = new System.Drawing.Point(852, 30);
        this._btnExportCsv.Size = new System.Drawing.Size(80, 32);
        this._btnExportCsv.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this._btnExportCsv.BackColor = System.Drawing.Color.FromArgb(16, 185, 129); // Emerald
        this._btnExportCsv.ForeColor = System.Drawing.Color.White;
        this._btnExportCsv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnExportCsv.FlatAppearance.BorderSize = 0;
        this._btnExportCsv.Cursor = System.Windows.Forms.Cursors.Hand;

        this._btnPrint.Text = "🖨️ Print";
        this._btnPrint.Location = new System.Drawing.Point(940, 30);
        this._btnPrint.Size = new System.Drawing.Size(80, 32);
        this._btnPrint.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this._btnPrint.BackColor = System.Drawing.Color.FromArgb(79, 70, 229); // Indigo
        this._btnPrint.ForeColor = System.Drawing.Color.White;
        this._btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this._btnPrint.FlatAppearance.BorderSize = 0;
        this._btnPrint.Cursor = System.Windows.Forms.Cursors.Hand;

        this._filterCard.Controls.Add(this._lblReportType);
        this._filterCard.Controls.Add(this._cboReportType);
        this._filterCard.Controls.Add(this._lblDateFrom);
        this._filterCard.Controls.Add(this._dtpFrom);
        this._filterCard.Controls.Add(this._lblDateTo);
        this._filterCard.Controls.Add(this._dtpTo);
        this._filterCard.Controls.Add(this._lblSecondaryFilter);
        this._filterCard.Controls.Add(this._cboSecondaryFilter);
        this._filterCard.Controls.Add(this._btnGenerate);
        this._filterCard.Controls.Add(this._btnExportCsv);
        this._filterCard.Controls.Add(this._btnPrint);

        // _kpiContainer
        this._kpiContainer.Dock = System.Windows.Forms.DockStyle.Top;
        this._kpiContainer.Height = 84;
        this._kpiContainer.Padding = new System.Windows.Forms.Padding(24, 8, 24, 8);
        this._kpiContainer.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);

        // Setup 4 KPI cards with modern styling
        void SetupKpiCard(System.Windows.Forms.Panel card, System.Windows.Forms.Label title, System.Windows.Forms.Label val, int left)
        {
            card.Size = new System.Drawing.Size(235, 68);
            card.Location = new System.Drawing.Point(left, 8);
            card.BackColor = System.Drawing.Color.White;
            card.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            card.Paint += (s, e) => {
                var rect = new System.Drawing.Rectangle(0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.FromArgb(226, 232, 240)), rect);
            };

            title.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            title.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            title.Dock = System.Windows.Forms.DockStyle.Top;
            title.Height = 18;

            val.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            val.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            val.Dock = System.Windows.Forms.DockStyle.Fill;

            card.Controls.Add(val);
            card.Controls.Add(title);
        }

        SetupKpiCard(this._cardKpi1, this._lblKpi1Title, this._lblKpi1Value, 24);
        SetupKpiCard(this._cardKpi2, this._lblKpi2Title, this._lblKpi2Value, 275);
        SetupKpiCard(this._cardKpi3, this._lblKpi3Title, this._lblKpi3Value, 526);
        SetupKpiCard(this._cardKpi4, this._lblKpi4Title, this._lblKpi4Value, 777);

        this._kpiContainer.Controls.Add(this._cardKpi1);
        this._kpiContainer.Controls.Add(this._cardKpi2);
        this._kpiContainer.Controls.Add(this._cardKpi3);
        this._kpiContainer.Controls.Add(this._cardKpi4);

        // _chartPanel (Visual Distribution Breakdown Chart)
        this._chartPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this._chartPanel.Height = 54;
        this._chartPanel.Padding = new System.Windows.Forms.Padding(24, 4, 24, 4);
        this._chartPanel.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);

        // _gridPanel
        this._gridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridPanel.Padding = new System.Windows.Forms.Padding(24, 8, 24, 20);
        this._gridPanel.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);

        this._gridReport.Dock = System.Windows.Forms.DockStyle.Fill;
        this._gridReport.BackgroundColor = System.Drawing.Color.White;
        this._gridReport.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this._gridReport.ReadOnly = true;
        this._gridReport.AllowUserToAddRows = false;
        this._gridReport.AllowUserToDeleteRows = false;
        this._gridReport.RowHeadersVisible = false;
        this._gridReport.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this._gridReport.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

        this._lblEmptyState.Text = "No transactions found matching the selected filter criteria.";
        this._lblEmptyState.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular);
        this._lblEmptyState.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
        this._lblEmptyState.Dock = System.Windows.Forms.DockStyle.Fill;
        this._lblEmptyState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this._lblEmptyState.Visible = false;

        this._gridPanel.Controls.Add(this._lblEmptyState);
        this._gridPanel.Controls.Add(this._gridReport);

        // ReportsForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1040, 720);
        this.Controls.Add(this._gridPanel);
        this.Controls.Add(this._chartPanel);
        this.Controls.Add(this._kpiContainer);
        this.Controls.Add(this._filterCard);
        this.Controls.Add(this._headerPanel);
        this.Name = "ReportsForm";
        this.Text = "Reports & Analytics - ClinicFlow";
        this.Load += ReportsForm_Load;

        this.ResumeLayout(false);
    }

    #endregion
}
