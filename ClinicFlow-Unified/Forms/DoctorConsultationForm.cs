using ClinicFlow_Unified.Utilities;
#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Services;
namespace ClinicFlow_Unified.Forms
{
    public partial class DoctorConsultationForm : Form
    {
        private readonly int? _doctorId;
        private readonly DoctorConsultationService _service = new();
        private readonly BindingList<PrescriptionLineItem> _prescriptionList = new();
        private List<DoctorQueueItem> _queueItems = new();
        private DoctorQueueItem? _activePatient;
        private PrescriptionLineItem? _editingPrescriptionItem = null;
        private bool _isResettingSelection = false;
        private List<MedicineItem> _allMedicines = new();
        private bool _isViewingCategories = true;
        private string? _currentCategory = null;
        private bool _isUpdatingMedicineDropdown = false;

        public DoctorConsultationForm(int? doctorId = null)
        {
            _doctorId = doctorId ?? UserSession.DoctorId;
            InitializeComponent();
            ClinicTheme.ApplyForm(this);
            ApplyThemeHierarchy();
            ConfigureQueueGridColumns();
            ConfigureRxGridColumns();
            WireEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.WindowState = FormWindowState.Maximized;
            await LoadDataAsync();
        }

        private void ApplyThemeHierarchy()
        {
            Color headerBg = Color.FromArgb(241, 245, 249);
            Color headerFg = Color.FromArgb(71, 85, 105);

            _queueGrid.EnableHeadersVisualStyles = false;
            _queueGrid.ColumnHeadersDefaultCellStyle.BackColor = headerBg;
            _queueGrid.ColumnHeadersDefaultCellStyle.ForeColor = headerFg;
            _queueGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBg;
            _queueGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = headerFg;
            _queueGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            _rxGrid.EnableHeadersVisualStyles = false;
            _rxGrid.ColumnHeadersDefaultCellStyle.BackColor = headerBg;
            _rxGrid.ColumnHeadersDefaultCellStyle.ForeColor = headerFg;
            _rxGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBg;
            _rxGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = headerFg;
            _rxGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            Color disabledBg = Color.FromArgb(241, 245, 249);
            Color disabledFg = Color.FromArgb(148, 163, 184);
            Color disabledBorder = Color.FromArgb(226, 232, 240);

            Color emeraldCtaColor = Color.FromArgb(16, 185, 129);

            _completeConsultationButton.BackColor = emeraldCtaColor;
            _completeConsultationButton.ForeColor = Color.White;
            _completeConsultationButton.FlatStyle = FlatStyle.Flat;
            _completeConsultationButton.FlatAppearance.BorderSize = 0;
            _completeConsultationButton.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            _addRxButton.BackColor = emeraldCtaColor;
            _addRxButton.ForeColor = Color.White;
            _addRxButton.FlatStyle = FlatStyle.Flat;
            _addRxButton.FlatAppearance.BorderSize = 0;
            _addRxButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _addRxButton.EnabledChanged += (s, e) =>
            {
                if (_addRxButton.Enabled)
                {
                    _addRxButton.BackColor = emeraldCtaColor;
                    _addRxButton.ForeColor = Color.White;
                }
                else
                {
                    _addRxButton.BackColor = disabledBg;
                    _addRxButton.ForeColor = disabledFg;
                }
            };

            Color amberUpdateBg = Color.FromArgb(245, 158, 11);
            _updateRxButton.FlatStyle = FlatStyle.Flat;
            _updateRxButton.FlatAppearance.BorderSize = 0;
            _updateRxButton.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _updateRxButton.EnabledChanged += (s, e) =>
            {
                if (_updateRxButton.Enabled)
                {
                    _updateRxButton.BackColor = amberUpdateBg;
                    _updateRxButton.ForeColor = Color.White;
                }
                else
                {
                    _updateRxButton.BackColor = disabledBg;
                    _updateRxButton.ForeColor = disabledFg;
                }
            };

            _removeRxButton.FlatStyle = FlatStyle.Flat;
            _removeRxButton.FlatAppearance.BorderSize = 1;
            _removeRxButton.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            _removeRxButton.Text = "🗑";
            _removeRxButton.EnabledChanged += (s, e) =>
            {
                if (_removeRxButton.Enabled)
                {
                    _removeRxButton.BackColor = Color.FromArgb(254, 242, 242);
                    _removeRxButton.ForeColor = Color.FromArgb(220, 38, 38);
                    _removeRxButton.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
                }
                else
                {
                    _removeRxButton.BackColor = disabledBg;
                    _removeRxButton.ForeColor = disabledFg;
                    _removeRxButton.FlatAppearance.BorderColor = disabledBorder;
                }
            };

            _clearRxInputsButton.Text = "Reset to Default";

            var rxToolTips = new ToolTip();
            rxToolTips.SetToolTip(_addRxButton, "Add medication to prescription pad");
            rxToolTips.SetToolTip(_updateRxButton, "Update prescription");
            rxToolTips.SetToolTip(_removeRxButton, "Remove selected medication from prescription pad");
            rxToolTips.SetToolTip(_clearRxInputsButton, "Reset inputs to default values (Qty: 1, default dispense)");

            Button[] secondaries = { _viewPatientHistoryButton, _btnRefreshQueue, _btnDefer, _clearRxInputsButton, _clearButton };
            foreach (var b in secondaries)
            {
                b.BackColor = Color.White;
                b.ForeColor = Color.FromArgb(30, 41, 59);
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                b.FlatAppearance.BorderSize = 1;
                b.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            }

            _btnEmergency.BackColor = Color.FromArgb(254, 242, 242);
            _btnEmergency.ForeColor = Color.FromArgb(185, 28, 28);
            _btnEmergency.FlatStyle = FlatStyle.Flat;
            _btnEmergency.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);
            _btnEmergency.FlatAppearance.BorderSize = 1;
            _btnEmergency.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        }

