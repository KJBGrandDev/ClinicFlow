namespace ClinicFlow_Unified.Forms;

partial class MainDashboardForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Panel _sidebarPanel;
    private System.Windows.Forms.Label _lblClinicFlowBrand;
    private System.Windows.Forms.Panel _userIdentityCard;
    private System.Windows.Forms.Label _lblUserName;
    private System.Windows.Forms.Label _lblUserRoleBadge;
    private System.Windows.Forms.Label _lblUserSessionInfo;

    private System.Windows.Forms.Button _btnNavDashboard;
    private System.Windows.Forms.Button _btnNavPatients;
    private System.Windows.Forms.Button _btnNavAppointments;
    private System.Windows.Forms.Button _btnNavDoctors;
    private System.Windows.Forms.Button _btnNavSecretaries;
    private System.Windows.Forms.Button _btnNavPharmacists;
    private System.Windows.Forms.Button _btnNavPharmacy;
    private System.Windows.Forms.Button _btnNavBilling;
    private System.Windows.Forms.Button _btnNavReports;
    private System.Windows.Forms.Button _btnNavUsers;
    private System.Windows.Forms.Button _btnNavAuditLogs;
    private System.Windows.Forms.Button _btnNavBackup;
    private System.Windows.Forms.Button _btnNavLogout;

    private System.Windows.Forms.Panel _contentPanel;

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
            this._sidebarPanel = new System.Windows.Forms.Panel();
            this._lblClinicFlowBrand = new System.Windows.Forms.Label();
            this._userIdentityCard = new System.Windows.Forms.Panel();
            this._lblUserName = new System.Windows.Forms.Label();
            this._lblUserRoleBadge = new System.Windows.Forms.Label();
            this._lblUserSessionInfo = new System.Windows.Forms.Label();

            this._btnNavDashboard = new System.Windows.Forms.Button();
            this._btnNavPatients = new System.Windows.Forms.Button();
            this._btnNavAppointments = new System.Windows.Forms.Button();
            this._btnNavDoctors = new System.Windows.Forms.Button();
            this._btnNavSecretaries = new System.Windows.Forms.Button();
            this._btnNavPharmacists = new System.Windows.Forms.Button();
            this._btnNavPharmacy = new System.Windows.Forms.Button();
            this._btnNavBilling = new System.Windows.Forms.Button();
            this._btnNavReports = new System.Windows.Forms.Button();
            this._btnNavUsers = new System.Windows.Forms.Button();
            this._btnNavAuditLogs = new System.Windows.Forms.Button();
            this._btnNavBackup = new System.Windows.Forms.Button();
            this._btnNavLogout = new System.Windows.Forms.Button();
            
            this._contentPanel = new System.Windows.Forms.Panel();

            this.SuspendLayout();

            // _sidebarPanel
            this._sidebarPanel.BackColor = System.Drawing.Color.FromArgb(248, 250, 252); 
            this._sidebarPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this._sidebarPanel.Width = 240;
            this._sidebarPanel.Padding = new System.Windows.Forms.Padding(12, 16, 12, 16);
            this._sidebarPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1), this._sidebarPanel.Width - 1, 0, this._sidebarPanel.Width - 1, this._sidebarPanel.Height);
            
            // _lblClinicFlowBrand
            this._lblClinicFlowBrand.Text = "ClinicFlow";
            this._lblClinicFlowBrand.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this._lblClinicFlowBrand.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42); // Slate 900
            this._lblClinicFlowBrand.Dock = System.Windows.Forms.DockStyle.Top;
            this._lblClinicFlowBrand.Height = 50;
            this._lblClinicFlowBrand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblClinicFlowBrand.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);

            // _userIdentityCard
            this._userIdentityCard.Dock = System.Windows.Forms.DockStyle.Top;
            this._userIdentityCard.Height = 78;
            this._userIdentityCard.BackColor = System.Drawing.Color.FromArgb(241, 245, 249); // Slate 100
            this._userIdentityCard.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this._userIdentityCard.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this._userIdentityCard.Paint += (s, e) => {
                var rect = new System.Drawing.Rectangle(0, 0, this._userIdentityCard.Width - 1, this._userIdentityCard.Height - 1);
                e.Graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.FromArgb(226, 232, 240)), rect);
            };

            // _lblUserName
            this._lblUserName.Text = "Administrator";
            this._lblUserName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._lblUserName.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this._lblUserName.Location = new System.Drawing.Point(10, 8);
            this._lblUserName.AutoSize = true;
            this._lblUserName.MaximumSize = new System.Drawing.Size(200, 20);

            // _lblUserRoleBadge
            this._lblUserRoleBadge.Text = "ADMIN";
            this._lblUserRoleBadge.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this._lblUserRoleBadge.ForeColor = System.Drawing.Color.White;
            this._lblUserRoleBadge.BackColor = System.Drawing.Color.FromArgb(79, 70, 229); // Indigo 600
            this._lblUserRoleBadge.Location = new System.Drawing.Point(10, 30);
            this._lblUserRoleBadge.AutoSize = true;
            this._lblUserRoleBadge.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);

            // _lblUserSessionInfo
            this._lblUserSessionInfo.Text = "🟢 Active Session";
            this._lblUserSessionInfo.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Regular);
            this._lblUserSessionInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this._lblUserSessionInfo.Location = new System.Drawing.Point(10, 54);
            this._lblUserSessionInfo.AutoSize = true;

            this._userIdentityCard.Controls.Add(this._lblUserName);
            this._userIdentityCard.Controls.Add(this._lblUserRoleBadge);
            this._userIdentityCard.Controls.Add(this._lblUserSessionInfo);
            
            // Add Controls in Dock order (Top items added in reverse of dock order, Bottom first)
            this._sidebarPanel.Controls.Add(this._btnNavLogout);
            this._sidebarPanel.Controls.Add(this._btnNavBackup);
            this._sidebarPanel.Controls.Add(this._btnNavAuditLogs);
            this._sidebarPanel.Controls.Add(this._btnNavUsers);
            this._sidebarPanel.Controls.Add(this._btnNavReports);
            this._sidebarPanel.Controls.Add(this._btnNavBilling);
            this._sidebarPanel.Controls.Add(this._btnNavPharmacy);
            this._sidebarPanel.Controls.Add(this._btnNavPharmacists);
            this._sidebarPanel.Controls.Add(this._btnNavSecretaries);
            this._sidebarPanel.Controls.Add(this._btnNavDoctors);
            this._sidebarPanel.Controls.Add(this._btnNavAppointments);
            this._sidebarPanel.Controls.Add(this._btnNavPatients);
            this._sidebarPanel.Controls.Add(this._btnNavDashboard);
            this._sidebarPanel.Controls.Add(this._userIdentityCard);
            this._sidebarPanel.Controls.Add(this._lblClinicFlowBrand);

            // _contentPanel
            this._contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentPanel.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this._contentPanel.Padding = new System.Windows.Forms.Padding(0);

            // MainDashboardForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.Add(this._contentPanel);
            this.Controls.Add(this._sidebarPanel);
            this.Name = "MainDashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ClinicFlow - Integrated Clinical Information System";
            this.Load += MainDashboardForm_Load;

            this.ResumeLayout(false);
        }

    #endregion
}