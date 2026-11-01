#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public partial class SecretaryManagementForm : Form
    {
        private readonly SecretaryRepository _secretaryRepo = new();
        private List<SecretaryAccountItem> _secretaries = new();
        private SecretaryAccountItem? _selectedSecretary = null;

        public SecretaryManagementForm()
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
            this._secretariesGrid.BringToFront();

            // Double buffering for smooth grid rendering
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, this._secretariesGrid, new object[] { true });

            // Apply Theme
            ClinicTheme.ApplyForm(this);
            ClinicTheme.ApplyGrid(this._secretariesGrid);
            ApplyThemeHierarchy();

            ConfigureGridColumns();
            WireEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await LoadSecretariesAsync();
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
            SetButtonState(_btnAddNewSecretary, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));

            // Mode Banner
            _modeBannerPanel.BackColor = ClinicTheme.Sidebar;

            // GroupBoxes
            _grpProfile.ForeColor = ClinicTheme.TextPrimary;
            _grpCredentials.ForeColor = ClinicTheme.TextPrimary;

            // Initial Button States
            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));
            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));
            SetButtonState(_btnArchiveSecretary, false, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            SetButtonState(_btnDeleteSecretary, false, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));
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
            _secretariesGrid.AutoGenerateColumns = false;
            _secretariesGrid.Columns.Clear();

            _secretariesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _secretariesGrid.ColumnHeadersHeight = 36;
            _secretariesGrid.EnableHeadersVisualStyles = false;
            _secretariesGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _secretariesGrid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _secretariesGrid.GridColor = Color.FromArgb(203, 213, 225);

            _secretariesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            _secretariesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _secretariesGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            _secretariesGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _secretariesGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Column 0: Secretary Code (Centered)
            _secretariesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecretaryAccountItem.SecretaryCode),
                HeaderText = "Secretary Code",
                Width = 115,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Column 1: Full Name (Left, Fill)
            _secretariesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecretaryAccountItem.FullName),
                HeaderText = "Staff Full Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 170,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } },
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Column 2: Assigned Station / Desk (Left)
            _secretariesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecretaryAccountItem.AssignedDesk),
                HeaderText = "Assigned Desk / Station",
                Width = 175,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } }
            });

            // Column 3: Contact # (Centered)
            _secretariesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecretaryAccountItem.ContactNumber),
                HeaderText = "Contact #",
                Width = 120,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Column 4: Login Username (Left, Italic)
            _secretariesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecretaryAccountItem.Username),
                HeaderText = "Login Username",
                Width = 130,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } },
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Italic) }
            });

            // Column 5: Status (Centered)
            _secretariesGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(SecretaryAccountItem.StatusDisplay),
                HeaderText = "Status",
                Width = 95,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Custom Header Painting: Guarantees uniform Medical Teal and pixel-perfect vertical alignment
            _secretariesGrid.CellPainting += (s, e) =>
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
                        string text = _secretariesGrid.Columns[e.ColumnIndex].HeaderText;
                        using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        var align = _secretariesGrid.Columns[e.ColumnIndex].HeaderCell.Style.Alignment;

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
            _secretariesGrid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == _secretariesGrid.Columns.Count - 1)
                {
                    string status = e.Value?.ToString() ?? string.Empty;
                    if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(4, 120, 87);
                        e.CellStyle.BackColor = Color.FromArgb(236, 253, 245);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        e.CellStyle.BackColor = Color.FromArgb(254, 242, 242);
                    }
                }
            };
        }

        private void WireEvents()
        {
            _btnAddNewSecretary.Click += (_, _) => SwitchToAddMode();
            _btnPrimaryAction.Click += async (_, _) => await HandlePrimaryActionAsync();
            _btnResetOrProvision.Click += async (_, _) => await HandleCredentialsActionAsync();
            _btnClearOrCancel.Click += (_, _) => SwitchToAddMode();
            _btnTestPortal.Click += (_, _) => HandleTestPortal();
            _btnArchiveSecretary.Click += async (_, _) => await HandleArchiveSecretaryAsync();
            _btnDeleteSecretary.Click += async (_, _) => await HandleDeleteSecretaryAsync();

            // 1. DATA QUALITY ASSURANCE: Names (Only letters, spaces, hyphens, and apostrophes)
            KeyPressEventHandler nameKeyPress = (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ' && e.KeyChar != '-' && e.KeyChar != '\'')
                {
                    e.Handled = true;
                }
            };
            _txtFirstName.KeyPress += nameKeyPress;
            _txtLastName.KeyPress += nameKeyPress;

            // 2. DATA QUALITY ASSURANCE: Contact Number (Philippine format: digits only, max 11)
            _txtContact.MaxLength = 11;
            _txtContact.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };

            // 3. DATA QUALITY ASSURANCE: Username (Alphanumeric, dots, and underscores only)
            _txtUsername.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '_' && e.KeyChar != '.')
                {
                    e.Handled = true;
                }
            };

            // Grid selection
            _secretariesGrid.SelectionChanged += (_, _) =>
            {
                if (_secretariesGrid.SelectedRows.Count > 0)
                {
                    var item = _secretariesGrid.SelectedRows[0].DataBoundItem as SecretaryAccountItem;
                    if (item != null)
                    {
                        SwitchToEditMode(item);
                    }
                }
            };
        }

        private async Task LoadSecretariesAsync()
        {
            try
            {
                _secretaries = await Task.Run(() => _secretaryRepo.GetAllSecretaries());
                _secretariesGrid.DataSource = null;
                _secretariesGrid.DataSource = _secretaries;

                if (_secretaries.Count > 0)
                {
                    if (_selectedSecretary != null)
                    {
                        var match = _secretaries.Find(s => s.SecretaryId == _selectedSecretary.SecretaryId);
                        if (match != null)
                        {
                            SelectRowForSecretary(match.SecretaryId);
                            SwitchToEditMode(match);
                            return;
                        }
                    }
                    _secretariesGrid.Rows[0].Selected = true;
                    SwitchToEditMode(_secretaries[0]);
                }
                else
                {
                    SwitchToAddMode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load secretary roster: {ex.Message}", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectRowForSecretary(int secretaryId)
        {
            foreach (DataGridViewRow row in _secretariesGrid.Rows)
            {
                if (row.DataBoundItem is SecretaryAccountItem s && s.SecretaryId == secretaryId)
                {
                    row.Selected = true;
                    _secretariesGrid.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        private void SwitchToAddMode()
        {
            _selectedSecretary = null;
            _secretariesGrid.ClearSelection();

            _lblModeTitle.Text = "➕ Register New Secretary";
            _lblModeSubtitle.Text = "Fill in information to register a front-desk staff member and create portal credentials.";

            _txtFirstName.Clear();
            _txtLastName.Clear();
            _txtContact.Clear();
            _cboDesk.SelectedIndex = 0;
            _txtEmail.Clear();

            _txtUsername.Clear();
            _txtUsername.Enabled = true;
            _txtPassword.Clear();
            _txtPassword.Enabled = true;

            _btnPrimaryAction.Text = "Register Secretary";
            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));

            _btnClearOrCancel.Text = "Clear Fields";
            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));

            SetButtonState(_btnResetOrProvision, false, Color.FromArgb(239, 246, 255), Color.FromArgb(29, 78, 216), Color.FromArgb(191, 219, 254));
            SetButtonState(_btnArchiveSecretary, false, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            SetButtonState(_btnDeleteSecretary, false, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));
            SetButtonState(_btnTestPortal, false, Color.White, Color.FromArgb(15, 23, 42), Color.FromArgb(203, 213, 225));
        }

        private void SwitchToEditMode(SecretaryAccountItem item)
        {
            _selectedSecretary = item;

            string statusBadge = item.IsActive ? "ACTIVE" : "ARCHIVED";
            _lblModeTitle.Text = $"Edit Secretary Profile: {item.FullName}";
            _lblModeSubtitle.Text = $"Secretary Code: {item.SecretaryCode}  •  Status: {statusBadge}";

            _txtFirstName.Text = item.FirstName;
            _txtLastName.Text = item.LastName;
            _txtContact.Text = item.ContactNumber.Equals("N/A", StringComparison.OrdinalIgnoreCase) ? "" : item.ContactNumber;

            int deskIdx = _cboDesk.FindStringExact(item.AssignedDesk);
            if (deskIdx >= 0)
                _cboDesk.SelectedIndex = deskIdx;
            else
            {
                _cboDesk.Items.Add(item.AssignedDesk);
                _cboDesk.SelectedItem = item.AssignedDesk;
            }

            _txtEmail.Text = item.EmailAddress;

            _txtUsername.Text = item.Username.Equals("(No Login)", StringComparison.OrdinalIgnoreCase) ? "" : item.Username;
            _txtUsername.Enabled = true;
            _txtPassword.Clear();
            _txtPassword.Enabled = true;

            _btnPrimaryAction.Text = "Save Profile Changes";
            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(37, 99, 235), Color.White, Color.FromArgb(37, 99, 235)); // Medical Blue

            _btnClearOrCancel.Text = "Cancel";
            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));

            SetButtonState(_btnResetOrProvision, true, Color.FromArgb(239, 246, 255), Color.FromArgb(29, 78, 216), Color.FromArgb(191, 219, 254));

            if (item.IsActive)
            {
                _btnArchiveSecretary.Text = "🚫 Archive Secretary";
                SetButtonState(_btnArchiveSecretary, true, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            }
            else
            {
                _btnArchiveSecretary.Text = "♻️ Restore Secretary";
                SetButtonState(_btnArchiveSecretary, true, Color.FromArgb(236, 253, 245), Color.FromArgb(4, 120, 87), Color.FromArgb(110, 231, 183));
            }

            SetButtonState(_btnDeleteSecretary, true, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));
            SetButtonState(_btnTestPortal, item.IsActive, Color.White, Color.FromArgb(15, 23, 42), Color.FromArgb(203, 213, 225));
        }

        private async Task HandlePrimaryActionAsync()
        {
            // Validate Profile Inputs
            string firstName = _txtFirstName.Text.Trim();
            string lastName = _txtLastName.Text.Trim();
            string contact = _txtContact.Text.Trim();
            string desk = _cboDesk.SelectedItem?.ToString() ?? "Main Lobby Reception Desk";
            string email = _txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("First Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtFirstName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Last Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLastName.Focus();
                return;
            }

            // Phone number QA
            if (string.IsNullOrWhiteSpace(contact))
            {
                MessageBox.Show("Contact number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtContact.Focus();
                return;
            }
            if (!Regex.IsMatch(contact, @"^09\d{9}$"))
            {
                MessageBox.Show("Contact number must be an 11-digit Philippine mobile number starting with '09' (e.g., 09171234567).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtContact.Focus();
                return;
            }

            // ADD MODE
            if (_selectedSecretary == null)
            {
                string username = _txtUsername.Text.Trim();
                string password = _txtPassword.Text;

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Portal Username is required to register a new secretary.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }
                if (username.Length < 4)
                {
                    MessageBox.Show("Username must be at least 4 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
                {
                    MessageBox.Show("Password must be at least 6 characters long and contain both letters and numbers for account security.", "Security QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus();
                    return;
                }

                var (success, message, newId) = await Task.Run(() =>
                    _secretaryRepo.RegisterSecretaryWithAccount(firstName, lastName, contact, desk, email, username, password));

                if (success)
                {
                    MessageBox.Show(message, "Registration Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadSecretariesAsync();
                    SelectRowForSecretary(newId);
                }
                else
                {
                    MessageBox.Show(message, "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // EDIT MODE
            else
            {
                var (success, message) = await Task.Run(() =>
                    _secretaryRepo.UpdateSecretaryProfile(_selectedSecretary.SecretaryId, firstName, lastName, contact, desk, email));

                if (!success)
                {
                    MessageBox.Show(message, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // If username or password was modified in edit mode, update credentials as well
                string updatedUsername = _txtUsername.Text.Trim();
                string updatedPassword = _txtPassword.Text;
                bool usernameChanged = !string.IsNullOrWhiteSpace(updatedUsername) &&
                                       !updatedUsername.Equals(_selectedSecretary.Username, StringComparison.OrdinalIgnoreCase) &&
                                       !_selectedSecretary.Username.Equals("(No Login)", StringComparison.OrdinalIgnoreCase);
                bool passwordEntered = !string.IsNullOrWhiteSpace(updatedPassword);
                if (passwordEntered && (updatedPassword.Length < 6 || !updatedPassword.Any(char.IsLetter) || !updatedPassword.Any(char.IsDigit)))
                {
                    MessageBox.Show("New password must be at least 6 characters long and contain both letters and numbers for account security.", "Security QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus();
                    return;
                }

                if (_selectedSecretary.UserId.HasValue && (usernameChanged || passwordEntered))
                {
                    var (credSuccess, credMsg) = await Task.Run(() =>
                        _secretaryRepo.UpdateSecretaryCredentials(_selectedSecretary.UserId.Value, updatedUsername, passwordEntered ? updatedPassword : null));

                    if (!credSuccess)
                    {
                        MessageBox.Show($"Profile was saved, but updating login credentials failed: {credMsg}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Secretary profile and login credentials have been updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Secretary profile updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                int savedId = _selectedSecretary.SecretaryId;
                await LoadSecretariesAsync();
                SelectRowForSecretary(savedId);
            }
        }

        private async Task HandleCredentialsActionAsync()
        {
            if (_selectedSecretary == null) return;

            string username = _txtUsername.Text.Trim();
            string newPassword = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a valid username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsername.Focus();
                return;
            }

            if (!_selectedSecretary.UserId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
                {
                    MessageBox.Show("Please provide a password of at least 4 characters to provision this account.", "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus();
                    return;
                }

                var (success, message) = await Task.Run(() =>
                    _secretaryRepo.ProvisionLoginForSecretary(_selectedSecretary.SecretaryId, _selectedSecretary.FullName, username, newPassword));

                if (success)
                {
                    MessageBox.Show(message, "Account Provisioned", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtPassword.Clear();
                    await LoadSecretariesAsync();
                }
                else
                {
                    MessageBox.Show(message, "Provisioning Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                var (success, message) = await Task.Run(() =>
                    _secretaryRepo.UpdateSecretaryCredentials(_selectedSecretary.UserId.Value, username, string.IsNullOrWhiteSpace(newPassword) ? null : newPassword));

                if (success)
                {
                    MessageBox.Show(message, "Credentials Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtPassword.Clear();
                    await LoadSecretariesAsync();
                }
                else
                {
                    MessageBox.Show(message, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task HandleArchiveSecretaryAsync()
        {
            if (_selectedSecretary == null) return;

            bool isCurrentlyActive = _selectedSecretary.IsActive;
            string actionText = isCurrentlyActive ? "archive" : "restore";
            var result = MessageBox.Show($"Are you sure you want to {actionText} secretary '{_selectedSecretary.FullName}'?",
                $"Confirm {actionText.ToUpper()}", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var (success, message) = await Task.Run(() =>
                    _secretaryRepo.ToggleSecretaryArchive(_selectedSecretary.SecretaryId, isCurrentlyActive));

                if (success)
                {
                    MessageBox.Show(message, "Status Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    int currentId = _selectedSecretary.SecretaryId;
                    await LoadSecretariesAsync();
                    SelectRowForSecretary(currentId);
                }
                else
                {
                    MessageBox.Show(message, "Status Change Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task HandleDeleteSecretaryAsync()
        {
            if (_selectedSecretary == null) return;

            var confirm = MessageBox.Show(
                $"Are you sure you want to permanently delete front-desk secretary record for '{_selectedSecretary.FullName}' ({_selectedSecretary.SecretaryCode})?\n\nThis will also remove their portal login access.",
                "Confirm Permanent Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var (success, message) = await Task.Run(() =>
                _secretaryRepo.DeleteSecretary(_selectedSecretary.SecretaryId, _selectedSecretary.FullName));

            if (success)
            {
                MessageBox.Show(message, "Secretary Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _selectedSecretary = null;
                await LoadSecretariesAsync();
            }
            else
            {
                MessageBox.Show(message, "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleTestPortal()
        {
            if (_selectedSecretary == null) return;

            if (!_selectedSecretary.IsActive)
            {
                MessageBox.Show("Cannot launch portal for an archived secretary. Please restore them first.", "Archived Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var oldUserId = UserSession.UserId;
            var oldFullName = UserSession.FullName;
            var oldRoleId = UserSession.RoleId;
            var oldRoleName = UserSession.RoleName;

            try
            {
                UserSession.SetActiveSecretary(_selectedSecretary.SecretaryId, _selectedSecretary.FullName, _selectedSecretary.AssignedDesk);

                using var portal = new SecretaryDashboardForm();
                portal.ShowDialog();
            }
            finally
            {
                // Restore previous session
                if (oldUserId.HasValue)
                {
                    UserSession.Start(new ClinicFlow_Unified.Models.User
                    {
                        UserId = oldUserId.Value,
                        FullName = oldFullName,
                        RoleId = oldRoleId ?? 1,
                        RoleName = oldRoleName ?? "Admin"
                    });
                }
            }
        }
    }
}

