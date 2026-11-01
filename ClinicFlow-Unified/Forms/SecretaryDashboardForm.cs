#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public partial class SecretaryDashboardForm : Form
    {
        private Button? _currentActiveButton;

        public SecretaryDashboardForm()
        {
            InitializeComponent();
            ClinicTheme.ApplyForm(this);
            InitializeSidebar();
        }

        private void InitializeSidebar()
        {
            Button[] navButtons = { _btnNavOverview, _btnNavPatients, _btnNavAppointments, _btnNavBilling };

            foreach (var btn in navButtons)
            {
                btn.Dock = DockStyle.Top;
                btn.Height = 46;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(71, 85, 105); // Slate 600
                btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(14, 0, 0, 0);
                btn.Cursor = Cursors.Hand;

                btn.MouseEnter += (s, e) =>
                {
                    if (btn != _currentActiveButton)
                    {
                        btn.BackColor = Color.FromArgb(241, 245, 249); // Slate 100
                        btn.ForeColor = Color.FromArgb(15, 23, 42);
                    }
                };
                btn.MouseLeave += (s, e) =>
                {
                    if (btn != _currentActiveButton)
                    {
                        btn.BackColor = Color.Transparent;
                        btn.ForeColor = Color.FromArgb(71, 85, 105);
                    }
                };
            }

            _btnNavOverview.Text = "📊 Overview";
            _btnNavPatients.Text = "👥 Patients";
            _btnNavAppointments.Text = "📅 Appointments & Queue";
            _btnNavBilling.Text = "💳 Billing & Checkout";

            _btnNavLogout.Text = "🚪 Log Out";
            _btnNavLogout.Dock = DockStyle.Bottom;
            _btnNavLogout.Height = 44;
            _btnNavLogout.FlatStyle = FlatStyle.Flat;
            _btnNavLogout.FlatAppearance.BorderSize = 0;
            _btnNavLogout.BackColor = Color.Transparent;
            _btnNavLogout.ForeColor = Color.FromArgb(220, 38, 38); // Red 600
            _btnNavLogout.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _btnNavLogout.TextAlign = ContentAlignment.MiddleLeft;
            _btnNavLogout.Padding = new Padding(14, 0, 0, 0);
            _btnNavLogout.Cursor = Cursors.Hand;
            _btnNavLogout.MouseEnter += (s, e) => { _btnNavLogout.BackColor = Color.FromArgb(254, 242, 242); };
            _btnNavLogout.MouseLeave += (s, e) => { _btnNavLogout.BackColor = Color.Transparent; };

            _btnNavOverview.Click += (_, _) => NavigateToOverview();
            _btnNavPatients.Click += (_, _) => NavigateToPatients();
            _btnNavAppointments.Click += (_, _) => NavigateToAppointments();
            _btnNavLogout.Click += (_, _) =>
            {
                if (UserSession.IsAuthenticated && UserSession.UserId.HasValue)
                {
                    AuditLogger.Log("LOGOUT", "Users", UserSession.UserId.Value, $"User '{UserSession.Username ?? UserSession.FullName}' signed out.");
                }
                UserSession.Clear();
                this.Close();
            };

            // Set secretary credential label in top bar
            string secName = !string.IsNullOrWhiteSpace(UserSession.FullName) ? UserSession.FullName : "Secretary Staff";
            string station = !string.IsNullOrWhiteSpace(UserSession.SecretaryDesk) ? $"  |  {UserSession.SecretaryDesk}" : "  |  Reception Desk";
            _lblSecretaryInfo.Text = $"{secName}{station}";

            // Default to Overview mini dashboard
            NavigateToOverview();
        }

        public void NavigateToOverview() => SelectNav(_btnNavOverview, "Front-Desk Overview", () => new SecretaryOverviewForm(this));
        public void NavigateToPatients() => SelectNav(_btnNavPatients, "Patients Directory", () => new PatientForm());
        public void NavigateToAppointments() => SelectNav(_btnNavAppointments, "Appointments & Clinical Queue", () => new AppointmentForm());
        public void NavigateToBilling() => SelectNav(_btnNavBilling, "Billing & Cashier", () => new BillingForm());

        private void SelectNav(Button activeBtn, string breadcrumbTitle, Func<Form> createForm)
        {
            _currentActiveButton = activeBtn;
            Button[] navButtons = { _btnNavOverview, _btnNavPatients, _btnNavAppointments, _btnNavBilling };
            foreach (var b in navButtons)
            {
                if (b == activeBtn)
                {
                    b.BackColor = Color.FromArgb(239, 246, 255); // Blue 50
                    b.ForeColor = Color.FromArgb(37, 99, 235); // Blue 600
                }
                else
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(71, 85, 105);
                }
            }

            _lblWelcome.Text = $"Secretary Portal  ›  {breadcrumbTitle}";
            LoadModule(createForm());
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
    }
}

