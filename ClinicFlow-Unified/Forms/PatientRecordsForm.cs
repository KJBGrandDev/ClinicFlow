using ClinicFlow_Unified.Utilities;
#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
namespace ClinicFlow_Unified.Forms
{
    public partial class PatientRecordsForm : Form
    {
        private readonly PatientRepository _patientRepo = new();
        private readonly PatientHistoryService _historyService = new();
        private List<Patient> _patients = new();
        private List<PatientEncounterRecord> _encounters = new();
        private Patient? _selectedPatient = null;
        private bool _isPopulating = false;

        // 300ms Debounce Timer for smooth live typing
        private readonly System.Windows.Forms.Timer _searchDebounceTimer = new();

        public PatientRecordsForm()
        {
            InitializeComponent();
            ClinicTheme.ApplyForm(this);
            ApplyThemeHierarchy();
            ConfigureGrids();
            ConfigureDebounceTimer();
            WireEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.WindowState = FormWindowState.Maximized;
            await SearchPatientsAsync(string.Empty);
        }

        private void ConfigureDebounceTimer()
        {
            _searchDebounceTimer.Interval = 300; // 300ms pause triggers search
            _searchDebounceTimer.Tick += async (s, e) =>
            {
                _searchDebounceTimer.Stop();
                await SearchPatientsAsync(_searchTextBox.Text.Trim());
            };
        }

        private void ApplyThemeHierarchy()
        {
            Color headerBg = Color.FromArgb(241, 245, 249);
            Color headerFg = Color.FromArgb(71, 85, 105);

            foreach (var grid in new[] { _patientsGrid, _timelineGrid, _rxGrid })
            {
                grid.EnableHeadersVisualStyles = false;
                grid.ColumnHeadersDefaultCellStyle.BackColor = headerBg;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = headerFg;
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBg;
                grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = headerFg;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            }

            _btnSearch.BackColor = Color.FromArgb(16, 185, 129);
            _btnSearch.ForeColor = Color.White;
            _btnSearch.FlatStyle = FlatStyle.Flat;
            _btnSearch.FlatAppearance.BorderSize = 0;

            _btnRefresh.BackColor = Color.White;
            _btnRefresh.ForeColor = Color.FromArgb(30, 41, 59);
            _btnRefresh.FlatStyle = FlatStyle.Flat;
            _btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnRefresh.FlatAppearance.BorderSize = 1;
        }

