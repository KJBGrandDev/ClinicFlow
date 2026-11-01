#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class UserManagementForm : Form
{
    private readonly UserManagementService _userService;
    private List<User> _allUsers = [];
    private List<Role> _roles = [];
    private User? _selectedUser;
    private bool _isClearing;

    public UserManagementForm()
    {
        // Enforce RBAC guard: Admin-only access
        UserSession.RequireRole("Admin");

        var connFactory = new SqlConnectionFactory();
        var userRepo = new UserRepository(connFactory);
        var roleRepo = new RoleRepository(connFactory);
        var auditRepo = new AuditLogRepository(connFactory);
        var auditService = new AuditService(auditRepo);

        _userService = new UserManagementService(userRepo, roleRepo, auditService);

        InitializeComponent();
        ClinicTheme.ApplyForm(this);
        ClinicTheme.ApplyGrid(_gridUsers);
        ApplyThemeHierarchy();
        ConfigureGridColumns();
        WireEvents();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await LoadRolesAsync();
        await LoadUsersAsync();
    }

    private void ApplyThemeHierarchy()
    {
        Color emeraldCta = Color.FromArgb(16, 185, 129);
        Color amberUpdate = Color.FromArgb(245, 158, 11);

        _btnCreateUser.FlatStyle = FlatStyle.Flat;
        _btnCreateUser.FlatAppearance.BorderSize = 0;
        _btnCreateUser.BackColor = emeraldCta;
        _btnCreateUser.ForeColor = Color.White;
        _btnCreateUser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        _btnUpdateUser.FlatStyle = FlatStyle.Flat;
        _btnUpdateUser.FlatAppearance.BorderSize = 0;
        _btnUpdateUser.BackColor = amberUpdate;
        _btnUpdateUser.ForeColor = Color.White;
        _btnUpdateUser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        _btnToggleStatus.FlatStyle = FlatStyle.Flat;
        _btnToggleStatus.FlatAppearance.BorderSize = 1;
        _btnToggleStatus.BackColor = Color.FromArgb(254, 242, 242);
        _btnToggleStatus.ForeColor = Color.FromArgb(220, 38, 38);
        _btnToggleStatus.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
        _btnToggleStatus.Font = new Font("Segoe UI", 8.5F);

        _btnResetPassword.FlatStyle = FlatStyle.Flat;
        _btnResetPassword.FlatAppearance.BorderSize = 1;
        _btnResetPassword.BackColor = Color.FromArgb(239, 246, 255);
        _btnResetPassword.ForeColor = Color.FromArgb(37, 99, 235);
        _btnResetPassword.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
        _btnResetPassword.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        _btnClear.FlatStyle = FlatStyle.Flat;
        _btnClear.FlatAppearance.BorderSize = 1;
        _btnClear.BackColor = Color.White;
        _btnClear.ForeColor = Color.FromArgb(71, 85, 105);
        _btnClear.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
    }

    private void ConfigureGridColumns()
    {
        _gridUsers.AutoGenerateColumns = false;
        _gridUsers.Columns.Clear();
        _gridUsers.RowTemplate.Height = 32;

        _gridUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(User.UserId),
            HeaderText = "ID",
            Width = 50,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        _gridUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(User.Username),
            HeaderText = "Username",
            Width = 110,
            DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
        });

        _gridUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(User.FullName),
            HeaderText = "Full Name",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 140
        });

        _gridUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(User.RoleName),
            HeaderText = "Role",
            Width = 95,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        _gridUsers.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "StatusText",
            HeaderText = "Status",
            Width = 90,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });

        _gridUsers.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.RowIndex >= _gridUsers.Rows.Count) return;

            if (_gridUsers.Rows[e.RowIndex].DataBoundItem is User u)
            {
                if (e.ColumnIndex == 4) // Status column
                {
                    e.Value = u.IsActive ? "Active" : "Deactivated";
                    e.CellStyle.ForeColor = u.IsActive ? Color.FromArgb(22, 101, 52) : Color.FromArgb(185, 28, 28);
                    e.CellStyle.BackColor = u.IsActive ? Color.FromArgb(240, 253, 244) : Color.FromArgb(254, 242, 242);
                    e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    e.FormattingApplied = true;
                }
            }
        };
    }

    private void WireEvents()
    {
        _txtSearch.TextChanged += (_, _) => FilterAndBindGrid();
        _cboRoleFilter.SelectedIndexChanged += (_, _) => FilterAndBindGrid();
        _chkShowInactive.CheckedChanged += (_, _) => FilterAndBindGrid();

        _gridUsers.SelectionChanged += GridUsers_SelectionChanged;

        _btnCreateUser.Click += async (_, _) => await CreateUserAsync();
        _btnUpdateUser.Click += async (_, _) => await UpdateUserAsync();
        _btnToggleStatus.Click += async (_, _) => await ToggleUserStatusAsync();
        _btnResetPassword.Click += async (_, _) => await ResetPasswordAsync();
        _btnClear.Click += (_, _) => ClearInputs();
    }

    private async Task LoadRolesAsync()
    {
        try
        {
            _roles = await _userService.GetActiveRolesAsync();

            _cboRole.DataSource = null;
            _cboRole.DisplayMember = nameof(Role.RoleName);
            _cboRole.ValueMember = nameof(Role.RoleId);
            _cboRole.DataSource = _roles.ToList();

            var filterRoles = new List<string> { "All Roles" };
            filterRoles.AddRange(_roles.Select(r => r.RoleName));
            _cboRoleFilter.DataSource = filterRoles;
            _cboRoleFilter.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "UserManagementForm.LoadRolesAsync");
        }
    }

    private async Task LoadUsersAsync()
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            _allUsers = await _userService.GetAllUsersAsync();
            FilterAndBindGrid();
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "UserManagementForm.LoadUsersAsync");
            MessageBox.Show(msg, "Error Loading Users", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void FilterAndBindGrid()
    {
        string search = _txtSearch.Text.Trim();
        string selectedRole = _cboRoleFilter.SelectedItem?.ToString() ?? "All Roles";
        bool showInactive = _chkShowInactive.Checked;

        var filtered = _allUsers.Where(u =>
        {
            if (!showInactive && !u.IsActive) return false;
            if (selectedRole != "All Roles" && !string.Equals(u.RoleName, selectedRole, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrWhiteSpace(search))
            {
                bool matchesName = u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase);
                bool matchesUser = u.Username.Contains(search, StringComparison.OrdinalIgnoreCase);
                if (!matchesName && !matchesUser) return false;
            }
            return true;
        }).ToList();

        _gridUsers.DataSource = null;
        _gridUsers.DataSource = filtered;
        _lblUserCount.Text = $"Showing {filtered.Count} of {_allUsers.Count} user accounts";
        _gridUsers.ClearSelection();
    }

    private void GridUsers_SelectionChanged(object? sender, EventArgs e)
    {
        if (_isClearing || _gridUsers.SelectedRows.Count == 0) return;

        if (_gridUsers.SelectedRows[0].DataBoundItem is User u)
        {
            _selectedUser = u;

            _txtUsername.Text = u.Username;
            _txtUsername.ReadOnly = true;
            _txtUsername.BackColor = Color.FromArgb(241, 245, 249);

            _txtFullName.Text = u.FullName;
            _txtPassword.Clear();
            _txtPassword.Enabled = false;
            _txtPassword.PlaceholderText = "Use 'Reset Password' to change";

            var roleItem = _roles.FirstOrDefault(r => r.RoleId == u.RoleId);
            if (roleItem != null)
            {
                _cboRole.SelectedItem = roleItem;
            }

            _btnCreateUser.Enabled = false;
            _btnUpdateUser.Enabled = true;
            _btnResetPassword.Enabled = true;
            _btnToggleStatus.Enabled = true;

            if (u.IsActive)
            {
                _btnToggleStatus.Text = "🚫 Deactivate";
                _btnToggleStatus.BackColor = Color.FromArgb(254, 242, 242);
                _btnToggleStatus.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else
            {
                _btnToggleStatus.Text = "✅ Reactivate";
                _btnToggleStatus.BackColor = Color.FromArgb(240, 253, 244);
                _btnToggleStatus.ForeColor = Color.FromArgb(22, 101, 52);
            }
        }
    }

    private async Task CreateUserAsync()
    {
        string username = _txtUsername.Text.Trim();
        string fullName = _txtFullName.Text.Trim();
        string password = _txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username))
        {
            MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Full Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtFullName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtPassword.Focus();
            return;
        }

        if (_cboRole.SelectedValue is not int roleId)
        {
            MessageBox.Show("Please select an assigned role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Cursor = Cursors.WaitCursor;
        try
        {
            int newUserId = await _userService.CreateUserAsync(username, password, fullName, roleId);
            MessageBox.Show($"User account '{username}' has been successfully created with ID #{newUserId}.", "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputs();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "UserManagementForm.CreateUserAsync");
            MessageBox.Show(msg, "Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async Task UpdateUserAsync()
    {
        if (_selectedUser == null) return;

        string fullName = _txtFullName.Text.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Full Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtFullName.Focus();
            return;
        }

        if (_cboRole.SelectedValue is not int roleId)
        {
            MessageBox.Show("Please select an assigned role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Are you sure you want to update user profile for '{_selectedUser.Username}'?",
            "Confirm Update",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        Cursor = Cursors.WaitCursor;
        try
        {
            await _userService.UpdateUserDetailsAsync(_selectedUser.UserId, fullName, roleId);
            MessageBox.Show($"User '{_selectedUser.Username}' has been updated successfully.", "User Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputs();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "UserManagementForm.UpdateUserAsync");
            MessageBox.Show(msg, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async Task ToggleUserStatusAsync()
    {
        if (_selectedUser == null) return;

        bool newStatus = !_selectedUser.IsActive;
        string actionVerb = newStatus ? "reactivate" : "deactivate";

        var confirm = MessageBox.Show(
            $"Are you sure you want to {actionVerb} account for '{_selectedUser.Username}' ({_selectedUser.FullName})?",
            "Confirm Account Status Change",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        Cursor = Cursors.WaitCursor;
        try
        {
            await _userService.SetUserActiveStatusAsync(_selectedUser.UserId, newStatus);
            MessageBox.Show($"Account '{_selectedUser.Username}' is now {(newStatus ? "Active" : "Deactivated")}.", "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputs();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "UserManagementForm.ToggleUserStatusAsync");
            MessageBox.Show(msg, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private async Task ResetPasswordAsync()
    {
        if (_selectedUser == null) return;

        using var prompt = new PasswordResetDialog(_selectedUser.Username);
        if (prompt.ShowDialog(this) != DialogResult.OK) return;

        string newPassword = prompt.NewPassword;
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Cursor = Cursors.WaitCursor;
        try
        {
            await _userService.ResetPasswordAsync(_selectedUser.UserId, newPassword);
            MessageBox.Show($"Password for '{_selectedUser.Username}' has been reset successfully.", "Password Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputs();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "UserManagementForm.ResetPasswordAsync");
            MessageBox.Show(msg, "Reset Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void ClearInputs()
    {
        _isClearing = true;
        try
        {
            _selectedUser = null;
            _txtUsername.Clear();
            _txtUsername.ReadOnly = false;
            _txtUsername.BackColor = Color.White;

            _txtFullName.Clear();
            _txtPassword.Clear();
            _txtPassword.Enabled = true;
            _txtPassword.PlaceholderText = string.Empty;

            if (_roles.Count > 0)
                _cboRole.SelectedIndex = 0;

            _btnCreateUser.Enabled = true;
            _btnUpdateUser.Enabled = false;
            _btnResetPassword.Enabled = false;
            _btnToggleStatus.Enabled = false;
            _btnToggleStatus.Text = "🚫 Deactivate";

            _gridUsers.ClearSelection();
        }
        finally
        {
            _isClearing = false;
        }
    }
}
