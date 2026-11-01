#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public partial class DoctorDashboardForm : Form
    {
        private Button? _currentActiveButton;

        public DoctorDashboardForm()
        {
            InitializeComponent();
            ClinicTheme.ApplyForm(this);
            InitializeSidebar();
        }

        private void InitializeSidebar()
        {
            Button[] navButtons = { _btnConsultation, _btnPatientRecords, _btnSchedule };
            
            foreach (var btn in navButtons)
            {
                btn.Dock = DockStyle.Top;
                btn.Height = 44;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Transparent; 
                btn.ForeColor = Color.FromArgb(71, 85, 105); // Slate 600
                btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(14, 0, 0, 0);
                btn.Cursor = Cursors.Hand;
                
                btn.MouseEnter += (s, e) => { 
                    if (btn != _currentActiveButton)
                    {
                        btn.BackColor = Color.FromArgb(241, 245, 249); // Slate 100
                        btn.ForeColor = Color.FromArgb(15, 23, 42); 
                    }
                };
                btn.MouseLeave += (s, e) => { 
                    if (btn != _currentActiveButton)
                    {
                        btn.BackColor = Color.Transparent; 
                        btn.ForeColor = Color.FromArgb(71, 85, 105); 
                    }
                };
            }

            _btnConsultation.Text = "Active Consultation";
            _btnPatientRecords.Text = "Patient Records";
            _btnSchedule.Text = "Practice Schedule";
            
            _btnNavLogout.Text = "Log Out";
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

            _btnConsultation.Click += (_, _) => SelectNav(_btnConsultation, "Active Consultation", () => new DoctorConsultationForm());
            _btnPatientRecords.Click += (_, _) => SelectNav(_btnPatientRecords, "Patient Records", () => new PatientRecordsForm());
            _btnNavLogout.Click += (_, _) =>
            {
                if (UserSession.IsAuthenticated && UserSession.UserId.HasValue)
                {
                    AuditLogger.Log("LOGOUT", "Users", UserSession.UserId.Value, $"User '{UserSession.Username ?? UserSession.FullName}' signed out.");
                }
                UserSession.Clear();
                this.Close();
            };

            // Set doctor credential label in top bar
            string docName = !string.IsNullOrWhiteSpace(UserSession.FullName) ? UserSession.FullName : "Doctor";
            string specialty = !string.IsNullOrWhiteSpace(UserSession.DoctorSpecialty) ? $" ({UserSession.DoctorSpecialty})" : "";
            _lblDoctorInfo.Text = $"Dr. {docName}{specialty}  |  Attending Physician";

            // Default to Active Consultation immediately
            SelectNav(_btnConsultation, "Active Consultation", () => new DoctorConsultationForm());
        }

        private void SelectNav(Button activeBtn, string breadcrumbTitle, Func<Form> createForm)
        {
            _currentActiveButton = activeBtn;
            Button[] navButtons = { _btnConsultation, _btnPatientRecords, _btnSchedule };
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

            _lblWelcome.Text = $"Doctor Portal  ›  {breadcrumbTitle}";
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
