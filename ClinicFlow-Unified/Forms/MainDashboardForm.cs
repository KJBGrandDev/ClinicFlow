#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms;

public partial class MainDashboardForm : Form
{
    private Button? _currentActiveButton;

    private bool _isReportsAllowed;
    private bool _isBackupAllowed;

    public string LoggedInUserFullName => _lblUserName.Text;
    public string LoggedInUserRole => _lblUserRoleBadge.Text;
    public string LoggedInUserStatus => _lblUserSessionInfo.Text;
    public bool HasReportsButton => _btnNavReports != null;
    public bool HasBackupButton => _btnNavBackup != null;
    public bool IsReportsAllowed => _isReportsAllowed;
    public bool IsBackupAllowed => _isBackupAllowed;

    public MainDashboardForm()
    {
        InitializeComponent();
        ClinicTheme.ApplyForm(this);
        InitializeSidebar();
    }

    private void InitializeSidebar()
    {
        Button[] navButtons = { 
            _btnNavDashboard, _btnNavPatients, _btnNavAppointments, _btnNavDoctors, 
            _btnNavSecretaries, _btnNavPharmacists, _btnNavPharmacy, _btnNavBilling, 
            _btnNavReports, _btnNavUsers, _btnNavAuditLogs, _btnNavBackup, _btnNavLogout 
        };
        
        foreach (var btn in navButtons)
        {
            btn.Dock = DockStyle.Top;
            btn.Height = 44;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.White; 
            btn.ForeColor = Color.FromArgb(71, 85, 105); // Slate 600
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            
            btn.MouseEnter += (s, e) => {
                if (btn != _currentActiveButton && btn != _btnNavLogout)
                {
                    btn.BackColor = Color.FromArgb(241, 245, 249); // Slate 100
                    btn.ForeColor = Color.FromArgb(15, 23, 42);   // Slate 900
                }
            };
            btn.MouseLeave += (s, e) => { 
                if (btn != _currentActiveButton && btn != _btnNavLogout)
                {
                    btn.BackColor = Color.White; 
                    btn.ForeColor = Color.FromArgb(71, 85, 105); 
                }
            };
        }

        _btnNavDashboard.Text = "📊 Overview";
        _btnNavPatients.Text = "👥 Patients";
        _btnNavAppointments.Text = "📅 Appointments";
        _btnNavDoctors.Text = "👨‍⚕️ Doctors";
        _btnNavSecretaries.Text = "👩‍💼 Secretaries";
        _btnNavPharmacists.Text = "🩺 Pharmacists";
        _btnNavPharmacy.Text = "💊 Central Pharmacy";
        _btnNavBilling.Text = "💳 Billing & Cashier";
        _btnNavReports.Text = "📈 Reports & Analytics";
        _btnNavUsers.Text = "🔐 User Accounts";
        _btnNavAuditLogs.Text = "📋 Audit Trail";
        _btnNavBackup.Text = "💾 Database Backup";
        
        _btnNavLogout.Text = "🚪 Logout";
        _btnNavLogout.Dock = DockStyle.Bottom;
        _btnNavLogout.ForeColor = Color.FromArgb(239, 68, 68); // Red
        _btnNavLogout.MouseEnter += (s, e) => { _btnNavLogout.BackColor = Color.FromArgb(254, 242, 242); };
        _btnNavLogout.MouseLeave += (s, e) => { _btnNavLogout.BackColor = Color.White; _btnNavLogout.ForeColor = Color.FromArgb(239, 68, 68); };

        // Click handlers
        _btnNavDashboard.Click += (_, _) => NavigateToOverview();
        _btnNavPatients.Click += (_, _) => NavigateToPatients();
        _btnNavAppointments.Click += (_, _) => NavigateToAppointments();
        _btnNavDoctors.Click += (_, _) => NavigateToDoctors();
        _btnNavSecretaries.Click += (_, _) => NavigateToSecretaries();
        _btnNavPharmacists.Click += (_, _) => NavigateToPharmacists();
        _btnNavPharmacy.Click += (_, _) => NavigateToPharmacy();
        _btnNavBilling.Click += (_, _) => NavigateToBilling();
        _btnNavReports.Click += (_, _) => NavigateToReports();
        _btnNavUsers.Click += (_, _) => NavigateToUsers();
        _btnNavAuditLogs.Click += (_, _) => NavigateToAuditLogs();
        _btnNavBackup.Click += (_, _) => NavigateToBackup();
        _btnNavLogout.Click += btnLogout_Click;
    }

