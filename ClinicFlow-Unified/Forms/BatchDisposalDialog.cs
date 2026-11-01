#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class BatchDisposalDialog : Form
{
    private readonly PharmacyRepository _repo;
    private readonly int? _preselectedMedicineId;
    private readonly string? _preselectedBatch;

    private List<DisposalCandidateItem> _candidates = new();

    // UI Components
    private Panel _pnlHeader = null!;
    private Label _lblHeaderTitle = null!;
    private Label _lblHeaderSubtitle = null!;

    private Panel _pnlTopToolbar = null!;
    private ComboBox _cmbFilter = null!;
    private Button _btnSelectExpired = null!;
    private Button _btnSelectCritical = null!;
    private Button _btnDeselectAll = null!;

    private DataGridView _dgvCandidates = null!;

    private Panel _pnlBottom = null!;
    private Label _lblSummaryBadge = null!;
    private Label _lblReason = null!;
    private ComboBox _cmbReason = null!;
    private Label _lblMethod = null!;
    private ComboBox _cmbMethod = null!;
    private Label _lblNotes = null!;
    private TextBox _txtNotes = null!;
    private Button _btnConfirm = null!;
    private Button _btnCancel = null!;

    public int DisposedCount { get; private set; }

    public BatchDisposalDialog(PharmacyRepository repo, int? preselectedMedicineId = null, string? preselectedBatch = null)
    {
        _repo = repo;
        _preselectedMedicineId = preselectedMedicineId;
        _preselectedBatch = preselectedBatch;

        InitializeComponent();
        LoadCandidates();
    }

    private void InitializeComponent()
    {
        Text = "Pharmaceutical Stock Disposal & Decommissioning — ClinicFlow";
        Size = new Size(1180, 740);
        MinimumSize = new Size(1120, 680);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        BackColor = Color.FromArgb(248, 250, 252); // Slate-50

        // 1. Header Banner
        _pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = Color.FromArgb(153, 27, 27), // Crimson Red-800
            Padding = new Padding(24, 14, 24, 12)
        };

        _lblHeaderTitle = new Label
        {
            Text = "🗑️ Pharmaceutical Stock Disposal & Decommissioning Station",
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(22, 12)
        };

        _lblHeaderSubtitle = new Label
        {
            Text = "Select expired, damaged, or quarantined lots to safely write off. Supports itemized physical counts or bulk removal.",
            Font = new Font("Segoe UI", 9.25F, FontStyle.Regular),
            ForeColor = Color.FromArgb(254, 202, 202), // Red-200
            AutoSize = true,
            Location = new Point(24, 40)
        };

        _pnlHeader.Controls.AddRange(new Control[] { _lblHeaderTitle, _lblHeaderSubtitle });

        // 2. Top Quick Selection & Filter Toolbar
        _pnlTopToolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = Color.White,
            Padding = new Padding(20, 8, 20, 8)
        };
        _pnlTopToolbar.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawLine(pen, 0, _pnlTopToolbar.Height - 1, _pnlTopToolbar.Width, _pnlTopToolbar.Height - 1);
        };

        var lblFilter = new Label
        {
            Text = "Filter:",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            AutoSize = true,
            Location = new Point(20, 16)
        };

        _cmbFilter = new ComboBox
        {
            Location = new Point(68, 11),
            Width = 265,
            Font = new Font("Segoe UI", 9.5F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbFilter.Items.AddRange(new object[]
        {
            "⚠️ All Expiring & Expired Lots",
            "🔴 Already Expired Only (< Today)",
            "⚠️ Critical Near-Expiry (< 30 Days)",
            "🟡 Near-Expiry (< 60 Days)",
            "📦 All Active Inventory Lots"
        });
        _cmbFilter.SelectedIndex = 0;
        _cmbFilter.SelectedIndexChanged += (_, _) => LoadCandidates();

        _btnSelectExpired = new Button
        {
            Text = "☑️ Select All Expired",
            Location = new Point(345, 9),
            Size = new Size(175, 34),
            BackColor = Color.FromArgb(254, 242, 242),
            ForeColor = Color.FromArgb(185, 28, 28),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnSelectExpired.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);
        _btnSelectExpired.Click += (_, _) => SelectByCondition(c => c.IsExpired);

        _btnSelectCritical = new Button
        {
            Text = "☑️ Select All < 30 Days",
            Location = new Point(530, 9),
            Size = new Size(180, 34),
            BackColor = Color.FromArgb(254, 243, 199),
            ForeColor = Color.FromArgb(180, 83, 9),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnSelectCritical.FlatAppearance.BorderColor = Color.FromArgb(251, 191, 36);
        _btnSelectCritical.Click += (_, _) => SelectByCondition(c => c.IsCritical);

        _btnDeselectAll = new Button
        {
            Text = "⬜ Deselect All",
            Location = new Point(720, 9),
            Size = new Size(130, 34),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(71, 85, 105),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F),
            Cursor = Cursors.Hand
        };
        _btnDeselectAll.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        _btnDeselectAll.Click += (_, _) => SelectByCondition(_ => false);

        _pnlTopToolbar.Controls.AddRange(new Control[]
        {
            lblFilter, _cmbFilter,
            _btnSelectExpired, _btnSelectCritical, _btnDeselectAll
        });

        // 3. Grid for Candidate Batches
        _dgvCandidates = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ScrollBars = ScrollBars.Vertical,
            RowTemplate = { Height = 38 },
            Font = new Font("Segoe UI", 9.5F)
        };

        ClinicTheme.ApplyGrid(_dgvCandidates, 38);
        _dgvCandidates.ReadOnly = false;
        _dgvCandidates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dgvCandidates.ScrollBars = ScrollBars.Vertical;
        _dgvCandidates.SelectionMode = DataGridViewSelectionMode.CellSelect;

        ConfigureGridColumns();

        var pnlGridWrap = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 10, 20, 10)
        };
        pnlGridWrap.Controls.Add(_dgvCandidates);

        // 4. Bottom Configuration & Action Panel
        _pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 170,
            BackColor = Color.White,
            Padding = new Padding(24, 12, 24, 12)
        };
        _pnlBottom.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawLine(pen, 0, 0, _pnlBottom.Width, 0);
        };
        _pnlBottom.Resize += (_, _) => RepositionBottomControls();

        _lblSummaryBadge = new Label
        {
            Text = "Selected: 0 batch(es)  •  Total Units: 0 pcs  •  Financial Loss: ₱0.00",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(185, 28, 28),
            BackColor = Color.FromArgb(254, 242, 242),
            Padding = new Padding(8, 4, 8, 4),
            Location = new Point(20, 10),
            AutoSize = true
        };

        // Reason Row
        _lblReason = new Label
        {
            Text = "Reason for Disposal *",
            Location = new Point(20, 42),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true
        };
        _cmbReason = new ComboBox
        {
            Location = new Point(20, 64),
            Width = 390,
            Font = new Font("Segoe UI", 9.5F),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DropDownWidth = 480
        };
        _cmbReason.Items.AddRange(new object[]
        {
            "Expired (Beyond Use Date / Shelf-Life Elapsed)",
            "Damaged / Broken Packaging or Blister",
            "Compromised Storage / Temp Excursion",
            "Manufacturer / Regulatory Recall",
            "Physical Inventory Shrinkage / Missing Units",
            "Clinic Pre-Expiry Pull-Out Policy (< 30d Cutoff)"
        });
        _cmbReason.SelectedIndex = 0;

        // Disposal Method Row
        _lblMethod = new Label
        {
            Text = "Destruction / Disposal Protocol *",
            Location = new Point(430, 42),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            AutoSize = true
        };
        _cmbMethod = new ComboBox
        {
            Location = new Point(430, 64),
            Width = 410,
            Font = new Font("Segoe UI", 9.5F),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DropDownWidth = 520
        };
        _cmbMethod.Items.AddRange(new object[]
        {
            "Chemical Denaturing & Biohazard Disposal Bin",
            "Returned to Distributor / Manufacturer for Credit",
            "Surrendered to Accredited Medical Waste Hauler (DENR)",
            "Witnessed Destruction & Sink Dispersal per Clinic SOP"
        });
        _cmbMethod.SelectedIndex = 0;

        // Notes Row
        _lblNotes = new Label
        {
            Text = "Pharmacist Disposal Notes / Serial",
            Location = new Point(20, 100),
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true
        };
        _txtNotes = new TextBox
        {
            Location = new Point(20, 122),
            Width = 820,
            Font = new Font("Segoe UI", 9.5F),
            PlaceholderText = "e.g. Physically counted on shelf 2B, denatured per DOH waste management guidelines."
        };

        // Action Buttons
        _btnConfirm = new Button
        {
            Text = "🗑️  Confirm Destruction",
            Location = new Point(870, 92),
            Size = new Size(270, 48),
            BackColor = Color.FromArgb(185, 28, 28), // Red-700
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0),
            Cursor = Cursors.Hand
        };
        _btnConfirm.FlatAppearance.BorderSize = 0;
        _btnConfirm.Click += BtnConfirm_Click;

        _btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(870, 46),
            Size = new Size(270, 36),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(71, 85, 105),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(0),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        _pnlBottom.Controls.AddRange(new Control[]
        {
            _lblSummaryBadge,
            _lblReason, _cmbReason,
            _lblMethod, _cmbMethod,
            _lblNotes, _txtNotes,
            _btnConfirm, _btnCancel
        });

        Controls.Add(pnlGridWrap);
        Controls.Add(_pnlTopToolbar);
        Controls.Add(_pnlBottom);
        Controls.Add(_pnlHeader);

        RepositionBottomControls();
    }

    private void RepositionBottomControls()
    {
        if (_pnlBottom == null || _btnConfirm == null || _btnCancel == null) return;
        int rightMargin = 24;
        int btnWidth = 270;
        int btnX = Math.Max(600, _pnlBottom.ClientSize.Width - btnWidth - rightMargin);

        _btnCancel.Location = new Point(btnX, 46);
        _btnConfirm.Location = new Point(btnX, 92);

        int leftWidth = btnX - 44;
        if (leftWidth > 300)
        {
            int colWidth = (leftWidth - 20) / 2;
            _lblReason.Location = new Point(20, 42);
            _cmbReason.Location = new Point(20, 64);
            _cmbReason.Width = colWidth;

            int methodX = 20 + colWidth + 20;
            _lblMethod.Location = new Point(methodX, 42);
            _cmbMethod.Location = new Point(methodX, 64);
            _cmbMethod.Width = colWidth;

            _lblNotes.Location = new Point(20, 100);
            _txtNotes.Location = new Point(20, 122);
            _txtNotes.Width = leftWidth;
        }
    }

    private void ConfigureGridColumns()
    {
        _dgvCandidates.Columns.Clear();

        // 0. Checkbox
        var chkCol = new DataGridViewCheckBoxColumn
        {
            HeaderText = "Select",
            DataPropertyName = nameof(DisposalCandidateItem.IsSelected),
            FillWeight = 5,
            MinimumWidth = 50,
            Resizable = DataGridViewTriState.False,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        _dgvCandidates.Columns.Add(chkCol);

        // 1. Medicine
        _dgvCandidates.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Medication Formulation",
            DataPropertyName = nameof(DisposalCandidateItem.MedicineDisplayName),
            ReadOnly = true,
            FillWeight = 26,
            MinimumWidth = 240
        });

        // 2. Batch Number
        _dgvCandidates.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Batch / Lot #",
            DataPropertyName = nameof(DisposalCandidateItem.BatchNumber),
            ReadOnly = true,
            FillWeight = 14,
            MinimumWidth = 140
        });

        // 3. Expiry Date
        _dgvCandidates.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Expiry Date",
            DataPropertyName = nameof(DisposalCandidateItem.ExpiryDate),
            ReadOnly = true,
            FillWeight = 10,
            MinimumWidth = 100,
            DefaultCellStyle = { Format = "yyyy-MM-dd", Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        // 4. Status (Generous width to show full text without ellipsis)
        _dgvCandidates.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Expiry Status",
            DataPropertyName = nameof(DisposalCandidateItem.ExpiryStatusText),
            ReadOnly = true,
            FillWeight = 17,
            MinimumWidth = 175
        });

        // 5. Recorded Stock
        _dgvCandidates.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Recorded",
            DataPropertyName = nameof(DisposalCandidateItem.QuantityOnHand),
            ReadOnly = true,
            FillWeight = 8,
            MinimumWidth = 80,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        // 6. Qty to Dispose (Editable!)
        var numCol = new DataGridViewTextBoxColumn
        {
            HeaderText = "Qty to Dispose",
            DataPropertyName = nameof(DisposalCandidateItem.QuantityToDispose),
            ReadOnly = false,
            FillWeight = 10,
            MinimumWidth = 105,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28) }
        };
        _dgvCandidates.Columns.Add(numCol);

        // 7. Unit Price
        _dgvCandidates.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Unit Price",
            DataPropertyName = nameof(DisposalCandidateItem.UnitPrice),
            ReadOnly = true,
            FillWeight = 8,
            MinimumWidth = 80,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C2" }
        });

        // 8. Total Loss
        _dgvCandidates.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Est. Loss",
            DataPropertyName = nameof(DisposalCandidateItem.EstLossAmount),
            ReadOnly = true,
            FillWeight = 9,
            MinimumWidth = 85,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "C2", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28) }
        });

        _dgvCandidates.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.RowIndex < _candidates.Count)
            {
                var item = _candidates[e.RowIndex];
                if (item.QuantityToDispose > item.QuantityOnHand)
                {
                    item.QuantityToDispose = item.QuantityOnHand;
                }
                if (item.QuantityToDispose < 0) item.QuantityToDispose = 0;
                UpdateSummary();
                UpdateButtonStates();
                _dgvCandidates.InvalidateRow(e.RowIndex);
            }
        };

        _dgvCandidates.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_dgvCandidates.IsCurrentCellDirty)
            {
                _dgvCandidates.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };

        _dgvCandidates.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _candidates.Count) return;
            var item = _candidates[e.RowIndex];

            // For the editable quantity column, keep a clean white background with bold red numbers
            if (e.ColumnIndex == 6)
            {
                e.CellStyle.BackColor = Color.White;
                e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                e.CellStyle.Font = new Font(_dgvCandidates.Font, FontStyle.Bold);
                return;
            }

            if (item.IsExpired)
            {
                e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
                e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
            }
            else if (item.IsCritical)
            {
                e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
            }
        };
    }

    private void LoadCandidates()
    {
        string filterKey = _cmbFilter.SelectedIndex switch
        {
            1 => "ExpiredOnly",
            2 => "Critical30",
            3 => "NearExpiry60",
            4 => "AllActive",
            _ => "AllExpiring"
        };

        _candidates = _repo.GetDisposalCandidates(filterKey);

        // If pre-selected batch was provided from inventory row, auto-select it!
        if (_preselectedMedicineId.HasValue && !string.IsNullOrEmpty(_preselectedBatch))
        {
            foreach (var c in _candidates)
            {
                if (c.MedicineId == _preselectedMedicineId.Value && c.BatchNumber.Equals(_preselectedBatch, StringComparison.OrdinalIgnoreCase))
                {
                    c.IsSelected = true;
                }
            }
        }

        _dgvCandidates.DataSource = null;
        _dgvCandidates.DataSource = _candidates;
        UpdateSummary();
        UpdateButtonStates();
    }

    private void SelectByCondition(Func<DisposalCandidateItem, bool> predicate)
    {
        foreach (var c in _candidates)
        {
            c.IsSelected = predicate(c);
        }
        _dgvCandidates.Refresh();
        UpdateSummary();
        UpdateButtonStates();
    }

    private void UpdateSummary()
    {
        var selected = _candidates.Where(c => c.IsSelected && c.QuantityToDispose > 0).ToList();
        int totalUnits = selected.Sum(c => c.QuantityToDispose);
        decimal totalLoss = selected.Sum(c => c.EstLossAmount);

        _lblSummaryBadge.Text = $"Selected: {selected.Count} batch(es)  •  Total Units: {totalUnits} pcs  •  Financial Loss: ₱{totalLoss:N2}";
    }

    private void UpdateButtonStates()
    {
        bool hasExpired = _candidates.Any(c => c.IsExpired);
        bool hasCritical = _candidates.Any(c => c.IsCritical);
        bool anySelected = _candidates.Any(c => c.IsSelected);
        var selectedItems = _candidates.Where(c => c.IsSelected && c.QuantityToDispose > 0).ToList();

        // 1. Select All Expired button (Red) - Disabled if 0 expired lots exist
        SetToolbarButtonState(
            _btnSelectExpired,
            hasExpired,
            Color.FromArgb(254, 242, 242), // Red-50
            Color.FromArgb(185, 28, 28),   // Red-700
            Color.FromArgb(252, 165, 165)  // Red-300
        );

        // 2. Select All < 30 Days button (Amber) - Disabled if 0 critical lots exist
        SetToolbarButtonState(
            _btnSelectCritical,
            hasCritical,
            Color.FromArgb(254, 243, 199), // Amber-100
            Color.FromArgb(180, 83, 9),    // Amber-700
            Color.FromArgb(251, 191, 36)   // Amber-400
        );

        // 3. Deselect All button - Disabled if no items are selected
        SetToolbarButtonState(
            _btnDeselectAll,
            anySelected,
            Color.White,
            Color.FromArgb(71, 85, 105),
            Color.FromArgb(203, 213, 225)
        );

        // 4. Confirm Destruction button - Disabled if no items selected
        bool canConfirm = selectedItems.Count > 0;
        _btnConfirm.Enabled = canConfirm;
        if (canConfirm)
        {
            _btnConfirm.BackColor = Color.FromArgb(185, 28, 28);
            _btnConfirm.ForeColor = Color.White;
            _btnConfirm.Cursor = Cursors.Hand;
        }
        else
        {
            _btnConfirm.BackColor = Color.FromArgb(241, 245, 249);
            _btnConfirm.ForeColor = Color.FromArgb(148, 163, 184);
            _btnConfirm.Cursor = Cursors.Default;
        }
    }

    private static void SetToolbarButtonState(Button btn, bool enabled, Color activeBg, Color activeFg, Color activeBorder)
    {
        btn.Enabled = enabled;
        if (enabled)
        {
            btn.BackColor = activeBg;
            btn.ForeColor = activeFg;
            btn.FlatAppearance.BorderColor = activeBorder;
            btn.Cursor = Cursors.Hand;
        }
        else
        {
            btn.BackColor = Color.FromArgb(241, 245, 249);
            btn.ForeColor = Color.FromArgb(148, 163, 184);
            btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btn.Cursor = Cursors.Default;
        }
    }

    private void BtnConfirm_Click(object? sender, EventArgs e)
    {
        var selected = _candidates.Where(c => c.IsSelected && c.QuantityToDispose > 0).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Please select at least one batch with a valid quantity to dispose.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string reason = _cmbReason.Text.Trim();
        string method = _cmbMethod.Text.Trim();
        string notes = _txtNotes.Text.Trim();
        int totalUnits = selected.Sum(c => c.QuantityToDispose);
        decimal totalLoss = selected.Sum(c => c.EstLossAmount);

        var confirm = MessageBox.Show(
            $"Confirm permanent write-off and destruction of {selected.Count} batch lot(s) ({totalUnits} total units)?\n\n" +
            $"• Reason: {reason}\n" +
            $"• Protocol: {method}\n" +
            $"• Financial Loss to Clinic: ₱{totalLoss:N2}\n\n" +
            "This will permanently decrement clinic inventory and create regulatory audit records. Proceed?",
            "Authorize Pharmaceutical Destruction",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        int userId = UserSession.UserId ?? 1;
        string userName = UserSession.FullName ?? "Maria Santos, RPh";

        int successCount = 0;
        var errors = new List<string>();

        foreach (var item in selected)
        {
            var res = _repo.DisposeBatch(
                item.MedicineId,
                item.BatchNumber,
                item.QuantityToDispose,
                reason,
                method,
                notes,
                userId,
                userName);

            if (res.Success)
            {
                successCount++;
            }
            else
            {
                errors.Add($"{item.GenericName} (Lot {item.BatchNumber}): {res.Message}");
            }
        }

        DisposedCount = successCount;

        if (errors.Count == 0)
        {
            MessageBox.Show(
                $"Successfully destroyed and written off {successCount} batch lot(s) ({totalUnits} pcs).\n\nDisposal manifest logged to Waste Audit History.",
                "Disposal Completed Successfully",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            string errSummary = string.Join("\n", errors);
            MessageBox.Show(
                $"Completed {successCount} disposal(s), but encountered errors on {errors.Count} lot(s):\n\n{errSummary}",
                "Partial Disposal Warning",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
