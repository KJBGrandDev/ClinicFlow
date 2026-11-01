#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public sealed class AffectedPatientPreviewItem
    {
        public int AppointmentId { get; set; }
        public string AppointmentNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string TimeDisplay { get; set; } = string.Empty;
    }

    public sealed class EmergencyBlockoutDialog : Form
    {
        private readonly List<AffectedPatientPreviewItem> _patients;
        private readonly string _scopeDescription;

        private ComboBox _cboReason = null!;
        private TextBox _txtCustomReason = null!;
        private Label _lblCustomReason = null!;
        private Button _btnConfirm = null!;
        private Button _btnCancel = null!;

        public string SelectedReason { get; private set; } = string.Empty;

        public EmergencyBlockoutDialog(List<AffectedPatientPreviewItem> patients, string scopeDescription)
        {
            _patients = patients ?? new List<AffectedPatientPreviewItem>();
            _scopeDescription = scopeDescription;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Emergency Schedule Blockout & Patient Dispatch";
            this.Size = new Size(720, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            // 1. Header Banner
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
                Text = "🚨 Emergency Schedule Blockout Declaration",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(153, 27, 27),
                AutoSize = true,
                Location = new Point(20, 14)
            };
            var lblHeaderSubtitle = new Label
            {
                Text = $"Action Scope: {_scopeDescription} • {_patients.Count} patient appointment(s) will be displaced to front-desk triage.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(185, 28, 28),
                AutoSize = true,
                Location = new Point(22, 44)
            };
            pnlHeader.Controls.AddRange(new Control[] { lblHeaderTitle, lblHeaderSubtitle });

            // 2. Bottom Action Panel
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(20, 12, 20, 12)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240), 1);
                e.Graphics.DrawLine(p, 0, 0, pnlBottom.Width, 0);
            };

            _btnCancel = new Button
            {
                Text = "Cancel / Keep Booking",
                Size = new Size(180, 38),
                Location = new Point(20, 12),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnCancel.Click += (_, _) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            _btnConfirm = new Button
            {
                Text = "⚡ Confirm Emergency Blockout & Dispatch",
                Size = new Size(320, 38),
                Location = new Point(364, 12),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(220, 38, 38),
                Cursor = Cursors.Hand
            };
            _btnConfirm.FlatAppearance.BorderSize = 0;
            _btnConfirm.Click += OnConfirmClick;

            pnlBottom.Controls.AddRange(new Control[] { _btnCancel, _btnConfirm });

            // 3. Center Content Panel
            var pnlCenter = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16)
            };

            // Reason Selector
            var lblReason = new Label
            {
                Text = "Mandatory Justification / Emergency Category:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(24, 16),
                AutoSize = true
            };

            _cboReason = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(24, 42),
                Size = new Size(656, 32),
                Font = new Font("Segoe UI", 10F)
            };
            _cboReason.Items.AddRange(new object[]
            {
                "Emergency Surgery / STAT Procedure",
                "Acute Physician Illness / Medical Emergency",
                "Hospital Ward STAT Emergency Call",
                "Critical Personal / Family Emergency",
                "Clinic Equipment / Facility Emergency",
                "Other Justified Emergency (Specify Below)"
            });
            _cboReason.SelectedIndex = 0;

            _lblCustomReason = new Label
            {
                Text = "Specify Clinical Reason:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(24, 80),
                AutoSize = true,
                Visible = false
            };

            _txtCustomReason = new TextBox
            {
                Location = new Point(24, 102),
                Size = new Size(656, 28),
                Font = new Font("Segoe UI", 9.5F),
                Visible = false
            };

            _cboReason.SelectedIndexChanged += (_, _) =>
            {
                bool isOther = _cboReason.SelectedIndex == 5;
                _lblCustomReason.Visible = isOther;
                _txtCustomReason.Visible = isOther;
            };

            // Affected Patients Grid
            var lblGridTitle = new Label
            {
                Text = "Affected Confirmed Patients (Will be Dispatched to Triage):",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(24, 138),
                AutoSize = true
            };

            var grid = new DataGridView
            {
                Location = new Point(24, 164),
                Size = new Size(656, 175),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(241, 245, 249),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 34 }
            };
            ClinicTheme.ApplyGrid(grid);

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AffectedPatientPreviewItem.TimeDisplay),
                HeaderText = "Slot Time",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AffectedPatientPreviewItem.PatientNumber),
                HeaderText = "Patient #",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AffectedPatientPreviewItem.PatientName),
                HeaderText = "Patient Name",
                Width = 200,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(AffectedPatientPreviewItem.ContactNumber),
                HeaderText = "Contact #",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            grid.DataSource = _patients;

            pnlCenter.Controls.AddRange(new Control[]
            {
                lblReason,
                _cboReason,
                _lblCustomReason,
                _txtCustomReason,
                lblGridTitle,
                grid
            });

            this.Controls.Add(pnlCenter);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlHeader);
        }

        private void OnConfirmClick(object? sender, EventArgs e)
        {
            string reason = _cboReason.SelectedItem?.ToString() ?? "Doctor Emergency Blockout";
            if (_cboReason.SelectedIndex == 5)
            {
                string custom = _txtCustomReason.Text.Trim();
                if (string.IsNullOrWhiteSpace(custom) || custom.Length < 3)
                {
                    MessageBox.Show("Please enter a brief explanation for the emergency blockout.", "Reason Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtCustomReason.Focus();
                    return;
                }
                reason = $"Emergency: {custom}";
            }

            SelectedReason = reason;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
