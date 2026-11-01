#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public sealed class DoctorStationItem
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string RoomStatus { get; set; } = "AVAILABLE";
        public string QueueLine { get; set; } = "0 waiting";
        public string CurrentPatient { get; set; } = string.Empty;
    }

    public partial class SecretaryOverviewForm : Form
    {
        private readonly SecretaryDashboardForm? _parent;
        private readonly AdminDashboardRepository _dashboardRepo = new();
        private readonly AppointmentRepository _appointmentRepo = new();
        private readonly DoctorScheduleRepository _scheduleRepo = new();

        public SecretaryOverviewForm(SecretaryDashboardForm? parent = null)
        {
            _parent = parent;
            InitializeComponent();

            // Double buffering for both grids
            EnableDoubleBuffer(_gridSchedule);
            EnableDoubleBuffer(_gridStations);

            ClinicTheme.ApplyForm(this);
            ConfigureGrids();
            WireEvents();
        }

        private static void EnableDoubleBuffer(DataGridView grid)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, grid, new object[] { true });
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await LoadDashboardDataAsync();
        }

        private void ConfigureGrids()
        {
            // -------------------------------------------------------------
            // 1. SCHEDULE & LOBBY QUEUE GRID
            // -------------------------------------------------------------
            _gridSchedule.AutoGenerateColumns = false;
            _gridSchedule.Columns.Clear();
            _gridSchedule.EnableHeadersVisualStyles = false;
            _gridSchedule.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridSchedule.ColumnHeadersHeight = 34;
            _gridSchedule.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _gridSchedule.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridSchedule.GridColor = Color.FromArgb(226, 232, 240);

            _gridSchedule.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            _gridSchedule.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridSchedule.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            _gridSchedule.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _gridSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            _gridSchedule.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _gridSchedule.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            _gridSchedule.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            // Ticket #
            _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.QueueNumber),
                HeaderText = "Ticket",
                Width = 65,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Scheduled Time
            _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.ScheduledTime),
                HeaderText = "Time",
                Width = 65,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) }
            });

            // Patient Full Name
            _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.PatientName),
                HeaderText = "Patient Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 140,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Patient #
            _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.PatientNumber),
                HeaderText = "Patient #",
                Width = 90,
                DefaultCellStyle = { ForeColor = Color.FromArgb(100, 116, 139) }
            });

            // Attending Physician
            _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.DoctorName),
                HeaderText = "Attending Physician",
                Width = 135
            });

            // Current Status
            _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.QueueStatus),
                HeaderText = "Status",
                Width = 95,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) }
            });

            // Chief Concern
            _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(QueueBoardItem.Reason),
                HeaderText = "Chief Concern",
                Width = 140
            });

            // Header painting for schedule grid
            AttachHeaderPainter(_gridSchedule);
            _gridSchedule.CellFormatting += GridSchedule_CellFormatting;

            // -------------------------------------------------------------
            // 2. DOCTOR STATIONS STATUS GRID
            // -------------------------------------------------------------
            _gridStations.AutoGenerateColumns = false;
            _gridStations.Columns.Clear();
            _gridStations.EnableHeadersVisualStyles = false;
            _gridStations.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridStations.ColumnHeadersHeight = 34;
            _gridStations.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _gridStations.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridStations.GridColor = Color.FromArgb(226, 232, 240);

            _gridStations.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            _gridStations.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridStations.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            _gridStations.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _gridStations.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            _gridStations.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _gridStations.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            _gridStations.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            _gridStations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorStationItem.DoctorName),
                HeaderText = "Physician",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 130,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _gridStations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorStationItem.Specialty),
                HeaderText = "Specialization",
                Width = 120
            });

            _gridStations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorStationItem.RoomStatus),
                HeaderText = "Station Status",
                Width = 115,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F, FontStyle.Bold) }
            });

            _gridStations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorStationItem.QueueLine),
                HeaderText = "Lobby Line",
                Width = 85,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F) }
            });

            // Header painting for stations grid
            AttachHeaderPainter(_gridStations);
            _gridStations.CellFormatting += GridStations_CellFormatting;
        }

        private static void AttachHeaderPainter(DataGridView grid)
        {
            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                {
                    e.Handled = true;

                    using var bgBrush = new SolidBrush(Color.FromArgb(15, 118, 110));
                    e.Graphics?.FillRectangle(bgBrush, e.CellBounds);

                    using var borderPen = new Pen(Color.FromArgb(13, 98, 91));
                    if (e.Graphics != null)
                    {
                        e.Graphics.DrawLine(borderPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(borderPen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);

                        string text = grid.Columns[e.ColumnIndex].HeaderText;
                        using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                        var align = grid.Columns[e.ColumnIndex].HeaderCell.Style.Alignment;

                        TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                        if (align == DataGridViewContentAlignment.MiddleCenter)
                        {
                            flags |= TextFormatFlags.HorizontalCenter;
                        }
                        else
                        {
                            flags |= TextFormatFlags.Left;
                        }

                        var textRect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y, e.CellBounds.Width - 12, e.CellBounds.Height);
                        TextRenderer.DrawText(e.Graphics, text, font, textRect, Color.White, flags);
                    }
                }
            };
        }

        private void GridSchedule_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Ticket display formatting
            if (_gridSchedule.Columns[e.ColumnIndex].DataPropertyName == nameof(QueueBoardItem.QueueNumber))
            {
                if (e.Value is int qNum)
                {
                    e.Value = qNum > 0 ? $"Q-{qNum:D2}" : "—";
                    e.FormattingApplied = true;
                }
            }

            // Status color formatting
            if (_gridSchedule.Columns[e.ColumnIndex].DataPropertyName == nameof(QueueBoardItem.QueueStatus))
            {
                string status = e.Value?.ToString() ?? string.Empty;
                switch (status)
                {
                    case "In Service":
                        e.CellStyle.BackColor = Color.FromArgb(236, 253, 245); // Mint Green
                        e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);
                        e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                        break;
                    case "Called":
                        e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // Warm Amber
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                        e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                        break;
                    case "Waiting":
                        e.CellStyle.BackColor = Color.FromArgb(239, 246, 255); // Soft Sky Blue
                        e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216);
                        e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                        break;
                    case "Completed":
                        e.CellStyle.BackColor = Color.FromArgb(241, 245, 249); // Muted Slate
                        e.CellStyle.ForeColor = Color.FromArgb(100, 116, 139);
                        e.CellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
                        break;
                    case "Displaced":
                        e.CellStyle.BackColor = Color.FromArgb(254, 242, 242); // Soft Red Alert
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                        e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                        break;
                    case "No-Show":
                    case "Cancelled":
                        e.CellStyle.BackColor = Color.FromArgb(254, 242, 242); // Soft Red
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        e.CellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
                        break;
                    default:
                        e.CellStyle.BackColor = Color.White;
                        e.CellStyle.ForeColor = Color.FromArgb(71, 85, 105);
                        break;
                }
            }
        }

        private void GridStations_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (_gridStations.Columns[e.ColumnIndex].DataPropertyName == nameof(DoctorStationItem.RoomStatus))
            {
                string status = e.Value?.ToString() ?? string.Empty;
                if (status.StartsWith("EMERGENCY", StringComparison.OrdinalIgnoreCase) || status.StartsWith("LEAVE", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
                    e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                }
                else if (status.StartsWith("IN SERVICE", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(236, 253, 245);
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);
                }
                else if (status.StartsWith("AWAITING", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                    e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                }
                else
                {
                    e.CellStyle.BackColor = Color.FromArgb(239, 246, 255);
                    e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216);
                }
            }
        }

        private void WireEvents()
        {
            _btnRefresh.Click += async (_, _) => await LoadDashboardDataAsync();

            _btnQuickPatient.Click += (_, _) => _parent?.NavigateToPatients();
            _btnQuickAppointment.Click += (_, _) => _parent?.NavigateToAppointments();
            _btnQuickBilling.Click += (_, _) => _parent?.NavigateToBilling();
            _btnQuickRefresh.Click += (_, _) => _parent?.NavigateToPatients();
        }

        public async Task LoadDashboardDataAsync()
        {
            Cursor = Cursors.WaitCursor;
            _btnRefresh.Enabled = false;

            try
            {
                var statsTask = Task.Run(() => _dashboardRepo.GetOverviewStats());
                var queueTask = Task.Run(() => _appointmentRepo.GetTodayQueue(null));
                var doctorsTask = Task.Run(() => _appointmentRepo.GetActiveDoctors());

                await Task.WhenAll(statsTask, queueTask, doctorsTask);

                var stats = await statsTask;
                var queue = await queueTask;
                var doctors = await doctorsTask;

                // 1. Calculate Secretary Specific Metrics
                int inServiceCount = queue.Count(q => q.QueueStatus.Equals("In Service", StringComparison.OrdinalIgnoreCase));
                int waitingCount = queue.Count(q => q.QueueStatus.Equals("Waiting", StringComparison.OrdinalIgnoreCase) ||
                                                    q.QueueStatus.Equals("Called", StringComparison.OrdinalIgnoreCase));

                _lblKpiBookingsVal.Text = stats.TodayAppointments.ToString("N0");
                _lblKpiWaitingVal.Text = waitingCount.ToString("N0");
                _lblKpiInServiceVal.Text = inServiceCount.ToString("N0");
                _lblKpiPatientsVal.Text = stats.TotalPatients.ToString("N0");

                // 2. Populate Schedule & Queue Grid
                _gridSchedule.DataSource = null;
                if (queue.Count > 0)
                {
                    _gridSchedule.DataSource = queue;
                    _gridSchedule.Visible = true;
                    _lblScheduleEmpty.Visible = false;
                    _lblScheduleBadge.Text = $"{queue.Count} Appointments Today";
                }
                else
                {
                    _gridSchedule.Visible = false;
                    _lblScheduleEmpty.Visible = true;
                    _lblScheduleBadge.Text = "0 Appointments";
                }

                // 3. Build & Populate Doctor Stations Status
                var stations = new List<DoctorStationItem>();
                foreach (var doc in doctors)
                {
                    var docQueue = queue.Where(q => q.DoctorId == doc.DoctorId).ToList();
                    var activeConsult = docQueue.FirstOrDefault(q => q.QueueStatus.Equals("In Service", StringComparison.OrdinalIgnoreCase));
                    int docWaiting = docQueue.Count(q => q.QueueStatus.Equals("Waiting", StringComparison.OrdinalIgnoreCase) ||
                                                         q.QueueStatus.Equals("Called", StringComparison.OrdinalIgnoreCase));

                    bool isDayOff = _scheduleRepo.IsDayOff(doc.DoctorId, DateTime.Today);
                    string status;
                    if (isDayOff)
                    {
                        status = "EMERGENCY LEAVE";
                    }
                    else if (activeConsult != null)
                    {
                        status = "IN SERVICE";
                    }
                    else if (docWaiting > 0)
                    {
                        status = "AWAITING DOCTOR";
                    }
                    else
                    {
                        status = "AVAILABLE";
                    }

                    stations.Add(new DoctorStationItem
                    {
                        DoctorId = doc.DoctorId,
                        DoctorName = doc.DoctorName,
                        Specialty = doc.Specialty,
                        RoomStatus = status,
                        QueueLine = isDayOff ? "On Leave" : (docWaiting > 0 ? $"{docWaiting} waiting" : "None"),
                        CurrentPatient = activeConsult?.PatientName ?? string.Empty
                    });
                }

                _gridStations.DataSource = null;
                if (stations.Count > 0)
                {
                    _gridStations.DataSource = stations;
                    _gridStations.Visible = true;
                    _lblStationsEmpty.Visible = false;
                    _lblStationsBadge.Text = $"{stations.Count} Physicians on Duty";
                }
                else
                {
                    _gridStations.Visible = false;
                    _lblStationsEmpty.Visible = true;
                    _lblStationsBadge.Text = "0 Physicians";
                }

                _lblLastUpdated.Text = $"Updated at {DateTime.Now:hh:mm:ss tt}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load overview data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnRefresh.Enabled = true;
                Cursor = Cursors.Default;
            }
        }
    }
}

