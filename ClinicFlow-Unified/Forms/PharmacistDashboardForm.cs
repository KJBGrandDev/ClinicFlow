#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class PharmacistDashboardForm : Form
{
    private readonly PharmacyRepository _repo;
    private List<PharmacyQueueItem> _queueItems = new();
    private List<PharmacyPrescriptionItem> _currentItems = new();
    private PharmacyQueueItem? _selectedQueueItem;

    private int _currentTabIndex = 0; // 0 = Dispensing, 1 = Inventory, 2 = Audit
    private bool _isInitialized = false;

    public PharmacistDashboardForm()
    {
        _repo = new PharmacyRepository();
        InitializeComponent();

        ClinicTheme.ApplyForm(this);
        ClinicTheme.SetupMnemonicSafeguard(this);

        EnableDoubleBuffer(_dgvQueue);
        EnableDoubleBuffer(_dgvItems);
        EnableDoubleBuffer(_dgvInventory);
        EnableDoubleBuffer(_dgvAudit);
        EnableDoubleBuffer(_dgvDisposal);

        ConfigureGrids();
        WireEvents();
        _isInitialized = true;
    }

    private static void EnableDoubleBuffer(DataGridView grid)
    {
        typeof(DataGridView).InvokeMember("DoubleBuffered",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
            null, grid, new object[] { true });
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ApplyResponsiveLayout();
        UpdateTabSelection(0);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        ApplyResponsiveLayout();
        if (_splitDispensing != null)
        {
            _splitDispensing.SplitterMoved += (_, _) => { };
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        if (!_isInitialized || _splitDispensing == null) return;

        try
        {
            if (_splitDispensing.Width > 500)
            {
                int target = (int)(_splitDispensing.Width * 0.48);
                int minLeft = 320;
                int maxLeft = _splitDispensing.Width - 460;
                if (maxLeft > minLeft)
                {
                    _splitDispensing.SplitterDistance = Math.Clamp(target, minLeft, maxLeft);
                }
            }
        }
        catch { }
    }

    private void ConfigureGrids()
    {
        // Apply uniform theme to all DataGridViews
        ClinicTheme.ApplyGrid(_dgvQueue);
        ClinicTheme.ApplyGrid(_dgvItems);
        ClinicTheme.ApplyGrid(_dgvInventory);
        ClinicTheme.ApplyGrid(_dgvAudit);

        // 1. Queue Grid
        _dgvQueue.AutoGenerateColumns = false;
        _dgvQueue.Columns.Clear();
        _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyQueueItem.ReceiptNumber), HeaderText = "Receipt #", FillWeight = 22, MinimumWidth = 95 });
        _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyQueueItem.PatientName), HeaderText = "Patient Name", FillWeight = 32, MinimumWidth = 120 });
        _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyQueueItem.DoctorName), HeaderText = "Prescribing Doctor", FillWeight = 26, MinimumWidth = 110 });
        _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyQueueItem.ItemCount), HeaderText = "Items", FillWeight = 10, MinimumWidth = 45 });
        _dgvQueue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyQueueItem.PrescriptionStatus), HeaderText = "Status", FillWeight = 22, MinimumWidth = 95 });

        // 2. Prescription Line Items Grid
        _dgvItems.AutoGenerateColumns = false;
        _dgvItems.Columns.Clear();
        _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyPrescriptionItem.MedicineDisplayName), HeaderText = "Medication Formulation & Strength", FillWeight = 42, MinimumWidth = 180 });
        _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyPrescriptionItem.QuantityPrescribed), HeaderText = "Prescribed", FillWeight = 14, MinimumWidth = 65 });
        _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyPrescriptionItem.InHouseStock), HeaderText = "In-House", FillWeight = 14, MinimumWidth = 65 });
        _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyPrescriptionItem.StockStatusText), HeaderText = "Stock Status", FillWeight = 30, MinimumWidth = 120 });

        // 3. Inventory Grid
        _dgvInventory.AutoGenerateColumns = false;
        _dgvInventory.Columns.Clear();
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.GenericName), HeaderText = "Generic Name", FillWeight = 20, MinimumWidth = 130 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.BrandName), HeaderText = "Brand Name", FillWeight = 15, MinimumWidth = 100 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.Category), HeaderText = "Category", FillWeight = 18, MinimumWidth = 130 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.Strength), HeaderText = "Strength", FillWeight = 10, MinimumWidth = 70 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.DosageForm), HeaderText = "Form", FillWeight = 9, MinimumWidth = 60 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.QuantityOnHand), HeaderText = "Stock On Hand", FillWeight = 12, MinimumWidth = 85 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.ReorderLevel), HeaderText = "Reorder Level", FillWeight = 10, MinimumWidth = 75 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.StockStatus), HeaderText = "Stock Status", FillWeight = 12, MinimumWidth = 85 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.UnitPrice), HeaderText = "Unit Price", FillWeight = 10, MinimumWidth = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.BatchNumber), HeaderText = "Batch #", FillWeight = 12, MinimumWidth = 90 });
        _dgvInventory.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PharmacyInventoryItem.ExpiryDate), HeaderText = "Earliest Expiry", FillWeight = 12, MinimumWidth = 95, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } });

        // 4. Audit Grid
        _dgvAudit.AutoGenerateColumns = false;
        _dgvAudit.Columns.Clear();
        _dgvAudit.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _dgvAudit.RowTemplate.MinimumHeight = 38;
        _dgvAudit.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.PrescriptionNumber),
            HeaderText = "Rx Number",
            FillWeight = 14,
            MinimumWidth = 110,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 8, 4, 8) }
        });
        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.ReceiptNumber),
            HeaderText = "Receipt #",
            FillWeight = 13,
            MinimumWidth = 100,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 8, 4, 8) }
        });
        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.PatientName),
            HeaderText = "Patient Name",
            FillWeight = 17,
            MinimumWidth = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Padding = new Padding(4, 8, 4, 8) }
        });
        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.DoctorName),
            HeaderText = "Doctor",
            FillWeight = 15,
            MinimumWidth = 110,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 8, 4, 8) }
        });
        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.MedicineSummary),
            HeaderText = "Medications Dispensed",
            FillWeight = 34,
            MinimumWidth = 220,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                WrapMode = DataGridViewTriState.True,
                Alignment = DataGridViewContentAlignment.TopLeft,
                Padding = new Padding(4, 8, 4, 8)
            }
        });
        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.TotalQuantity),
            HeaderText = "Total Qty",
            FillWeight = 9,
            MinimumWidth = 65,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopCenter, Padding = new Padding(4, 8, 4, 8) }
        });
        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.DispensedAt),
            HeaderText = "Dispensed Time",
            FillWeight = 17,
            MinimumWidth = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "MMM dd, yyyy hh:mm tt", Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 8, 4, 8) }
        });
        _dgvAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(DispensedAuditItem.DispensedBy),
            HeaderText = "Dispensed By",
            FillWeight = 15,
            MinimumWidth = 110,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 8, 4, 8) }
        });

        // 5. Waste & Disposal Audit Grid
        _dgvDisposal.AutoGenerateColumns = false;
        _dgvDisposal.Columns.Clear();
        _dgvDisposal.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _dgvDisposal.RowTemplate.MinimumHeight = 36;
        _dgvDisposal.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.DisposedAt),
            HeaderText = "Destruction Date & Time",
            FillWeight = 16,
            MinimumWidth = 135,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd hh:mm tt", Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.MedicineDisplayName),
            HeaderText = "Medication Formulation",
            FillWeight = 24,
            MinimumWidth = 160,
            DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.BatchNumber),
            HeaderText = "Batch / Lot #",
            FillWeight = 12,
            MinimumWidth = 90,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.ExpiryDate),
            HeaderText = "Lot Expiry",
            FillWeight = 11,
            MinimumWidth = 85,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd", Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.QuantityDisposed),
            HeaderText = "Qty Destroyed",
            FillWeight = 10,
            MinimumWidth = 70,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28), Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.TotalLossAmount),
            HeaderText = "Financial Loss",
            FillWeight = 12,
            MinimumWidth = 85,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.TopRight, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28), Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.Reason),
            HeaderText = "Disposal Reason",
            FillWeight = 18,
            MinimumWidth = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.DisposalMethod),
            HeaderText = "Protocol / Method",
            FillWeight = 18,
            MinimumWidth = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 6, 4, 6) }
        });
        _dgvDisposal.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(MedicineDisposalLogItem.DisposedBy),
            HeaderText = "Authorized Pharmacist",
            FillWeight = 14,
            MinimumWidth = 110,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.TopLeft, Padding = new Padding(4, 6, 4, 6) }
        });

        // 6. Top Revenue Drugs Grid
        ClinicTheme.ApplyGrid(_dgvTopRevenue);
        _dgvTopRevenue.BorderStyle = BorderStyle.None;
        _dgvTopRevenue.AutoGenerateColumns = false;
        _dgvTopRevenue.Columns.Clear();
        _dgvTopRevenue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(TopRevenueDrugItem.MedicineDisplayName), HeaderText = "Medication Formulation", FillWeight = 48, MinimumWidth = 160 });
        _dgvTopRevenue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(TopRevenueDrugItem.Category), HeaderText = "Category", FillWeight = 18, MinimumWidth = 90 });
        _dgvTopRevenue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(TopRevenueDrugItem.UnitsDispensed), HeaderText = "Dispensed", FillWeight = 10, MinimumWidth = 45, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _dgvTopRevenue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(TopRevenueDrugItem.GrossRevenue), HeaderText = "Gross Sales", FillWeight = 14, MinimumWidth = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F, FontStyle.Bold) } });
        _dgvTopRevenue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(TopRevenueDrugItem.ProfitMarginPct), HeaderText = "Margin %", FillWeight = 10, MinimumWidth = 55, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.0'%'", Alignment = DataGridViewContentAlignment.MiddleRight } });

        // 7. Imminent Expiry Risk Radar Grid
        ClinicTheme.ApplyGrid(_dgvFinancialRisk);
        _dgvFinancialRisk.BorderStyle = BorderStyle.None;
        _dgvFinancialRisk.AutoGenerateColumns = false;
        _dgvFinancialRisk.Columns.Clear();
        _dgvFinancialRisk.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AtRiskExpiryItem.MedicineDisplayName), HeaderText = "Medication Formulation", FillWeight = 36, MinimumWidth = 130 });
        _dgvFinancialRisk.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AtRiskExpiryItem.BatchNumber), HeaderText = "Batch #", FillWeight = 16, MinimumWidth = 75 });
        _dgvFinancialRisk.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AtRiskExpiryItem.ExpiryDate), HeaderText = "Expiry", FillWeight = 14, MinimumWidth = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" } });
        _dgvFinancialRisk.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AtRiskExpiryItem.QuantityOnHand), HeaderText = "Stock", FillWeight = 9, MinimumWidth = 40, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _dgvFinancialRisk.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AtRiskExpiryItem.ValueAtRisk), HeaderText = "Value at Risk", FillWeight = 13, MinimumWidth = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28) } });
        _dgvFinancialRisk.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(AtRiskExpiryItem.StatusText), HeaderText = "Urgency", FillWeight = 12, MinimumWidth = 65 });

        // 8. Disposal Loss Breakdown Grid
        ClinicTheme.ApplyGrid(_dgvLossBreakdown);
        _dgvLossBreakdown.BorderStyle = BorderStyle.None;
        _dgvLossBreakdown.AutoGenerateColumns = false;
        _dgvLossBreakdown.Columns.Clear();
        _dgvLossBreakdown.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DisposalLossReasonItem.Reason), HeaderText = "Disposal Reason", FillWeight = 48, MinimumWidth = 140 });
        _dgvLossBreakdown.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DisposalLossReasonItem.BatchesDestroyed), HeaderText = "Batches", FillWeight = 13, MinimumWidth = 45, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _dgvLossBreakdown.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DisposalLossReasonItem.UnitsDestroyed), HeaderText = "Units", FillWeight = 13, MinimumWidth = 45, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
        _dgvLossBreakdown.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DisposalLossReasonItem.TotalLossAmount), HeaderText = "Total Loss", FillWeight = 14, MinimumWidth = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28) } });
        _dgvLossBreakdown.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DisposalLossReasonItem.LossSharePct), HeaderText = "% Share", FillWeight = 12, MinimumWidth = 50, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.0'%'", Alignment = DataGridViewContentAlignment.MiddleRight } });
    }

    private void WireEvents()
    {
        Load += async (_, _) => await InitialLoadAsync();
        _btnLogout.Click += (_, _) =>
        {
            if (UserSession.IsAuthenticated && UserSession.UserId.HasValue)
            {
                AuditLogger.Log("LOGOUT", "Users", UserSession.UserId.Value, $"User '{UserSession.Username ?? UserSession.FullName}' signed out.");
            }
            UserSession.Clear();
            Close();
        };

        // Tab switcher buttons
        _btnTabDispensing.Click += (_, _) => UpdateTabSelection(0);
        _btnTabInventory.Click += (_, _) => UpdateTabSelection(1);
        _btnTabAudit.Click += (_, _) => UpdateTabSelection(2);
        _btnTabDisposal.Click += (_, _) => UpdateTabSelection(3);
        _btnTabFinancial.Click += (_, _) => UpdateTabSelection(4);

        // Financial Actions
        _btnRefreshFinancial.Click += async (_, _) => await LoadFinancialAnalyticsAsync();
        _cmbFinancialPeriod.SelectedIndexChanged += async (_, _) => await LoadFinancialAnalyticsAsync();

        // Queue search & selection
        _txtQueueSearch.TextChanged += async (_, _) => await LoadQueueAsync();
        _dgvQueue.SelectionChanged += DgvQueue_SelectionChanged;

        // Dispensing actions
        _btnDispenseAll.Click += BtnDispenseAll_Click;
        _btnDispenseSingle.Click += BtnDispenseSingle_Click;
        _btnTagOutOfStock.Click += BtnTagOutOfStock_Click;
        _btnPrintOfficialRx.Click += BtnPrintOfficialRx_Click;

        // Inventory Actions
        _cmbCategoryFilter.SelectedIndexChanged += async (_, _) => await LoadInventoryAsync();
        _txtInventorySearch.TextChanged += async (_, _) => await LoadInventoryAsync();
        _dgvInventory.SelectionChanged += DgvInventory_SelectionChanged;
        _dgvInventory.CellDoubleClick += (_, _) => OpenRestockDialog();

        _btnAddMedicine.Click += async (_, _) => await OpenAddMedicineDialogAsync();
        _btnRestockMedicine.Click += (_, _) => OpenRestockDialog();
        _btnDisposeMedicine.Click += async (_, _) => await OpenDisposalFromSelectedAsync();

        // Audit Search
        _txtAuditSearch.TextChanged += async (_, _) => await LoadAuditAsync();

        // Disposal Actions
        _btnNewDisposal.Click += async (_, _) => await OpenDisposalDialogAsync();
        _txtDisposalSearch.TextChanged += async (_, _) => await LoadDisposalLogsAsync();

        // Row formatting for inventory stock alert & batch group borders
        _dgvInventory.CellFormatting += DgvInventory_CellFormatting;
        _dgvInventory.RowPostPaint += DgvInventory_RowPostPaint;
        _dgvItems.CellFormatting += DgvItems_CellFormatting;

        // Setup initial button states
        SetButtonState(_btnDispenseAll, false, Color.FromArgb(16, 185, 129), Color.White);
        SetButtonState(_btnDispenseSingle, false, Color.White, Color.FromArgb(30, 41, 59), Color.FromArgb(203, 213, 225));
        SetButtonState(_btnTagOutOfStock, false, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(251, 191, 36));
        SetButtonState(_btnPrintOfficialRx, false, Color.White, Color.FromArgb(37, 99, 235), Color.FromArgb(191, 219, 254));

        SetButtonState(_btnRestockMedicine, false, Color.White, Color.FromArgb(30, 41, 59), Color.FromArgb(203, 213, 225));
        SetButtonState(_btnDisposeMedicine, false, Color.White, Color.FromArgb(185, 28, 28), Color.FromArgb(254, 202, 202));
        SetButtonState(_btnAddMedicine, true, Color.FromArgb(15, 118, 110), Color.White);
    }

    private void UpdateTabSelection(int tabIndex)
    {
        _currentTabIndex = tabIndex;

        Button[] tabButtons = { _btnTabDispensing, _btnTabInventory, _btnTabAudit, _btnTabDisposal, _btnTabFinancial };
        Panel[] tabViews = { _viewDispensing, _viewInventory, _viewAudit, _viewDisposal, _viewFinancial };

        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (i == tabIndex)
            {
                // Active Tab: Medical Teal (matching BillingForm active tab)
                tabButtons[i].BackColor = Color.FromArgb(15, 118, 110);
                tabButtons[i].ForeColor = Color.White;
                tabButtons[i].FlatAppearance.BorderSize = 0;
                tabViews[i].Visible = true;
                tabViews[i].BringToFront();
            }
            else
            {
                // Inactive Tab: Slate 100 with Slate 300 border
                tabButtons[i].BackColor = Color.FromArgb(241, 245, 249);
                tabButtons[i].ForeColor = Color.FromArgb(71, 85, 105);
                tabButtons[i].FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                tabButtons[i].FlatAppearance.BorderSize = 1;
                tabViews[i].Visible = false;
            }
        }

        if (tabIndex == 0) ApplyResponsiveLayout();
        if (tabIndex == 3) _ = LoadDisposalLogsAsync();
        if (tabIndex == 4) _ = LoadFinancialAnalyticsAsync();
    }

    private void SetButtonState(Button btn, bool enabled, Color activeBg, Color activeFg, Color? activeBorder = null)
    {
        btn.Enabled = enabled;
        if (enabled)
        {
            btn.BackColor = activeBg;
            btn.ForeColor = activeFg;
            if (activeBorder.HasValue)
            {
                btn.FlatAppearance.BorderColor = activeBorder.Value;
                btn.FlatAppearance.BorderSize = 1;
            }
            else
            {
                btn.FlatAppearance.BorderSize = 0;
            }
            btn.Cursor = Cursors.Hand;
        }
        else
        {
            btn.BackColor = Color.FromArgb(241, 245, 249); // Slate-100 disabled
            btn.ForeColor = Color.FromArgb(148, 163, 184); // Slate-400 disabled text
            btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btn.FlatAppearance.BorderSize = 1;
            btn.Cursor = Cursors.Default;
        }
    }

    private async Task InitialLoadAsync()
    {
        // If embedded in MainDashboardForm (TopLevel is false), hide redundant logout button
        if (!this.TopLevel)
        {
            _btnLogout.Visible = false;
        }

        // Badge
        string name = !string.IsNullOrWhiteSpace(UserSession.FullName) ? UserSession.FullName : "Maria Santos, RPh";
        _lblPharmacistBadge.Text = $"{name}  |  Davao Central Pharmacy";

        // Categories
        await ReloadCategoriesAsync();

        await RefreshCurrentTabAsync();
    }

    private async Task ReloadCategoriesAsync()
    {
        string current = _cmbCategoryFilter.SelectedItem?.ToString() ?? "All";
        var cats = await Task.Run(() => _repo.GetCategories());
        _cmbCategoryFilter.Items.Clear();
        foreach (var c in cats) _cmbCategoryFilter.Items.Add(c);
        int idx = _cmbCategoryFilter.Items.IndexOf(current);
        _cmbCategoryFilter.SelectedIndex = idx >= 0 ? idx : (_cmbCategoryFilter.Items.Count > 0 ? 0 : -1);
    }

    private async Task RefreshCurrentTabAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            await LoadQueueAsync();
            await LoadInventoryAsync();
            await LoadAuditAsync();
            await LoadDisposalLogsAsync();
            await LoadFinancialAnalyticsAsync();
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    // =========================================================================
    // DISPENSING QUEUE & DETAILS
    // =========================================================================
    private async Task LoadQueueAsync()
    {
        string search = _txtQueueSearch.Text.Trim();
        _queueItems = await Task.Run(() => _repo.GetDispenseQueue(search));

        _dgvQueue.DataSource = null;
        _dgvQueue.DataSource = _queueItems;

        _lblKpiQueueValue.Text = _queueItems.Count.ToString();
        _lblEmptyQueue.Visible = _queueItems.Count == 0;

        if (_queueItems.Count > 0)
        {
            _dgvQueue.ClearSelection();
            _dgvQueue.Rows[0].Selected = true;
            LoadPrescriptionDetails(_queueItems[0]);
        }
        else
        {
            ClearPrescriptionDetails();
        }
    }

    private void DgvQueue_SelectionChanged(object? sender, EventArgs e)
    {
        if (_dgvQueue.CurrentRow?.DataBoundItem is PharmacyQueueItem item)
        {
            LoadPrescriptionDetails(item);
        }
    }

    private void LoadPrescriptionDetails(PharmacyQueueItem item)
    {
        _selectedQueueItem = item;

        _lblPatientName.Text = item.PatientName;
        _lblRxBadge.Text = $"Rx #: {item.PrescriptionNumber}";
        _lblPatientMeta.Text = $"{item.PatientNumber}  |  {item.Age} yrs  |  {item.Sex}  |  Attending: {item.DoctorName} ({item.Specialty})";
        _lblReceiptTag.Text = $"✅ Official Receipt: {item.ReceiptNumber}  •  Amount Paid: ₱{item.TotalAmountPaid:N2}  (Confirmed Cashier Settlement)";

        _currentItems = _repo.GetPrescriptionDetails(item.PrescriptionId);
        _dgvItems.DataSource = null;
        _dgvItems.DataSource = _currentItems;

        bool hasPending = _currentItems.Any(i => i.AvailabilityStatus != "Dispensed" && i.AvailabilityStatus != "Out of Stock / External");

        SetButtonState(_btnDispenseAll, hasPending, Color.FromArgb(16, 185, 129), Color.White);
        SetButtonState(_btnDispenseSingle, hasPending, Color.White, Color.FromArgb(30, 41, 59), Color.FromArgb(203, 213, 225));
        SetButtonState(_btnTagOutOfStock, hasPending, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(251, 191, 36));
        SetButtonState(_btnPrintOfficialRx, true, Color.White, Color.FromArgb(37, 99, 235), Color.FromArgb(191, 219, 254));
    }

    private void ClearPrescriptionDetails()
    {
        _selectedQueueItem = null;
        _lblPatientName.Text = "No Paid Prescription Selected";
        _lblRxBadge.Text = "";
        _lblPatientMeta.Text = "Select an item from the confirmed paid queue on the left to inspect formulation stock and dispense.";
        _lblReceiptTag.Text = "Waiting for cashier settlements...";
        _dgvItems.DataSource = null;
        _currentItems.Clear();

        SetButtonState(_btnDispenseAll, false, Color.FromArgb(16, 185, 129), Color.White);
        SetButtonState(_btnDispenseSingle, false, Color.White, Color.FromArgb(30, 41, 59), Color.FromArgb(203, 213, 225));
        SetButtonState(_btnTagOutOfStock, false, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(251, 191, 36));
        SetButtonState(_btnPrintOfficialRx, false, Color.White, Color.FromArgb(37, 99, 235), Color.FromArgb(191, 219, 254));
    }

    private void BtnDispenseAll_Click(object? sender, EventArgs e)
    {
        if (_selectedQueueItem == null) return;

        var understocked = _currentItems.Where(i => i.AvailabilityStatus == "Paid" && i.InHouseStock < i.QuantityPrescribed).ToList();
        if (understocked.Count > 0)
        {
            string names = string.Join(", ", understocked.Select(u => $"{u.GenericName} (Stock: {u.InHouseStock}, Prescribed: {u.QuantityPrescribed})"));
            MessageBox.Show(
                $"The following medications do not have sufficient clinic stock to dispense:\n\n{names}\n\nPlease tag the understocked items as 'Out of Stock' to issue an Official Doctor Rx for external purchase.",
                "Insufficient In-House Stock",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Confirm dispensing all available medications for {_selectedQueueItem.PatientName}?\n\nThis will automatically deduct quantities from clinic inventory.",
            "Confirm Dispensing",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        int userId = UserSession.UserId ?? 1;
        var result = _repo.DispensePrescription(_selectedQueueItem.PrescriptionId, userId);

        if (result.Success)
        {
            MessageBox.Show(result.Message, "Dispensed Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _ = RefreshCurrentTabAsync();
        }
        else
        {
            MessageBox.Show(result.Message, "Dispense Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BtnDispenseSingle_Click(object? sender, EventArgs e)
    {
        if (_dgvItems.CurrentRow?.DataBoundItem is not PharmacyPrescriptionItem item)
        {
            MessageBox.Show("Please select an item from the medication list.", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (item.AvailabilityStatus == "Dispensed")
        {
            MessageBox.Show("This medication has already been dispensed.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (item.InHouseStock < item.QuantityPrescribed)
        {
            MessageBox.Show($"Insufficient stock for {item.GenericName}. Available: {item.InHouseStock} pcs, Prescribed: {item.QuantityPrescribed} pcs.\n\nPlease tag as 'Out of Stock' to print an external Doctor Rx.", "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int userId = UserSession.UserId ?? 1;
        var result = _repo.DispenseSingleItem(item.PrescriptionItemId, userId);
        if (result.Success)
        {
            MessageBox.Show(result.Message, "Dispensed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (_selectedQueueItem != null) LoadPrescriptionDetails(_selectedQueueItem);
            _ = LoadInventoryAsync();
        }
        else
        {
            MessageBox.Show(result.Message, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BtnTagOutOfStock_Click(object? sender, EventArgs e)
    {
        if (_selectedQueueItem == null || _dgvItems.CurrentRow?.DataBoundItem is not PharmacyPrescriptionItem item)
        {
            MessageBox.Show("Please select a medication to tag as Out of Stock.", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Tag '{item.GenericName}' as Out of Stock?\n\nThis will mark the item for external fulfillment and immediately open the Official Physician Prescription Slip (Rx) with Dr. {_selectedQueueItem.DoctorName}'s credentials for the patient to buy outside.",
            "Tag Out of Stock / External Rx",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        int userId = UserSession.UserId ?? 1;
        var result = _repo.TagOutOfStock(item.PrescriptionItemId, userId);

        if (result.Success)
        {
            MessageBox.Show(result.Message, "Tagged for External Rx", MessageBoxButtons.OK, MessageBoxIcon.Information);
            OpenPrescriptionSlipDialog(_selectedQueueItem.PrescriptionId);
            if (_selectedQueueItem != null) LoadPrescriptionDetails(_selectedQueueItem);
        }
        else
        {
            MessageBox.Show(result.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BtnPrintOfficialRx_Click(object? sender, EventArgs e)
    {
        if (_selectedQueueItem == null) return;
        OpenPrescriptionSlipDialog(_selectedQueueItem.PrescriptionId);
    }

    private void OpenPrescriptionSlipDialog(int prescriptionId)
    {
        var rxData = _repo.GetOfficialRxData(prescriptionId);
        if (rxData == null)
        {
            MessageBox.Show("Unable to load prescription data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new PrescriptionSlipDialog(rxData);
        dlg.ShowDialog(this);
    }

    // =========================================================================
    // INVENTORY & RESTOCKING STATION
    // =========================================================================
    private async Task LoadInventoryAsync()
    {
        string category = _cmbCategoryFilter.SelectedItem?.ToString() ?? "All";
        string search = _txtInventorySearch.Text.Trim();

        var inv = await Task.Run(() => _repo.GetInventory(category, search));
        _dgvInventory.DataSource = null;
        _dgvInventory.DataSource = inv;

        int catalogCount = inv.Select(i => i.MedicineId).Distinct().Count();
        int lowCount = inv.Where(i => i.IsFirstBatch && i.IsLowStock).Count();
        _lblKpiLowStockValue.Text = lowCount.ToString();
        _lblKpiCatalogValue.Text = catalogCount.ToString();
    }

    private void DgvInventory_SelectionChanged(object? sender, EventArgs e)
    {
        bool hasSelection = _dgvInventory.CurrentRow?.DataBoundItem is PharmacyInventoryItem;
        SetButtonState(_btnRestockMedicine, hasSelection, Color.White, Color.FromArgb(30, 41, 59), Color.FromArgb(203, 213, 225));
        SetButtonState(_btnDisposeMedicine, hasSelection, Color.FromArgb(254, 242, 242), Color.FromArgb(185, 28, 28), Color.FromArgb(252, 165, 165));
    }

    private async Task OpenAddMedicineDialogAsync()
    {
        using var dlg = new AddMedicineDialog(_repo);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await ReloadCategoriesAsync();
            await LoadInventoryAsync();
        }
    }

    private void OpenRestockDialog()
    {
        if (_dgvInventory.CurrentRow?.DataBoundItem is not PharmacyInventoryItem item)
        {
            MessageBox.Show("Please select a medication from the table to restock.", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new RestockMedicineDialog(item, _repo);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _ = LoadInventoryAsync();
            if (_selectedQueueItem != null) LoadPrescriptionDetails(_selectedQueueItem);
        }
    }

    private async Task OpenDisposalFromSelectedAsync()
    {
        if (_dgvInventory.CurrentRow?.DataBoundItem is not PharmacyInventoryItem item)
        {
            MessageBox.Show("Please select a medication batch from the table to dispose or write off.", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new BatchDisposalDialog(_repo, item.MedicineId, item.BatchNumber);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await LoadInventoryAsync();
            await LoadDisposalLogsAsync();
        }
    }

    private async Task OpenDisposalDialogAsync()
    {
        using var dlg = new BatchDisposalDialog(_repo);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            await LoadInventoryAsync();
            await LoadDisposalLogsAsync();
        }
    }

    // =========================================================================
    // AUDIT LOGS
    // =========================================================================
    private async Task LoadAuditAsync()
    {
        string search = _txtAuditSearch.Text.Trim();
        var audits = await Task.Run(() => _repo.GetDispensedHistory(search));
        _dgvAudit.DataSource = null;
        _dgvAudit.DataSource = audits;
        _lblKpiAuditValue.Text = audits.Count.ToString();
    }

    // =========================================================================
    // WASTE & DISPOSAL AUDIT LOGS
    // =========================================================================
    private async Task LoadDisposalLogsAsync()
    {
        string search = _txtDisposalSearch.Text.Trim();
        var logs = await Task.Run(() => _repo.GetDisposalLogs(search));
        _dgvDisposal.DataSource = null;
        _dgvDisposal.DataSource = logs;
    }

    private void DgvInventory_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _dgvInventory.Rows.Count) return;
        if (_dgvInventory.Rows[e.RowIndex].DataBoundItem is PharmacyInventoryItem item)
        {
            string colProp = _dgvInventory.Columns[e.ColumnIndex].DataPropertyName;

            // Grouping: If subsequent batch under the same medication, visually indent and group
            if (!item.IsFirstBatch)
            {
                if (colProp == nameof(PharmacyInventoryItem.GenericName))
                {
                    e.Value = $"   ↳ Lot: {item.BatchNumber ?? $"Batch #{item.BatchSeq}"}";
                    e.CellStyle.Font = new Font(_dgvInventory.Font, FontStyle.Italic);
                    e.CellStyle.ForeColor = Color.FromArgb(71, 85, 105);
                    e.FormattingApplied = true;
                }
                else if (colProp == nameof(PharmacyInventoryItem.BrandName) || colProp == nameof(PharmacyInventoryItem.Category))
                {
                    e.Value = "↳";
                    e.CellStyle.ForeColor = Color.FromArgb(148, 163, 184);
                    e.FormattingApplied = true;
                }
                else if (colProp == nameof(PharmacyInventoryItem.StockStatus))
                {
                    e.Value = $"Sub-lot #{item.BatchSeq}";
                    e.CellStyle.ForeColor = Color.FromArgb(100, 116, 139);
                    e.FormattingApplied = true;
                }
            }
            else
            {
                // First batch row: display rollup total stock if multiple batches exist
                if (colProp == nameof(PharmacyInventoryItem.QuantityOnHand))
                {
                    if (item.ActiveBatchCount > 1)
                    {
                        e.Value = $"{item.QuantityOnHand} (Total: {item.TotalStock})";
                        e.FormattingApplied = true;
                    }
                }
            }

            if (colProp == nameof(PharmacyInventoryItem.BatchNumber) && string.IsNullOrEmpty(item.BatchNumber))
            {
                e.Value = "—";
                e.FormattingApplied = true;
            }

            if (colProp == nameof(PharmacyInventoryItem.ExpiryDate) && !item.ExpiryDate.HasValue)
            {
                e.Value = "—";
                e.FormattingApplied = true;
            }

            // Expiry & stock level color styling
            if (item.ExpiryDate.HasValue && item.ExpiryDate.Value < DateTime.Today)
            {
                // Expired: Red highlight
                e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
            }
            else if (item.ExpiryDate.HasValue && item.ExpiryDate.Value <= DateTime.Today.AddDays(60))
            {
                // Expiring soon (<60 days): Amber highlight
                e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
            }
            else if (item.TotalStock <= 0)
            {
                e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
            }
            else if (item.IsLowStock)
            {
                e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
            }
            else if (!item.IsFirstBatch)
            {
                // Soft muted background for child batch rows
                e.CellStyle.BackColor = Color.FromArgb(248, 250, 252);
            }
        }
    }

    private void DgvInventory_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _dgvInventory.Rows.Count) return;
        if (_dgvInventory.Rows[e.RowIndex].DataBoundItem is PharmacyInventoryItem item)
        {
            // If this is the last batch of the medicine group, draw a crisp group separator border at the bottom
            if (item.IsLastBatch || e.RowIndex == _dgvInventory.Rows.Count - 1)
            {
                using var pen = new Pen(Color.FromArgb(203, 213, 225), 2); // Slate-300 divider
                int y = e.RowBounds.Bottom - 1;
                e.Graphics.DrawLine(pen, e.RowBounds.Left, y, e.RowBounds.Right, y);
            }
        }
    }

    private void DgvItems_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _dgvItems.Rows.Count) return;
        if (_dgvItems.Rows[e.RowIndex].DataBoundItem is PharmacyPrescriptionItem item)
        {
            if (item.AvailabilityStatus == "Dispensed")
            {
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(240, 253, 244);
            }
            else if (item.AvailabilityStatus == "Out of Stock / External")
            {
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199);
            }
            else if (!item.HasSufficientStock)
            {
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                _dgvItems.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(254, 242, 242);
            }
        }
    }

    // =========================================================================
    // VIEW 5: FINANCIAL FLOW & INTELLIGENCE
    // =========================================================================
    private async Task LoadFinancialAnalyticsAsync()
    {
        string period = _cmbFinancialPeriod.SelectedItem?.ToString() ?? "This Month";

        PharmacyFinancialSummary summary;
        try
        {
            summary = await Task.Run(() => _repo.GetFinancialAnalytics(period));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to calculate pharmacy financial analytics: {ex.Message}", "Financial Analytics Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // 1. KPI Cards
        _lblFinGrossSalesVal.Text = $"₱{summary.GrossSalesRevenue:N2}";
        _lblFinGrossSalesSub.Text = $"COGS: ₱{summary.CostOfGoodsSold:N2} ({summary.PrescriptionsDispensedCount} Rx)";

        _lblFinGrossProfitVal.Text = $"₱{summary.GrossProfit:N2}";
        _lblFinGrossProfitSub.Text = $"Profit Margin: {summary.ProfitMarginPct:F1}%";

        _lblFinShrinkageVal.Text = $"₱{summary.TotalDisposalLoss:N2} ({summary.ShrinkageRatioPct:F2}%)";
        _lblFinShrinkageSub.Text = $"{summary.DisposalCount} batch disposal run(s)";

        _lblFinMissedSalesVal.Text = $"₱{summary.MissedSalesOpportunity:N2}";
        _lblFinMissedSalesSub.Text = $"{summary.MissedRxItemsCount} stockout line item(s)";

        _lblFinActiveCapitalVal.Text = $"₱{summary.ActiveInventoryCapital:N2}";
        _lblFinActiveCapitalSub.Text = $"At Expiry Risk: ₱{summary.CapitalAtRisk30Days:N2}";

        // Shrinkage Badge
        string statusText = summary.ShrinkageRatioPct switch
        {
            <= 2.0m => "Healthy (< 2.0%)",
            <= 4.0m => "Moderate (2.0% - 4.0%)",
            _ => "High Waste (> 4.0%)"
        };
        _lblFinancialSummaryBadge.Text = $"Shrinkage Rate: {summary.ShrinkageRatioPct:F2}%  •  {statusText}";
        if (summary.ShrinkageRatioPct <= 2.0m)
        {
            _lblFinancialSummaryBadge.ForeColor = Color.FromArgb(16, 185, 129);
            _lblFinancialSummaryBadge.BackColor = Color.FromArgb(240, 253, 244);
        }
        else if (summary.ShrinkageRatioPct <= 4.0m)
        {
            _lblFinancialSummaryBadge.ForeColor = Color.FromArgb(180, 83, 9);
            _lblFinancialSummaryBadge.BackColor = Color.FromArgb(254, 243, 199);
        }
        else
        {
            _lblFinancialSummaryBadge.ForeColor = Color.FromArgb(185, 28, 28);
            _lblFinancialSummaryBadge.BackColor = Color.FromArgb(254, 242, 242);
        }

        // 2. Grids
        _dgvTopRevenue.DataSource = summary.TopRevenueDrugs;
        _dgvFinancialRisk.DataSource = summary.ImminentExpiryRisks;
        _dgvLossBreakdown.DataSource = summary.LossReasons;
    }
}
