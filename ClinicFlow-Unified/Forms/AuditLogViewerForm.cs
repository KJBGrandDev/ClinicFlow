#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class AuditLogViewerForm : Form
{
    private readonly AuditLogRepository _repository;
    private List<AuditLogViewItem> _logs = [];

    public AuditLogViewerForm()
    {
        // Enforce RBAC guard: Admin-only access
        UserSession.RequireRole("Admin");

        var connFactory = new SqlConnectionFactory();
        _repository = new AuditLogRepository(connFactory);

        InitializeComponent();
        ClinicTheme.ApplyForm(this);
        ClinicTheme.ApplyGrid(_gridAuditLogs);
        ApplyThemeHierarchy();
        ConfigureGridColumns();
        WireEvents();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _dtpFromDate.Value = DateTime.Today.AddDays(-7);
        _dtpToDate.Value = DateTime.Today;

        await LoadActionTypesAsync();
        await LoadLogsAsync();
    }

    private void ApplyThemeHierarchy()
    {
        _btnFilter.FlatStyle = FlatStyle.Flat;
        _btnFilter.FlatAppearance.BorderSize = 0;
        _btnFilter.BackColor = Color.FromArgb(37, 99, 235);
        _btnFilter.ForeColor = Color.White;
        _btnFilter.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        _btnReset.FlatStyle = FlatStyle.Flat;
        _btnReset.FlatAppearance.BorderSize = 1;
        _btnReset.BackColor = Color.White;
        _btnReset.ForeColor = Color.FromArgb(71, 85, 105);
        _btnReset.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

        _btnExportCsv.FlatStyle = FlatStyle.Flat;
        _btnExportCsv.FlatAppearance.BorderSize = 1;
        _btnExportCsv.BackColor = Color.FromArgb(240, 253, 244);
        _btnExportCsv.ForeColor = Color.FromArgb(22, 101, 52);
        _btnExportCsv.FlatAppearance.BorderColor = Color.FromArgb(187, 247, 208);
        _btnExportCsv.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
    }

    private void ConfigureGridColumns()
    {
        _gridAuditLogs.AutoGenerateColumns = false;
        _gridAuditLogs.Columns.Clear();
        _gridAuditLogs.RowTemplate.Height = 32;

        _gridAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AuditLogViewItem.LoggedAt),
            HeaderText = "Date & Time",
            Width = 145,
            DefaultCellStyle = { Format = "yyyy-MM-dd HH:mm:ss" }
        });

        _gridAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AuditLogViewItem.ActionType),
            HeaderText = "Action",
            Width = 120,
            DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
        });

        _gridAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AuditLogViewItem.EntityName),
            HeaderText = "Entity",
            Width = 110
        });

        _gridAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AuditLogViewItem.Username),
            HeaderText = "User",
            Width = 100
        });

        _gridAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AuditLogViewItem.RecordId),
            HeaderText = "Record ID",
            Width = 85,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        _gridAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AuditLogViewItem.Description),
            HeaderText = "Audit Description",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 200
        });

        _gridAuditLogs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(AuditLogViewItem.MachineName),
            HeaderText = "Machine",
            Width = 110
        });

        _gridAuditLogs.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _logs.Count) return;

            if (e.ColumnIndex == 1) // ActionType column color accent
            {
                string action = e.Value?.ToString() ?? "";
                if (action.Contains("DELETE", StringComparison.OrdinalIgnoreCase) || action.Contains("LOCKOUT", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                }
                else if (action.Contains("LOGIN", StringComparison.OrdinalIgnoreCase) || action.Contains("RESTORE", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52);
                }
                else if (action.Contains("LOGOUT", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(100, 116, 139);
                }
            }
        };
    }

    private void WireEvents()
    {
        _btnFilter.Click += async (_, _) => await LoadLogsAsync();
        _btnReset.Click += async (_, _) =>
        {
            _txtSearch.Clear();
            _cboActionType.SelectedIndex = 0;
            _dtpFromDate.Value = DateTime.Today.AddDays(-7);
            _dtpToDate.Value = DateTime.Today;
            await LoadLogsAsync();
        };

        _txtSearch.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoadLogsAsync();
            }
        };

        _btnExportCsv.Click += (_, _) => ExportCsv();
    }

    private async Task LoadActionTypesAsync()
    {
        try
        {
            var actions = await _repository.GetDistinctActionTypesAsync();
            var list = new List<string> { "All Actions" };
            list.AddRange(actions);

            _cboActionType.DataSource = list;
            _cboActionType.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "AuditLogViewerForm.LoadActionTypesAsync");
        }
    }

    private async Task LoadLogsAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            string? action = _cboActionType.SelectedItem?.ToString();
            if (action == "All Actions") action = null;

            string? search = string.IsNullOrWhiteSpace(_txtSearch.Text) ? null : _txtSearch.Text.Trim();
            DateTime from = _dtpFromDate.Value.Date;
            DateTime to = _dtpToDate.Value.Date.AddDays(1).AddTicks(-1);

            _logs = await _repository.SearchLogsAsync(action, from, to, search, 500);

            _gridAuditLogs.DataSource = null;
            _gridAuditLogs.DataSource = _logs;
            _lblStatusCount.Text = $"Showing {_logs.Count} audit records for date range {from:yyyy-MM-dd} to {_dtpToDate.Value:yyyy-MM-dd}.";
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "AuditLogViewerForm.LoadLogsAsync");
            MessageBox.Show(msg, "Error Loading Audit Logs", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void ExportCsv()
    {
        if (_logs.Count == 0)
        {
            MessageBox.Show("No audit logs available to export.", "Export Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "CSV Spreadsheet (*.csv)|*.csv",
            FileName = $"AuditLogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (sfd.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("LogId,Timestamp,Action,Entity,User,RecordId,Description,Machine");

            foreach (var log in _logs)
            {
                string desc = log.Description.Replace("\"", "\"\"");
                sb.AppendLine($"{log.AuditLogId},\"{log.LoggedAt:yyyy-MM-dd HH:mm:ss}\",\"{log.ActionType}\",\"{log.EntityName}\",\"{log.Username}\",\"{log.RecordId}\",\"{desc}\",\"{log.MachineName}\"");
            }

            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Exported {_logs.Count} audit records successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "AuditLogViewerForm.ExportCsv");
            MessageBox.Show(msg, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
