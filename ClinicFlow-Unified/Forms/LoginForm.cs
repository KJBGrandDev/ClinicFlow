using ClinicFlow_Unified.Utilities;
#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Services;
namespace ClinicFlow_Unified.Forms;

public partial class LoginForm : Form
{
    private readonly AuthenticationService _authenticationService;
    private readonly DoctorRepository _doctorRepo = new();
    private readonly SecretaryRepository _secretaryRepo = new();
    private readonly PharmacyRepository _pharmacyRepo = new();

    public LoginForm()
    {
        InitializeComponent();
            ClinicTheme.ApplyForm(this);

        SqlConnectionFactory connectionFactory = new();
        AuditLogRepository auditLogRepository = new AuditLogRepository(connectionFactory);
        AuditService auditService = new AuditService(auditLogRepository);

        _authenticationService = new AuthenticationService(
            new UserRepository(connectionFactory),
            auditService);
    }

    private void btnQuickAdmin_Click(object? sender, EventArgs e)
    {
        // One-click Admin sandbox for development & testing
        UserSession.Start(new ClinicFlow_Unified.Models.User
        {
            UserId = 1,
            Username = "admin",
            FullName = "System Administrator",
            RoleId = 1,
            RoleName = "Admin",
            IsActive = true
        });

        Hide();
        using (MainDashboardForm dashboardForm = new())
        {
            dashboardForm.ShowDialog();
        }
        txtPassword.Clear();
        Show();
    }

    private async void btnLogin_Click(object? sender, EventArgs e)
    {
        lblStatus.Visible = false;
        btnLogin.Enabled = false;

        try
        {
            var authResult = await _authenticationService.AuthenticateWithResultAsync(
                txtUsername.Text.Trim(),
                txtPassword.Text);

            if (!authResult.Success || authResult.User is null)
            {
                lblStatus.Text = authResult.ErrorMessage ?? "Invalid username or password.";
                lblStatus.ForeColor = authResult.IsLockedOut ? Color.FromArgb(185, 28, 28) : Color.Firebrick;
                lblStatus.Visible = true;
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            var authenticatedUser = authResult.User;

            UserSession.Start(authenticatedUser);

            // If a Doctor logs in, map their DoctorId and open Doctor Portal exclusively
            if (string.Equals(authenticatedUser.RoleName, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var docInfo = _doctorRepo.GetDoctorByUserId(authenticatedUser.UserId);
                if (docInfo.HasValue)
                {
                    UserSession.SetActiveDoctor(docInfo.Value.DoctorId, docInfo.Value.DoctorName, docInfo.Value.Specialty);
                }

                Hide();
                using (DoctorDashboardForm docPortal = new())
                {
                    docPortal.ShowDialog();
                }
                txtPassword.Clear();
                Show();
                return;
            }

            // If a Secretary or Receptionist logs in, map their SecretaryId and open Secretary Portal exclusively
            if (string.Equals(authenticatedUser.RoleName, "Secretary", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(authenticatedUser.RoleName, "Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var secInfo = _secretaryRepo.GetSecretaryByUserId(authenticatedUser.UserId);
                if (secInfo.HasValue)
                {
                    UserSession.SetActiveSecretary(secInfo.Value.SecretaryId, secInfo.Value.SecretaryName, secInfo.Value.AssignedDesk);
                }

                Hide();
                using (SecretaryDashboardForm secPortal = new())
                {
                    secPortal.ShowDialog();
                }
                txtPassword.Clear();
                Show();
                return;
            }

            // If a Pharmacist logs in, map their PharmacistId and open Pharmacy & Dispensing Portal exclusively
            if (string.Equals(authenticatedUser.RoleName, "Pharmacist", StringComparison.OrdinalIgnoreCase))
            {
                var pharInfo = _pharmacyRepo.GetPharmacistByUserId(authenticatedUser.UserId);
                if (pharInfo.HasValue)
                {
                    UserSession.SetActivePharmacist(pharInfo.Value.PharmacistId, pharInfo.Value.PharmacistName, pharInfo.Value.LicenseNumber);
                }

                Hide();
                using (PharmacistDashboardForm pharPortal = new())
                {
                    pharPortal.ShowDialog();
                }
                txtPassword.Clear();
                Show();
                return;
            }

            // Admin & other staff roles proceed to the Main Navigation Hub
            Hide();
            using (MainDashboardForm dashboardForm = new())
            {
                dashboardForm.ShowDialog();
            }
            txtPassword.Clear();
            Show();
        }
        catch (SqlException)
        {
            lblStatus.Text = "The database is currently unavailable. Please try again.";
            lblStatus.ForeColor = Color.Firebrick;
            lblStatus.Visible = true;
        }
        finally
        {
            btnLogin.Enabled = true;
        }
    }
}