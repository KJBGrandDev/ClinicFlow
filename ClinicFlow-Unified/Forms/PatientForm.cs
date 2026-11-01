using ClinicFlow_Unified.Utilities;
#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Services;
namespace ClinicFlow_Unified.Forms
{
    public partial class PatientForm : Form
    {
        private readonly PatientService _service = new();
        private List<Patient> _patients = new();
        private Patient? _selectedPatient = null;
        private bool _isClearing = false;

        public PatientForm()
        {
            InitializeComponent();
            ClinicTheme.ApplyForm(this);
            ClinicTheme.ApplyGrid(this._patientsGrid);
            ApplyThemeHierarchy();
            ConfigureGridColumns();
            WireEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.WindowState = FormWindowState.Maximized;
            UpdateAgeDisplay();
            await LoadPatientsAsync();
        }

        private void ApplyThemeHierarchy()
        {
            // Grid Header Styling
            Color headerBg = Color.FromArgb(241, 245, 249);
            Color headerFg = Color.FromArgb(71, 85, 105);

            _patientsGrid.EnableHeadersVisualStyles = false;
            _patientsGrid.ColumnHeadersDefaultCellStyle.BackColor = headerBg;
            _patientsGrid.ColumnHeadersDefaultCellStyle.ForeColor = headerFg;
            _patientsGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBg;
            _patientsGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = headerFg;
            _patientsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _patientsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _patientsGrid.ColumnHeadersHeight = 34;

            // Wire action button state changes
            _btnRegister.EnabledChanged += (_, _) => UpdateActionButtonsVisualState();
            _btnUpdate.EnabledChanged += (_, _) => UpdateActionButtonsVisualState();
            _btnArchive.EnabledChanged += (_, _) => UpdateActionButtonsVisualState();
            UpdateActionButtonsVisualState();

            // Restore button styling
            _btnRestore.FlatStyle = FlatStyle.Flat;
            _btnRestore.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _btnRestore.BackColor = Color.FromArgb(240, 253, 244);
            _btnRestore.ForeColor = Color.FromArgb(22, 101, 52);
            _btnRestore.FlatAppearance.BorderColor = Color.FromArgb(187, 247, 208);
            _btnRestore.FlatAppearance.BorderSize = 1;

            // Search & Clear Outlined Buttons
            Button[] outlined = { _btnSearch, _btnClear };
            foreach (var b in outlined)
            {
                b.BackColor = Color.White;
                b.ForeColor = Color.FromArgb(30, 41, 59);
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                b.FlatAppearance.BorderSize = 1;
                b.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            }
        }

        private void UpdateActionButtonsVisualState()
        {
            Color emeraldCta = Color.FromArgb(16, 185, 129);
            Color amberUpdateBg = Color.FromArgb(245, 158, 11);
            Color disabledBg = Color.FromArgb(226, 232, 240);
            Color disabledFg = Color.FromArgb(148, 163, 184);

            // Register CTA: Emerald Green (#10B981) when enabled, Slate Grey when disabled
            _btnRegister.FlatStyle = FlatStyle.Flat;
            _btnRegister.FlatAppearance.BorderSize = 0;
            _btnRegister.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            if (_btnRegister.Enabled)
            {
                _btnRegister.BackColor = emeraldCta;
                _btnRegister.ForeColor = Color.White;
                _btnRegister.Cursor = Cursors.Hand;
            }
            else
            {
                _btnRegister.BackColor = disabledBg;
                _btnRegister.ForeColor = disabledFg;
                _btnRegister.Cursor = Cursors.Default;
            }

            // Update Profile: Warm Amber (#F59E0B) when enabled, Slate Grey when disabled
            _btnUpdate.FlatStyle = FlatStyle.Flat;
            _btnUpdate.FlatAppearance.BorderSize = 0;
            _btnUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            if (_btnUpdate.Enabled)
            {
                _btnUpdate.BackColor = amberUpdateBg;
                _btnUpdate.ForeColor = Color.White;
                _btnUpdate.Cursor = Cursors.Hand;
            }
            else
            {
                _btnUpdate.BackColor = disabledBg;
                _btnUpdate.ForeColor = disabledFg;
                _btnUpdate.Cursor = Cursors.Default;
            }

            // Archive Button: Crimson Red Outline when enabled, Slate Grey when disabled
            _btnArchive.FlatStyle = FlatStyle.Flat;
            _btnArchive.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            if (_btnArchive.Enabled)
            {
                _btnArchive.BackColor = Color.FromArgb(254, 242, 242);
                _btnArchive.ForeColor = Color.FromArgb(220, 38, 38);
                _btnArchive.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
                _btnArchive.FlatAppearance.BorderSize = 1;
                _btnArchive.Cursor = Cursors.Hand;
            }
            else
            {
                _btnArchive.BackColor = Color.FromArgb(241, 245, 249);
                _btnArchive.ForeColor = disabledFg;
                _btnArchive.FlatAppearance.BorderColor = disabledBg;
                _btnArchive.FlatAppearance.BorderSize = 1;
                _btnArchive.Cursor = Cursors.Default;
            }
        }

