#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public sealed class PasswordResetDialog : Form
{
    private readonly Label _lblPrompt = new();
    private readonly Label _lblNewPassword = new();
    private readonly TextBox _txtNewPassword = new();
    private readonly Label _lblConfirm = new();
    private readonly TextBox _txtConfirm = new();
    private readonly Button _btnSubmit = new();
    private readonly Button _btnCancel = new();

    public string NewPassword { get; private set; } = string.Empty;

    public PasswordResetDialog(string username)
    {
        InitializeUi(username);
    }

    private void InitializeUi(string username)
    {
        Text = "Reset User Password";
        ClientSize = new Size(380, 240);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        _lblPrompt.Text = $"Enter a new password for account '{username}':";
        _lblPrompt.Location = new Point(24, 20);
        _lblPrompt.Size = new Size(330, 30);
        _lblPrompt.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _lblPrompt.ForeColor = Color.FromArgb(15, 23, 42);

        _lblNewPassword.Text = "New Password (min 6 chars):";
        _lblNewPassword.Location = new Point(24, 56);
        _lblNewPassword.Size = new Size(330, 20);
        _lblNewPassword.Font = new Font("Segoe UI", 8.5F);

        _txtNewPassword.Location = new Point(24, 78);
        _txtNewPassword.Size = new Size(330, 26);
        _txtNewPassword.Font = new Font("Segoe UI", 9.5F);
        _txtNewPassword.UseSystemPasswordChar = true;

        _lblConfirm.Text = "Confirm New Password:";
        _lblConfirm.Location = new Point(24, 114);
        _lblConfirm.Size = new Size(330, 20);
        _lblConfirm.Font = new Font("Segoe UI", 8.5F);

        _txtConfirm.Location = new Point(24, 136);
        _txtConfirm.Size = new Size(330, 26);
        _txtConfirm.Font = new Font("Segoe UI", 9.5F);
        _txtConfirm.UseSystemPasswordChar = true;

        _btnSubmit.Text = "Reset Password";
        _btnSubmit.Location = new Point(140, 180);
        _btnSubmit.Size = new Size(120, 34);
        _btnSubmit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _btnSubmit.BackColor = Color.FromArgb(37, 99, 235);
        _btnSubmit.ForeColor = Color.White;
        _btnSubmit.FlatStyle = FlatStyle.Flat;
        _btnSubmit.FlatAppearance.BorderSize = 0;
        _btnSubmit.Cursor = Cursors.Hand;
        _btnSubmit.Click += OnSubmitClick;

        _btnCancel.Text = "Cancel";
        _btnCancel.Location = new Point(270, 180);
        _btnCancel.Size = new Size(84, 34);
        _btnCancel.Font = new Font("Segoe UI", 9F);
        _btnCancel.BackColor = Color.White;
        _btnCancel.ForeColor = Color.FromArgb(71, 85, 105);
        _btnCancel.FlatStyle = FlatStyle.Flat;
        _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        _btnCancel.Cursor = Cursors.Hand;
        _btnCancel.DialogResult = DialogResult.Cancel;

        Controls.Add(_lblPrompt);
        Controls.Add(_lblNewPassword);
        Controls.Add(_txtNewPassword);
        Controls.Add(_lblConfirm);
        Controls.Add(_txtConfirm);
        Controls.Add(_btnSubmit);
        Controls.Add(_btnCancel);

        AcceptButton = _btnSubmit;
        CancelButton = _btnCancel;
    }

    private void OnSubmitClick(object? sender, EventArgs e)
    {
        string p1 = _txtNewPassword.Text;
        string p2 = _txtConfirm.Text;

        if (string.IsNullOrWhiteSpace(p1) || p1.Length < 6)
        {
            MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNewPassword.Focus();
            return;
        }

        if (p1 != p2)
        {
            MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtConfirm.Focus();
            return;
        }

        NewPassword = p1;
        DialogResult = DialogResult.OK;
        Close();
    }
}
