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
    public partial class DoctorManagementForm : Form
    {
        private readonly DoctorRepository _doctorRepo = new();
        private List<DoctorAccountItem> _doctors = new();
        private DoctorAccountItem? _selectedDoctor = null;

        public DoctorManagementForm()
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                UserSession.RequireRole("Admin");
            }

            InitializeComponent();
            
            // Layout setup
            this._headerPanel.SendToBack();
            this._rightPanel.SendToBack();
            this._leftGridPanel.BringToFront();
            this._rosterHeaderPanel.SendToBack();
            this._doctorsGrid.BringToFront();
            
            // Double buffering for smoother grid rendering
            typeof(DataGridView).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.SetProperty, 
                null, this._doctorsGrid, new object[] { true });
                
            // Apply new color scheme using ClinicTheme utility
            ClinicTheme.ApplyForm(this);
            ClinicTheme.ApplyGrid(this._doctorsGrid);
            ApplyThemeHierarchy();
            
            ConfigureGridColumns();
            WireEvents();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await LoadDoctorsAsync();
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
            SetButtonState(_btnAddNewDoctor, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));

            // Mode Banner
            _modeBannerPanel.BackColor = ClinicTheme.Sidebar;

            // GroupBoxes
            _grpProfile.ForeColor = ClinicTheme.TextPrimary;
            _grpCredentials.ForeColor = ClinicTheme.TextPrimary;

            // Initial Button States
            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));
            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));
            SetButtonState(_btnArchiveDoctor, false, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            SetButtonState(_btnDeleteDoctor, false, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));
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
                btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240); // Slate-200 subtle border
                btn.FlatAppearance.BorderSize = 1;
                btn.Cursor = Cursors.No;
            }
        }

        private void ConfigureGridColumns()
        {
            _doctorsGrid.AutoGenerateColumns = false;
            _doctorsGrid.Columns.Clear();

            _doctorsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _doctorsGrid.ColumnHeadersHeight = 36;
            _doctorsGrid.EnableHeadersVisualStyles = false;
            _doctorsGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _doctorsGrid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _doctorsGrid.GridColor = Color.FromArgb(203, 213, 225);

            _doctorsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            _doctorsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _doctorsGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            _doctorsGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _doctorsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Column 0: Doctor Code (Centered)
            _doctorsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorAccountItem.DoctorCode),
                HeaderText = "Doctor Code",
                Width = 110,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Column 1: Attending Physician Name (Left, Fill)
            _doctorsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorAccountItem.FullName),
                HeaderText = "Attending Physician Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 180,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } },
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Column 2: Specialization (Left)
            _doctorsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorAccountItem.Specialty),
                HeaderText = "Specialization",
                Width = 165,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } }
            });

            // Column 3: PRC License # (Centered)
            _doctorsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorAccountItem.LicenseNumber),
                HeaderText = "PRC License #",
                Width = 125,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Column 4: Login Username (Left, Italic)
            _doctorsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorAccountItem.Username),
                HeaderText = "Login Username",
                Width = 130,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleLeft } },
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Italic) }
            });

            // Column 5: Status (Centered)
            _doctorsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DoctorAccountItem.StatusDisplay),
                HeaderText = "Status",
                Width = 95,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            // Custom Header Painting: Guarantees uniform Medical Teal and pixel-perfect vertical alignment with cell borders
            _doctorsGrid.CellPainting += (s, e) =>
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
                        string text = _doctorsGrid.Columns[e.ColumnIndex].HeaderText;
                        using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
                        var align = _doctorsGrid.Columns[e.ColumnIndex].HeaderCell.Style.Alignment;

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
            _doctorsGrid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == _doctorsGrid.Columns.Count - 1)
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
            _btnAddNewDoctor.Click += (_, _) => SwitchToAddMode();
            _btnPrimaryAction.Click += async (_, _) => await HandlePrimaryActionAsync();
            _btnResetOrProvision.Click += async (_, _) => await HandleCredentialsActionAsync();
            _btnClearOrCancel.Click += (_, _) => SwitchToAddMode();
            _btnTestPortal.Click += (_, _) => HandleTestPortal();
            _btnArchiveDoctor.Click += async (_, _) => await HandleArchiveDoctorAsync();
            _btnDeleteDoctor.Click += async (_, _) => await HandleDeleteDoctorAsync();

            // 1. DATA QUALITY ASSURANCE: Names (Only letters, spaces, hyphens, and apostrophes)
            KeyPressEventHandler nameKeyPress = (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (char.IsLetter(e.KeyChar) || e.KeyChar == ' ' || e.KeyChar == '-' || e.KeyChar == '\'') return;
                e.Handled = true; // Reject digits and nuisance symbols
            };

            EventHandler nameTextChanged = (s, e) =>
            {
                if (s is TextBox tb)
                {
                    string current = tb.Text;
                    string sanitized = Regex.Replace(current, @"[^a-zA-Z\s\-']", "");
                    if (current != sanitized)
                    {
                        int sel = tb.SelectionStart;
                        tb.Text = sanitized;
                        tb.SelectionStart = Math.Min(sel, sanitized.Length);
                    }
                }
            };

            _txtFirstName.MaxLength = 50;
            _txtFirstName.KeyPress += nameKeyPress;
            _txtFirstName.TextChanged += nameTextChanged;

            _txtLastName.MaxLength = 50;
            _txtLastName.KeyPress += nameKeyPress;
            _txtLastName.TextChanged += nameTextChanged;

            // 2. DATA QUALITY ASSURANCE: Contact Number (Philippine mobile: starts with 09 and exactly 11 digits)
            _txtContact.MaxLength = 11;
            _txtContact.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (char.IsDigit(e.KeyChar)) return;
                e.Handled = true; // Block letters and symbols
            };

            _txtContact.TextChanged += (s, e) =>
            {
                string current = _txtContact.Text;
                string sanitized = Regex.Replace(current, @"\D", "");
                if (sanitized.Length > 11) sanitized = sanitized.Substring(0, 11);
                if (current != sanitized)
                {
                    int sel = _txtContact.SelectionStart;
                    _txtContact.Text = sanitized;
                    _txtContact.SelectionStart = Math.Min(sel, sanitized.Length);
                }
            };

            // 3. DATA QUALITY ASSURANCE: Username (Login ID) - Alphanumeric, underscores, dots only
            _txtUsername.MaxLength = 50;
            _txtUsername.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '_' || e.KeyChar == '.') return;
                e.Handled = true; // Reject spaces and nuisance symbols
            };

            _txtUsername.TextChanged += (s, e) =>
            {
                string current = _txtUsername.Text;
                string sanitized = Regex.Replace(current, @"[^a-zA-Z0-9_\.]", "");
                if (current != sanitized)
                {
                    int sel = _txtUsername.SelectionStart;
                    _txtUsername.Text = sanitized;
                    _txtUsername.SelectionStart = Math.Min(sel, sanitized.Length);
                }
            };

            // 4. DATA QUALITY ASSURANCE: PRC License #
            _txtLicense.MaxLength = 50;
            _txtLicense.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '-' || e.KeyChar == ' ') return;
                e.Handled = true;
            };

            _doctorsGrid.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _doctors.Count)
                {
                    SwitchToEditMode(_doctors[e.RowIndex]);
                }
            };
        }

        private async Task LoadDoctorsAsync()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _doctors = await Task.Run(() => _doctorRepo.GetAllDoctors());
                _doctorsGrid.DataSource = null;
                _doctorsGrid.DataSource = _doctors;

                if (_selectedDoctor != null)
                {
                    var existing = _doctors.Find(d => d.DoctorId == _selectedDoctor.DoctorId);
                    if (existing != null)
                    {
                        SwitchToEditMode(existing);
                        return;
                    }
                }

                if (_doctors.Count > 0)
                {
                    _doctorsGrid.Rows[0].Selected = true;
                    SwitchToEditMode(_doctors[0]);
                }
                else
                {
                    SwitchToAddMode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load doctors: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SwitchToAddMode()
        {
            _selectedDoctor = null;
            _doctorsGrid.ClearSelection();

            _lblModeTitle.Text = "➕ Register New Doctor";
            _lblModeTitle.ForeColor = Color.FromArgb(15, 118, 110);
            _lblModeSubtitle.Text = "Enter doctor clinical profile and provision login credentials below.";
            _lblModeSubtitle.ForeColor = ClinicTheme.TextSecondary;

            _txtFirstName.Clear();
            _txtLastName.Clear();
            _txtLicense.Clear();
            _txtContact.Clear();
            _txtUsername.Clear();
            _txtPassword.Clear();

            _txtUsername.Enabled = true;
            _txtPassword.Enabled = true;
            _btnResetOrProvision.Visible = false;
            _lblCredentialsNote.Text = "System will create the login account in dbo.Users and default M-F practice hours.";

            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(15, 118, 110), Color.White, Color.FromArgb(15, 118, 110));
            _btnPrimaryAction.Text = "✓ Register Doctor & Login";

            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));
            _btnClearOrCancel.Text = "Clear Fields";
            
            if (_cboSpecialty.Items.Count > 0)
                _cboSpecialty.SelectedIndex = 0;

            _btnArchiveDoctor.Visible = false;
            _btnDeleteDoctor.Visible = false;
            _btnTestPortal.Visible = false;

            _txtFirstName.Focus();
        }

        private void SwitchToEditMode(DoctorAccountItem doc)
        {
            _selectedDoctor = doc;

            _lblModeTitle.Text = $"Edit Doctor Profile: {doc.FullName}";
            _lblModeTitle.ForeColor = ClinicTheme.TextPrimary;

            _lblModeSubtitle.Text = $"Doctor Code: {doc.DoctorCode}  •  Status: {(doc.IsActive ? "ACTIVE" : "ARCHIVED")}";
            _lblModeSubtitle.ForeColor = doc.IsActive ? Color.FromArgb(15, 118, 110) : Color.FromArgb(220, 38, 38);

            _txtFirstName.Text = doc.FirstName;
            _txtLastName.Text = doc.LastName;
            _txtLicense.Text = doc.LicenseNumber;
            _cboSpecialty.SelectedItem = doc.Specialty;
            _txtContact.Text = doc.ContactNumber;
            _txtPassword.Clear();

            // Username is ALWAYS EDITABLE now (in case of typos or login ID changes)
            _txtUsername.Text = doc.Username;
            _txtUsername.Enabled = true;

            if (doc.UserId.HasValue)
            {
                _btnResetOrProvision.Text = "🔑 Update Login / Pwd";
                _btnResetOrProvision.Visible = true;
                SetButtonState(_btnResetOrProvision, true, Color.FromArgb(239, 246, 255), Color.FromArgb(29, 78, 216), Color.FromArgb(191, 219, 254));
                _lblCredentialsNote.Text = "Edit the username or enter a new password above, then save or click 'Update Login / Pwd'.";
            }
            else
            {
                _btnResetOrProvision.Text = "➕ Provision Login";
                _btnResetOrProvision.Visible = true;
                SetButtonState(_btnResetOrProvision, true, Color.FromArgb(236, 253, 245), Color.FromArgb(4, 120, 87), Color.FromArgb(167, 243, 208));
                _lblCredentialsNote.Text = "This doctor has (No Login). Enter username & password to provision credentials.";
            }

            SetButtonState(_btnPrimaryAction, true, Color.FromArgb(37, 99, 235), Color.White, Color.FromArgb(37, 99, 235));
            _btnPrimaryAction.Text = "💾 Save Profile Changes";

            SetButtonState(_btnClearOrCancel, true, Color.White, Color.FromArgb(71, 85, 105), Color.FromArgb(203, 213, 225));
            _btnClearOrCancel.Text = "Cancel";

            // Archive / Restore button
            _btnArchiveDoctor.Visible = true;
            if (doc.IsActive)
            {
                _btnArchiveDoctor.Text = "🚫 Archive Doctor";
                SetButtonState(_btnArchiveDoctor, true, Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9), Color.FromArgb(252, 211, 77));
            }
            else
            {
                _btnArchiveDoctor.Text = "✅ Restore Doctor";
                SetButtonState(_btnArchiveDoctor, true, Color.FromArgb(236, 253, 245), Color.FromArgb(4, 120, 87), Color.FromArgb(110, 231, 183));
            }

            // Delete Record button
            _btnDeleteDoctor.Visible = true;
            SetButtonState(_btnDeleteDoctor, true, Color.FromArgb(254, 242, 242), Color.FromArgb(220, 38, 38), Color.FromArgb(252, 165, 165));

            // Test Portal button (greyed out if doctor is archived or has no credentials)
            _btnTestPortal.Visible = true;
            SetButtonState(_btnTestPortal, doc.IsActive && doc.UserId.HasValue, Color.White, Color.FromArgb(15, 23, 42), Color.FromArgb(203, 213, 225));
        }

        private async Task HandleArchiveDoctorAsync()
        {
            if (_selectedDoctor == null) return;

            string actionName = _selectedDoctor.IsActive ? "Archive / Deactivate" : "Restore to Active";
            string promptText = _selectedDoctor.IsActive
                ? $"Are you sure you want to archive {_selectedDoctor.FullName}?\n\n• The doctor will be hidden from appointment booking.\n• Their login portal access will be disabled.\n• All past patient appointments and medical records remain safely preserved."
                : $"Restore {_selectedDoctor.FullName} back to active practice?\n\n• The doctor will reappear in appointment booking.\n• Their login portal access will be re-enabled.";

            var confirm = MessageBox.Show(promptText, $"{actionName} Doctor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _doctorRepo.ToggleDoctorArchive(_selectedDoctor.DoctorId, _selectedDoctor.IsActive));
                MessageBox.Show(result.Message, "Doctor Status", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (result.Success)
                {
                    await LoadDoctorsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error during archive operation:\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task HandleDeleteDoctorAsync()
        {
            if (_selectedDoctor == null) return;

            var confirm = MessageBox.Show(
                $"Permanently delete {_selectedDoctor.FullName}?\n\nWarning: This will completely remove the doctor and their account. It can only proceed if NO patient consultations or appointments exist for this doctor.",
                "Confirm Permanent Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            try
            {
                var result = await Task.Run(() => _doctorRepo.DeleteDoctor(_selectedDoctor.DoctorId, _selectedDoctor.FullName));
                MessageBox.Show(result.Message, "Delete Doctor", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                if (result.Success)
                {
                    _selectedDoctor = null;
                    await LoadDoctorsAsync();
                    SwitchToAddMode();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (errorMsg.IndexOf("REFERENCE", StringComparison.OrdinalIgnoreCase) >= 0 || 
                    errorMsg.IndexOf("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    MessageBox.Show("Cannot delete this doctor because they have related records (e.g., patient appointments, medical notes). Please archive the doctor instead.", "Constraint Violation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Failed to delete the doctor due to an unexpected database error:\n{errorMsg}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task HandlePrimaryActionAsync()
        {
            string firstName = _txtFirstName.Text.Trim();
            string lastName = _txtLastName.Text.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("First Name and Last Name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // QA: Name must only contain letters, spaces, hyphens, and apostrophes
            if (!Regex.IsMatch(firstName, @"^[a-zA-Z\s\-']+$") || !Regex.IsMatch(lastName, @"^[a-zA-Z\s\-']+$"))
            {
                MessageBox.Show("Doctor names cannot contain numbers or special symbols. Please enter valid alphabetic characters only.", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // QA: Contact number validation (Philippine 11-digit mobile starting with 09)
            string contact = _txtContact.Text.Trim();
            if (!string.IsNullOrWhiteSpace(contact))
            {
                if (!Regex.IsMatch(contact, @"^09\d{9}$"))
                {
                    MessageBox.Show("Contact number must be a valid Philippine mobile number with exactly 11 digits starting with '09' (e.g., 09123456789).", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtContact.Focus();
                    return;
                }
            }

            string license = _txtLicense.Text.Trim();
            if (string.IsNullOrWhiteSpace(license) || !Regex.IsMatch(license, @"^\d{6,8}$"))
            {
                MessageBox.Show("PRC Medical License Number must be a valid 6 to 8-digit numeric registration (e.g. 0123456).", "Credentials QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLicense.Focus();
                return;
            }

            string specialty = _cboSpecialty.Text;
            string username = _txtUsername.Text.Trim();
            string password = _txtPassword.Text.Trim();

            // QA: Username validation
            if (!string.IsNullOrWhiteSpace(username))
            {
                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_\.]+$"))
                {
                    MessageBox.Show("Login username may only contain letters, digits, underscores, and dots (no spaces or symbols).", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }
            }

            // QA: Password complexity validation
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 6 || !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
                {
                    MessageBox.Show("Password must be at least 6 characters long and contain both letters and numbers for account security.", "Security QA Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus();
                    return;
                }
            }

            if (_selectedDoctor != null)
            {
                int doctorId = _selectedDoctor.DoctorId;

                Cursor = Cursors.WaitCursor;
                try
                {
                    var result = await Task.Run(() => _doctorRepo.UpdateDoctorProfile(doctorId, firstName, lastName, license, specialty, contact));
                    
                    // Handle Username / Password changes
                    if (_selectedDoctor.UserId.HasValue)
                    {
                        bool usernameChanged = !string.Equals(username, _selectedDoctor.Username, StringComparison.OrdinalIgnoreCase);
                        bool passwordEntered = !string.IsNullOrWhiteSpace(password);

                        if (usernameChanged || passwordEntered)
                        {
                            if (string.IsNullOrWhiteSpace(username))
                            {
                                MessageBox.Show("Username (Login ID) cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            var credRes = await Task.Run(() => _doctorRepo.UpdateDoctorCredentials(_selectedDoctor.UserId.Value, username, passwordEntered ? password : null));
                            if (!credRes.Success)
                            {
                                MessageBox.Show($"Doctor profile updated, but credentials update failed:\n{credRes.Message}", "Credentials Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                await LoadDoctorsAsync();
                                return;
                            }
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                    {
                        // Provision login if previously had none
                        var provRes = await Task.Run(() => _doctorRepo.ProvisionLoginForDoctor(doctorId, $"Dr. {firstName} {lastName}", username, password));
                        if (!provRes.Success)
                        {
                            MessageBox.Show($"Doctor profile updated, but provisioning login failed:\n{provRes.Message}", "Provisioning Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    MessageBox.Show("Doctor profile and portal credentials saved successfully.", "Doctor Profile Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDoctorsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error updating profile:\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Username and initial password are required to register a doctor with portal access.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }

                Cursor = Cursors.WaitCursor;
                try
                {
                    var result = await Task.Run(() => _doctorRepo.RegisterDoctorWithAccount(firstName, lastName, license, specialty, contact, username, password));
                    MessageBox.Show(result.Message, "Doctor Registration", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    if (result.Success) await LoadDoctorsAsync();
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message;
                    if (errorMsg.IndexOf("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        errorMsg.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        MessageBox.Show("That Username or License Number is already registered in the system. Please choose a different one.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to register doctor due to a database error:\n{errorMsg}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private async Task HandleCredentialsActionAsync()
        {
            if (_selectedDoctor == null) return;

            string username = _txtUsername.Text.Trim();
            string password = _txtPassword.Text.Trim();

            if (_selectedDoctor.UserId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Username (Login ID) cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }

                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_\.]+$"))
                {
                    MessageBox.Show("Login username may only contain letters, digits, underscores, and dots.", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }

                bool usernameChanged = !string.Equals(username, _selectedDoctor.Username, StringComparison.OrdinalIgnoreCase);
                bool passwordEntered = !string.IsNullOrWhiteSpace(password);

                if (!usernameChanged && !passwordEntered)
                {
                    MessageBox.Show("No changes detected in credentials. Type a new username or new password to update.", "No Changes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int userId = _selectedDoctor.UserId.Value;

                Cursor = Cursors.WaitCursor;
                try
                {
                    var result = await Task.Run(() => _doctorRepo.UpdateDoctorCredentials(userId, username, passwordEntered ? password : null));
                    MessageBox.Show(result.Message, "Credentials Updated", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    if (result.Success)
                    {
                        _txtPassword.Clear();
                        await LoadDoctorsAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error updating credentials:\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please provide both a Username and Password to provision this doctor's login.", "Credentials Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }

                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_\.]+$"))
                {
                    MessageBox.Show("Login username may only contain letters, digits, underscores, and dots.", "Data Quality Assurance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtUsername.Focus();
                    return;
                }

                int doctorId = _selectedDoctor.DoctorId;
                string fullName = _selectedDoctor.FullName;

                Cursor = Cursors.WaitCursor;
                try
                {
                    var result = await Task.Run(() => _doctorRepo.ProvisionLoginForDoctor(doctorId, fullName, username, password));
                    MessageBox.Show(result.Message, "Provision Login", MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                    if (result.Success)
                    {
                        _txtPassword.Clear();
                        await LoadDoctorsAsync();
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message;
                    if (errorMsg.IndexOf("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        errorMsg.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        MessageBox.Show("That Username is already in use by another user. Please choose a different one.", "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"Database error provisioning login:\n{errorMsg}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void HandleTestPortal()
        {
            if (_selectedDoctor == null)
            {
                MessageBox.Show("Please select a doctor from the roster to test as.", "Select Doctor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_selectedDoctor.IsActive || !_selectedDoctor.UserId.HasValue)
            {
                MessageBox.Show("Cannot test portal for an archived doctor or a doctor with no login credentials.", "Account Inactive", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UserSession.SetActiveDoctor(_selectedDoctor.DoctorId, _selectedDoctor.FullName, _selectedDoctor.Specialty);

            using var portal = new DoctorDashboardForm();
            portal.ShowDialog(this);
        }
    }
}
