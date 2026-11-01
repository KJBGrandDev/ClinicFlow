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
    public partial class AppointmentForm : Form
    {
        private readonly AppointmentService _service = new();
        private readonly AppointmentRepository _repository = new();
        private List<DoctorLookupItem> _doctors = new();
        private List<PatientLookupItem> _patients = new();
        private List<QueueBoardItem> _queueItems = new();
        private List<TimeSlotItem> _slots = new();
        private int? _selectedFilterDoctorId = null;
        private readonly System.Windows.Forms.Timer _patientSearchTimer = new();

        private enum ConflictStatus
        {
            None,
            Clear,
            Notice,
            Conflict
        }
        private ConflictStatus _currentConflictStatus = ConflictStatus.None;

        public AppointmentForm()
        {
            InitializeComponent();
            ClinicTheme.ApplyForm(this);
            ClinicTheme.ApplyGrid(this._lobbyQueueGrid);
            ApplyThemeHierarchy();
            ConfigureLobbyGridColumns();
            ConfigureSearchDebounceTimer();
            WireEvents();
        }

        private void ConfigureSearchDebounceTimer()
        {
            _patientSearchTimer.Interval = 250;
            _patientSearchTimer.Tick += async (s, e) =>
            {
                _patientSearchTimer.Stop();
                await SearchPatientsAsync();
            };
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.WindowState = FormWindowState.Maximized;
            await LoadInitialDataAsync();
        }

        private void ApplyThemeHierarchy()
        {
            this.UseWaitCursor = false;
            this.Cursor = Cursors.Default;
            _lobbyQueueGrid.UseWaitCursor = false;
            _lobbyQueueGrid.Cursor = Cursors.Default;
            _rightPanel.UseWaitCursor = false;
            _rightPanel.Cursor = Cursors.Default;

            ClinicTheme.ApplyPrimaryButton(_btnBookAppointment);

            Button[] outlinedButtons = { _btnFindPatient, _btnClearBooking, _btnRefreshQueue, _btnFilterAll };
            foreach (var b in outlinedButtons)
            {
                ClinicTheme.ApplyOutlinedButton(b);
            }

            UpdateActionButtonStates();
        }

        private void ConfigureLobbyGridColumns()
        {
            _lobbyQueueGrid.AutoGenerateColumns = false;
            _lobbyQueueGrid.Columns.Clear();
            _lobbyQueueGrid.RowTemplate.Height = 34;

            _lobbyQueueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.QueueNumber),
                HeaderText = "Ticket",
                Width = 70,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _lobbyQueueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.PatientNumber),
                HeaderText = "Patient #",
                Width = 95
            });

            _lobbyQueueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.PatientName),
                HeaderText = "Patient Full Name",
                Width = 185,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _lobbyQueueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.DoctorName),
                HeaderText = "Attending",
                Width = 145
            });

            _lobbyQueueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.ScheduledTime),
                HeaderText = "Time",
                Width = 75,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _lobbyQueueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.QueueStatus),
                HeaderText = "Current Status",
                Width = 135,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _lobbyQueueGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.Reason),
                HeaderText = "Chief Concern",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            _lobbyQueueGrid.CellFormatting += LobbyQueueGrid_CellFormatting;
        }

        private void LobbyQueueGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _queueItems.Count) return;

            var item = _queueItems[e.RowIndex];
            string status = item.QueueStatus?.Trim() ?? "";

            // If Ticket column and QueueNumber is 0 (Booked, not checked in), show '—'
            if (e.ColumnIndex == 0 && item.QueueNumber <= 0)
            {
                e.Value = "—";
                e.FormattingApplied = true;
            }

            if (status.Equals("Displaced", StringComparison.OrdinalIgnoreCase))
            {
                // Displaced due to Doctor Emergency: Soft Crimson Alert
                e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
                e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                e.CellStyle.SelectionBackColor = Color.FromArgb(220, 38, 38);
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
            else if (status.Equals("In Service", StringComparison.OrdinalIgnoreCase))
            {
                // Active Consultation in Room: Mint Green
                e.CellStyle.BackColor = Color.FromArgb(236, 253, 245);
                e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);
                e.CellStyle.SelectionBackColor = Color.FromArgb(16, 185, 129);
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
            else if (status.Equals("Called", StringComparison.OrdinalIgnoreCase))
            {
                // Called to Station: Warm Amber
                e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                e.CellStyle.SelectionBackColor = Color.FromArgb(245, 158, 11);
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
            else if (status.Equals("Waiting", StringComparison.OrdinalIgnoreCase))
            {
                // In Lobby Waiting: Soft Sky Blue
                e.CellStyle.BackColor = Color.FromArgb(239, 246, 255);
                e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216);
                e.CellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
            else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                // Consultation Done: Muted Gray
                e.CellStyle.BackColor = Color.FromArgb(241, 245, 249);
                e.CellStyle.ForeColor = Color.FromArgb(148, 163, 184);
                e.CellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
                e.CellStyle.SelectionForeColor = Color.FromArgb(100, 116, 139);
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            }
            else if (status.Equals("Skipped", StringComparison.OrdinalIgnoreCase) ||
                     status.Equals("No-Show", StringComparison.OrdinalIgnoreCase))
            {
                // Skipped / No-Show: Soft Red
                e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
                e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                e.CellStyle.SelectionBackColor = Color.FromArgb(239, 68, 68);
                e.CellStyle.SelectionForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            }
            else
            {
                // Booked / Confirmed: Clean White / Neutral Slate
                e.CellStyle.BackColor = Color.White;
                e.CellStyle.ForeColor = Color.FromArgb(71, 85, 105);
                e.CellStyle.SelectionBackColor = Color.FromArgb(226, 232, 240);
                e.CellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            }
        }

        private void TimeSlotComboBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _slots.Count) return;

            var slot = _slots[e.Index];
            bool isBooked = !slot.IsAvailable;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color bgColor;
            Color textColor;

            if (isSelected)
            {
                if (isBooked)
                {
                    bgColor = Color.FromArgb(254, 226, 226);
                    textColor = Color.FromArgb(185, 28, 28);
                }
                else
                {
                    bgColor = Color.FromArgb(16, 185, 129);
                    textColor = Color.White;
                }
            }
            else
            {
                if (isBooked)
                {
                    bgColor = Color.FromArgb(248, 250, 252);
                    textColor = Color.FromArgb(148, 163, 184);
                }
                else
                {
                    bgColor = Color.White;
                    textColor = Color.FromArgb(15, 23, 42);
                }
            }

            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string text = slot.DisplayText;
            using (var font = new Font("Segoe UI", 9F, isBooked ? FontStyle.Regular : FontStyle.Bold))
            {
                var rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 2, e.Bounds.Width - 8, e.Bounds.Height - 4);
                TextRenderer.DrawText(e.Graphics, text, font, rect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }

            e.DrawFocusRectangle();
        }

        private void PatientComboBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _patients.Count)
            {
                using var winBrush = new SolidBrush(_patientComboBox.BackColor);
                e.Graphics.FillRectangle(winBrush, e.Bounds);
                return;
            }

            var patient = _patients[e.Index];
            bool isFace = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit || !_patientComboBox.DroppedDown;

            if (!isFace)
            {
                // Dropdown popup list item: ALWAYS clean, neutral, and readable across all rows
                bool isHighlighted = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color itemBg = isHighlighted ? Color.FromArgb(224, 242, 254) : Color.White;
                Color itemFg = isHighlighted ? Color.FromArgb(3, 105, 161) : Color.FromArgb(30, 41, 59);

                using (var brush = new SolidBrush(itemBg))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                using (var font = new Font("Segoe UI", 9F, FontStyle.Regular))
                {
                    var rect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 2, e.Bounds.Width - 12, e.Bounds.Height - 4);
                    TextRenderer.DrawText(e.Graphics, patient.DisplayText, font, rect, itemFg, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
                }

                if (isHighlighted)
                {
                    e.DrawFocusRectangle();
                }
                return;
            }

            // Closed ComboBox Face: Displays the confirmed status of the currently selected patient
            Color faceBg;
            Color faceFg;
            string prefix = "";

            switch (_currentConflictStatus)
            {
                case ConflictStatus.Conflict:
                    faceBg = Color.FromArgb(254, 242, 242);
                    faceFg = Color.FromArgb(185, 28, 28);
                    prefix = "⚠️ ";
                    break;
                case ConflictStatus.Notice:
                    faceBg = Color.FromArgb(239, 246, 255);
                    faceFg = Color.FromArgb(29, 78, 216);
                    prefix = "ℹ️ ";
                    break;
                case ConflictStatus.Clear:
                    faceBg = Color.FromArgb(240, 253, 244);
                    faceFg = Color.FromArgb(22, 101, 52);
                    prefix = "✓ ";
                    break;
                default:
                    faceBg = Color.White;
                    faceFg = Color.FromArgb(15, 23, 42);
                    prefix = "";
                    break;
            }

            using (var brush = new SolidBrush(faceBg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string text = prefix + patient.DisplayText;
            using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                var rect = new Rectangle(e.Bounds.X + 6, e.Bounds.Y + 2, e.Bounds.Width - 10, e.Bounds.Height - 4);
                TextRenderer.DrawText(e.Graphics, text, font, rect, faceFg, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }
        }

        private int GetSelectedDoctorId()
        {
            if (_doctorComboBox.SelectedValue is int id && id > 0) return id;
            if (_doctorComboBox.SelectedItem is DoctorLookupItem doc) return doc.DoctorId;
            if (_doctors.Count > 0) return _doctors[0].DoctorId;
            return 0;
        }

        private void WireEvents()
        {
            _patientComboBox.DrawItem += PatientComboBox_DrawItem;
            _timeSlotComboBox.DrawItem += TimeSlotComboBox_DrawItem;

            _timeSlotComboBox.SelectionChangeCommitted += (s, e) =>
            {
                if (_timeSlotComboBox.SelectedIndex >= 0 && _timeSlotComboBox.SelectedIndex < _slots.Count)
                {
                    var slot = _slots[_timeSlotComboBox.SelectedIndex];
                    if (!slot.IsAvailable)
                    {
                        MessageBox.Show("This time slot is already booked and cannot be selected. Please choose an open slot.", "Slot Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        int firstOpen = _slots.FindIndex(st => st.IsAvailable);
                        if (firstOpen >= 0)
                        {
                            _timeSlotComboBox.SelectedIndex = firstOpen;
                        }
                    }
                }
            };

            // Live Debounced Search: triggers 250ms after typing
            _patientSearchTextBox.TextChanged += (s, e) =>
            {
                _patientSearchTimer.Stop();
                _patientSearchTimer.Start();
            };

            _btnFindPatient.Click += async (_, _) =>
            {
                _patientSearchTimer.Stop();
                await SearchPatientsAsync();
            };

            _patientSearchTextBox.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _patientSearchTimer.Stop();
                    await SearchPatientsAsync();
                }
            };

            // Re-evaluate conflict dynamically on patient selection
            _patientComboBox.SelectedIndexChanged += async (_, _) => await UpdatePatientBookingConflictAsync();

            _doctorComboBox.SelectedIndexChanged += async (_, _) =>
            {
                await RefreshSlotsAsync();
                await UpdatePatientBookingConflictAsync();
            };

            _appointmentDatePicker.ValueChanged += async (_, _) =>
            {
                await RefreshSlotsAsync();
                await UpdatePatientBookingConflictAsync();
            };

            _btnBookAppointment.Click += async (_, _) => await BookAppointmentAsync();
            _btnClearBooking.Click += (_, _) => ClearBookingForm();

            _btnRefreshQueue.Click += async (_, _) => await ReloadQueueAsync();
            _btnFilterAll.Click += async (_, _) =>
            {
                _selectedFilterDoctorId = null;
                HighlightActiveDoctorFilterButton();
                await ReloadQueueAsync();
            };

            _btnCheckIn.Click += async (_, _) => await HandleCheckInAsync();
            _btnCallPatient.Click += async (_, _) => await HandleUpdateStatusAsync("Called");
            _btnSkipNoShow.Click += async (_, _) => await HandleUpdateStatusAsync("Skipped");

            _btnOpenEmergencyTriage.Click += async (_, _) =>
            {
                using var dlg = new EmergencyTriageDialog(DateTime.Today, _selectedFilterDoctorId);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    await RefreshSlotsAsync();
                    await ReloadQueueAsync();
                }
            };

            // Control button availability dynamically based on selected row status
            _lobbyQueueGrid.SelectionChanged += (s, e) => UpdateActionButtonStates();

            _lobbyQueueGrid.MouseEnter += (s, e) => { Cursor.Current = Cursors.Default; };
            _lobbyQueueGrid.MouseMove += (s, e) => { if (_lobbyQueueGrid.Cursor != Cursors.Default) _lobbyQueueGrid.Cursor = Cursors.Default; };
        }

        private void UpdateActionButtonStates()
        {
            if (_lobbyQueueGrid.CurrentRow?.DataBoundItem is QueueBoardItem item)
            {
                string status = item.QueueStatus?.Trim() ?? "";

                bool isDisplaced = status.Equals("Displaced", StringComparison.OrdinalIgnoreCase);
                bool isBooked = (status.Equals("Booked", StringComparison.OrdinalIgnoreCase) ||
                                status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)) && !isDisplaced;
                bool isWaiting = status.Equals("Waiting", StringComparison.OrdinalIgnoreCase);
                bool isCalled = status.Equals("Called", StringComparison.OrdinalIgnoreCase);
                bool isInService = status.Equals("In Service", StringComparison.OrdinalIgnoreCase);
                bool isCompleted = status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
                bool isSkipped = status.Equals("Skipped", StringComparison.OrdinalIgnoreCase) ||
                                 status.Equals("No-Show", StringComparison.OrdinalIgnoreCase);

                // Check-In is ONLY allowed for Booked/Confirmed patients who have not checked in and are not displaced
                bool canCheckIn = isBooked && item.QueueNumber <= 0 && !isDisplaced;

                // Call Patient is ONLY allowed for patients already physically waiting in lobby
                bool canCall = isWaiting;

                // Skip / No-Show is enabled for active pending states (Booked, Waiting, Called)
                bool canSkip = isBooked || isWaiting || isCalled;

                SetButtonState(_btnCheckIn, canCheckIn, Color.FromArgb(16, 185, 129), Color.White); // Emerald 600
                SetButtonState(_btnCallPatient, canCall, Color.FromArgb(245, 158, 11), Color.White); // Amber 500
                SetButtonState(_btnSkipNoShow, canSkip, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(254, 202, 202)); // Soft Danger outline
            }
            else
            {
                SetButtonState(_btnCheckIn, false, Color.FromArgb(16, 185, 129), Color.White);
                SetButtonState(_btnCallPatient, false, Color.FromArgb(245, 158, 11), Color.White);
                SetButtonState(_btnSkipNoShow, false, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(254, 202, 202));
            }

            SetButtonState(_btnRefreshQueue, true, Color.White, Color.FromArgb(30, 41, 59), Color.FromArgb(203, 213, 225));
        }

        private static void SetButtonState(Button button, bool enabled, Color activeBg, Color activeFg, Color? activeBorder = null)
        {
            button.Enabled = enabled;
            button.FlatStyle = FlatStyle.Flat;
            if (enabled)
            {
                button.BackColor = activeBg;
                button.ForeColor = activeFg;
                button.Cursor = Cursors.Hand;
                if (activeBorder.HasValue)
                {
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = activeBorder.Value;
                }
                else
                {
                    button.FlatAppearance.BorderSize = 0;
                }
            }
            else
            {
                // Consistent disabled/inactive appearance following ClinicFlow standards
                button.BackColor = Color.FromArgb(241, 245, 249); // Slate-100 disabled background
                button.ForeColor = Color.FromArgb(148, 163, 184); // Slate-400 disabled text
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240); // Slate-200 border
                button.Cursor = Cursors.Default;
            }
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var docRes = await Task.Run(() => _service.LoadDoctors());
                if (docRes.Success)
                {
                    _doctors = docRes.Doctors;

                    _doctorComboBox.DisplayMember = nameof(DoctorLookupItem.DisplayText);
                    _doctorComboBox.ValueMember = nameof(DoctorLookupItem.DoctorId);
                    _doctorComboBox.DataSource = _doctors;

                    BuildDoctorFilterButtons();
                }

                await SearchPatientsAsync();
                await RefreshSlotsAsync();
                await ReloadQueueAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error loading initial data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.UseWaitCursor = false;
                this.Cursor = Cursors.Default;
                _lobbyQueueGrid.UseWaitCursor = false;
                _lobbyQueueGrid.Cursor = Cursors.Default;
            }
        }

        private void BuildDoctorFilterButtons()
        {
            _flpDoctorFilter.Controls.Clear();
            _flpDoctorFilter.Controls.Add(_btnFilterAll);

            foreach (var doc in _doctors)
            {
                var btn = new Button
                {
                    Text = "🩺 " + doc.DoctorName,
                    AutoSize = true,
                    MinimumSize = new Size(140, 30),
                    Padding = new Padding(10, 0, 10, 0),
                    Tag = doc.DoctorId,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(30, 41, 59),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 8.5F)
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

                btn.Click += async (s, e) =>
                {
                    _selectedFilterDoctorId = (int?)((Button)s!).Tag;
                    HighlightActiveDoctorFilterButton();
                    await ReloadQueueAsync();
                };

                _flpDoctorFilter.Controls.Add(btn);
            }

            HighlightActiveDoctorFilterButton();
        }

        private void HighlightActiveDoctorFilterButton()
        {
            foreach (Control c in _flpDoctorFilter.Controls)
            {
                if (c is Button b)
                {
                    bool isActive = (b.Tag == null && _selectedFilterDoctorId == null) ||
                                    (b.Tag is int id && _selectedFilterDoctorId == id);
                    if (isActive)
                    {
                        b.BackColor = Color.FromArgb(15, 118, 110);
                        b.ForeColor = Color.White;
                        b.FlatAppearance.BorderColor = Color.FromArgb(15, 118, 110);
                        b.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    }
                    else
                    {
                        b.BackColor = Color.White;
                        b.ForeColor = Color.FromArgb(30, 41, 59);
                        b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
                        b.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
                    }
                }
            }
        }

        private async Task SearchPatientsAsync()
        {
            try
            {
                string query = _patientSearchTextBox.Text.Trim();
                var res = await Task.Run(() => _service.SearchPatients(query, 20));

                _patients = res.Patients;
                _patientComboBox.BeginUpdate();
                _patientComboBox.Items.Clear();

                foreach (var p in _patients)
                {
                    _patientComboBox.Items.Add(p.DisplayText);
                }

                if (string.IsNullOrWhiteSpace(query))
                {
                    _lblSearchFeedback.Text = $"Showing top {_patients.Count} registered patients • Type to live filter";
                    _lblSearchFeedback.ForeColor = Color.FromArgb(100, 116, 139);
                }
                else
                {
                    _lblSearchFeedback.Text = _patients.Count > 0
                        ? $"Found {_patients.Count} matching patient(s) (top 20 max)"
                        : "No matching patients found. Try another search term.";
                    _lblSearchFeedback.ForeColor = _patients.Count > 0 ? Color.FromArgb(15, 118, 110) : Color.FromArgb(220, 38, 38);
                }

                if (_patientComboBox.Items.Count > 0)
                {
                    _patientComboBox.SelectedIndex = 0;
                }
                else
                {
                    _patientComboBox.SelectedIndex = -1;
                }

                _patientComboBox.EndUpdate();
                await UpdatePatientBookingConflictAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error searching patients: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyPatientStatusVisuals(
            Color wrapperBorder,
            Color boxBg,
            Color boxText,
            Color cardBg,
            Color cardBorder,
            Color titleColor,
            string titleText,
            Color msgColor,
            string msgText,
            bool bookingEnabled)
        {
            _pnlPatientWrapper.BackColor = wrapperBorder;
            _patientComboBox.BackColor = boxBg;
            _patientComboBox.ForeColor = boxText;

            _pnlConflictAlert.BackColor = cardBg;
            _lblConflictTitle.ForeColor = titleColor;
            _lblConflictTitle.Text = titleText;
            _lblConflictMessage.ForeColor = msgColor;
            _lblConflictMessage.Text = msgText;

            _btnBookAppointment.Enabled = bookingEnabled;
            if (bookingEnabled)
            {
                ClinicTheme.ApplyPrimaryButton(_btnBookAppointment);
            }
            else
            {
                _btnBookAppointment.BackColor = Color.FromArgb(203, 213, 225);
                _btnBookAppointment.ForeColor = Color.FromArgb(148, 163, 184);
            }

            _patientComboBox.Invalidate();
            _pnlPatientWrapper.Invalidate();
            _pnlConflictAlert.Invalidate();
        }

        private static string FormatTimeString(string timeStr)
        {
            if (string.IsNullOrWhiteSpace(timeStr)) return "Scheduled Time";
            if (DateTime.TryParse(timeStr, out var dt))
                return dt.ToString("hh:mm tt");
            if (TimeSpan.TryParse(timeStr, out var ts))
                return DateTime.Today.Add(ts).ToString("hh:mm tt");
            return timeStr;
        }

        private string GetSelectedDoctorName()
        {
            if (_doctorComboBox.SelectedItem is DoctorLookupItem doc)
                return doc.DoctorName;
            int docId = GetSelectedDoctorId();
            var found = _doctors.FirstOrDefault(d => d.DoctorId == docId);
            return found?.DoctorName ?? "Attending Doctor";
        }

        private async Task UpdatePatientBookingConflictAsync()
        {
            try
            {
                if (_patientComboBox.SelectedIndex < 0 || _patientComboBox.SelectedIndex >= _patients.Count)
                {
                    _currentConflictStatus = ConflictStatus.None;
                    ApplyPatientStatusVisuals(
                        wrapperBorder: Color.FromArgb(203, 213, 225),
                        boxBg: Color.White,
                        boxText: Color.FromArgb(15, 23, 42),
                        cardBg: Color.FromArgb(248, 250, 252),
                        cardBorder: Color.FromArgb(226, 232, 240),
                        titleColor: Color.FromArgb(71, 85, 105),
                        titleText: "ℹ️ PATIENT SCHEDULE VERIFICATION",
                        msgColor: Color.FromArgb(100, 116, 139),
                        msgText: "Select a patient from the dropdown above to verify schedule availability and doctor conflicts.",
                        bookingEnabled: false
                    );
                    return;
                }

                // Immediately reset conflict status to neutral so prior patient's status does not leak
                _currentConflictStatus = ConflictStatus.None;
                _pnlPatientWrapper.BackColor = Color.FromArgb(203, 213, 225);
                _patientComboBox.BackColor = Color.White;
                _patientComboBox.ForeColor = Color.FromArgb(15, 23, 42);
                _lblConflictTitle.ForeColor = Color.FromArgb(71, 85, 105);
                _lblConflictTitle.Text = "⏳ VERIFYING PATIENT SCHEDULE...";
                _lblConflictMessage.ForeColor = Color.FromArgb(100, 116, 139);
                _lblConflictMessage.Text = "Checking database for existing appointments and schedule conflicts...";
                _pnlConflictAlert.BackColor = Color.FromArgb(248, 250, 252);
                _patientComboBox.Invalidate();

                var patient = _patients[_patientComboBox.SelectedIndex];
                int selectedDocId = GetSelectedDoctorId();
                string selectedDocName = GetSelectedDoctorName();
                DateTime selectedDate = _appointmentDatePicker.Value.Date;

                var res = await Task.Run(() => _service.GetPatientBookingsForDate(patient.PatientId, selectedDate));

                // Guard against stale async completion if user switched selection while querying
                if (_patientComboBox.SelectedIndex < 0 || _patientComboBox.SelectedIndex >= _patients.Count || _patients[_patientComboBox.SelectedIndex].PatientId != patient.PatientId)
                {
                    return;
                }
                if (!res.Success)
                {
                    _currentConflictStatus = ConflictStatus.None;
                    ApplyPatientStatusVisuals(
                        wrapperBorder: Color.FromArgb(203, 213, 225),
                        boxBg: Color.White,
                        boxText: Color.FromArgb(15, 23, 42),
                        cardBg: Color.FromArgb(254, 242, 242),
                        cardBorder: Color.FromArgb(248, 113, 113),
                        titleColor: Color.FromArgb(185, 28, 28),
                        titleText: "⚠️ VERIFICATION TEMPORARILY UNAVAILABLE",
                        msgColor: Color.FromArgb(153, 27, 27),
                        msgText: "Unable to verify existing patient schedule conflicts at this moment.",
                        bookingEnabled: true
                    );
                    return;
                }

                var bookings = res.Bookings;
                var sameDoctorBooking = selectedDocId > 0 ? bookings.FirstOrDefault(b => b.DoctorId == selectedDocId) : null;

                if (sameDoctorBooking != null)
                {
                    // HARD CONFLICT: Cannot double-book the same doctor on the same day
                    _currentConflictStatus = ConflictStatus.Conflict;
                    string timeFormatted = FormatTimeString(sameDoctorBooking.ScheduledTime);
                    string ticketInfo = sameDoctorBooking.QueueNumber > 0 ? $" (Queue Ticket #{sameDoctorBooking.QueueNumber:D2})" : "";

                    ApplyPatientStatusVisuals(
                        wrapperBorder: Color.FromArgb(248, 113, 113),
                        boxBg: Color.FromArgb(254, 242, 242),
                        boxText: Color.FromArgb(185, 28, 28),
                        cardBg: Color.FromArgb(254, 242, 242),
                        cardBorder: Color.FromArgb(248, 113, 113),
                        titleColor: Color.FromArgb(185, 28, 28),
                        titleText: "⚠️ SCHEDULING CONFLICT — SAME ATTENDING DOCTOR",
                        msgColor: Color.FromArgb(153, 27, 27),
                        msgText: $"Patient is already booked with {sameDoctorBooking.DoctorName} ({sameDoctorBooking.Specialty}) on {selectedDate:MMM dd, yyyy} at {timeFormatted}{ticketInfo}.\nDuplicate appointments with the same doctor on the same day are prohibited.",
                        bookingEnabled: false
                    );
                }
                else if (bookings.Count > 0)
                {
                    // INFORMATIVE NOTICE: Multi-doctor booking is allowed on the same day!
                    _currentConflictStatus = ConflictStatus.Notice;
                    var otherDocsList = string.Join("; ", bookings.Select(b => $"{b.DoctorName} ({b.Specialty}) at {FormatTimeString(b.ScheduledTime)}"));

                    ApplyPatientStatusVisuals(
                        wrapperBorder: Color.FromArgb(96, 165, 250),
                        boxBg: Color.FromArgb(239, 246, 255),
                        boxText: Color.FromArgb(29, 78, 216),
                        cardBg: Color.FromArgb(239, 246, 255),
                        cardBorder: Color.FromArgb(96, 165, 250),
                        titleColor: Color.FromArgb(29, 78, 216),
                        titleText: "ℹ️ MULTI-SPECIALTY NOTICE — CROSS-CONSULTATION PERMITTED",
                        msgColor: Color.FromArgb(30, 64, 175),
                        msgText: $"Patient has consultation(s) today with: {otherDocsList}.\nCross-consultation with {selectedDocName} is allowed. You may proceed with booking.",
                        bookingEnabled: true
                    );
                }
                else
                {
                    // CLEAR: No bookings on this date
                    _currentConflictStatus = ConflictStatus.Clear;

                    ApplyPatientStatusVisuals(
                        wrapperBorder: Color.FromArgb(74, 222, 128),
                        boxBg: Color.FromArgb(240, 253, 244),
                        boxText: Color.FromArgb(22, 101, 52),
                        cardBg: Color.FromArgb(240, 253, 244),
                        cardBorder: Color.FromArgb(74, 222, 128),
                        titleColor: Color.FromArgb(22, 101, 52),
                        titleText: "✓ SCHEDULE CLEAR — ELIGIBLE TO BOOK",
                        msgColor: Color.FromArgb(21, 128, 61),
                        msgText: $"No conflicting appointments found on {selectedDate:MMM dd, yyyy}.\nPatient is clear and eligible to be scheduled with {selectedDocName}.",
                        bookingEnabled: true
                    );
                }
            }
            catch
            {
                // Fail-safe
            }
        }

        private async Task RefreshSlotsAsync()
        {
            int docId = GetSelectedDoctorId();
            if (docId <= 0) return;

            DateTime date = _appointmentDatePicker.Value.Date;
            var res = await Task.Run(() => _service.GenerateAvailableSlots(docId, date));

            if (res.Success)
            {
                _slots = res.Slots;

                _timeSlotComboBox.DataSource = null;
                _timeSlotComboBox.DataSource = _slots;
                _timeSlotComboBox.DisplayMember = nameof(TimeSlotItem.DisplayText);

                int firstAvailableIndex = _slots.FindIndex(s => s.IsAvailable);
                if (firstAvailableIndex >= 0)
                {
                    _timeSlotComboBox.SelectedIndex = firstAvailableIndex;
                }
                else if (_slots.Count > 0)
                {
                    _timeSlotComboBox.SelectedIndex = 0;
                }
            }
        }

        private async Task BookAppointmentAsync()
        {
            try
            {
                if (_patientComboBox.SelectedIndex < 0 || _patientComboBox.SelectedIndex >= _patients.Count)
                {
                    MessageBox.Show("Please select a patient.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var selectedPatient = _patients[_patientComboBox.SelectedIndex];
                int doctorId = GetSelectedDoctorId();

                if (doctorId <= 0)
                {
                    MessageBox.Show("Please select an attending doctor.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_appointmentDatePicker.Value.Date < DateTime.Today)
                {
                    MessageBox.Show("Appointments cannot be booked for past dates. Please select today or a future date.", "Scheduling QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _appointmentDatePicker.Focus();
                    return;
                }

                string reason = _reasonTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(reason) || reason.Length < 3)
                {
                    MessageBox.Show("Please provide a reason or chief concern for the appointment (minimum 3 characters, e.g. 'General Consultation', 'Follow-up', 'Fever & Cough').", "Scheduling QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _reasonTextBox.Focus();
                    return;
                }

                if (_timeSlotComboBox.SelectedItem is not TimeSlotItem slot || !slot.IsAvailable)
                {
                    MessageBox.Show("Selected slot is not available. Please choose an open slot.", "Slot Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var res = await Task.Run(() => _service.BookAppointment(
                    selectedPatient.PatientId,
                    doctorId,
                    _appointmentDatePicker.Value.Date,
                    slot.StartTime,
                    slot.EndTime,
                    reason
                ));

                MessageBox.Show(res.Message, "Appointment Booking", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (res.Success)
                {
                    ClearBookingForm();
                    await SearchPatientsAsync();
                    await RefreshSlotsAsync();
                    await ReloadQueueAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error booking appointment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ReloadQueueAsync()
        {
            try
            {
                var qRes = await Task.Run(() => _service.LoadTodayQueue(_selectedFilterDoctorId));
                if (qRes.Success)
                {
                    _queueItems = qRes.Queue;
                    _lobbyQueueGrid.DataSource = null;
                    _lobbyQueueGrid.DataSource = _queueItems;

                    UpdateDashboardKpis();
                    UpdateActiveRoomCards();
                    UpdateActionButtonStates();
                }

                // Check for displaced appointments today to manage emergency banner
                var displacedRes = await Task.Run(() => _service.GetDisplacedAppointments(DateTime.Today, _selectedFilterDoctorId));
                int displacedCount = displacedRes.Items?.Count ?? 0;
                if (displacedCount > 0)
                {
                    _lblEmergencyAlertText.Text = $"🚨 EMERGENCY SCHEDULE ALERT: {displacedCount} patient(s) displaced due to doctor emergency blockout! Action required.";
                    _pnlEmergencyAlertBanner.Visible = true;
                }
                else
                {
                    _pnlEmergencyAlertBanner.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error reloading queue: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.UseWaitCursor = false;
                this.Cursor = Cursors.Default;
                _lobbyQueueGrid.UseWaitCursor = false;
                _lobbyQueueGrid.Cursor = Cursors.Default;
            }
        }

        private void UpdateDashboardKpis()
        {
            int total = _queueItems.Count;
            int inService = _queueItems.Count(q => q.QueueStatus.Equals("In Service", StringComparison.OrdinalIgnoreCase));
            int called = _queueItems.Count(q => q.QueueStatus.Equals("Called", StringComparison.OrdinalIgnoreCase));
            int waiting = _queueItems.Count(q => q.QueueStatus.Equals("Waiting", StringComparison.OrdinalIgnoreCase));
            int booked = _queueItems.Count(q => q.QueueStatus.Equals("Booked", StringComparison.OrdinalIgnoreCase) ||
                                                q.QueueStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase));

            _lblKpiTotal.Text = $"👥 Total Today: {total}";
            _lblKpiInService.Text = $"🩺 In Service: {inService}";
            _lblKpiCalled.Text = $"📢 Called: {called}";
            _lblKpiWaiting.Text = $"⏳ Waiting: {waiting}";
        }

        private void UpdateActiveRoomCards()
        {
            _flpRoomStations.Controls.Clear();

            // Active consultation rooms include stations with patients In Service, Called, or Waiting in lobby
            var activeDoctorGroups = _queueItems
                .Where(q => q.QueueStatus.Equals("In Service", StringComparison.OrdinalIgnoreCase) ||
                            q.QueueStatus.Equals("Called", StringComparison.OrdinalIgnoreCase) ||
                            q.QueueStatus.Equals("Waiting", StringComparison.OrdinalIgnoreCase))
                .GroupBy(q => q.DoctorId)
                .OrderBy(g => g.Any(q => q.QueueStatus.Equals("In Service", StringComparison.OrdinalIgnoreCase)) ? 0 :
                              g.Any(q => q.QueueStatus.Equals("Called", StringComparison.OrdinalIgnoreCase)) ? 1 : 2)
                .ThenBy(g => g.First().DoctorName);

            foreach (var docGroup in activeDoctorGroups)
            {
                var docItems = docGroup.ToList();
                var inService = docItems.FirstOrDefault(q => q.QueueStatus.Equals("In Service", StringComparison.OrdinalIgnoreCase));
                var called = docItems.FirstOrDefault(q => q.QueueStatus.Equals("Called", StringComparison.OrdinalIgnoreCase));
                var waitingItems = docItems.Where(q => q.QueueStatus.Equals("Waiting", StringComparison.OrdinalIgnoreCase))
                                           .OrderBy(q => q.QueueNumber > 0 ? q.QueueNumber : 9999)
                                           .ToList();
                int waitingCount = waitingItems.Count;
                string doctorName = docItems[0].DoctorName;

                var card = new Panel
                {
                    Size = new Size(275, 96),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(10, 8, 10, 8),
                    Margin = new Padding(0, 0, 12, 0)
                };

                var lblDoc = new Label
                {
                    Text = doctorName,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    Dock = DockStyle.Top,
                    Height = 20,
                    UseMnemonic = false
                };

                string badgeText;
                Color badgeFore;
                Color badgeBack;
                string patientText;
                string waitText;

                if (inService != null)
                {
                    badgeText = $"● IN SERVICE • Ticket Q-{inService.QueueNumber:D2}";
                    badgeFore = Color.FromArgb(4, 120, 87);
                    badgeBack = Color.FromArgb(236, 253, 244);
                    patientText = $"Patient: {inService.PatientName}";
                    waitText = waitingCount > 0 ? $"👥 {waitingCount} patient(s) waiting in lobby" : "👥 No other patients in line";
                }
                else if (called != null)
                {
                    badgeText = $"📢 CALLED • Ticket Q-{called.QueueNumber:D2}";
                    badgeFore = Color.FromArgb(180, 83, 9);
                    badgeBack = Color.FromArgb(254, 243, 199);
                    patientText = $"Called: {called.PatientName}";
                    waitText = waitingCount > 0 ? $"👥 {waitingCount} patient(s) waiting in lobby" : "👥 No other patients in line";
                }
                else
                {
                    // Doctor is not yet in or consulting, but patients are checked in and waiting in the lobby
                    var nextPatient = waitingItems.FirstOrDefault();
                    string ticketPart = nextPatient != null && nextPatient.QueueNumber > 0
                        ? $"Next: Q-{nextPatient.QueueNumber:D2}"
                        : "Line Forming";
                    badgeText = $"⏳ AWAITING DOCTOR • {ticketPart}";
                    badgeFore = Color.FromArgb(67, 56, 202);
                    badgeBack = Color.FromArgb(238, 242, 255);
                    patientText = nextPatient != null ? $"Next in line: {nextPatient.PatientName}" : "Patients checked in";
                    waitText = $"👥 {waitingCount} patient(s) waiting in lobby";
                }

                var lblBadge = new Label
                {
                    Text = badgeText,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = badgeFore,
                    BackColor = badgeBack,
                    Dock = DockStyle.Top,
                    Height = 20,
                    TextAlign = ContentAlignment.MiddleCenter,
                    UseMnemonic = false
                };

                var lblPat = new Label
                {
                    Text = patientText,
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    Dock = DockStyle.Top,
                    Height = 20,
                    UseMnemonic = false
                };

                var lblWait = new Label
                {
                    Text = waitText,
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    Dock = DockStyle.Bottom,
                    Height = 18,
                    UseMnemonic = false
                };

                card.Controls.Add(lblWait);
                card.Controls.Add(lblPat);
                card.Controls.Add(lblBadge);
                card.Controls.Add(lblDoc);

                _flpRoomStations.Controls.Add(card);
            }

            if (_flpRoomStations.Controls.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No active room queues currently. Stations will appear here when patients check in or enter consultation.",
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    AutoSize = true,
                    Padding = new Padding(4, 12, 0, 0),
                    UseMnemonic = false
                };
                _flpRoomStations.Controls.Add(emptyLabel);
            }
        }

        private async Task HandleCheckInAsync()
        {
            if (_lobbyQueueGrid.CurrentRow?.DataBoundItem is not QueueBoardItem item)
            {
                MessageBox.Show("Please select a booked patient from the queue to check-in.", "Select Patient", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string status = item.QueueStatus?.Trim() ?? "";

            if (status.Equals("Displaced", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    $"This appointment for {item.PatientName} ({item.PatientNumber}) is currently displaced due to a doctor emergency blockout.\n\nPlease click '⚡ Open Emergency Triage Center' above to transfer the patient to an available peer physician or to reschedule their visit.",
                    "Displaced Appointment - Triage Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This consultation has already been completed.", "Already Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (status.Equals("Waiting", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Called", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("In Service", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This patient has already checked in and is present in the lobby queue.", "Already Arrived", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var res = await Task.Run(() => _service.CheckInPatient(item.AppointmentId, item.PatientId, item.DoctorId));
            MessageBox.Show(res.Message, "Patient Check-In", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (res.Success)
                await ReloadQueueAsync();
        }

        private async Task HandleUpdateStatusAsync(string newStatus)
        {
            if (_lobbyQueueGrid.CurrentRow?.DataBoundItem is not QueueBoardItem item)
            {
                MessageBox.Show($"Please select a patient from the queue to mark as {newStatus}.", "Select Patient", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (item.QueueStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("This consultation has already been completed and cannot be modified.", "Already Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (item.QueueId <= 0)
            {
                MessageBox.Show("This patient has not checked in yet. Please check them in first before updating queue status.", "Patient Not Arrived", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var res = await Task.Run(() => _service.UpdateStatus(item.QueueId, newStatus));
            if (res.Success)
                await ReloadQueueAsync();
            else
                MessageBox.Show(res.Message, "Status Update Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ClearBookingForm()
        {
            _patientSearchTextBox.Clear();
            _reasonTextBox.Clear();
            _appointmentDatePicker.Value = DateTime.Today;
            if (_slots.Count > 0)
            {
                int firstAvailableIndex = _slots.FindIndex(s => s.IsAvailable);
                _timeSlotComboBox.SelectedIndex = firstAvailableIndex >= 0 ? firstAvailableIndex : 0;
            }
            if (_patientComboBox.Items.Count > 0) _patientComboBox.SelectedIndex = 0;
            _ = SearchPatientsAsync();
        }
    }
}