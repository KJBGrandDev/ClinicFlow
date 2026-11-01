#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public sealed class EmergencyTriageDialog : Form
    {
        private readonly AppointmentService _service = new();
        private readonly int? _filterDoctorId;
        private readonly DateTime _targetDate;

        private List<DisplacedAppointmentItem> _displacedList = new();
        private List<PeerDoctorAvailableSlotItem> _peerSlots = new();
        private List<DoctorNextAvailableSlotItem> _nextDoctorSlots = new();

        private DataGridView _gridDisplaced = null!;
        private Label _lblSelectedPatientBanner = null!;
        private ComboBox _cboPeerSlots = null!;
        private Button _btnTransferPeer = null!;
        private ComboBox _cboRescheduleSlots = null!;
        private Button _btnReschedule = null!;
        private ComboBox _cboContactStatus = null!;
        private Button _btnLogContact = null!;
        private Button _btnClose = null!;

        public bool AnyChangesMade { get; private set; } = false;

        public EmergencyTriageDialog(DateTime? targetDate = null, int? filterDoctorId = null)
        {
            _targetDate = targetDate ?? DateTime.Today;
            _filterDoctorId = filterDoctorId;
            InitializeComponent();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await LoadDisplacedPatientsAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "Front-Desk Emergency Triage & Resolution Center";
            this.Size = new Size(1060, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            // 1. Header Panel
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = Color.FromArgb(254, 242, 242),
                Padding = new Padding(24, 16, 24, 16)
            };
            pnlHeader.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(254, 202, 202), 1);
                e.Graphics.DrawLine(p, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            var lblHeaderTitle = new Label
            {
                Text = "🚨 Front-Desk Emergency Triage & Patient Resolution Center",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(153, 27, 27),
                AutoSize = true,
                Location = new Point(20, 14)
            };
            var lblHeaderSubtitle = new Label
            {
                Text = $"Active Date: {_targetDate:MMMM dd, yyyy} • Triage displaced patients to peer physicians, schedule fast-track reschedules, and record patient contact audits.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(185, 28, 28),
                AutoSize = true,
                Location = new Point(22, 46)
            };
            pnlHeader.Controls.AddRange(new Control[] { lblHeaderTitle, lblHeaderSubtitle });

            // 2. Bottom Close Panel
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawLine(p, 0, 0, pnlBottom.Width, 0);
            };

            _btnClose = new Button
            {
                Text = "Close Triage Center",
                Size = new Size(160, 36),
                Location = new Point(pnlBottom.Width - 180, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnClose.Click += (_, _) => { this.DialogResult = AnyChangesMade ? DialogResult.OK : DialogResult.Cancel; this.Close(); };
            pnlBottom.Controls.Add(_btnClose);

            // 3. Main Split Container / Layout
            var pnlCenter = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Top Half: Displaced Patients Grid
            var pnlGridContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 240,
                BackColor = Color.White,
                Padding = new Padding(1)
            };
            pnlGridContainer.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, pnlGridContainer.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            _gridDisplaced = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(241, 245, 249),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 36 }
            };
            ClinicTheme.ApplyGrid(_gridDisplaced);
            ConfigureGridColumns();

            pnlGridContainer.Controls.Add(_gridDisplaced);

            // Bottom Half: Resolution Action Card
            var pnlActionCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 16, 0, 0),
                Padding = new Padding(20)
            };
            pnlActionCard.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, pnlActionCard.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            _lblSelectedPatientBanner = new Label
            {
                Text = "Select a displaced patient from the table above to initiate triage resolution.",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 32
            };

            var pnlActionColumns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };
            pnlActionColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            pnlActionColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            pnlActionColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            // Column 1: Peer Doctor Transfer
            var grpPeer = CreateActionGroup("Option 1: Transfer to Peer Doctor Today", "Reassign to an active physician with open slots today:");
            _cboPeerSlots = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, Height = 28 };
            _btnTransferPeer = new Button
            {
                Text = "⚡ Transfer to Peer Doctor",
                Dock = DockStyle.Bottom,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnTransferPeer.FlatAppearance.BorderSize = 0;
            _btnTransferPeer.Click += async (_, _) => await HandleTransferPeerAsync();
            grpPeer.Controls.AddRange(new Control[] { _btnTransferPeer, _cboPeerSlots });

            // Column 2: Fast-Track Reschedule
            var grpResched = CreateActionGroup("Option 2: Fast-Track Reschedule", "Move patient to attending physician's earliest open future slot:");
            _cboRescheduleSlots = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, Height = 28 };
            _btnReschedule = new Button
            {
                Text = "📅 Fast-Track Reschedule",
                Dock = DockStyle.Bottom,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(15, 118, 110),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnReschedule.FlatAppearance.BorderSize = 0;
            _btnReschedule.Click += async (_, _) => await HandleRescheduleAsync();
            grpResched.Controls.AddRange(new Control[] { _btnReschedule, _cboRescheduleSlots });

            // Column 3: Patient Outreach Logger
            var grpOutreach = CreateActionGroup("Option 3: Log Patient Outreach", "Update patient phone notification / outreach call status:");
            _cboContactStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top, Height = 28 };
            _cboContactStatus.Items.AddRange(new object[]
            {
                "Contacted - Transferred",
                "Contacted - Rescheduled",
                "Contacted - Cancelled / Declined",
                "Attempted - Left Voicemail / SMS Sent",
                "Unreachable - Phone Unanswered",
                "Pending Outreach"
            });
            _cboContactStatus.SelectedIndex = 0;
            _btnLogContact = new Button
            {
                Text = "📞 Log Outreach Status",
                Dock = DockStyle.Bottom,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnLogContact.FlatAppearance.BorderSize = 0;
            _btnLogContact.Click += async (_, _) => await HandleLogContactAsync();
            grpOutreach.Controls.AddRange(new Control[] { _btnLogContact, _cboContactStatus });

            pnlActionColumns.Controls.Add(grpPeer, 0, 0);
            pnlActionColumns.Controls.Add(grpResched, 1, 0);
            pnlActionColumns.Controls.Add(grpOutreach, 2, 0);

            pnlActionCard.Controls.Add(pnlActionColumns);
            pnlActionCard.Controls.Add(_lblSelectedPatientBanner);

            // Spacer between grid and action card
            var spacer = new Panel { Dock = DockStyle.Top, Height = 12 };

            pnlCenter.Controls.Add(pnlActionCard);
            pnlCenter.Controls.Add(spacer);
            pnlCenter.Controls.Add(pnlGridContainer);

            this.Controls.Add(pnlCenter);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);

            _gridDisplaced.SelectionChanged += async (_, _) => await OnPatientSelectionChangedAsync();
        }

        private static Panel CreateActionGroup(string title, string hint)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(12),
                Margin = new Padding(6)
            };
            pnl.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);

            var lblT = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Dock = DockStyle.Top,
                Height = 22
            };
            var lblH = new Label
            {
                Text = hint,
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Top,
                Height = 36
            };
            pnl.Controls.Add(lblH);
            pnl.Controls.Add(lblT);
            return pnl;
        }

        private void ConfigureGridColumns()
        {
            _gridDisplaced.AutoGenerateColumns = false;
            _gridDisplaced.Columns.Clear();

            _gridDisplaced.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplacedAppointmentItem.AppointmentNumber),
                HeaderText = "Appt #",
                Width = 140,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });
            _gridDisplaced.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplacedAppointmentItem.PatientName),
                HeaderText = "Displaced Patient",
                Width = 180,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });
            _gridDisplaced.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplacedAppointmentItem.ContactNumber),
                HeaderText = "Contact #",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            _gridDisplaced.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplacedAppointmentItem.DoctorName),
                HeaderText = "Attending Physician",
                Width = 160
            });
            _gridDisplaced.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplacedAppointmentItem.ScheduledTime),
                HeaderText = "Displaced Slot",
                Width = 130,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            _gridDisplaced.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplacedAppointmentItem.DisplacementReason),
                HeaderText = "Emergency Reason",
                Width = 180
            });
            _gridDisplaced.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplacedAppointmentItem.ContactStatus),
                HeaderText = "Outreach Status",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _gridDisplaced.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _displacedList.Count) return;
                var item = _displacedList[e.RowIndex];

                if (e.ColumnIndex == 6) // Outreach Status
                {
                    if (item.ContactStatus.Contains("Transferred") || item.ContactStatus.Contains("Rescheduled"))
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52);
                        e.CellStyle.BackColor = Color.FromArgb(240, 253, 244);
                    }
                    else if (item.ContactStatus.Contains("Pending"))
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                        e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                    }
                }
            };
        }

        private async Task LoadDisplacedPatientsAsync()
        {
            var res = await Task.Run(() => _service.GetDisplacedAppointments(_targetDate, _filterDoctorId));
            _displacedList = res.Items ?? new List<DisplacedAppointmentItem>();

            _gridDisplaced.DataSource = null;
            _gridDisplaced.DataSource = _displacedList;

            if (_displacedList.Count > 0)
            {
                _gridDisplaced.Rows[0].Selected = true;
            }
            else
            {
                _lblSelectedPatientBanner.Text = "🎉 Excellent! No displaced patient appointments currently pending resolution.";
                _cboPeerSlots.Items.Clear();
                _cboRescheduleSlots.Items.Clear();
                _btnTransferPeer.Enabled = false;
                _btnReschedule.Enabled = false;
                _btnLogContact.Enabled = false;
            }
        }

        private DisplacedAppointmentItem? GetSelectedPatient()
        {
            if (_gridDisplaced.CurrentRow?.DataBoundItem is DisplacedAppointmentItem item)
                return item;
            return null;
        }

        private async Task OnPatientSelectionChangedAsync()
        {
            var patient = GetSelectedPatient();
            if (patient == null)
            {
                _btnTransferPeer.Enabled = false;
                _btnReschedule.Enabled = false;
                _btnLogContact.Enabled = false;
                return;
            }

            _lblSelectedPatientBanner.Text = $"Selected: {patient.PatientName} ({patient.PatientNumber}) • Displaced from {patient.DoctorName} at {patient.ScheduledTime} [{patient.DisplacementReason}]";
            _btnTransferPeer.Enabled = true;
            _btnReschedule.Enabled = true;
            _btnLogContact.Enabled = true;

            // Load Peer Slots
            var peerRes = await Task.Run(() => _service.GetAvailablePeerDoctorSlots(patient.DoctorId, _targetDate));
            _peerSlots = peerRes.Slots ?? new List<PeerDoctorAvailableSlotItem>();
            _cboPeerSlots.Items.Clear();
            foreach (var slot in _peerSlots)
            {
                _cboPeerSlots.Items.Add(slot.TimeDisplay);
            }
            if (_cboPeerSlots.Items.Count > 0)
            {
                _cboPeerSlots.SelectedIndex = 0;
                _btnTransferPeer.Enabled = true;
            }
            else
            {
                _cboPeerSlots.Items.Add("No peer doctors with open slots today");
                _cboPeerSlots.SelectedIndex = 0;
                _btnTransferPeer.Enabled = false;
            }

            // Load Next Slots for Attending Doctor
            var nextRes = await Task.Run(() => _service.GetDoctorNextAvailableSlots(patient.DoctorId, _targetDate));
            _nextDoctorSlots = nextRes.Slots ?? new List<DoctorNextAvailableSlotItem>();
            _cboRescheduleSlots.Items.Clear();
            foreach (var slot in _nextDoctorSlots)
            {
                _cboRescheduleSlots.Items.Add(slot.TimeDisplay);
            }
            if (_cboRescheduleSlots.Items.Count > 0)
            {
                _cboRescheduleSlots.SelectedIndex = 0;
                _btnReschedule.Enabled = true;
            }
            else
            {
                _cboRescheduleSlots.Items.Add("No upcoming open slots found");
                _cboRescheduleSlots.SelectedIndex = 0;
                _btnReschedule.Enabled = false;
            }
        }

        private async Task HandleTransferPeerAsync()
        {
            var patient = GetSelectedPatient();
            if (patient == null) return;

            int slotIdx = _cboPeerSlots.SelectedIndex;
            if (slotIdx < 0 || slotIdx >= _peerSlots.Count)
            {
                MessageBox.Show("Please select an available peer doctor slot.", "Select Slot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var slot = _peerSlots[slotIdx];
            var confirm = MessageBox.Show(
                $"Confirm transfer of patient {patient.PatientName} to {slot.DoctorName} ({slot.Specialty}) at {slot.StartTime:hh\\:mm}?",
                "Confirm Peer Transfer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var res = await Task.Run(() => _service.TransferToPeerDoctor(
                patient.AppointmentId,
                slot.DoctorId,
                slot.StartTime,
                slot.EndTime,
                $"Transferred from {patient.DoctorName} due to {patient.DisplacementReason}",
                1));

            if (res.Success)
            {
                AnyChangesMade = true;
                MessageBox.Show(res.Message, "Transfer Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDisplacedPatientsAsync();
            }
            else
            {
                MessageBox.Show(res.Message, "Transfer Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HandleRescheduleAsync()
        {
            var patient = GetSelectedPatient();
            if (patient == null) return;

            int slotIdx = _cboRescheduleSlots.SelectedIndex;
            if (slotIdx < 0 || slotIdx >= _nextDoctorSlots.Count)
            {
                MessageBox.Show("Please select an upcoming slot.", "Select Slot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var slot = _nextDoctorSlots[slotIdx];
            var confirm = MessageBox.Show(
                $"Confirm fast-track reschedule of patient {patient.PatientName} with {patient.DoctorName} to {slot.Date:MMM dd, yyyy} at {slot.StartTime:hh\\:mm}?",
                "Confirm Reschedule",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var res = await Task.Run(() => _service.RescheduleAppointment(
                patient.AppointmentId,
                slot.Date,
                slot.StartTime,
                slot.EndTime,
                $"Fast-track reschedule from {patient.AppointmentDate:yyyy-MM-dd} due to {patient.DisplacementReason}",
                1));

            if (res.Success)
            {
                AnyChangesMade = true;
                MessageBox.Show(res.Message, "Reschedule Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDisplacedPatientsAsync();
            }
            else
            {
                MessageBox.Show(res.Message, "Reschedule Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HandleLogContactAsync()
        {
            var patient = GetSelectedPatient();
            if (patient == null) return;

            string status = _cboContactStatus.SelectedItem?.ToString() ?? "Pending Outreach";
            var res = await Task.Run(() => _service.UpdateContactStatus(patient.AppointmentId, status, 1));

            if (res.Success)
            {
                AnyChangesMade = true;
                MessageBox.Show($"Outreach status for {patient.PatientName} set to: '{status}'.", "Contact Logged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDisplacedPatientsAsync();
            }
            else
            {
                MessageBox.Show(res.Message, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
