#nullable enable

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class AdminOverviewForm : Form
{
    private readonly MainDashboardForm? _parent;
    private readonly AdminDashboardRepository _dashboardRepo = new();
    private readonly AppointmentRepository _appointmentRepo = new();

    public AdminOverviewForm(MainDashboardForm? parent = null)
    {
        _parent = parent;
        InitializeComponent();
        ClinicTheme.ApplyForm(this);
        ConfigureGrids();
        WireEvents();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await LoadDashboardDataAsync();
    }

    private void ConfigureGrids()
    {
        Color headerBg = Color.FromArgb(241, 245, 249);
        Color headerFg = Color.FromArgb(71, 85, 105);

        // --- 1. Schedule Grid ---
        _gridSchedule.AutoGenerateColumns = false;
        _gridSchedule.Columns.Clear();
        _gridSchedule.EnableHeadersVisualStyles = false;
        _gridSchedule.ColumnHeadersDefaultCellStyle.BackColor = headerBg;
        _gridSchedule.ColumnHeadersDefaultCellStyle.ForeColor = headerFg;
        _gridSchedule.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBg;
        _gridSchedule.ColumnHeadersDefaultCellStyle.SelectionForeColor = headerFg;
        _gridSchedule.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        _gridSchedule.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        _gridSchedule.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
        _gridSchedule.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

        _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(QueueBoardItem.ScheduledTime),
            HeaderText = "Time",
            Width = 65,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) }
        });

        _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(QueueBoardItem.PatientName),
            HeaderText = "Patient Name",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
        });

        _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(QueueBoardItem.PatientNumber),
            HeaderText = "Patient #",
            Width = 95,
            DefaultCellStyle = { ForeColor = Color.FromArgb(100, 116, 139) }
        });

        _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(QueueBoardItem.DoctorName),
            HeaderText = "Doctor",
            Width = 130
        });

        _gridSchedule.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(QueueBoardItem.QueueStatus),
            HeaderText = "Status",
            Width = 95,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        _gridSchedule.CellFormatting += GridSchedule_CellFormatting;

        // --- 2. Audit Grid ---
        _gridAudit.AutoGenerateColumns = false;
        _gridAudit.Columns.Clear();
        _gridAudit.EnableHeadersVisualStyles = false;
        _gridAudit.ColumnHeadersDefaultCellStyle.BackColor = headerBg;
        _gridAudit.ColumnHeadersDefaultCellStyle.ForeColor = headerFg;
        _gridAudit.ColumnHeadersDefaultCellStyle.SelectionBackColor = headerBg;
        _gridAudit.ColumnHeadersDefaultCellStyle.SelectionForeColor = headerFg;
        _gridAudit.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        _gridAudit.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        _gridAudit.DefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
        _gridAudit.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

        _gridAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AdminAuditItem.TimeFormatted),
            HeaderText = "Time",
            Width = 65,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        _gridAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AdminAuditItem.PerformedBy),
            HeaderText = "User",
            Width = 145,
            DefaultCellStyle = { Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) }
        });

        _gridAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AdminAuditItem.ActionType),
            HeaderText = "Action",
            Width = 135
        });

        _gridAudit.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AdminAuditItem.Description),
            HeaderText = "Activity Detail",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    private void GridSchedule_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        if (_gridSchedule.Columns[e.ColumnIndex].DataPropertyName == nameof(QueueBoardItem.QueueStatus))
        {
            string status = e.Value?.ToString() ?? string.Empty;
            switch (status)
            {
                case "Waiting":
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199); // Amber 100
                    e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);   // Amber 700
                    e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    break;
                case "Called":
                case "In Service":
                    e.CellStyle.BackColor = Color.FromArgb(239, 246, 255); // Blue 100
                    e.CellStyle.ForeColor = Color.FromArgb(29, 78, 216);  // Blue 700
                    e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    break;
                case "Completed":
                case "Consulted":
                    e.CellStyle.BackColor = Color.FromArgb(236, 253, 245); // Emerald 100
                    e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);    // Emerald 700
                    e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    break;
                default:
                    e.CellStyle.BackColor = Color.FromArgb(241, 245, 249); // Slate 100
                    e.CellStyle.ForeColor = Color.FromArgb(71, 85, 105);   // Slate 600
                    break;
            }
        }
    }

    private void WireEvents()
    {
        _btnRefresh.Click += async (_, _) => await LoadDashboardDataAsync();

        _btnQuickPatient.Click += (_, _) => _parent?.NavigateToPatients();
        _btnQuickAppointment.Click += (_, _) => _parent?.NavigateToAppointments();
        _btnQuickDoctor.Click += (_, _) => _parent?.NavigateToDoctors();
    }

    public async Task LoadDashboardDataAsync()
    {
        Cursor = Cursors.WaitCursor;
        _btnRefresh.Enabled = false;

        try
        {
            var statsTask = Task.Run(() => _dashboardRepo.GetOverviewStats());
            var scheduleTask = Task.Run(() => _appointmentRepo.GetTodayQueue(null));
            var auditTask = Task.Run(() => _dashboardRepo.GetRecentAuditLogs(15));

            await Task.WhenAll(statsTask, scheduleTask, auditTask);

            var stats = await statsTask;
            var schedule = await scheduleTask;
            var audit = await auditTask;

            // 1. KPI Cards
            _lblKpiPatientsVal.Text = stats.TotalPatients.ToString("N0");
            _lblKpiApptsVal.Text = stats.TodayAppointments.ToString("N0");
            _lblKpiDoctorsVal.Text = stats.ActiveDoctors.ToString("N0");
            _lblKpiQueueVal.Text = stats.WaitingQueueCount.ToString("N0");

            // 2. Schedule Grid & Empty State
            _gridSchedule.DataSource = schedule;
            bool hasSchedule = schedule.Count > 0;
            _gridSchedule.Visible = hasSchedule;
            _lblScheduleEmpty.Visible = !hasSchedule;
            _lblScheduleCountBadge.Text = $"{schedule.Count} scheduled";

            // 3. Audit Grid & Empty State
            _gridAudit.DataSource = audit;
            bool hasAudit = audit.Count > 0;
            _gridAudit.Visible = hasAudit;
            _lblAuditEmpty.Visible = !hasAudit;

            _lblLastUpdated.Text = $"Updated: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to refresh dashboard data: {ex.Message}", "ClinicFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _btnRefresh.Enabled = true;
            Cursor = Cursors.Default;
        }
    }
}
