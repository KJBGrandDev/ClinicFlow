#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public partial class PharmacistManagementForm : Form
    {
        private readonly PharmacyRepository _repo = new();
        private List<PharmacistRecord> _pharmacists = new();
        private PharmacistRecord? _selectedPharmacist = null;

        public PharmacistManagementForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                UserSession.RequireRole("Admin");
            }

            InitializeComponent();

            // Layout Z-Order setup
            this._headerPanel.SendToBack();
            this._rightPanel.SendToBack();
            this._leftGridPanel.BringToFront();
            this._rosterHeaderPanel.SendToBack();
            this._gridPharmacists.BringToFront();

            // Double buffering for smooth grid rendering
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, this._gridPharmacists, new object[] { true });

            // Apply Theme
            ClinicTheme.ApplyForm(this);
            ClinicTheme.ApplyGrid(this._gridPharmacists);
            ApplyThemeHierarchy();

            ConfigureGridColumns();
            WireEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await LoadPharmacistsAsync();
        }

        private void ApplyThemeHierarchy()
        {
            this.BackColor = ClinicTheme.Background;

            // Header
            _headerPanel.BackColor = ClinicTheme.Sidebar;
            _headerPanel.BorderStyle = BorderStyle.FixedSingle;
            _lblTitle.ForeColor = ClinicTheme.TextPrimary;
            _lblSubtitle.ForeColor = ClinicTheme.TextSecondary;

            // Roster elements
            _lblRosterHeader.ForeColor = ClinicTheme.TextPrimary;
            SetButtonState(_btnAddNew, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));

            // Mode Banner
            _modeBannerPanel.BackColor = ClinicTheme.Sidebar;

            // GroupBoxes
            _grpProfile.ForeColor = ClinicTheme.TextPrimary;
            _grpCredentials.ForeColor = ClinicTheme.TextPrimary;

            // Initial Button States
            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));
            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));
            SetButtonState(_btnArchivePharmacist, false, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            SetButtonState(_btnDeletePharmacist, false, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));
            SetButtonState(_btnTestPortal, false, Color.White, Color.FromArgb(15, 23, 42), Color.FromArgb(203, 213, 225));
            SetButtonState(_btnResetOrProvision, false, Color.FromArgb(239, 246, 255), Color.FromArgb(29, 78, 216), Color.FromArgb(191, 219, 254));
        }

        private void SetButtonState(Button btn, bool enabled, Color activeBg, Color activeFg, Color activeBorder)
        {
            btn.Enabled = enabled;
            btn.FlatStyle = FlatStyle.Flat;
            if (enabled)
            {
                btn.BackColor = activeBg;
                btn.ForeColor = activeFg;
                btn.FlatAppearance.BorderColor = activeBorder;
                btn.FlatAppearance.BorderSize = 1;
                btn.Cursor = Cursors.Hand;
            }
            else
            {
                btn.BackColor = Color.FromArgb(241, 245, 249); // Slate-100 muted
                btn.ForeColor = Color.FromArgb(148, 163, 184); // Slate-400 muted text
                btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240); // Slate-200 border
                btn.FlatAppearance.BorderSize = 1;
                btn.Cursor = Cursors.No;
            }
        }

        private void ConfigureGridColumns()
        {
            _gridPharmacists.AutoGenerateColumns = false;
            _gridPharmacists.Columns.Clear();

            _gridPharmacists.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridPharmacists.ColumnHeadersHeight = 36;
            _gridPharmacists.EnableHeadersVisualStyles = false;
            _gridPharmacists.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _gridPharmacists.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridPharmacists.GridColor = Color.FromArgb(203, 213, 225);

            _gridPharmacists.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            _gridPharmacists.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridPharmacists.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            _gridPharmacists.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _gridPharmacists.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Column 0: Code (Centered)
            _gridPharmacists.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PharmacistRecord.PharmacistCode),
                HeaderText = "Pharmacist Code",
                Width = 120,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Column 1: Full Name (Left, Fill)
            _gridPharmacists.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PharmacistRecord.FullName),
                HeaderText = "Staff Full Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 170,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } },
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Column 2: PRC License # (Left)
            _gridPharmacists.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PharmacistRecord.LicenseNumber),
                HeaderText = "PRC License #",
                Width = 140,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } }
            });

            // Column 3: Contact # (Centered)
            _gridPharmacists.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PharmacistRecord.ContactNumber),
                HeaderText = "Contact #",
                Width = 125,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Column 4: Login Username (Left, Italic)
            _gridPharmacists.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PharmacistRecord.Username),
                HeaderText = "Login Username",
                Width = 130,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } },
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Italic) }
            });

            // Column 5: Status (Centered)
            _gridPharmacists.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PharmacistRecord.StatusDisplay),
                HeaderText = "Status",
                Width = 95,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Custom Header Painting: Guarantees uniform Medical Teal and pixel-perfect vertical alignment
            _gridPharmacists.CellPainting += (s, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                {
                    e.Handled = true;

                    // 1. Uniform Medical Teal Background
                    using var bgBrush = new SolidBrush(Color.FromArgb(15, 118, 110));
                    e.Graphics?.FillRectangle(bgBrush, e.CellBounds);

                    // 2. Pixel-perfect divider lines matching cell gridlines
                    using var borderPen = new Pen(Color.FromArgb(13, 98, 91));
                    if (e.Graphics != null)
                    {
                        e.Graphics.DrawLine(borderPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(borderPen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);

                        // 3. Header Text with matched alignment
                        string text = _gridPharmacists.Columns[e.ColumnIndex].HeaderText;
                        using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        var align = _gridPharmacists.Columns[e.ColumnIndex].HeaderCell.Style.Alignment;

                        TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                        if (align == DataGridViewContentAlignment.MiddleCenter)
                        {
                            flags |= TextFormatFlags.HorizontalCenter;
                        }
                        else
                        {
                            flags |= TextFormatFlags.Left;
                        }

                        var textRect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y, e.CellBounds.Width - 16, e.CellBounds.Height);
                        TextRenderer.DrawText(e.Graphics, text, font, textRect, Color.White, flags);
                    }
                }
            };

            // Color code the status cell based on Active/Archived
            _gridPharmacists.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == _gridPharmacists.Columns.Count - 1)
                {
                    string status = e.Value?.ToString() ?? string.Empty;
                    if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);
                        e.CellStyle.SelectionForeColor = Color.FromArgb(4, 120, 87);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                        e.CellStyle.SelectionForeColor = Color.FromArgb(180, 83, 9);
                    }
                }
            };
        }

        private void WireEvents()
        {
            _btnAddNew.Click += (_, _) => SwitchToCreationMode();
            _gridPharmacists.SelectionChanged += GridPharmacists_SelectionChanged;

            _btnPrimaryAction.Click += async (_, _) => await HandlePrimaryActionAsync();
            _btnClearOrCancel.Click += (_, _) => HandleCancelOrClear();
            _btnArchivePharmacist.Click += async (_, _) => await HandleArchiveOrRestoreAsync();
            _btnDeletePharmacist.Click += async (_, _) => await HandleDeletePharmacistAsync();
            _btnResetOrProvision.Click += async (_, _) => await HandleUpdateCredentialsAsync();
            _btnTestPortal.Click += BtnTestPortal_Click;
        }

        private async Task LoadPharmacistsAsync(int? selectPharmacistId = null)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _pharmacists = await Task.Run(() => _repo.GetAllPharmacists());
                _gridPharmacists.DataSource = null;
                _gridPharmacists.DataSource = _pharmacists;

                if (_pharmacists.Count > 0)
                {
                    int targetIndex = 0;
                    if (selectPharmacistId.HasValue)
                    {
                        int found = _pharmacists.FindIndex(p => p.PharmacistId == selectPharmacistId.Value);
                        if (found >= 0) targetIndex = found;
                    }

                    _gridPharmacists.ClearSelection();
                    _gridPharmacists.Rows[targetIndex].Selected = true;
                    SwitchToEditMode(_pharmacists[targetIndex]);
                }
                else
                {
                    SwitchToCreationMode();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void GridPharmacists_SelectionChanged(object? sender, EventArgs e)
        {
            if (_gridPharmacists.CurrentRow?.DataBoundItem is PharmacistRecord item)
            {
                SwitchToEditMode(item);
            }
        }

        private void SwitchToCreationMode()
        {
            _selectedPharmacist = null;
            _gridPharmacists.ClearSelection();

            // Mode banner
            _lblModeTitle.Text = "➕ Register New Pharmacist";
            _lblModeTitle.ForeColor = ClinicTheme.TextPrimary;
            _lblModeSubtitle.Text = "Fill in information to register a licensed pharmacist and create portal credentials.";
            _lblModeSubtitle.ForeColor = ClinicTheme.TextSecondary;

            // Fields
            _txtFirstName.Clear();
            _txtLastName.Clear();
            _txtLicenseNumber.Clear();
            _txtContactNumber.Clear();
            _txtEmailAddress.Clear();

            _txtUsername.Clear();
            _txtUsername.ReadOnly = false;
            _txtPassword.Clear();
            _lblPassword.Text = "Password *";

            // Buttons
            _btnPrimaryAction.Text = "Register Pharmacist";
            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));

            _btnClearOrCancel.Text = "Clear Fields";
            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));

            SetButtonState(_btnArchivePharmacist, false, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            SetButtonState(_btnDeletePharmacist, false, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));
            SetButtonState(_btnTestPortal, false, Color.White, Color.FromArgb(15, 23, 42), Color.FromArgb(203, 213, 225));
            SetButtonState(_btnResetOrProvision, false, Color.FromArgb(239, 246, 255), Color.FromArgb(29, 78, 216), Color.FromArgb(191, 219, 254));

            _txtFirstName.Focus();
        }

        private void SwitchToEditMode(PharmacistRecord item)
        {
            _selectedPharmacist = item;

            // Mode banner
            string statusStr = item.IsActive ? "ACTIVE" : "ARCHIVED";
            _lblModeTitle.Text = $"Edit Profile: {item.FullName}";
            _lblModeTitle.ForeColor = ClinicTheme.TextPrimary;
            _lblModeSubtitle.Text = $"Pharmacist Code: {item.PharmacistCode}  •  Status: {statusStr}";
            _lblModeSubtitle.ForeColor = item.IsActive ? Color.FromArgb(4, 120, 87) : Color.FromArgb(180, 83, 9);

            // Fields
            _txtFirstName.Text = item.FirstName;
            _txtLastName.Text = item.LastName;
            _txtLicenseNumber.Text = item.LicenseNumber;
            _txtContactNumber.Text = item.ContactNumber;
            _txtEmailAddress.Text = item.EmailAddress;

            _txtUsername.Text = item.Username;
            _txtUsername.ReadOnly = false;
            _txtPassword.Clear();
            _lblPassword.Text = "New Password (Leave blank to keep):";

            // Buttons
            _btnPrimaryAction.Text = "Save Profile Changes";
            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(37, 99, 235), Color.White, Color.FromArgb(37, 99, 235)); // Royal Blue

            _btnClearOrCancel.Text = "Cancel";
            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));

            // Archive button text and styling based on state
            if (item.IsActive)
            {
                _btnArchivePharmacist.Text = "🚫 Archive Pharmacist";
                SetButtonState(_btnArchivePharmacist, true, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            }
            else
            {
                _btnArchivePharmacist.Text = "✅ Reactivate Pharmacist";
                SetButtonState(_btnArchivePharmacist, true, Color.FromArgb(236, 253, 245), Color.FromArgb(4, 120, 87), Color.FromArgb(110, 231, 183));
            }

            SetButtonState(_btnDeletePharmacist, true, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));
            SetButtonState(_btnTestPortal, true, Color.White, Color.FromArgb(15, 23, 42), Color.FromArgb(203, 213, 225));
            SetButtonState(_btnResetOrProvision, true, Color.FromArgb(239, 246, 255), Color.FromArgb(29, 78, 216), Color.FromArgb(191, 219, 254));
        }

        private void HandleCancelOrClear()
        {
            SwitchToCreationMode();
        }

        private async Task HandlePrimaryActionAsync()
        {
            if (!ValidateInputs(isNew: _selectedPharmacist == null)) return;

            string firstName = _txtFirstName.Text.Trim();
            string lastName = _txtLastName.Text.Trim();
            string license = _txtLicenseNumber.Text.Trim();
            string contact = _txtContactNumber.Text.Trim();
            string email = _txtEmailAddress.Text.Trim();
            string username = _txtUsername.Text.Trim();
            string password = _txtPassword.Text.Trim();

            Cursor = Cursors.WaitCursor;
            try
            {
                if (_selectedPharmacist == null)
                {
                    // Register New Pharmacist
                    var ph = new PharmacistRecord
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        LicenseNumber = license,
                        ContactNumber = contact,
                        EmailAddress = email,
                        Username = username,
                        IsActive = true
                    };

                    var result = await Task.Run(() => _repo.SavePharmacist(ph, password));
                    if (result.Success)
                    {
                        MessageBox.Show($"Pharmacist '{firstName} {lastName}, RPh' has been successfully registered.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPharmacistsAsync();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // Update Profile Changes
                    var ph = new PharmacistRecord
                    {
                        PharmacistId = _selectedPharmacist.PharmacistId,
                        PharmacistCode = _selectedPharmacist.PharmacistCode,
                        UserId = _selectedPharmacist.UserId,
                        FirstName = firstName,
                        LastName = lastName,
                        LicenseNumber = license,
                        ContactNumber = contact,
                        EmailAddress = email,
                        Username = username,
                        IsActive = _selectedPharmacist.IsActive
                    };

                    string? pwd = string.IsNullOrWhiteSpace(password) ? null : password;
                    var result = await Task.Run(() => _repo.SavePharmacist(ph, pwd));
                    if (result.Success)
                    {
                        MessageBox.Show("Pharmacist profile updated successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPharmacistsAsync(_selectedPharmacist.PharmacistId);
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task HandleArchiveOrRestoreAsync()
        {
            if (_selectedPharmacist == null) return;

            string action = _selectedPharmacist.IsActive ? "archive" : "reactivate";
            string prompt = _selectedPharmacist.IsActive
                ? $"Are you sure you want to archive {_selectedPharmacist.FullName}?\n\nThey will no longer be able to log into the pharmacy dispensing station until reactivated."
                : $"Are you sure you want to reactivate {_selectedPharmacist.FullName}?\n\nTheir portal access and dispensing privileges will be restored.";

            if (MessageBox.Show(prompt, $"Confirm {action.ToUpper()}", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _repo.TogglePharmacistActive(_selectedPharmacist.PharmacistId));
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadPharmacistsAsync(_selectedPharmacist.PharmacistId);
                }
                else
                {
                    MessageBox.Show(result.Message, "Action Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task HandleDeletePharmacistAsync()
        {
            if (_selectedPharmacist == null) return;

            var confirm = MessageBox.Show(
                $"WARNING: Are you sure you want to permanently delete pharmacist record '{_selectedPharmacist.FullName}'?\n\nThis will remove their profile and login account. This action cannot be undone.",
                "Confirm Permanent Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop);

            if (confirm != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _repo.DeletePharmacist(_selectedPharmacist.PharmacistId, _selectedPharmacist.FullName));
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Record Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadPharmacistsAsync();
                }
                else
                {
                    MessageBox.Show(result.Message, "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task HandleUpdateCredentialsAsync()
        {
            if (_selectedPharmacist == null) return;

            string username = _txtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsername.Focus();
                return;
            }

            string password = _txtPassword.Text.Trim();
            string? pwd = string.IsNullOrWhiteSpace(password) ? null : password;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _repo.UpdatePharmacistCredentials(_selectedPharmacist.UserId, username, pwd));
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "Credentials Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtPassword.Clear();
                    await LoadPharmacistsAsync(_selectedPharmacist.PharmacistId);
                }
                else
                {
                    MessageBox.Show(result.Message, "Credentials Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BtnTestPortal_Click(object? sender, EventArgs e)
        {
            if (_selectedPharmacist == null) return;

            if (!_selectedPharmacist.IsActive)
            {
                MessageBox.Show("Cannot open dispensing station for an archived pharmacist. Please reactivate them first.", "Account Inactive", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UserSession.SetActivePharmacist(_selectedPharmacist.PharmacistId, _selectedPharmacist.FullName, _selectedPharmacist.LicenseNumber);

            using var portal = new PharmacistDashboardForm();
            portal.ShowDialog(this);
        }

        private bool ValidateInputs(bool isNew)
        {
            string firstName = _txtFirstName.Text.Trim();
            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("First Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtFirstName.Focus();
                return false;
            }

            string lastName = _txtLastName.Text.Trim();
            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Last Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLastName.Focus();
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(firstName, @"^[a-zA-Z\s\-']+$") ||
                !System.Text.RegularExpressions.Regex.IsMatch(lastName, @"^[a-zA-Z\s\-']+$"))
            {
                MessageBox.Show("Pharmacist names must contain valid alphabetic characters only (no numbers or symbols).", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string license = _txtLicenseNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(license))
            {
                MessageBox.Show("PRC Professional License Number is required for dispensary privileges.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLicenseNumber.Focus();
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(license, @"^\d{6,8}$"))
            {
                MessageBox.Show("PRC License Number must be a valid 6 to 8-digit numeric registration (e.g. 0123456).", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLicenseNumber.Focus();
                return false;
            }

            string username = _txtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsername.Focus();
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_\.]+$"))
            {
                MessageBox.Show("Username may only contain letters, digits, underscores, and dots.", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsername.Focus();
                return false;
            }

            string password = _txtPassword.Text.Trim();
            if (isNew && string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password is required for new pharmacist registration.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPassword.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 6 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
                {
                    MessageBox.Show("Password must be at least 6 characters long and contain both letters and numbers for account security.", "Security QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus();
                    return false;
                }
            }

            return true;
        }
    }
}

