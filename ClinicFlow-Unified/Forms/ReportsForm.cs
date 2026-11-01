#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class ReportsForm : Form
{
    private readonly ReportsRepository _reportsRepo = new();
    private List<(int DoctorId, string DisplayName)> _doctorOptions = new();

    private DataTable? _currentDataTable;
    private string _currentReportTitle = "Daily Clinic Revenue & Collections";
    private int _printCurrentRowIndex = 0;
    private readonly List<(string Label, decimal Value, Color BarColor)> _chartSegments = new();

    public ReportsForm()
    {
        InitializeComponent();
        ClinicTheme.ApplyForm(this);
        ClinicTheme.ApplyGrid(_gridReport);
        _chartPanel.Paint += ChartPanel_Paint;
    }

    private async void ReportsForm_Load(object? sender, EventArgs e)
    {
        InitializeDropdowns();
        await LoadDoctorFilterAsync();
        _cboReportType.SelectedIndex = 0;
        await GenerateReportAsync();
    }

    private void InitializeDropdowns()
    {
        _cboReportType.Items.Clear();
        _cboReportType.Items.Add("💳 Daily Clinic Revenue & Collections");
        _cboReportType.Items.Add("💊 Central Pharmacy Inventory & Expiry Watch");
        _cboReportType.Items.Add("👨‍⚕️ Physician Consultations & Diagnostics");

        _dtpFrom.Value = DateTime.Today.AddDays(-30);
        _dtpTo.Value = DateTime.Today;

        _cboReportType.SelectedIndexChanged += CboReportType_SelectedIndexChanged;
        _btnGenerate.Click += async (_, _) => await GenerateReportAsync();
        _btnExportCsv.Click += async (_, _) => await ExportCsvAsync();
        _btnPrint.Click += (_, _) => ShowPrintPreview();
    }

    private async Task LoadDoctorFilterAsync()
    {
        try
        {
            _doctorOptions = await _reportsRepo.GetDoctorFilterOptionsAsync();
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "ReportsForm.LoadDoctorFilterAsync");
        }
    }

    private void CboReportType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _cboSecondaryFilter.Items.Clear();

        switch (_cboReportType.SelectedIndex)
        {
            case 0: // Revenue
                _lblSecondaryFilter.Text = "Payment Method:";
                _dtpFrom.Enabled = true;
                _dtpTo.Enabled = true;
                _cboSecondaryFilter.Items.AddRange(new object[] { "All", "Cash", "GCash", "Credit Card", "Debit Card" });
                _cboSecondaryFilter.SelectedIndex = 0;
                break;

            case 1: // Inventory
                _lblSecondaryFilter.Text = "Stock Status:";
                _dtpFrom.Enabled = false;
                _dtpTo.Enabled = false;
                _cboSecondaryFilter.Items.AddRange(new object[] { "All", "Low Stock", "Expired", "Near Expiry (<60d)", "Optimal" });
                _cboSecondaryFilter.SelectedIndex = 0;
                break;

            case 2: // Doctor Consultations
                _lblSecondaryFilter.Text = "Attending Physician:";
                _dtpFrom.Enabled = true;
                _dtpTo.Enabled = true;
                _cboSecondaryFilter.Items.Add("All Doctors");
                foreach (var doc in _doctorOptions)
                {
                    _cboSecondaryFilter.Items.Add(doc.DisplayName);
                }
                _cboSecondaryFilter.SelectedIndex = 0;
                break;
        }
    }

    public async Task GenerateReportAsync()
    {
        _btnGenerate.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            switch (_cboReportType.SelectedIndex)
            {
                case 0:
                    await GenerateRevenueReportAsync();
                    break;
                case 1:
                    await GenerateInventoryReportAsync();
                    break;
                case 2:
                    await GenerateConsultationReportAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "ReportsForm.GenerateReportAsync");
            MessageBox.Show(ErrorLogger.GetFriendlyMessage(ex), "Report Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _btnGenerate.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    // =========================================================================
    // 1. REVENUE REPORT
    // =========================================================================
    private async Task GenerateRevenueReportAsync()
    {
        _currentReportTitle = "Daily Clinic Revenue & Collections Report";
        string? method = _cboSecondaryFilter.SelectedItem?.ToString();
        var (items, summary) = await _reportsRepo.GetDailyRevenueReportAsync(_dtpFrom.Value, _dtpTo.Value, method);

        // Update KPIs
        _lblKpi1Title.Text = "TOTAL TRANSACTIONS";
        _lblKpi1Value.Text = summary.TotalTransactions.ToString("N0");

        _lblKpi2Title.Text = "NET COLLECTIONS (PHP)";
        _lblKpi2Value.Text = $"₱ {summary.NetRevenueCollected:N2}";

        _lblKpi3Title.Text = "CASH SUB-TOTAL";
        _lblKpi3Value.Text = $"₱ {summary.CashCollections:N2}";

        _lblKpi4Title.Text = "DIGITAL (GCASH/CARD)";
        _lblKpi4Value.Text = $"₱ {(summary.GCashCollections + summary.CardCollections):N2}";

        // Convert to DataTable
        var dt = new DataTable();
        dt.Columns.Add("Receipt #", typeof(string));
        dt.Columns.Add("Invoice #", typeof(string));
        dt.Columns.Add("Payment Date", typeof(DateTime));
        dt.Columns.Add("Patient Name", typeof(string));
        dt.Columns.Add("Attending Doctor", typeof(string));
        dt.Columns.Add("Method", typeof(string));
        dt.Columns.Add("Subtotal", typeof(decimal));
        dt.Columns.Add("Discount", typeof(decimal));
        dt.Columns.Add("Amount Paid", typeof(decimal));

        foreach (var item in items)
        {
            dt.Rows.Add(
                item.ReceiptNumber,
                item.InvoiceNumber,
                item.PaymentDate,
                item.PatientName,
                item.DoctorName,
                item.PaymentMethod,
                item.SubTotal,
                item.DiscountAmount,
                item.AmountPaid
            );
        }

        _currentDataTable = dt;
        _gridReport.DataSource = dt;

        // Visual Breakdown Chart Data
        _chartSegments.Clear();
        if (summary.CashCollections > 0)
            _chartSegments.Add(("Cash", summary.CashCollections, Color.FromArgb(16, 185, 129)));
        if (summary.GCashCollections > 0)
            _chartSegments.Add(("GCash", summary.GCashCollections, Color.FromArgb(14, 165, 233)));
        if (summary.CardCollections > 0)
            _chartSegments.Add(("Card", summary.CardCollections, Color.FromArgb(139, 92, 246)));
        if (summary.TotalDiscountsGranted > 0)
            _chartSegments.Add(("Discounts", summary.TotalDiscountsGranted, Color.FromArgb(245, 158, 11)));
        _chartPanel.Invalidate();

        FormatGridForRevenue();
        UpdateEmptyState(dt.Rows.Count == 0);
    }

    private void FormatGridForRevenue()
    {
        if (_gridReport.Columns["Payment Date"] is { } colDate)
            colDate.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";

        if (_gridReport.Columns["Subtotal"] is { } colSubtotal)
            colSubtotal.DefaultCellStyle.Format = "₱ #,##0.00";

        if (_gridReport.Columns["Discount"] is { } colDiscount)
            colDiscount.DefaultCellStyle.Format = "₱ #,##0.00";

        if (_gridReport.Columns["Amount Paid"] is { } colAmountPaid)
        {
            colAmountPaid.DefaultCellStyle.Format = "₱ #,##0.00";
            colAmountPaid.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            colAmountPaid.DefaultCellStyle.ForeColor = Color.FromArgb(16, 185, 129); // Emerald
        }
    }

    // =========================================================================
    // 2. INVENTORY REPORT
    // =========================================================================
    private async Task GenerateInventoryReportAsync()
    {
        _currentReportTitle = "Central Pharmacy Stock Level & Expiry Risk Report";
        string? status = _cboSecondaryFilter.SelectedItem?.ToString();
        var (items, summary) = await _reportsRepo.GetPharmacyInventoryReportAsync(status);

        // Update KPIs
        _lblKpi1Title.Text = "TOTAL BATCHES";
        _lblKpi1Value.Text = summary.TotalBatchesMonitored.ToString("N0");

        _lblKpi2Title.Text = "INVENTORY VALUATION";
        _lblKpi2Value.Text = $"₱ {summary.TotalValuation:N2}";

        _lblKpi3Title.Text = "LOW STOCK BATCHES";
        _lblKpi3Value.Text = summary.LowStockBatchCount.ToString("N0");
        _lblKpi3Value.ForeColor = summary.LowStockBatchCount > 0 ? Color.FromArgb(217, 119, 6) : Color.FromArgb(15, 23, 42);

        _lblKpi4Title.Text = "EXPIRED / NEAR EXPIRY";
        _lblKpi4Value.Text = (summary.ExpiredBatchCount + summary.NearExpiryBatchCount).ToString("N0");
        _lblKpi4Value.ForeColor = (summary.ExpiredBatchCount + summary.NearExpiryBatchCount) > 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(15, 23, 42);

        var dt = new DataTable();
        dt.Columns.Add("Batch #", typeof(string));
        dt.Columns.Add("Generic Name", typeof(string));
        dt.Columns.Add("Brand Name", typeof(string));
        dt.Columns.Add("Dosage & Form", typeof(string));
        dt.Columns.Add("Stock Qty", typeof(int));
        dt.Columns.Add("Reorder Lvl", typeof(int));
        dt.Columns.Add("Unit Price", typeof(decimal));
        dt.Columns.Add("Total Value", typeof(decimal));
        dt.Columns.Add("Expiry Date", typeof(DateTime));
        dt.Columns.Add("Days to Expiry", typeof(int));
        dt.Columns.Add("Status", typeof(string));

        foreach (var item in items)
        {
            dt.Rows.Add(
                item.BatchNumber,
                item.GenericName,
                item.BrandName,
                $"{item.Strength} {item.DosageForm}",
                item.QuantityOnHand,
                item.ReorderLevel,
                item.UnitPrice,
                item.TotalInventoryValue,
                item.ExpiryDate,
                item.DaysUntilExpiry,
                item.StockStatus
            );
        }

        _currentDataTable = dt;
        _gridReport.DataSource = dt;

        // Visual Breakdown Chart Data
        _chartSegments.Clear();
        if (summary.OptimalBatchCount > 0)
            _chartSegments.Add(("Optimal", summary.OptimalBatchCount, Color.FromArgb(16, 185, 129)));
        if (summary.LowStockBatchCount > 0)
            _chartSegments.Add(("Low Stock", summary.LowStockBatchCount, Color.FromArgb(245, 158, 11)));
        if (summary.NearExpiryBatchCount > 0)
            _chartSegments.Add(("Near Expiry", summary.NearExpiryBatchCount, Color.FromArgb(244, 63, 94)));
        if (summary.ExpiredBatchCount > 0)
            _chartSegments.Add(("Expired", summary.ExpiredBatchCount, Color.FromArgb(225, 29, 72)));
        _chartPanel.Invalidate();

        FormatGridForInventory();
        UpdateEmptyState(dt.Rows.Count == 0);
    }

    private void FormatGridForInventory()
    {
        if (_gridReport.Columns["Unit Price"] is { } colUnitPrice)
            colUnitPrice.DefaultCellStyle.Format = "₱ #,##0.00";

        if (_gridReport.Columns["Total Value"] is { } colTotalValue)
            colTotalValue.DefaultCellStyle.Format = "₱ #,##0.00";

        if (_gridReport.Columns["Expiry Date"] is { } colExpiry)
            colExpiry.DefaultCellStyle.Format = "yyyy-MM-dd";
    }

    // =========================================================================
    // 3. CONSULTATION REPORT
    // =========================================================================
    private async Task GenerateConsultationReportAsync()
    {
        _currentReportTitle = "Physician Consultations & Diagnostic Activity Report";
        int? doctorId = null;
        if (_cboSecondaryFilter.SelectedIndex > 0 && _cboSecondaryFilter.SelectedIndex - 1 < _doctorOptions.Count)
        {
            doctorId = _doctorOptions[_cboSecondaryFilter.SelectedIndex - 1].DoctorId;
        }

        var (items, summary) = await _reportsRepo.GetDoctorConsultationReportAsync(_dtpFrom.Value, _dtpTo.Value, doctorId);

        // Update KPIs
        _lblKpi1Title.Text = "TOTAL CONSULTATIONS";
        _lblKpi1Value.Text = summary.TotalConsultations.ToString("N0");

        _lblKpi2Title.Text = "TOTAL FEES (PHP)";
        _lblKpi2Value.Text = $"₱ {summary.TotalConsultationFees:N2}";

        _lblKpi3Title.Text = "PATIENTS SERVED";
        _lblKpi3Value.Text = summary.UniquePatientsCount.ToString("N0");

        _lblKpi4Title.Text = "PRESCRIPTIONS ISSUED";
        _lblKpi4Value.Text = summary.TotalPrescriptionItems.ToString("N0");

        var dt = new DataTable();
        dt.Columns.Add("Date", typeof(DateTime));
        dt.Columns.Add("Doctor", typeof(string));
        dt.Columns.Add("Specialty", typeof(string));
        dt.Columns.Add("Patient #", typeof(string));
        dt.Columns.Add("Patient Name", typeof(string));
        dt.Columns.Add("Age/Sex", typeof(string));
        dt.Columns.Add("Chief Complaint", typeof(string));
        dt.Columns.Add("Diagnosis", typeof(string));
        dt.Columns.Add("Rx Items", typeof(int));
        dt.Columns.Add("Fee", typeof(decimal));

        foreach (var item in items)
        {
            dt.Rows.Add(
                item.ConsultationDate,
                item.DoctorName,
                item.Specialty,
                item.PatientNumber,
                item.PatientName,
                $"{item.PatientAge} / {item.PatientSex}",
                item.ChiefComplaint,
                item.Diagnosis,
                item.PrescribedItemsCount,
                item.ConsultationFee
            );
        }

        _currentDataTable = dt;
        _gridReport.DataSource = dt;

        // Visual Breakdown Chart Data
        _chartSegments.Clear();
        var docGroups = items.GroupBy(i => i.DoctorName).Select(g => (Name: g.Key, Count: g.Count())).Take(4).ToList();
        Color[] chartColors = [Color.FromArgb(79, 70, 229), Color.FromArgb(14, 165, 233), Color.FromArgb(16, 185, 129), Color.FromArgb(245, 158, 11)];
        for (int i = 0; i < docGroups.Count; i++)
        {
            _chartSegments.Add((docGroups[i].Name, docGroups[i].Count, chartColors[i % chartColors.Length]));
        }
        _chartPanel.Invalidate();

        FormatGridForConsultation();
        UpdateEmptyState(dt.Rows.Count == 0);
    }

    private void FormatGridForConsultation()
    {
        if (_gridReport.Columns["Date"] is { } colDate)
            colDate.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";

        if (_gridReport.Columns["Fee"] is { } colFee)
            colFee.DefaultCellStyle.Format = "₱ #,##0.00";
    }

    private void UpdateEmptyState(bool isEmpty)
    {
        _lblEmptyState.Visible = isEmpty;
        _gridReport.Visible = !isEmpty;
    }

    // =========================================================================
    // 4. EXPORT TO CSV
    // =========================================================================
    private async Task ExportCsvAsync()
    {
        if (_currentDataTable == null || _currentDataTable.Rows.Count == 0)
        {
            MessageBox.Show("There is no report data available to export.", "Export Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            FileName = $"{_currentReportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            Title = "Export Report to CSV"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                await ReportsRepository.ExportToCsvAsync(_currentDataTable, sfd.FileName);
                MessageBox.Show($"Report exported successfully to:\n{sfd.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "ReportsForm.ExportCsvAsync");
                MessageBox.Show($"Failed to export CSV: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // =========================================================================
    // 5. PRINT PREVIEW (Satisfies REQ-019 / REQ-020 Printable report slip)
    // =========================================================================
    private void ShowPrintPreview()
    {
        if (_currentDataTable == null || _currentDataTable.Rows.Count == 0)
        {
            MessageBox.Show("There is no report data available to print.", "Print Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _printCurrentRowIndex = 0;
        using var printDoc = new PrintDocument();
        printDoc.DefaultPageSettings.Landscape = true;
        printDoc.DocumentName = _currentReportTitle;
        printDoc.PrintPage += PrintDoc_PrintPage;

        using var previewDlg = new PrintPreviewDialog
        {
            Document = printDoc,
            Width = 960,
            Height = 700,
            StartPosition = FormStartPosition.CenterParent
        };

        previewDlg.ShowDialog(this);
    }

    private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
    {
        var g = e.Graphics;
        if (g == null) return;

        int marginX = 40;
        int currentY = 40;

        // Header Title
        using var fontHeader = new Font("Segoe UI", 16F, FontStyle.Bold);
        using var fontSub = new Font("Segoe UI", 9F, FontStyle.Regular);
        using var fontTableHead = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        using var fontTableBody = new Font("Segoe UI", 8F, FontStyle.Regular);

        g.DrawString("CLINICFLOW HEALTH NETWORK — DAVAO MEDICAL PLAZA", fontHeader, Brushes.Black, marginX, currentY);
        currentY += 28;

        g.DrawString($"{_currentReportTitle}  |  Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} by {UserSession.FullName ?? "Staff"}", fontSub, Brushes.DimGray, marginX, currentY);
        currentY += 20;

        // Divider
        g.DrawLine(Pens.LightGray, marginX, currentY, e.PageBounds.Width - marginX, currentY);
        currentY += 15;

        // KPI Summary Block
        string kpiSummary = $"SUMMARY: {_lblKpi1Title.Text}: {_lblKpi1Value.Text}  |  {_lblKpi2Title.Text}: {_lblKpi2Value.Text}  |  {_lblKpi3Title.Text}: {_lblKpi3Value.Text}  |  {_lblKpi4Title.Text}: {_lblKpi4Value.Text}";
        g.DrawString(kpiSummary, fontTableHead, Brushes.DarkSlateBlue, marginX, currentY);
        currentY += 24;

        if (_currentDataTable == null) return;

        // Draw Table Header
        int colCount = Math.Min(_currentDataTable.Columns.Count, 9);
        int colWidth = (e.PageBounds.Width - (marginX * 2)) / colCount;

        g.FillRectangle(new SolidBrush(Color.FromArgb(241, 245, 249)), marginX, currentY, colWidth * colCount, 22);
        for (int i = 0; i < colCount; i++)
        {
            g.DrawString(_currentDataTable.Columns[i].ColumnName, fontTableHead, Brushes.Black, marginX + (i * colWidth) + 4, currentY + 3);
        }
        currentY += 24;

        // Draw Data Rows
        while (_printCurrentRowIndex < _currentDataTable.Rows.Count)
        {
            var row = _currentDataTable.Rows[_printCurrentRowIndex];

            if (currentY + 22 > e.PageBounds.Height - 60)
            {
                e.HasMorePages = true;
                return;
            }

            if (_printCurrentRowIndex % 2 == 1)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(248, 250, 252)), marginX, currentY, colWidth * colCount, 20);
            }

            for (int i = 0; i < colCount; i++)
            {
                string text = row[i]?.ToString() ?? "";
                if (row[i] is DateTime dt) text = dt.ToString("yyyy-MM-dd");
                if (row[i] is decimal dec) text = $"P {dec:N2}";

                // Truncate if long
                if (text.Length > 22) text = text.Substring(0, 20) + "..";

                g.DrawString(text, fontTableBody, Brushes.Black, marginX + (i * colWidth) + 4, currentY + 3);
            }

            currentY += 20;
            _printCurrentRowIndex++;
        }

        // Signature line at bottom
        currentY += 20;
        g.DrawLine(Pens.Gray, marginX, currentY, marginX + 220, currentY);
        g.DrawString("Prepared By: " + (UserSession.FullName ?? "Authorized Personnel"), fontSub, Brushes.Black, marginX, currentY + 4);

        e.HasMorePages = false;
        _printCurrentRowIndex = 0;
    }

    private void ChartPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int padX = 24;
        int w = _chartPanel.ClientSize.Width - (padX * 2);
        int h = _chartPanel.ClientSize.Height - 8;
        if (w <= 50 || h <= 20) return;

        var bounds = new Rectangle(padX, 4, w, h);
        using (var brush = new SolidBrush(Color.White))
        using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
        {
            g.FillRectangle(brush, bounds);
            g.DrawRectangle(pen, bounds);
        }

        using var fontTitle = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var fontItem = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        using var brushText = new SolidBrush(Color.FromArgb(51, 65, 85));

        string title = "📊 Distribution Breakdown:";
        g.DrawString(title, fontTitle, brushText, bounds.X + 12, bounds.Y + (bounds.Height - 16) / 2);

        int barStartX = bounds.X + 180;
        int barWidth = Math.Min(320, bounds.Width - 480);
        int barHeight = 16;
        int barY = bounds.Y + (bounds.Height - barHeight) / 2;

        decimal total = _chartSegments.Sum(s => s.Value);

        if (total > 0 && barWidth > 50)
        {
            using (var trackBrush = new SolidBrush(Color.FromArgb(241, 245, 249)))
            {
                g.FillRectangle(trackBrush, barStartX, barY, barWidth, barHeight);
            }

            int curX = barStartX;
            foreach (var seg in _chartSegments)
            {
                int segWidth = (int)Math.Round((seg.Value / total) * barWidth);
                if (segWidth <= 0) continue;
                if (curX + segWidth > barStartX + barWidth) segWidth = (barStartX + barWidth) - curX;

                using var segBrush = new SolidBrush(seg.BarColor);
                g.FillRectangle(segBrush, curX, barY, segWidth, barHeight);
                curX += segWidth;
            }

            // Legend
            int legendX = barStartX + barWidth + 20;
            int legendY = bounds.Y + (bounds.Height - 16) / 2;

            foreach (var seg in _chartSegments)
            {
                if (legendX + 80 > bounds.Right) break;

                decimal pct = (seg.Value / total) * 100m;
                using var chipBrush = new SolidBrush(seg.BarColor);
                g.FillRectangle(chipBrush, legendX, legendY + 3, 10, 10);

                string label = $"{seg.Label} ({pct:F0}%)";
                g.DrawString(label, fontItem, brushText, legendX + 14, legendY);

                var size = g.MeasureString(label, fontItem);
                legendX += (int)size.Width + 16;
            }
        }
        else
        {
            using var brushMuted = new SolidBrush(Color.FromArgb(148, 163, 184));
            g.DrawString("No data available to display visual distribution chart.", fontItem, brushMuted, barStartX, bounds.Y + (bounds.Height - 16) / 2);
        }
    }
}