    public void UpdateIdentityBadge()
    {
        string fullName = !string.IsNullOrWhiteSpace(UserSession.FullName) ? UserSession.FullName : (UserSession.Username ?? "Administrator");
        string role = !string.IsNullOrWhiteSpace(UserSession.RoleName) ? UserSession.RoleName : "Admin";
        string username = !string.IsNullOrWhiteSpace(UserSession.Username) ? UserSession.Username : "admin";

        _lblUserName.Text = fullName;
        _lblUserRoleBadge.Text = role.ToUpperInvariant();
        _lblUserSessionInfo.Text = $"🟢 Active Session (@{username})";

        switch (role.ToLowerInvariant())
        {
            case "admin":
                _lblUserRoleBadge.BackColor = Color.FromArgb(79, 70, 229); // Indigo 600
                _lblUserRoleBadge.ForeColor = Color.White;
                break;
            case "doctor":
                _lblUserRoleBadge.BackColor = Color.FromArgb(13, 148, 136); // Teal 600
                _lblUserRoleBadge.ForeColor = Color.White;
                break;
            case "secretary":
                _lblUserRoleBadge.BackColor = Color.FromArgb(217, 119, 6);  // Amber 600
                _lblUserRoleBadge.ForeColor = Color.White;
                break;
            case "pharmacist":
                _lblUserRoleBadge.BackColor = Color.FromArgb(16, 185, 129); // Emerald 600
                _lblUserRoleBadge.ForeColor = Color.White;
                break;
            default:
                _lblUserRoleBadge.BackColor = Color.FromArgb(100, 116, 139); // Slate 500
                _lblUserRoleBadge.ForeColor = Color.White;
                break;
        }
    }

    public void NavigateToOverview() => SelectNav(_btnNavDashboard, () => new AdminOverviewForm(this));
    public void NavigateToPatients() => SelectNav(_btnNavPatients, () => new PatientForm());
    public void NavigateToAppointments() => SelectNav(_btnNavAppointments, () => new AppointmentForm());
    public void NavigateToDoctors() => SelectNav(_btnNavDoctors, () => new DoctorManagementForm());
    public void NavigateToSecretaries() => SelectNav(_btnNavSecretaries, () => new SecretaryManagementForm());
    public void NavigateToPharmacists() => SelectNav(_btnNavPharmacists, () => new PharmacistManagementForm());
    public void NavigateToPharmacy() => SelectNav(_btnNavPharmacy, () => new PharmacistDashboardForm());
    public void NavigateToBilling() => SelectNav(_btnNavBilling, () => new BillingForm());
    public void NavigateToReports() => SelectNav(_btnNavReports, () => new ReportsForm());
    public void NavigateToUsers() => SelectNav(_btnNavUsers, () => new UserManagementForm());
    public void NavigateToAuditLogs() => SelectNav(_btnNavAuditLogs, () => new AuditLogViewerForm());
    public void NavigateToBackup() => SelectNav(_btnNavBackup, () => new DatabaseBackupRestoreDialog());

    private void SelectNav(Button activeBtn, Func<Form> formFactory)
    {
        Form moduleForm;
        try
        {
            moduleForm = formFactory();
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(
                ex.Message,
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _currentActiveButton = activeBtn;
        Button[] navButtons = { 
            _btnNavDashboard, _btnNavPatients, _btnNavAppointments, _btnNavDoctors, 
            _btnNavSecretaries, _btnNavPharmacists, _btnNavPharmacy, _btnNavBilling, 
            _btnNavReports, _btnNavUsers, _btnNavAuditLogs, _btnNavBackup 
        };

        foreach (var b in navButtons)
        {
            if (b == activeBtn)
            {
                b.BackColor = Color.FromArgb(239, 246, 255); // Blue 50
                b.ForeColor = Color.FromArgb(37, 99, 235);   // Blue 600
            }
            else
            {
                b.BackColor = Color.White;
                b.ForeColor = Color.FromArgb(71, 85, 105);
            }
        }

        LoadModule(moduleForm);
    }

    private void LoadModule(Form moduleForm)
    {
        _contentPanel.Controls.Clear();
        moduleForm.TopLevel = false;
        moduleForm.FormBorderStyle = FormBorderStyle.None;
        moduleForm.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(moduleForm);
        moduleForm.Show();
    }

    private void MainDashboardForm_Load(object? sender, EventArgs e)
    {
        if (!UserSession.IsAuthenticated)
        {
            Application.Restart();
            return;
        }

        UpdateIdentityBadge();
        ApplyRoleVisibility();
        NavigateToOverview();
    }

    private void ApplyRoleVisibility()
    {
        string role = UserSession.RoleName ?? string.Empty;
        bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        
        _isReportsAllowed = isAdmin || string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase);
        _isBackupAllowed = isAdmin;

        _btnNavDoctors.Visible = isAdmin;
        _btnNavSecretaries.Visible = isAdmin;
        _btnNavPharmacists.Visible = isAdmin;
        _btnNavPharmacy.Visible = isAdmin || string.Equals(role, "Pharmacist", StringComparison.OrdinalIgnoreCase);
        _btnNavReports.Visible = _isReportsAllowed;
        _btnNavUsers.Visible = isAdmin;
        _btnNavAuditLogs.Visible = isAdmin;
        _btnNavBackup.Visible = _isBackupAllowed;
    }

    private void btnLogout_Click(object? sender, EventArgs e)
    {
        if (UserSession.IsAuthenticated && UserSession.UserId.HasValue)
        {
            AuditLogger.Log("LOGOUT", "Users", UserSession.UserId.Value, $"User '{UserSession.Username ?? UserSession.FullName}' signed out.");
        }
        UserSession.Clear();
        this.Close();
    }
}