        private void ConfigureGrids()
        {
            // 1. Patient Directory Grid
            _patientsGrid.AutoGenerateColumns = false;
            _patientsGrid.Columns.Clear();

            _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.PatientNumber),
                HeaderText = "Patient #",
                Width = 80
            });

            _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.FullName),
                HeaderText = "Full Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.Age),
                HeaderText = "Age",
                Width = 38,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(Patient.Sex),
                HeaderText = "Sex",
                Width = 45,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _patientsGrid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _patients.Count) return;
                var pat = _patients[e.RowIndex];
                if (e.ColumnIndex >= 0 && _patientsGrid.Columns[e.ColumnIndex].DataPropertyName == nameof(Patient.Age))
                {
                    if (pat.BirthDate.Year <= 1800 || pat.Age <= 0)
                    {
                        e.Value = "--";
                        e.FormattingApplied = true;
                    }
                }
            };

            // 2. Timeline Grid
            _timelineGrid.AutoGenerateColumns = false;
            _timelineGrid.Columns.Clear();

            _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PatientEncounterRecord.FormattedDate),
                HeaderText = "Visit Date",
                Width = 165,
                DefaultCellStyle = { Font = new Font("Segoe UI", 8.5F, FontStyle.Regular), Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _timelineGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PatientEncounterRecord.Diagnosis),
                HeaderText = "Diagnosis",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // 3. Prescriptions Grid
            _rxGrid.AutoGenerateColumns = false;
            _rxGrid.Columns.Clear();

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PastPrescriptionItem.MedicineName),
                HeaderText = "Medication",
                Width = 300,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PastPrescriptionItem.Quantity),
                HeaderText = "Qty",
                Width = 55,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PastPrescriptionItem.Instructions),
                HeaderText = "Sig / Dosage Instructions",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _rxGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PastPrescriptionItem.FulfillmentChoice),
                HeaderText = "Fulfillment",
                Width = 140
            });
        }

        private void WireEvents()
        {
            // Live Search: restarts the 300ms timer on each keystroke
            _searchTextBox.TextChanged += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                _searchDebounceTimer.Start();
            };

            // Instant Search on Enter or Click
            _btnSearch.Click += async (_, _) =>
            {
                _searchDebounceTimer.Stop();
                await SearchPatientsAsync(_searchTextBox.Text.Trim());
            };

            _searchTextBox.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _searchDebounceTimer.Stop();
                    await SearchPatientsAsync(_searchTextBox.Text.Trim());
                }
            };

            _patientsGrid.CellClick += async (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _patients.Count)
                {
                    _selectedPatient = _patients[e.RowIndex];
                    await LoadSelectedPatientHistoryAsync(_selectedPatient.PatientId);
                }
            };

            _patientsGrid.SelectionChanged += async (_, _) =>
            {
                if (_isPopulating) return;

                if (_patientsGrid.CurrentRow?.DataBoundItem is Patient p && p != _selectedPatient)
                {
                    _selectedPatient = p;
                    await LoadSelectedPatientHistoryAsync(p.PatientId);
                }
            };

            _timelineGrid.SelectionChanged += (_, _) =>
            {
                if (_timelineGrid.CurrentRow?.DataBoundItem is PatientEncounterRecord enc)
                {
                    DisplayEncounter(enc);
                }
            };

            _btnRefresh.Click += async (_, _) =>
            {
                if (_selectedPatient != null)
                    await LoadSelectedPatientHistoryAsync(_selectedPatient.PatientId);
                else
                    await SearchPatientsAsync(_searchTextBox.Text.Trim());
            };
        }

        private async Task SearchPatientsAsync(string query)
        {
            Cursor = Cursors.WaitCursor;
            _isPopulating = true;
            try
            {
                _patients = await Task.Run(() => _patientRepo.Search(query));
                _patientsGrid.DataSource = null;
                _patientsGrid.DataSource = _patients;
                _lblDirectoryCount.Text = $"Found {_patients.Count} registered patient(s).";

                if (_patients.Count > 0)
                {
                    _patientsGrid.Rows[0].Selected = true;
                    _selectedPatient = _patients[0];
                    await LoadSelectedPatientHistoryAsync(_selectedPatient.PatientId);
                }
                else
                {
                    ClearRecordView();
                }
            }
            finally
            {
                _isPopulating = false;
                Cursor = Cursors.Default;
            }
        }

        private async Task LoadSelectedPatientHistoryAsync(int patientId)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _historyService.LoadPatientRecord(patientId));
                if (result.Success && result.Summary != null)
                {
                    _lblPatientTitle.Text = $"{result.Summary.FullName} ({result.Summary.PatientNumber})";
                    string ageDisplay = result.Summary.Age > 0 ? $"{result.Summary.Age}" : "Not Set";
                    _lblPatientDetails.Text = $"Age: {ageDisplay} • Sex: {result.Summary.Sex} • Contact: {result.Summary.ContactNumber} • Total Lifetime Visits: {result.Summary.TotalVisitsCount}";

                    _encounters = result.Encounters;
                    _timelineGrid.DataSource = null;
                    _timelineGrid.DataSource = _encounters;

                    if (_encounters.Count > 0)
                    {
                        _timelineGrid.Rows[0].Selected = true;
                        DisplayEncounter(_encounters[0]);
                    }
                    else
                    {
                        ClearEncounterView();
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DisplayEncounter(PatientEncounterRecord enc)
        {
            _lblEncounterInfo.Text = $"Encounter: {enc.FormattedDate} • Attending: {enc.DoctorName} ({enc.Specialty})";
            _txtChief.Text = enc.ChiefComplaint;
            _txtDiag.Text = enc.Diagnosis;
            _txtNotes.Text = enc.ClinicalNotes;
            _txtRecs.Text = enc.DoctorRecommendation;

            _rxGrid.DataSource = null;
            _rxGrid.DataSource = enc.Prescriptions;
        }

        private void ClearEncounterView()
        {
            _lblEncounterInfo.Text = "No previous clinical encounter recorded for this patient.";
            _txtChief.Clear();
            _txtDiag.Clear();
            _txtNotes.Clear();
            _txtRecs.Clear();
            _rxGrid.DataSource = null;
        }

        private void ClearRecordView()
        {
            _selectedPatient = null;
            _lblPatientTitle.Text = "No Patient Selected";
            _lblPatientDetails.Text = "Select any patient from the directory on the left.";
            _timelineGrid.DataSource = null;
            ClearEncounterView();
        }
    }
}
