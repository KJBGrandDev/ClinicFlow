#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Forms
{
    partial class DoctorDashboardForm
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel _sidebarPanel = null!;
        private Label _lblClinicFlowBrand = null!;
        private Button _btnConsultation = null!;
        private Button _btnPatientRecords = null!;
        private Button _btnSchedule = null!;
        private Button _btnNavLogout = null!;

        private Panel _topHeaderPanel = null!;
        private Label _lblWelcome = null!;
        private Label _lblDoctorInfo = null!;

        private Panel _contentPanel = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this._sidebarPanel = new Panel();
            this._lblClinicFlowBrand = new Label();
            this._btnConsultation = new Button();
            this._btnPatientRecords = new Button();
            this._btnSchedule = new Button();
            this._btnNavLogout = new Button();

            this._topHeaderPanel = new Panel();
            this._lblWelcome = new Label();
            this._lblDoctorInfo = new Label();

            this._contentPanel = new Panel();

            this.SuspendLayout();

            // =========================================================================
            // SIDEBAR PANEL: NAVIGATION SHELL (LEVEL 1)
            // =========================================================================
            this._sidebarPanel.BackColor = Color.FromArgb(248, 250, 252); // Slate 50
            this._sidebarPanel.Dock = DockStyle.Left;
            this._sidebarPanel.Width = 240;
            this._sidebarPanel.Padding = new Padding(12, 16, 12, 16);
            this._sidebarPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), this._sidebarPanel.Width - 1, 0, this._sidebarPanel.Width - 1, this._sidebarPanel.Height);

            this._lblClinicFlowBrand.Text = "ClinicFlow";
            this._lblClinicFlowBrand.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this._lblClinicFlowBrand.ForeColor = Color.FromArgb(15, 23, 42); // Slate 900
            this._lblClinicFlowBrand.Dock = DockStyle.Top;
            this._lblClinicFlowBrand.Height = 50;
            this._lblClinicFlowBrand.TextAlign = ContentAlignment.MiddleLeft;
            this._lblClinicFlowBrand.Padding = new Padding(10, 0, 0, 0);

            var lblCategory = new Label
            {
                Text = "CLINICAL WORKSPACE",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 6, 0, 0)
            };

            this._sidebarPanel.Controls.Add(this._btnNavLogout);
            this._sidebarPanel.Controls.Add(this._btnSchedule);
            this._sidebarPanel.Controls.Add(this._btnPatientRecords);
            this._sidebarPanel.Controls.Add(this._btnConsultation);
            this._sidebarPanel.Controls.Add(lblCategory);
            this._sidebarPanel.Controls.Add(this._lblClinicFlowBrand);

            // =========================================================================
            // TOP HEADER PANEL: BREADCRUMB & DOCTOR CREDENTIALS BAR
            // =========================================================================
            this._topHeaderPanel.Dock = DockStyle.Top;
            this._topHeaderPanel.Height = 54;
            this._topHeaderPanel.BackColor = Color.White;
            this._topHeaderPanel.Padding = new Padding(24, 0, 24, 0);
            this._topHeaderPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), 0, this._topHeaderPanel.Height - 1, this._topHeaderPanel.Width, this._topHeaderPanel.Height - 1);

            this._lblWelcome.Dock = DockStyle.Left;
            this._lblWelcome.AutoSize = true;
            this._lblWelcome.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this._lblWelcome.ForeColor = Color.FromArgb(15, 23, 42);
            this._lblWelcome.TextAlign = ContentAlignment.MiddleLeft;
            this._lblWelcome.Padding = new Padding(0, 16, 0, 0);
            this._lblWelcome.Text = "Doctor Portal  ›  Active Consultation";

            this._lblDoctorInfo.Dock = DockStyle.Right;
            this._lblDoctorInfo.AutoSize = true;
            this._lblDoctorInfo.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this._lblDoctorInfo.ForeColor = Color.FromArgb(71, 85, 105);
            this._lblDoctorInfo.TextAlign = ContentAlignment.MiddleRight;
            this._lblDoctorInfo.Padding = new Padding(0, 18, 0, 0);
            this._lblDoctorInfo.Text = "Attending Physician";

            this._topHeaderPanel.Controls.Add(this._lblDoctorInfo);
            this._topHeaderPanel.Controls.Add(this._lblWelcome);

            // =========================================================================
            // CONTENT PANEL: HOST FOR EMBEDDED CLINICAL MINI DASHBOARDS
            // =========================================================================
            this._contentPanel.Dock = DockStyle.Fill;
            this._contentPanel.BackColor = Color.FromArgb(248, 250, 252);
            this._contentPanel.Padding = Padding.Empty;

            // DoctorDashboardForm
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1366, 850);
            this.WindowState = FormWindowState.Maximized;
            this.Controls.Add(this._contentPanel);
            this.Controls.Add(this._topHeaderPanel);
            this.Controls.Add(this._sidebarPanel);
            this.Name = "DoctorDashboardForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "ClinicFlow - Doctor Clinical Portal";

            this.ResumeLayout(false);
        }

        #endregion
    }
}
