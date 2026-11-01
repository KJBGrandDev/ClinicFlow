#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class AddMedicineDialog : Form
{
    private readonly PharmacyRepository _repo;

    // Header Controls
    private Panel _pnlHeader = null!;
    private Label _lblHeaderTitle = null!;
    private Label _lblHeaderSubtitle = null!;

    // Form Controls
    private TextBox _txtGenericName = null!;
    private ListBox _lstSuggestions = null!;
    private System.Windows.Forms.Timer _debounceTimer = null!;
    private TextBox _txtBrandName = null!;
    private ComboBox _cmbCategory = null!;
    private ComboBox _cmbDosageForm = null!;
    private TextBox _txtStrength = null!;
    private NumericUpDown _numUnitPrice = null!;
    private NumericUpDown _numCostPrice = null!;
    private NumericUpDown _numReorderLevel = null!;

    // Initial Stock Section
    private CheckBox _chkAddStock = null!;
    private Panel _pnlStockDetails = null!;
    private NumericUpDown _numInitialQty = null!;
    private TextBox _txtBatchNumber = null!;
    private DateTimePicker _dtpExpiryDate = null!;

    // Bottom Action Controls
    private Button _btnSave = null!;
    private Button _btnCancel = null!;

    private sealed record FormularySuggestion(string GenericName, string Category, string DosageForm, string CommonStrength)
    {
        public override string ToString() => $"{GenericName}   —   [{Category} • {DosageForm} • {CommonStrength}]";
    }

    private static readonly FormularySuggestion[] StandardFormulary = new[]
    {
        // Antibiotics
        new FormularySuggestion("Amoxicillin", "Antibiotics", "Capsule", "500 mg"),
        new FormularySuggestion("Amoxicillin + Clavulanic Acid (Co-amoxiclav)", "Antibiotics", "Tablet", "625 mg"),
        new FormularySuggestion("Azithromycin", "Antibiotics", "Tablet", "500 mg"),
        new FormularySuggestion("Cephalexin", "Antibiotics", "Capsule", "500 mg"),
        new FormularySuggestion("Cefuroxime Axetil", "Antibiotics", "Tablet", "500 mg"),
        new FormularySuggestion("Cefixime", "Antibiotics", "Capsule", "200 mg"),
        new FormularySuggestion("Ciprofloxacin", "Antibiotics", "Tablet", "500 mg"),
        new FormularySuggestion("Clarithromycin", "Antibiotics", "Tablet", "500 mg"),
        new FormularySuggestion("Clindamycin", "Antibiotics", "Capsule", "300 mg"),
        new FormularySuggestion("Doxycycline", "Antibiotics", "Capsule", "100 mg"),
        new FormularySuggestion("Levofloxacin", "Antibiotics", "Tablet", "500 mg"),
        new FormularySuggestion("Metronidazole", "Antibiotics", "Tablet", "500 mg"),
        new FormularySuggestion("Co-trimoxazole (Sulfamethoxazole + Trimethoprim)", "Antibiotics", "Tablet", "800 mg / 160 mg"),

        // Analgesics & Antipyretics
        new FormularySuggestion("Paracetamol", "Analgesics & Antipyretics", "Tablet", "500 mg"),
        new FormularySuggestion("Ibuprofen", "Analgesics & Antipyretics", "Tablet", "400 mg"),
        new FormularySuggestion("Mefenamic Acid", "Analgesics & Antipyretics", "Capsule", "500 mg"),
        new FormularySuggestion("Tramadol", "Analgesics & Antipyretics", "Capsule", "50 mg"),
        new FormularySuggestion("Celecoxib", "Analgesics & Antipyretics", "Capsule", "200 mg"),
        new FormularySuggestion("Naproxen", "Analgesics & Antipyretics", "Tablet", "500 mg"),
        new FormularySuggestion("Diclofenac Sodium", "Analgesics & Antipyretics", "Tablet", "50 mg"),
        new FormularySuggestion("Ketorolac", "Analgesics & Antipyretics", "Tablet", "10 mg"),
        new FormularySuggestion("Paracetamol (Pediatric Syrup)", "Analgesics & Antipyretics", "Syrup", "250 mg / 5 mL"),

        // Antihistamines & Allergy
        new FormularySuggestion("Cetirizine", "Antihistamines", "Tablet", "10 mg"),
        new FormularySuggestion("Loratadine", "Antihistamines", "Tablet", "10 mg"),
        new FormularySuggestion("Levocetirizine", "Antihistamines", "Tablet", "5 mg"),
        new FormularySuggestion("Diphenhydramine", "Antihistamines", "Capsule", "50 mg"),
        new FormularySuggestion("Chlorphenamine Maleate", "Antihistamines", "Tablet", "4 mg"),
        new FormularySuggestion("Bilastine", "Antihistamines", "Tablet", "20 mg"),

        // Respiratory
        new FormularySuggestion("Salbutamol", "Respiratory", "Tablet", "2 mg"),
        new FormularySuggestion("Salbutamol (Inhaler)", "Respiratory", "Inhaler", "100 mcg / dose"),
        new FormularySuggestion("Salbutamol + Ipratropium", "Respiratory", "Inhaler", "100 mcg / 20 mcg"),
        new FormularySuggestion("Fluticasone + Salmeterol", "Respiratory", "Inhaler", "250 mcg / 50 mcg"),
        new FormularySuggestion("Budesonide", "Respiratory", "Inhaler", "200 mcg / dose"),
        new FormularySuggestion("Montelukast", "Respiratory", "Tablet", "10 mg"),
        new FormularySuggestion("Carbocisteine", "Respiratory", "Capsule", "500 mg"),
        new FormularySuggestion("Acetylcysteine", "Respiratory", "Tablet", "600 mg"),
        new FormularySuggestion("Ambroxol", "Respiratory", "Tablet", "30 mg"),
        new FormularySuggestion("Dextromethorphan", "Respiratory", "Tablet", "15 mg"),

        // Cardiovascular
        new FormularySuggestion("Amlodipine", "Cardiovascular", "Tablet", "5 mg"),
        new FormularySuggestion("Losartan", "Cardiovascular", "Tablet", "50 mg"),
        new FormularySuggestion("Metoprolol Tartrate", "Cardiovascular", "Tablet", "50 mg"),
        new FormularySuggestion("Atenolol", "Cardiovascular", "Tablet", "50 mg"),
        new FormularySuggestion("Carvedilol", "Cardiovascular", "Tablet", "25 mg"),
        new FormularySuggestion("Candesartan", "Cardiovascular", "Tablet", "8 mg"),
        new FormularySuggestion("Telmisartan", "Cardiovascular", "Tablet", "40 mg"),
        new FormularySuggestion("Enalapril", "Cardiovascular", "Tablet", "10 mg"),
        new FormularySuggestion("Captopril", "Cardiovascular", "Tablet", "25 mg"),
        new FormularySuggestion("Atorvastatin", "Cardiovascular", "Tablet", "20 mg"),
        new FormularySuggestion("Rosuvastatin", "Cardiovascular", "Tablet", "10 mg"),
        new FormularySuggestion("Simvastatin", "Cardiovascular", "Tablet", "20 mg"),
        new FormularySuggestion("Clopidogrel", "Cardiovascular", "Tablet", "75 mg"),
        new FormularySuggestion("Aspirin (Low Dose)", "Cardiovascular", "Tablet", "80 mg"),
        new FormularySuggestion("Furosemide", "Cardiovascular", "Tablet", "40 mg"),
        new FormularySuggestion("Spironolactone", "Cardiovascular", "Tablet", "25 mg"),

        // Gastrointestinal
        new FormularySuggestion("Omeprazole", "Gastrointestinal", "Capsule", "20 mg"),
        new FormularySuggestion("Esomeprazole", "Gastrointestinal", "Tablet", "40 mg"),
        new FormularySuggestion("Pantoprazole", "Gastrointestinal", "Tablet", "40 mg"),
        new FormularySuggestion("Ranitidine", "Gastrointestinal", "Tablet", "150 mg"),
        new FormularySuggestion("Famotidine", "Gastrointestinal", "Tablet", "20 mg"),
        new FormularySuggestion("Antacid (Aluminum Hydroxide + Magnesium)", "Gastrointestinal", "Chewable Tablet", "400 mg / 400 mg"),
        new FormularySuggestion("Hyoscine-N-Butylbromide", "Gastrointestinal", "Tablet", "10 mg"),
        new FormularySuggestion("Loperamide", "Gastrointestinal", "Capsule", "2 mg"),
        new FormularySuggestion("Metoclopramide", "Gastrointestinal", "Tablet", "10 mg"),
        new FormularySuggestion("Domperidone", "Gastrointestinal", "Tablet", "10 mg"),
        new FormularySuggestion("Oral Rehydration Salts (ORS)", "Gastrointestinal", "Tablet", "Sachet"),

        // Endocrine / Diabetes
        new FormularySuggestion("Metformin", "Endocrine / Diabetes", "Tablet", "500 mg"),
        new FormularySuggestion("Gliclazide", "Endocrine / Diabetes", "Tablet", "80 mg"),
        new FormularySuggestion("Glimepiride", "Endocrine / Diabetes", "Tablet", "2 mg"),
        new FormularySuggestion("Sitagliptin", "Endocrine / Diabetes", "Tablet", "50 mg"),
        new FormularySuggestion("Empagliflozin", "Endocrine / Diabetes", "Tablet", "10 mg"),
        new FormularySuggestion("Levothyroxine", "Endocrine / Diabetes", "Tablet", "50 mcg"),

        // Vitamins & Supplements
        new FormularySuggestion("Ascorbic Acid (Vitamin C)", "Vitamins & Supplements", "Tablet", "500 mg"),
        new FormularySuggestion("Ascorbic Acid + Zinc", "Vitamins & Supplements", "Capsule", "500 mg / 10 mg"),
        new FormularySuggestion("Vitamin B-Complex", "Vitamins & Supplements", "Tablet", "100 mg / 5 mg / 50 mcg"),
        new FormularySuggestion("Ferrous Sulfate + Folic Acid", "Vitamins & Supplements", "Tablet", "300 mg / 500 mcg"),
        new FormularySuggestion("Calcium Carbonate + Vitamin D3", "Vitamins & Supplements", "Tablet", "600 mg / 400 IU"),
        new FormularySuggestion("Multivitamins + Minerals", "Vitamins & Supplements", "Capsule", "Standard formulation"),

        // Dermatological
        new FormularySuggestion("Hydrocortisone Cream", "Dermatological", "Ointment / Cream", "1%"),
        new FormularySuggestion("Betamethasone Cream", "Dermatological", "Ointment / Cream", "0.1%"),
        new FormularySuggestion("Mupirocin", "Dermatological", "Ointment / Cream", "2%"),
        new FormularySuggestion("Silver Sulfadiazine", "Dermatological", "Ointment / Cream", "1%"),
        new FormularySuggestion("Clotrimazole Cream", "Dermatological", "Ointment / Cream", "1%"),
        new FormularySuggestion("Ketoconazole Cream", "Dermatological", "Ointment / Cream", "2%")
    };

    public int CreatedMedicineId { get; private set; }

    public AddMedicineDialog(PharmacyRepository repo)
    {
        _repo = repo;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Add New Medication Formulation";
        Size = new Size(580, 730);
        MinimumSize = new Size(580, 730);
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
            Text = "💊 Add New Medication Formulation",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 12)
        };

        _lblHeaderSubtitle = new Label
        {
            Text = "Register a new pharmaceutical item into the central pharmacy formulary.",
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            ForeColor = Color.FromArgb(204, 251, 241), // Teal-100
            AutoSize = true,
            Location = new Point(22, 38)
        };

        _pnlHeader.Controls.AddRange(new Control[] { _lblHeaderTitle, _lblHeaderSubtitle });

        // 2. Main Body Container
        var pnlBody = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 16, 24, 16),
            AutoScroll = true
        };

        int curY = 10;

        // Generic Name
        var lblGeneric = CreateFieldLabel("Generic Name *", 20, curY);
        pnlBody.Controls.Add(lblGeneric);
        curY += 22;

        _txtGenericName = new TextBox
        {
            Location = new Point(20, curY),
            Width = 515,
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = "e.g. Amoxicillin, Paracetamol, Metformin"
        };
        pnlBody.Controls.Add(_txtGenericName);
        curY += 40;

        // Brand Name & Strength (Side-by-side)
        var lblBrand = CreateFieldLabel("Brand Name (Optional)", 20, curY);
        var lblStrength = CreateFieldLabel("Strength / Concentration", 285, curY);
        pnlBody.Controls.AddRange(new Control[] { lblBrand, lblStrength });
        curY += 22;

        _txtBrandName = new TextBox
        {
            Location = new Point(20, curY),
            Width = 250,
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = "e.g. Biogesic, Amoxil"
        };

        _txtStrength = new TextBox
        {
            Location = new Point(285, curY),
            Width = 250,
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = "e.g. 500 mg, 250 mg/5 mL"
        };
        pnlBody.Controls.AddRange(new Control[] { _txtBrandName, _txtStrength });
        curY += 40;

        // Category & Dosage Form (Side-by-side)
        var lblCategory = CreateFieldLabel("Category *", 20, curY);
        var lblForm = CreateFieldLabel("Dosage Form *", 285, curY);
        pnlBody.Controls.AddRange(new Control[] { lblCategory, lblForm });
        curY += 22;

        _cmbCategory = new ComboBox
        {
            Location = new Point(20, curY),
            Width = 250,
            Font = new Font("Segoe UI", 9.5F),
            DropDownStyle = ComboBoxStyle.DropDown
        };
        _cmbCategory.Items.AddRange(new object[]
        {
            "Antibiotics",
            "Analgesics & Antipyretics",
            "Antihistamines",
            "Cardiovascular",
            "Gastrointestinal",
            "Respiratory",
            "Vitamins & Supplements",
            "Dermatological",
            "Endocrine / Diabetes",
            "Ophthalmic",
            "General"
        });
        _cmbCategory.SelectedIndex = 0;

        _cmbDosageForm = new ComboBox
        {
            Location = new Point(285, curY),
            Width = 250,
            Font = new Font("Segoe UI", 9.5F),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbDosageForm.Items.AddRange(new object[]
        {
            "Tablet",
            "Capsule",
            "Syrup",
            "Suspension",
            "Injectable",
            "Ointment / Cream",
            "Drops",
            "Inhaler",
            "Chewable Tablet",
            "Patch"
        });
        _cmbDosageForm.SelectedIndex = 0;

        pnlBody.Controls.AddRange(new Control[] { _cmbCategory, _cmbDosageForm });
        curY += 40;

        // Unit Price & Cost Price (Side-by-side)
        var lblPrice = CreateFieldLabel("Unit Selling Price (₱) *", 20, curY);
        var lblCost = CreateFieldLabel("Acquisition / Cost Price (₱)", 285, curY);
        pnlBody.Controls.AddRange(new Control[] { lblPrice, lblCost });
        curY += 22;

        _numUnitPrice = new NumericUpDown
        {
            Location = new Point(20, curY),
            Width = 250,
            Font = new Font("Segoe UI", 10F),
            DecimalPlaces = 2,
            Minimum = 0.00m,
            Maximum = 100000.00m,
            Value = 15.00m
        };

        _numCostPrice = new NumericUpDown
        {
            Location = new Point(285, curY),
            Width = 250,
            Font = new Font("Segoe UI", 10F),
            DecimalPlaces = 2,
            Minimum = 0.00m,
            Maximum = 100000.00m,
            Value = 9.75m // Default ~65% baseline
        };
        pnlBody.Controls.AddRange(new Control[] { _numUnitPrice, _numCostPrice });
        curY += 40;

        // Low Stock Alert Threshold
        var lblReorder = CreateFieldLabel("Low Stock Alert Threshold (pcs) *", 20, curY);
        pnlBody.Controls.Add(lblReorder);
        curY += 22;

        _numReorderLevel = new NumericUpDown
        {
            Location = new Point(20, curY),
            Width = 250,
            Font = new Font("Segoe UI", 10F),
            Minimum = 1,
            Maximum = 10000,
            Value = 20
        };
        pnlBody.Controls.Add(_numReorderLevel);
        curY += 46;

        // Opening Stock Card
        var pnlStockCard = new Panel
        {
            Location = new Point(20, curY),
            Width = 515,
            Height = 135,
            BackColor = Color.White,
            Padding = new Padding(14)
        };
        pnlStockCard.Paint += (s, e) =>
            ControlPaint.DrawBorder(e.Graphics, pnlStockCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

        _chkAddStock = new CheckBox
        {
            Text = "📦 Record opening stock immediately",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 118, 110),
            Location = new Point(14, 10),
            AutoSize = true,
            Checked = true
        };

        _pnlStockDetails = new Panel
        {
            Location = new Point(14, 38),
            Width = 485,
            Height = 85
        };

        var lblInitQty = CreateFieldLabel("Initial Qty (pcs):", 0, 4);
        _numInitialQty = new NumericUpDown
        {
            Location = new Point(0, 26),
            Width = 110,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Minimum = 1,
            Maximum = 10000,
            Value = 100
        };

        var lblBatch = CreateFieldLabel("Batch / Lot #:", 130, 4);
        _txtBatchNumber = new TextBox
        {
            Location = new Point(130, 26),
            Width = 160,
            Font = new Font("Segoe UI", 9.5F),
            Text = $"BAT-{DateTime.Now:yyyyMMdd}"
        };

        var lblExp = CreateFieldLabel("Expiration Date:", 310, 4);
        _dtpExpiryDate = new DateTimePicker
        {
            Location = new Point(310, 26),
            Width = 160,
            Font = new Font("Segoe UI", 9.5F),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddYears(2)
        };

        _chkAddStock.CheckedChanged += (s, e) =>
        {
            _pnlStockDetails.Enabled = _chkAddStock.Checked;
        };

        _pnlStockDetails.Controls.AddRange(new Control[]
        {
            lblInitQty, _numInitialQty,
            lblBatch, _txtBatchNumber,
            lblExp, _dtpExpiryDate
        });

        pnlStockCard.Controls.AddRange(new Control[] { _chkAddStock, _pnlStockDetails });
        pnlBody.Controls.Add(pnlStockCard);

        // Autocomplete Suggestions ListBox (Overlaid beneath Generic Name)
        _lstSuggestions = new ListBox
        {
            Location = new Point(_txtGenericName.Left, _txtGenericName.Bottom),
            Width = _txtGenericName.Width,
            Height = 140,
            Visible = false,
            Font = new Font("Segoe UI", 9.5F),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(15, 23, 42),
            Cursor = Cursors.Hand
        };
        _lstSuggestions.Click += (_, _) => ApplySelectedSuggestion();
        _lstSuggestions.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplySelectedSuggestion();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                _lstSuggestions.Visible = false;
                _txtGenericName.Focus();
            }
        };

        _debounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
        _debounceTimer.Tick += DebounceTimer_Tick;

        _txtGenericName.TextChanged += (_, _) =>
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        };

        _txtGenericName.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Down && _lstSuggestions.Visible && _lstSuggestions.Items.Count > 0)
            {
                e.SuppressKeyPress = true;
                _lstSuggestions.Focus();
                _lstSuggestions.SelectedIndex = 0;
            }
            else if (e.KeyCode == Keys.Escape && _lstSuggestions.Visible)
            {
                e.SuppressKeyPress = true;
                _lstSuggestions.Visible = false;
            }
        };

        pnlBody.MouseDown += (s, e) =>
        {
            if (!_lstSuggestions.Bounds.Contains(e.Location) && !_txtGenericName.Bounds.Contains(e.Location))
            {
                _lstSuggestions.Visible = false;
            }
        };

        pnlBody.Controls.Add(_lstSuggestions);
        _lstSuggestions.BringToFront();

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

        _btnSave = new Button
        {
            Text = "💾 Save & Add to Formulary",
            Size = new Size(215, 38),
            Location = new Point(320, 14),
            BackColor = Color.FromArgb(15, 118, 110), // Medical Teal
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;

        _btnCancel = new Button
        {
            Text = "Cancel",
            Size = new Size(95, 38),
            Location = new Point(215, 14),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(71, 85, 105),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Cancel
        };
        _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        pnlBottom.Controls.AddRange(new Control[] { _btnSave, _btnCancel });

        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        Controls.Add(pnlBody);
        Controls.Add(pnlBottom);
        Controls.Add(_pnlHeader);
    }

    private void DebounceTimer_Tick(object? sender, EventArgs e)
    {
        _debounceTimer.Stop();
        string query = _txtGenericName.Text.Trim();
        if (query.Length < 2)
        {
            _lstSuggestions.Visible = false;
            return;
        }

        var matches = StandardFormulary
            .Where(f => f.GenericName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(f => f.GenericName.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(f => f.GenericName)
            .Take(10)
            .ToList();

        if (matches.Count > 0 && _txtGenericName.Focused)
        {
            _lstSuggestions.BeginUpdate();
            _lstSuggestions.Items.Clear();
            foreach (var m in matches)
            {
                _lstSuggestions.Items.Add(m);
            }
            _lstSuggestions.EndUpdate();
            _lstSuggestions.Height = Math.Min(160, matches.Count * 24 + 6);
            _lstSuggestions.Visible = true;
            _lstSuggestions.BringToFront();
        }
        else
        {
            _lstSuggestions.Visible = false;
        }
    }

    private void ApplySelectedSuggestion()
    {
        if (_lstSuggestions.SelectedItem is FormularySuggestion item)
        {
            _txtGenericName.Text = item.GenericName;
            _txtGenericName.SelectionStart = _txtGenericName.Text.Length;

            int catIdx = _cmbCategory.FindStringExact(item.Category);
            if (catIdx >= 0) _cmbCategory.SelectedIndex = catIdx;
            else _cmbCategory.Text = item.Category;

            int formIdx = _cmbDosageForm.FindStringExact(item.DosageForm);
            if (formIdx >= 0) _cmbDosageForm.SelectedIndex = formIdx;
            else _cmbDosageForm.Text = item.DosageForm;

            if (string.IsNullOrWhiteSpace(_txtStrength.Text))
            {
                _txtStrength.Text = item.CommonStrength;
            }

            _lstSuggestions.Visible = false;
            _txtBrandName.Focus();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _debounceTimer?.Dispose();
        }
        base.Dispose(disposing);
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

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        string generic = _txtGenericName.Text.Trim();
        if (string.IsNullOrWhiteSpace(generic))
        {
            MessageBox.Show("Please enter the Generic Name of the medication.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtGenericName.Focus();
            return;
        }

        string category = _cmbCategory.Text.Trim();
        if (string.IsNullOrWhiteSpace(category))
        {
            MessageBox.Show("Please select or enter a Category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _cmbCategory.Focus();
            return;
        }

        string dosageForm = _cmbDosageForm.Text.Trim();
        if (string.IsNullOrWhiteSpace(dosageForm))
        {
            MessageBox.Show("Please select a Dosage Form.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _cmbDosageForm.Focus();
            return;
        }

        string? brand = string.IsNullOrWhiteSpace(_txtBrandName.Text) ? null : _txtBrandName.Text.Trim();
        string? strength = string.IsNullOrWhiteSpace(_txtStrength.Text) ? null : _txtStrength.Text.Trim();
        decimal unitPrice = _numUnitPrice.Value;
        if (unitPrice <= 0)
        {
            MessageBox.Show("Unit Selling Price must be greater than ₱0.00.", "Pharmacy QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _numUnitPrice.Focus();
            return;
        }

        int reorder = (int)_numReorderLevel.Value;

        int initStock = 0;
        string? batch = null;
        DateTime? expiry = null;

        if (_chkAddStock.Checked)
        {
            initStock = (int)_numInitialQty.Value;
            if (initStock <= 0)
            {
                MessageBox.Show("Initial stock quantity must be at least 1 unit if adding opening stock.", "Pharmacy QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numInitialQty.Focus();
                return;
            }

            batch = _txtBatchNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(batch))
            {
                batch = $"BAT-{DateTime.Now:yyyyMMdd}-01";
                _txtBatchNumber.Text = batch;
            }

            expiry = _dtpExpiryDate.Value;
            if (expiry.Value.Date <= DateTime.Today.AddDays(30))
            {
                MessageBox.Show("Stock expiration date must be at least 30 days in the future to prevent adding expired or near-expiry batches.", "Pharmacy QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dtpExpiryDate.Focus();
                return;
            }
        }

        int userId = UserSession.UserId ?? 1;
        decimal costPrice = _numCostPrice.Value;

        var result = _repo.AddMedicine(
            generic,
            brand,
            category,
            dosageForm,
            strength,
            unitPrice,
            reorder,
            initStock,
            batch,
            expiry,
            userId,
            costPrice);

        if (result.Success)
        {
            CreatedMedicineId = result.MedicineId;
            MessageBox.Show(result.Message, "Medicine Added Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            string msg = result.Message;
            if (msg.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                msg += "\n\n💡 QA Guidance: To replenish stock for an existing medication, close this dialog, highlight the item on the Pharmacist Dashboard, and click '📥 Restock Selected'.";
            }
            MessageBox.Show(msg, "Formulary QA Guard", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