        private void ConfigureQueueGridColumns()
        {
            _queueGrid.AutoGenerateColumns = false;
            _queueGrid.Columns.Clear();
            _queueGrid.RowTemplate.Height = 36;

            _queueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorQueueItem.QueueNumber),
                HeaderText = "#",
                Width = 32,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _queueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorQueueItem.PatientNumber),
                HeaderText = "ID",
                Width = 68,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _queueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorQueueItem.PatientName),
                HeaderText = "Patient Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 100,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _queueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorQueueItem.Age),
                HeaderText = "Age",
                Width = 38,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _queueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorQueueItem.Sex),
                HeaderText = "Sex",
                Width = 38,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _queueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorQueueItem.ArrivedTime),
                HeaderText = "Time",
                Width = 54,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _queueGrid.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _queueItems.Count)
                {
                    SetActivePatient(_queueItems[e.RowIndex]);
                }
            };

            _queueGrid.CellFormatting += QueueGrid_CellFormatting;
        }

        private void QueueGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _queueItems.Count) return;

            var item = _queueItems[e.RowIndex];
            bool isActive = (_activePatient != null && item.PatientId == _activePatient.PatientId);

            if (isActive)
            {
                e.CellStyle.BackColor = Color.FromArgb(236, 253, 245);
                e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);
                e.CellStyle.SelectionBackColor = Color.FromArgb(16, 185, 129);
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
            else
            {
                e.CellStyle.BackColor = Color.White;
                e.CellStyle.ForeColor = Color.FromArgb(148, 163, 184);
                e.CellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
                e.CellStyle.SelectionForeColor = Color.FromArgb(51, 65, 85);
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            }
        }

        private void ConfigureRxGridColumns()
        {
            _rxGrid.AutoGenerateColumns = false;
            _rxGrid.Columns.Clear();
            _rxGrid.RowTemplate.Height = 30;

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PrescriptionLineItem.MedicineName),
                HeaderText = "Medicine / Item",
                Width = 260,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PrescriptionLineItem.Quantity),
                HeaderText = "Qty",
                Width = 60,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PrescriptionLineItem.Instructions),
                HeaderText = "Dosage & Instructions",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PrescriptionLineItem.FulfillmentChoice),
                HeaderText = "Dispense Location",
                Width = 140
            });

            _rxGrid.DataSource = _prescriptionList;
        }

        private void WireEvents()
        {
            _btnRefreshQueue.Click += async (_, _) => await ReloadQueueAsync();
            _btnDefer.Click += DeferButton_Click;
            _btnEmergency.Click += EmergencyButton_Click;
            _viewPatientHistoryButton.Click += ViewPatientHistoryButton_Click;

            _addRxButton.Click += AddRxButton_Click;
            _updateRxButton.Click += UpdateRxButton_Click;
            _removeRxButton.Click += RemoveRxButton_Click;
            _clearRxInputsButton.Click += (_, _) => ClearRxInputs();
            _rxGrid.SelectionChanged += RxGrid_SelectionChanged;

            _medicineComboBox.SelectionChangeCommitted += MedicineComboBox_SelectionChangeCommitted;
            _medicineComboBox.SelectedIndexChanged += (_, _) =>
            {
                if (!_isUpdatingMedicineDropdown)
                    UpdateStockBadge();
            };
            _fulfillmentComboBox.SelectedIndexChanged += (_, _) => UpdateStockBadge();
            _quantityNumeric.ValueChanged += (_, _) => UpdateStockBadge();

            _completeConsultationButton.Click += CompleteConsultationButton_Click;
            _clearButton.Click += (_, _) => ClearWorkspace();
        }

        private int? GetCurrentDoctorId() => _doctorId ?? UserSession.DoctorId ?? 1;

        private async Task LoadDataAsync()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                int? docId = GetCurrentDoctorId();
                var qResult = await Task.Run(() => _service.LoadQueue(docId));
                var catResult = await Task.Run(() => _service.LoadCatalogs());

                if (catResult.Success)
                {
                    _allMedicines = catResult.Medicines ?? new List<MedicineItem>();
                    PopulateCategoryDropdown();
                }

                if (qResult.Success)
                {
                    _queueItems = qResult.Queue;
                    BindQueue();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task ReloadQueueAsync()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                int? docId = GetCurrentDoctorId();
                var qResult = await Task.Run(() => _service.LoadQueue(docId));
                if (qResult.Success)
                {
                    _queueItems = qResult.Queue;
                    BindQueue();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BindQueue()
        {
            _queueGrid.DataSource = null;
            _queueGrid.DataSource = _queueItems;

            if (_queueItems.Count > 0)
            {
                _queueGrid.ClearSelection();
                _queueGrid.Rows[0].Selected = true;
                SetActivePatient(_queueItems[0]);
            }
            else
            {
                ClearWorkspace();
            }
        }

        private void SetActivePatient(DoctorQueueItem item)
        {
            _activePatient = item;

            _lblActivePatientName.Text = item.PatientName;
            _lblActivePatientMeta.Text = $"{item.PatientNumber} | {item.Age} yrs | {item.Sex} | Queue #{item.QueueNumber}";
            _lblActiveConcern.Text = string.IsNullOrWhiteSpace(item.Reason) ? "" : $"Chief Concern: {item.Reason}";

            _lblBannerPatientName.Text = $"{item.PatientName} ({item.PatientNumber})";
            _lblBannerPatientDetails.Text = $"Age: {item.Age} | Sex: {item.Sex} | Arrival: {item.ArrivedTime}";

            _chiefComplaintTextBox.Text = item.Reason;
            _chiefComplaintTextBox.Focus();

            _queueGrid.Refresh();

            // Transition to In Service so lobby reflects active attendance
            Task.Run(() => _service.StartConsultation(item.QueueId));
        }

        private async void DeferButton_Click(object? sender, EventArgs e)
        {
            if (_queueItems.Count < 2)
            {
                MessageBox.Show("There are no other waiting patients to call next.", "Queue Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var current = _queueItems.First();
            var next = _queueItems.Skip(1).First();

            Cursor = Cursors.WaitCursor;
            try
            {
                await Task.Run(() => _service.DeferPatient(current.QueueId, next.QueueId));

                _queueItems.RemoveAt(0);
                _queueItems.Add(current);

                for (int i = 0; i < _queueItems.Count; i++)
                {
                    _queueItems[i].QueueNumber = i + 1;
                }

                BindQueue();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error advancing queue: " + ex.Message, "Queue Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async void EmergencyButton_Click(object? sender, EventArgs e)
        {
            using var pwdDialog = new Form
            {
                Text = "Authorize Emergency Leave",
                Size = new Size(380, 190),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblMsg = new Label { Text = "Enter password to authorize emergency leave and postpone queue:", Dock = DockStyle.Top, Height = 45, Padding = new Padding(12) };
            var txtPwd = new TextBox { PasswordChar = '*', Width = 330, Location = new Point(16, 52), Font = new Font("Segoe UI", 10F) };
            var btnConfirm = new Button { Text = "Confirm Emergency", DialogResult = DialogResult.OK, Location = new Point(16, 95), Size = new Size(180, 36), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(205, 95), Size = new Size(140, 36) };

            pwdDialog.Controls.Add(btnCancel);
            pwdDialog.Controls.Add(btnConfirm);
            pwdDialog.Controls.Add(txtPwd);
            pwdDialog.Controls.Add(lblMsg);
            pwdDialog.AcceptButton = btnConfirm;

            if (pwdDialog.ShowDialog(this) != DialogResult.OK) return;

            if (txtPwd.Text != "Admin@123")
            {
                MessageBox.Show("Incorrect password. Emergency leave was not activated.", "Authorization Failed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            int? docId = GetCurrentDoctorId();
            int doctorId = docId ?? 1;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _service.EmergencyPostpone(doctorId));
                MessageBox.Show(result.Message, "Emergency Leave Activated", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (result.Success)
                {
                    ClearWorkspace();
                    await ReloadQueueAsync();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ViewPatientHistoryButton_Click(object? sender, EventArgs e)
        {
            if (_activePatient == null || _activePatient.PatientId <= 0)
            {
                MessageBox.Show("Please select an active patient from the waiting queue first.", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var historyForm = new PatientHistoryForm(_activePatient.PatientId);
            historyForm.ShowDialog(this);
        }

        private void AddRxButton_Click(object? sender, EventArgs e)
        {
            if (_medicineComboBox.SelectedItem is not MedicineItem med)
            {
                MessageBox.Show("Please select a medicine.", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string instructions = _instructionsTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(instructions) || instructions.Length < 3)
            {
                MessageBox.Show("Please specify clear dosage instructions (minimum 3 characters, e.g. '1 tab 3x daily after meals for 7 days').", "Prescription QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _instructionsTextBox.Focus();
                return;
            }

            if (_prescriptionList.Any(p => p.MedicineId == med.MedicineId))
            {
                MessageBox.Show($"'{med.GenericName}' is already on this prescription list.", "Duplicate Medicine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fulfillment = string.IsNullOrWhiteSpace(_fulfillmentComboBox.Text) ? "Clinic Pharmacy" : _fulfillmentComboBox.Text;

            if (string.Equals(fulfillment, "Clinic Pharmacy", StringComparison.OrdinalIgnoreCase))
            {
                if (med.AvailableStock <= 0)
                {
                    var opt = MessageBox.Show(
                        $"'{med.GenericName}' is currently OUT OF STOCK in the clinic pharmacy (0 pcs available).\n\nWould you like to fulfill via 'External Pharmacy' instead so the patient can buy it outside?",
                        "In-House Stock Unavailable",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (opt == DialogResult.Yes)
                    {
                        fulfillment = "External Pharmacy";
                        _fulfillmentComboBox.SelectedItem = "External Pharmacy";
                    }
                    else
                    {
                        return;
                    }
                }
                else if ((int)_quantityNumeric.Value > med.AvailableStock)
                {
                    var opt = MessageBox.Show(
                        $"Prescribed quantity ({(int)_quantityNumeric.Value} pcs) exceeds available in-house stock ({med.AvailableStock} pcs).\n\nProceed with Clinic Pharmacy anyway? (The pharmacist can tag unfulfilled portions for external Rx)",
                        "Insufficient In-House Stock",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (opt != DialogResult.Yes)
                        return;
                }
            }

            _prescriptionList.Add(new PrescriptionLineItem
            {
                MedicineId = med.MedicineId,
                MedicineName = med.GenericName,
                Quantity = (int)_quantityNumeric.Value,
                Instructions = _instructionsTextBox.Text.Trim(),
                FulfillmentChoice = fulfillment
            });

            ClearRxInputs();
        }

        private void UpdateRxButton_Click(object? sender, EventArgs e)
        {
            if (_editingPrescriptionItem == null) return;

            string instructions = _instructionsTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(instructions) || instructions.Length < 3)
            {
                MessageBox.Show("Please specify clear dosage instructions (minimum 3 characters, e.g. '1 tab 3x daily after meals for 7 days').", "Prescription QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _instructionsTextBox.Focus();
                return;
            }

            string fulfillment = string.IsNullOrWhiteSpace(_fulfillmentComboBox.Text) ? "Clinic Pharmacy" : _fulfillmentComboBox.Text;

            if (_medicineComboBox.SelectedItem is MedicineItem med && string.Equals(fulfillment, "Clinic Pharmacy", StringComparison.OrdinalIgnoreCase))
            {
                if (med.AvailableStock <= 0)
                {
                    var opt = MessageBox.Show(
                        $"'{med.GenericName}' is currently OUT OF STOCK in the clinic pharmacy (0 pcs available).\n\nWould you like to fulfill via 'External Pharmacy' instead so the patient can buy it outside?",
                        "In-House Stock Unavailable",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (opt == DialogResult.Yes)
                    {
                        fulfillment = "External Pharmacy";
                        _fulfillmentComboBox.SelectedItem = "External Pharmacy";
                    }
                    else
                    {
                        return;
                    }
                }
                else if ((int)_quantityNumeric.Value > med.AvailableStock)
                {
                    var opt = MessageBox.Show(
                        $"Prescribed quantity ({(int)_quantityNumeric.Value} pcs) exceeds available in-house stock ({med.AvailableStock} pcs).\n\nProceed with Clinic Pharmacy anyway? (The pharmacist can tag unfulfilled portions for external Rx)",
                        "Insufficient In-House Stock",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (opt != DialogResult.Yes)
                        return;
                }
            }

            _editingPrescriptionItem.Quantity = (int)_quantityNumeric.Value;
            _editingPrescriptionItem.Instructions = _instructionsTextBox.Text.Trim();
            _editingPrescriptionItem.FulfillmentChoice = fulfillment;

            _isResettingSelection = true;
            try
            {
                _rxGrid.Refresh();
                _rxGrid.ClearSelection();
                _rxGrid.CurrentCell = null;
            }
            finally
            {
                _isResettingSelection = false;
            }

            ClearRxInputs();
        }

        private void RemoveRxButton_Click(object? sender, EventArgs e)
        {
            if (_rxGrid.CurrentRow?.DataBoundItem is PrescriptionLineItem item)
            {
                _prescriptionList.Remove(item);
                ClearRxInputs();
            }
        }

        private void RxGrid_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isResettingSelection || _rxGrid.SelectedRows.Count == 0) return;

            if (_rxGrid.SelectedRows[0].DataBoundItem is PrescriptionLineItem item)
            {
                _editingPrescriptionItem = item;

                var med = _allMedicines.FirstOrDefault(m => m.MedicineId == item.MedicineId);
                if (med != null)
                {
                    string cat = string.IsNullOrWhiteSpace(med.Category) ? "General" : med.Category.Trim();
                    ShowMedicinesForCategory(cat, selectMedicineId: med.MedicineId);
                }

                _medicineComboBox.Enabled = false;
                _lblMed.Text = "Editing Medication (Locked):";
                _lblMed.ForeColor = Color.FromArgb(217, 119, 6);

                _quantityNumeric.Value = Math.Max(1, item.Quantity);
                _instructionsTextBox.Text = item.Instructions;
                if (!string.IsNullOrEmpty(item.FulfillmentChoice) && _fulfillmentComboBox.Items.Contains(item.FulfillmentChoice))
                {
                    _fulfillmentComboBox.SelectedItem = item.FulfillmentChoice;
                }
                else if (_fulfillmentComboBox.Items.Count > 0)
                {
                    _fulfillmentComboBox.SelectedIndex = 0;
                }

                _updateRxButton.Enabled = true;
                _updateRxButton.Text = "✏ Save Update";
                _removeRxButton.Text = "🗑";
                _removeRxButton.Enabled = true;
                _addRxButton.Enabled = false;
                _clearRxInputsButton.Text = "Cancel Edit";
                UpdateStockBadge();
            }
        }

        private void ClearRxInputs()
        {
            _editingPrescriptionItem = null;

            _medicineComboBox.Enabled = true;
            _lblMed.Text = "Medication Type / Category *";
            _lblMed.ForeColor = Color.FromArgb(71, 85, 105);

            _instructionsTextBox.Clear();
            _quantityNumeric.Value = 1;
            if (_fulfillmentComboBox.Items.Count == 0)
            {
                _fulfillmentComboBox.Items.AddRange(new object[] { "Clinic Pharmacy", "External Pharmacy" });
            }
            if (_fulfillmentComboBox.Items.Count > 0) _fulfillmentComboBox.SelectedIndex = 0;

            _updateRxButton.Text = "✏ Update";
            _updateRxButton.Enabled = false;
            _removeRxButton.Text = "🗑";
            _removeRxButton.Enabled = false;
            _addRxButton.Enabled = false;
            _clearRxInputsButton.Text = "Reset to Default";

            _isResettingSelection = true;
            try
            {
                _rxGrid.ClearSelection();
                _rxGrid.CurrentCell = null;
            }
            finally
            {
                _isResettingSelection = false;
            }

            PopulateCategoryDropdown();
        }

        private void MedicineComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_isUpdatingMedicineDropdown) return;

            if (_isViewingCategories)
            {
                if (_medicineComboBox.SelectedItem is MedicineCategoryOption cat)
                {
                    if (!cat.IsPrompt && !string.IsNullOrWhiteSpace(cat.CategoryName))
                    {
                        ShowMedicinesForCategory(cat.CategoryName);
                    }
                    else
                    {
                        UpdateStockBadge();
                    }
                }
            }
            else
            {
                if (_medicineComboBox.SelectedItem is MedicineReturnOption)
                {
                    PopulateCategoryDropdown();
                    if (IsHandleCreated)
                    {
                        BeginInvoke(() =>
                        {
                            if (_isViewingCategories && !_medicineComboBox.IsDisposed && _medicineComboBox.Enabled)
                            {
                                _medicineComboBox.DroppedDown = true;
                            }
                        });
                    }
                }
                else if (_medicineComboBox.SelectedItem is MedicineItem)
                {
                    UpdateStockBadge();
                }
            }
        }

        private void PopulateCategoryDropdown()
        {
            _isUpdatingMedicineDropdown = true;
            _isViewingCategories = true;
            _currentCategory = null;

            try
            {
                _medicineComboBox.BeginUpdate();
                _medicineComboBox.DataSource = null;
                _medicineComboBox.DisplayMember = "DisplayText";
                _medicineComboBox.Items.Clear();

                _medicineComboBox.Items.Add(new MedicineCategoryOption { IsPrompt = true });

                var categories = _allMedicines
                    .GroupBy(m => string.IsNullOrWhiteSpace(m.Category) ? "General" : m.Category.Trim())
                    .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

                foreach (var group in categories)
                {
                    _medicineComboBox.Items.Add(new MedicineCategoryOption
                    {
                        CategoryName = group.Key,
                        Count = group.Count()
                    });
                }

                _medicineComboBox.SelectedIndex = 0;
                _medicineComboBox.EndUpdate();
            }
            finally
            {
                _isUpdatingMedicineDropdown = false;
            }

            _lblMed.Text = "Medication Type / Category *";
            _lblMed.ForeColor = Color.FromArgb(71, 85, 105);
            UpdateStockBadge();
        }

        private void ShowMedicinesForCategory(string categoryName, int? selectMedicineId = null)
        {
            _isUpdatingMedicineDropdown = true;
            _isViewingCategories = false;
            _currentCategory = categoryName;

            try
            {
                _medicineComboBox.BeginUpdate();
                _medicineComboBox.DataSource = null;
                _medicineComboBox.DisplayMember = "DisplayText";
                _medicineComboBox.Items.Clear();

                // First choice MUST be go back or return to category types
                _medicineComboBox.Items.Add(new MedicineReturnOption());

                var matchingMedicines = _allMedicines
                    .Where(m => string.Equals(string.IsNullOrWhiteSpace(m.Category) ? "General" : m.Category.Trim(), categoryName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(m => m.GenericName)
                    .ToList();

                foreach (var med in matchingMedicines)
                {
                    _medicineComboBox.Items.Add(med);
                }

                int targetIndex = 0;
                if (selectMedicineId.HasValue)
                {
                    for (int i = 1; i < _medicineComboBox.Items.Count; i++)
                    {
                        if (_medicineComboBox.Items[i] is MedicineItem m && m.MedicineId == selectMedicineId.Value)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                }

                _medicineComboBox.SelectedIndex = targetIndex;
                _medicineComboBox.EndUpdate();
            }
            finally
            {
                _isUpdatingMedicineDropdown = false;
            }

            _lblMed.Text = $"Medication ({categoryName}) *";
            _lblMed.ForeColor = Color.FromArgb(71, 85, 105);

            UpdateStockBadge();

            if (!selectMedicineId.HasValue && IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    if (!_isViewingCategories && !_medicineComboBox.IsDisposed && _medicineComboBox.Enabled)
                    {
                        _medicineComboBox.DroppedDown = true;
                    }
                });
            }
        }

        private void UpdateStockBadge()
        {
            if (_medicineComboBox.SelectedItem is not MedicineItem med)
            {
                _addRxButton.Enabled = false;
                if (_isViewingCategories)
                {
                    _lblStockBadge.Text = "👉 Please select a medicine type above to view available medications.";
                    _lblStockBadge.ForeColor = Color.FromArgb(100, 116, 139);
                }
                else
                {
                    _lblStockBadge.Text = "👉 Select a medication from the list or choose 'Go Back' to return to types.";
                    _lblStockBadge.ForeColor = Color.FromArgb(100, 116, 139);
                }
                return;
            }

            if (_editingPrescriptionItem == null)
            {
                _addRxButton.Enabled = true;
            }

            string fulfillment = _fulfillmentComboBox.Text;
            if (string.Equals(fulfillment, "Clinic Pharmacy", StringComparison.OrdinalIgnoreCase))
            {
                if (med.AvailableStock <= 0)
                {
                    _lblStockBadge.Text = "🔴 In-House Stock: 0 pcs (Out of Stock — External Rx Recommended)";
                    _lblStockBadge.ForeColor = Color.FromArgb(220, 38, 38);
                }
                else if (med.AvailableStock <= 10)
                {
                    _lblStockBadge.Text = $"🟡 In-House Stock: {med.AvailableStock} pcs remaining (Low Stock)";
                    _lblStockBadge.ForeColor = Color.FromArgb(217, 119, 6);
                }
                else
                {
                    _lblStockBadge.Text = $"🟢 In-House Stock: {med.AvailableStock} pcs available";
                    _lblStockBadge.ForeColor = Color.FromArgb(16, 185, 129);
                }
            }
            else
            {
                _lblStockBadge.Text = "🌐 External Pharmacy (Patient will purchase outside)";
                _lblStockBadge.ForeColor = Color.FromArgb(79, 70, 229);
            }
        }

        private async void CompleteConsultationButton_Click(object? sender, EventArgs e)
        {
            if (_activePatient is null)
            {
                MessageBox.Show("Please select an active patient from the waiting queue.", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string chiefComplaint = _chiefComplaintTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(chiefComplaint))
            {
                MessageBox.Show("Please enter the patient's Chief Complaint before completing the consultation.", "Clinical QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _chiefComplaintTextBox.Focus();
                return;
            }

            string diagnosis = _diagnosisTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(diagnosis) || diagnosis.Length < 3)
            {
                MessageBox.Show("A valid clinical Diagnosis is required to complete this consultation (minimum 3 characters).\n\nPlease enter the primary diagnosis (e.g. Acute Upper Respiratory Tract Infection, Essential Hypertension).", "Clinical QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _diagnosisTextBox.Focus();
                return;
            }

            if (MessageBox.Show("Complete and submit this consultation?", "Confirm Completion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int? docId = GetCurrentDoctorId();
            int doctorId = docId ?? 1;
            int currentPatientId = _activePatient.PatientId;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _service.CompleteConsultation(
                    _activePatient.AppointmentId,
                    _activePatient.PatientId,
                    doctorId,
                    _chiefComplaintTextBox.Text,
                    _clinicalNotesTextBox.Text,
                    _diagnosisTextBox.Text,
                    _recommendationTextBox.Text,
                    _prescriptionList.ToList()
                ));

                MessageBox.Show(result.Message, "ClinicFlow", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (result.Success)
                {
                    if (_prescriptionList.Count > 0)
                    {
                        var printPrompt = MessageBox.Show(
                            "Consultation completed successfully!\n\nWould you like to open the Patient EHR & Prescription View to preview or print the prescription?",
                            "Prescription Completed",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (printPrompt == DialogResult.Yes)
                        {
                            try
                            {
                                using var historyDlg = new PatientHistoryForm(currentPatientId);
                                historyDlg.ShowDialog(this);
                            }
                            catch { }
                        }
                    }

                    ClearWorkspace();
                    await ReloadQueueAsync();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearWorkspace()
        {
            _activePatient = null;
            _lblActivePatientName.Text = "No Patient Selected";
            _lblActivePatientMeta.Text = "Waiting for queue selection...";
            _lblActiveConcern.Text = "";

            _lblBannerPatientName.Text = "No Consultation Active";
            _lblBannerPatientDetails.Text = "Select a patient from the waiting queue to begin consultation.";

            _chiefComplaintTextBox.Clear();
            _clinicalNotesTextBox.Clear();
            _diagnosisTextBox.Clear();
            _recommendationTextBox.Clear();
            _prescriptionList.Clear();
            ClearRxInputs();
        }
    }
}