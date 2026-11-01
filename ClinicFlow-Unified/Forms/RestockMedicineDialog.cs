#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class RestockMedicineDialog : Form
{
    private readonly PharmacyInventoryItem _item;
    private readonly PharmacyRepository _repo;

    // Header Controls
    private Panel _pnlHeader = null!;
    private Label _lblHeaderTitle = null!;
    private Label _lblHeaderSubtitle = null!;

    // Info Banner
    private Panel _pnlMedBanner = null!;
    private Label _lblMedName = null!;
    private Label _lblMedMeta = null!;
    private Label _lblCurrentStock = null!;

    // Restock Inputs
    private NumericUpDown _numQty = null!;
    private TextBox _txtBatch = null!;
    private DateTimePicker _dtpExpiry = null!;
    private NumericUpDown _numCostPrice = null!;

    // Bottom Action Controls
    private Button _btnConfirm = null!;
    private Button _btnCancel = null!;

    public RestockMedicineDialog(PharmacyInventoryItem item, PharmacyRepository repo)
    {
        _item = item;
        _repo = repo;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = $"Restock Medication — {_item.GenericName}";
        Size = new Size(520, 530);
        MinimumSize = new Size(520, 530);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(248, 250, 252); // Slate-50

        // 1. Header Banner
        _pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(15, 118, 110), // Medical Teal
            Padding = new Padding(24, 14, 24, 12)
        };

        _lblHeaderTitle = new Label
        {
            Text = "📥 Restock Medication Formulation",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 12)
        };

        _lblHeaderSubtitle = new Label
        {
            Text = "Replenish in-house inventory stock with lot tracking.",
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(204, 251, 241),
            AutoSize = true,
            Location = new Point(22, 38)
        };

        _pnlHeader.Controls.AddRange(new Control[] { _lblHeaderTitle, _lblHeaderSubtitle });

        // 2. Main Body Container
        var pnlBody = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 16, 24, 16)
        };

        // Medication Info Card
        _pnlMedBanner = new Panel
        {
            Location = new Point(20, 12),
            Width = 460,
            Height = 84,
            BackColor = Color.White,
            Padding = new Padding(14, 10, 14, 10)
        };
        _pnlMedBanner.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, _pnlMedBanner.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        _lblMedName = new Label
        {
            Text = _item.DisplayName,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(12, 10),
            AutoSize = true
        };

        _lblMedMeta = new Label
        {
            Text = $"{_item.DosageForm}  •  {_item.Category}  •  Unit Price: ₱{_item.UnitPrice:N2}",
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(13, 34),
            AutoSize = true
        };

        Color stockColor = _item.TotalStock <= 0 ? Color.FromArgb(220, 38, 38) :
                           _item.IsLowStock ? Color.FromArgb(180, 83, 9) : Color.FromArgb(16, 185, 129);

        _lblCurrentStock = new Label
        {
            Text = $"Current In-House Stock: {_item.TotalStock} pcs  [{_item.StockStatus}]",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = stockColor,
            Location = new Point(13, 56),
            AutoSize = true
        };

        _pnlMedBanner.Controls.AddRange(new Control[] { _lblMedName, _lblMedMeta, _lblCurrentStock });
        pnlBody.Controls.Add(_pnlMedBanner);

        int curY = 110;

        // Quantity Section
        var lblQty = CreateFieldLabel("Quantity to Add (pcs) *", 20, curY);
        pnlBody.Controls.Add(lblQty);
        curY += 22;

        _numQty = new NumericUpDown
        {
            Location = new Point(20, curY),
            Width = 100,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            Minimum = 1,
            Maximum = 10000,
            Value = 50
        };
        pnlBody.Controls.Add(_numQty);

        // Quick Preset Chips (Flow)
        var flowPresets = new FlowLayoutPanel
        {
            Location = new Point(130, curY - 2),
            Width = 350,
            Height = 34,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        int[] presets = { 10, 25, 50, 100, 250, 500 };
        foreach (int p in presets)
        {
            int val = p;
            var chip = new Button
            {
                Text = $"+{val}",
                Size = new Size(50, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };
            chip.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            chip.MouseEnter += (s, e) => chip.BackColor = Color.FromArgb(241, 245, 249);
            chip.MouseLeave += (s, e) => chip.BackColor = Color.White;
            chip.Click += (s, e) => _numQty.Value = val;
            flowPresets.Controls.Add(chip);
        }
        pnlBody.Controls.Add(flowPresets);
        curY += 46;

        // Batch # & Expiration Date
        var lblBatch = CreateFieldLabel("Batch / Lot #", 20, curY);
        var lblExp = CreateFieldLabel("Expiration Date", 255, curY);
        pnlBody.Controls.AddRange(new Control[] { lblBatch, lblExp });
        curY += 22;

        _txtBatch = new TextBox
        {
            Location = new Point(20, curY),
            Width = 220,
            Font = new Font("Segoe UI", 10F),
            Text = $"BAT-{DateTime.Now:yyyyMMdd}"
        };

        _dtpExpiry = new DateTimePicker
        {
            Location = new Point(255, curY),
            Width = 225,
            Font = new Font("Segoe UI", 10F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddYears(2)
        };
        pnlBody.Controls.AddRange(new Control[] { _txtBatch, _dtpExpiry });
        curY += 46;

        // Batch Acquisition / Cost Price
        var lblCost = CreateFieldLabel("Batch Acquisition / Cost Price (₱)", 20, curY);
        pnlBody.Controls.Add(lblCost);
        curY += 22;

        _numCostPrice = new NumericUpDown
        {
            Location = new Point(20, curY),
            Width = 220,
            Font = new Font("Segoe UI", 10F),
            DecimalPlaces = 2,
            Minimum = 0.00m,
            Maximum = 100000.00m,
            Value = _item.CostPrice > 0 ? _item.CostPrice : Math.Round(_item.UnitPrice * 0.65m, 2)
        };
        pnlBody.Controls.Add(_numCostPrice);

        // 3. Bottom Action Bar
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = Color.White,
            Padding = new Padding(24, 14, 24, 14)
        };
        pnlBottom.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(226, 232, 240));
            e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
        };

        _btnConfirm = new Button
        {
            Text = "📥 Confirm Restock",
            Size = new Size(180, 38),
            Location = new Point(300, 14),
            BackColor = Color.FromArgb(16, 185, 129), // Emerald
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnConfirm.FlatAppearance.BorderSize = 0;
        _btnConfirm.Click += BtnConfirm_Click;

        _btnCancel = new Button
        {
            Text = "Cancel",
            Size = new Size(95, 38),
            Location = new Point(195, 14),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(71, 85, 105),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        pnlBottom.Controls.AddRange(new Control[] { _btnConfirm, _btnCancel });

        AcceptButton = _btnConfirm;
        CancelButton = _btnCancel;

        Controls.Add(pnlBody);
        Controls.Add(pnlBottom);
        Controls.Add(_pnlHeader);
    }

    private Label CreateFieldLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(71, 85, 105),
            Location = new Point(x, y),
            AutoSize = true
        };
    }

    private void BtnConfirm_Click(object? sender, EventArgs e)
    {
        int qty = (int)_numQty.Value;
        if (qty <= 0)
        {
            MessageBox.Show("Please specify a valid quantity greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _numQty.Focus();
            return;
        }

        string batch = _txtBatch.Text.Trim();
        if (string.IsNullOrEmpty(batch))
        {
            batch = $"BAT-{DateTime.Now:yyyyMMdd}";
            _txtBatch.Text = batch;
        }

        DateTime expiry = _dtpExpiry.Value;
        if (expiry.Date <= DateTime.Today.AddDays(30))
        {
            MessageBox.Show("Stock expiration date must be at least 30 days in the future to prevent adding expired or near-expiry batches.", "Pharmacy QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _dtpExpiry.Focus();
            return;
        }

        int userId = UserSession.UserId ?? 1;
        decimal costPrice = _numCostPrice.Value;

        var result = _repo.RestockMedicine(_item.MedicineId, qty, batch, expiry, userId, costPrice);

        if (result.Success)
        {
            MessageBox.Show(
                $"Successfully added +{qty} pcs to {_item.GenericName} ({_item.BrandName})!\n\nNew Total Stock: {_item.TotalStock + qty} pcs\nBatch: {batch} | Expiry: {expiry:yyyy-MM-dd}",
                "Restock Completed Successfully",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show(result.Message, "Restock Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