        private void ConfigureGridColumns()
        {
            _patientsGrid.AutoGenerateColumns = false;
            _patientsGrid.Columns.Clear();
            _patientsGrid.RowTemplate.Height = 32;

            _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.PatientNumber),
                HeaderText = "Patient #",
                Width = 85
            });

            _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.FullName),
                HeaderText = "Full Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 120,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            var colAge = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.Age),
                HeaderText = "Age",
                Width = 42,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            colAge.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _patientsGrid.Columns.Add(colAge);

            var colSex = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.Sex),
                HeaderText = "Sex",
                Width = 48,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            colSex.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _patientsGrid.Columns.Add(colSex);

            _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.ContactNumber),
                HeaderText = "Contact #",
                Width = 105
            });

            _patientsGrid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _patients.Count) return;

                var pat = _patients[e.RowIndex];

                // If age column and birth year is 1800 or unconfigured, display warning
                if (e.ColumnIndex >= 0 && _patientsGrid.Columns[e.ColumnIndex].DataPropertyName == nameof(Patient.Age))
                {
                    if (pat.BirthDate.Year <= 1800 || pat.BirthDate < new DateTime(1900, 1, 1) || pat.Age <= 0)
                    {
                        e.Value = "⚠️ Set DOB";
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        e.FormattingApplied = true;
                    }
                }

                bool isSelected = (_selectedPatient != null && pat.PatientId == _selectedPatient.PatientId);

                if (isSelected)
                {
                    e.CellStyle.BackColor = Color.FromArgb(239, 246, 255);
                    e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
                    e.CellStyle.SelectionForeColor = Color.White;
                }
                else
                {
                    e.CellStyle.BackColor = Color.White;
                    e.CellStyle.ForeColor = Color.FromArgb(30, 41, 59);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
                    e.CellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
                }
            };
        }

        private void WireEvents()
        {
            _btnSearch.Click += async (_, _) => await LoadPatientsAsync(_searchTextBox.Text.Trim());
            _searchTextBox.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await LoadPatientsAsync(_searchTextBox.Text.Trim());
                }
            };

            _dtpBirthDate.ValueChanged += (_, _) => UpdateAgeDisplay();
            _dtpBirthDate.DropDown += (_, _) =>
            {
                if (_dtpBirthDate.Value.Year <= 1800)
                {
                    _dtpBirthDate.Value = new DateTime(2000, 1, 1);
                    UpdateAgeDisplay();
                }
            };
            _patientsGrid.SelectionChanged += PatientsGrid_SelectionChanged;

            _btnRegister.Click += async (_, _) => await RegisterPatientAsync();
            _btnUpdate.Click += async (_, _) => await UpdatePatientAsync();
            _btnArchive.Click += async (_, _) => await ArchivePatientAsync();
            _btnRestore.Click += async (_, _) => await RestorePatientAsync();
            _btnClear.Click += (_, _) => ClearFormFields();
            _chkShowArchived.CheckedChanged += async (_, _) => await LoadPatientsAsync(_searchTextBox.Text.Trim());

            // Quality Assurance: Text-only restrictions for names & locality
            _txtFirstName.KeyPress += TextOnly_KeyPress;
            _txtMiddleName.KeyPress += TextOnly_KeyPress;
            _txtLastName.KeyPress += TextOnly_KeyPress;
            _txtSuffix.KeyPress += TextOnly_KeyPress;
            _txtEmergencyPerson.KeyPress += TextOnly_KeyPress;
            _txtCity.KeyPress += TextOnly_KeyPress;
            _txtMunicipality.KeyPress += TextOnly_KeyPress;

            // Quality Assurance: Numeric-only restrictions for 11-digit phone numbers
            _txtContactNumber.KeyPress += NumericOnly_KeyPress;
            _txtEmergencyPhone.KeyPress += NumericOnly_KeyPress;
        }

        private void TextOnly_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            // Allow letters (including international characters like ñ, accented letters), spaces, hyphens, periods, apostrophes
            if (char.IsLetter(e.KeyChar) || e.KeyChar == ' ' || e.KeyChar == '-' || e.KeyChar == '.' || e.KeyChar == '\'')
            {
                return;
            }

            e.Handled = true; // Reject numbers and symbols
        }

        private void NumericOnly_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            // Reject any non-digit character
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Enforce Philippine mobile number starting with "09"
            if (sender is TextBox tb)
            {
                int cursor = tb.SelectionStart;
                // If typing at the first position, it must start with '0'
                if (cursor == 0 && e.KeyChar != '0')
                {
                    e.Handled = true;
                    return;
                }
                // If typing at the second position right after '0', it must be '9'
                if (cursor == 1 && tb.Text.Length > 0 && tb.Text[0] == '0' && e.KeyChar != '9')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void UpdateAgeDisplay()
        {
            DateTime birth = _dtpBirthDate.Value.Date;
            if (birth.Year <= 1800 || birth < new DateTime(1900, 1, 1))
            {
                _lblCalculatedAge.Text = "⚠️ Birth Date Required";
                _lblCalculatedAge.ForeColor = Color.FromArgb(185, 28, 28);
                _lblCalculatedAge.BackColor = Color.FromArgb(254, 226, 226);
                return;
            }

            int age = DateTime.Today.Year - birth.Year;
            if (birth.Date > DateTime.Today.AddYears(-age)) age--;
            age = Math.Max(0, age);

            _lblCalculatedAge.Text = $"{age} yrs old";
            _lblCalculatedAge.ForeColor = Color.FromArgb(29, 78, 216);
            _lblCalculatedAge.BackColor = Color.FromArgb(239, 246, 255);
        }

        private async Task LoadPatientsAsync(string search = "")
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                bool showArchived = _chkShowArchived.Checked;
                var res = await Task.Run(() => _service.Search(search, showArchived));
                if (res.Success)
                {
                    _patients = res.Patients;
                    _patientsGrid.DataSource = null;
                    _patientsGrid.DataSource = _patients;
                    _lblDirectoryTitle.Text = showArchived ? "Archived Patient Directory" : "Registered Patient Directory";
                    _lblDirectoryCount.Text = showArchived
                        ? $"Total Archived: {_patients.Count} patient(s)"
                        : $"Total Registered: {_patients.Count} patient(s)";
                    _patientsGrid.ClearSelection();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PatientsGrid_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isClearing || _patientsGrid.SelectedRows.Count == 0) return;

            if (_patientsGrid.SelectedRows[0].DataBoundItem is Patient p)
            {
                _selectedPatient = p;

                // Populate form fields
                _txtFirstName.Text = p.FirstName;
                _txtMiddleName.Text = p.MiddleName ?? "";
                _txtLastName.Text = p.LastName;
                _txtSuffix.Text = p.Suffix ?? "";
                _cboSex.SelectedItem = p.Sex;
                _dtpBirthDate.Value = (p.BirthDate.Year <= 1800 || p.BirthDate == DateTime.MinValue)
                    ? new DateTime(1800, 1, 1)
                    : p.BirthDate;
                UpdateAgeDisplay();

                _txtContactNumber.Text = p.ContactNumber;
                _txtStreetAddress.Text = p.StreetAddress;
                _txtCity.Text = p.City;
                _txtMunicipality.Text = p.Municipality;
                _txtEmergencyPerson.Text = p.EmergencyContactName ?? "";
                _txtEmergencyPhone.Text = p.EmergencyContactNumber ?? "";

                // Update visual header and action buttons based on archived vs active state
                if (!p.IsActive || _chkShowArchived.Checked)
                {
                    _lblBannerTitle.Text = $"📁 Archived Profile: {p.FullName} ({p.PatientNumber})";
                    _lblBannerTitle.ForeColor = Color.FromArgb(100, 116, 139);
                    _lblBannerSubtitle.Text = "This patient record is currently archived. Click 'Restore' to reactivate this patient.";

                    _btnRegister.Enabled = false;
                    _btnUpdate.Enabled = false;
                    _btnArchive.Visible = false;
                    _btnArchive.Enabled = false;
                    _btnRestore.Visible = true;
                    _btnRestore.Enabled = true;
                }
                else
                {
                    _lblBannerTitle.Text = $"✏ Editing Profile: {p.FullName} ({p.PatientNumber})";
                    _lblBannerTitle.ForeColor = Color.FromArgb(180, 83, 9); // Warm Amber
                    _lblBannerSubtitle.Text = "Modify the patient record and click 'Update Profile', or click 'Clear Fields' to switch back to new registration.";

                    _btnRegister.Enabled = false;
                    _btnUpdate.Enabled = true;
                    _btnArchive.Visible = true;
                    _btnArchive.Enabled = true;
                    _btnRestore.Visible = false;
                    _btnRestore.Enabled = false;
                }

                _patientsGrid.Refresh();
            }
        }

        private bool ValidateFormInputs()
        {
            if (string.IsNullOrWhiteSpace(_txtFirstName.Text))
            {
                MessageBox.Show("First Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtLastName.Text))
            {
                MessageBox.Show("Last Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLastName.Focus();
                return false;
            }

            if (_dtpBirthDate.Value.Year <= 1800 || _dtpBirthDate.Value < new DateTime(1900, 1, 1))
            {
                MessageBox.Show("Birth Date is required and cannot be 1/1/1800 or unconfigured. Please select a valid birth date (1900 to present).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dtpBirthDate.Focus();
                return false;
            }

            if (_dtpBirthDate.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Birth date cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dtpBirthDate.Focus();
                return false;
            }

            string contact = _txtContactNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(contact))
            {
                MessageBox.Show("Contact Number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtContactNumber.Focus();
                return false;
            }
            if (contact.Length != 11 || !contact.All(char.IsDigit) || !contact.StartsWith("09"))
            {
                MessageBox.Show("Contact Number must be a valid 11-digit Philippine mobile number starting with '09' (e.g., 09171234567).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtContactNumber.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtStreetAddress.Text))
            {
                MessageBox.Show("Street / Barangay Address is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtStreetAddress.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_txtCity.Text))
            {
                MessageBox.Show("City is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtCity.Focus();
                return false;
            }

            // Emergency Contact Mutual Dependency Validation
            bool hasEmergName = !string.IsNullOrWhiteSpace(_txtEmergencyPerson.Text);
            bool hasEmergPhone = !string.IsNullOrWhiteSpace(_txtEmergencyPhone.Text);

            if (hasEmergPhone && !hasEmergName)
            {
                MessageBox.Show("Contact Person Full Name must be stated when an Emergency Contact Phone # is provided.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmergencyPerson.Focus();
                return false;
            }

            if (hasEmergName && !hasEmergPhone)
            {
                MessageBox.Show("Emergency Contact Phone # must be stated when a Contact Person Name is provided.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmergencyPhone.Focus();
                return false;
            }

            if (hasEmergPhone)
            {
                string emergPhone = _txtEmergencyPhone.Text.Trim();
                if (emergPhone.Length != 11 || !emergPhone.All(char.IsDigit) || !emergPhone.StartsWith("09"))
                {
                    MessageBox.Show("Emergency Contact Phone must be a valid 11-digit Philippine mobile number starting with '09' (e.g., 09171234567).",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtEmergencyPhone.Focus();
                    return false;
                }
            }

            return true;
        }

        private async Task RegisterPatientAsync()
        {
            if (!ValidateFormInputs()) return;

            var p = BuildPatientFromInputs();
            p.PatientId = 0;

            Cursor = Cursors.WaitCursor;
            try
            {
                var res = await Task.Run(() => _service.Register(p));
                MessageBox.Show(res.Message, "Patient Registration", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (res.Success)
                {
                    ClearFormFields();
                    await LoadPatientsAsync();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task UpdatePatientAsync()
        {
            if (_selectedPatient == null)
            {
                MessageBox.Show("Please select a patient from the directory to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateFormInputs()) return;

            var p = BuildPatientFromInputs();
            p.PatientId = _selectedPatient.PatientId;
            p.PatientNumber = _selectedPatient.PatientNumber;
            p.RowVersion = _selectedPatient.RowVersion;

            Cursor = Cursors.WaitCursor;
            try
            {
                var res = await Task.Run(() => _service.Update(p));
                if (!res.Success && res.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase))
                {
                    var reload = MessageBox.Show(res.Message + "\n\nWould you like to reload the latest patient record now?", "Concurrency Conflict", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (reload == DialogResult.Yes)
                    {
                        await LoadPatientsAsync(_searchTextBox.Text.Trim());
                    }
                    return;
                }

                MessageBox.Show(res.Message, "Update Profile", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (res.Success)
                {
                    ClearFormFields();
                    await LoadPatientsAsync(_searchTextBox.Text.Trim());
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task ArchivePatientAsync()
        {
            if (_selectedPatient == null) return;

            var confirm = MessageBox.Show(
                $"Are you sure you want to archive patient record {_selectedPatient.FullName} ({_selectedPatient.PatientNumber})?",
                "Confirm Archive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var res = await Task.Run(() => _service.Delete(_selectedPatient.PatientId));
                MessageBox.Show(res.Message, "Archive Patient", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (res.Success)
                {
                    ClearFormFields();
                    await LoadPatientsAsync(_searchTextBox.Text.Trim());
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task RestorePatientAsync()
        {
            if (_selectedPatient == null) return;

            var confirm = MessageBox.Show(
                $"Are you sure you want to restore patient record {_selectedPatient.FullName} ({_selectedPatient.PatientNumber}) back to active status?",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var res = await Task.Run(() => _service.Restore(_selectedPatient.PatientId));
                MessageBox.Show(res.Message, "Restore Patient", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (res.Success)
                {
                    ClearFormFields();
                    await LoadPatientsAsync(_searchTextBox.Text.Trim());
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private Patient BuildPatientFromInputs()
        {
            DateTime birth = _dtpBirthDate.Value.Date;
            int age = 0;
            if (birth.Year > 1800)
            {
                age = DateTime.Today.Year - birth.Year;
                if (birth.Date > DateTime.Today.AddYears(-age)) age--;
                age = Math.Max(0, age);
            }
            else
            {
                birth = new DateTime(1800, 1, 1);
            }

            return new Patient
            {
                FirstName = _txtFirstName.Text.Trim(),
                MiddleName = string.IsNullOrWhiteSpace(_txtMiddleName.Text) ? null : _txtMiddleName.Text.Trim(),
                LastName = _txtLastName.Text.Trim(),
                Suffix = string.IsNullOrWhiteSpace(_txtSuffix.Text) ? null : _txtSuffix.Text.Trim(),
                Sex = _cboSex.SelectedItem?.ToString() ?? "Male",
                BirthDate = birth,
                Age = age,
                ContactNumber = _txtContactNumber.Text.Trim(),
                StreetAddress = _txtStreetAddress.Text.Trim(),
                City = _txtCity.Text.Trim(),
                Municipality = _txtMunicipality.Text.Trim(),
                EmergencyContactName = string.IsNullOrWhiteSpace(_txtEmergencyPerson.Text) ? null : _txtEmergencyPerson.Text.Trim(),
                EmergencyContactNumber = string.IsNullOrWhiteSpace(_txtEmergencyPhone.Text) ? null : _txtEmergencyPhone.Text.Trim()
            };
        }

        private void ClearFormFields()
        {
            _isClearing = true;
            try
            {
                _selectedPatient = null;

                _txtFirstName.Clear();
                _txtMiddleName.Clear();
                _txtLastName.Clear();
                _txtSuffix.Clear();
                _cboSex.SelectedIndex = 0;
                _dtpBirthDate.Value = new DateTime(1800, 1, 1);
                UpdateAgeDisplay();

                _txtContactNumber.Clear();
                _txtStreetAddress.Clear();
                _txtCity.Text = "Davao City";
                _txtMunicipality.Text = "Davao";
                _txtEmergencyPerson.Clear();
                _txtEmergencyPhone.Clear();

                _lblBannerTitle.Text = "➕ New Patient Registration";
                _lblBannerTitle.ForeColor = Color.FromArgb(15, 23, 42);
                _lblBannerSubtitle.Text = "Fill in the demographic and contact information below to register a new clinical profile.";

                _btnRegister.Enabled = true;
                _btnUpdate.Enabled = false;
                _btnArchive.Visible = true;
                _btnArchive.Enabled = false;
                _btnRestore.Visible = false;
                _btnRestore.Enabled = false;

                _patientsGrid.ClearSelection();
                _patientsGrid.Refresh();
            }
            finally
            {
                _isClearing = false;
            }
        }
    }
}