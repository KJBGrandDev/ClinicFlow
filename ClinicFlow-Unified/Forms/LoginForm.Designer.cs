namespace ClinicFlow_Unified.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Panel _mainPanel;
    private System.Windows.Forms.Panel _loginCard;
    private System.Windows.Forms.Label _lblBrand;
    private System.Windows.Forms.Label _lblSubtitle;
    private System.Windows.Forms.Label _lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label _lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Button btnQuickAdmin;

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
        this._mainPanel = new System.Windows.Forms.Panel();
        this._loginCard = new System.Windows.Forms.Panel();
        this._lblBrand = new System.Windows.Forms.Label();
        this._lblSubtitle = new System.Windows.Forms.Label();
        this._lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this._lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.btnLogin = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.btnQuickAdmin = new System.Windows.Forms.Button();

        this.SuspendLayout();

        // _mainPanel (Canvas)
        this._mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._mainPanel.BackColor = System.Drawing.Color.FromArgb(241, 245, 249); // Slate 100
        
        // _loginCard
        this._loginCard.BackColor = System.Drawing.Color.White;
        this._loginCard.Size = new System.Drawing.Size(460, 480);

        // _lblBrand
        this._lblBrand.Text = "?? ClinicFlow";
        this._lblBrand.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
        this._lblBrand.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42); // Slate 900
        this._lblBrand.Location = new System.Drawing.Point(40, 40);
        this._lblBrand.Size = new System.Drawing.Size(380, 50);
        this._lblBrand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // _lblSubtitle
        this._lblSubtitle.Text = "Secure Clinical Authentication";
        this._lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
        this._lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139); // Slate 500
        this._lblSubtitle.Location = new System.Drawing.Point(40, 95);
        this._lblSubtitle.Size = new System.Drawing.Size(380, 30);
        this._lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // Username
        this._lblUsername.Text = "Staff ID / Username";
        this._lblUsername.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
        this._lblUsername.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblUsername.Location = new System.Drawing.Point(40, 150);
        this._lblUsername.Size = new System.Drawing.Size(380, 25);

        this.txtUsername.Location = new System.Drawing.Point(40, 180);
        this.txtUsername.Size = new System.Drawing.Size(380, 34);
        this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 11F);

        // Password
        this._lblPassword.Text = "Password";
        this._lblPassword.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
        this._lblPassword.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
        this._lblPassword.Location = new System.Drawing.Point(40, 230);
        this._lblPassword.Size = new System.Drawing.Size(380, 25);

        this.txtPassword.Location = new System.Drawing.Point(40, 260);
        this.txtPassword.Size = new System.Drawing.Size(380, 34);
        this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
        this.txtPassword.UseSystemPasswordChar = true;

        // Status
        this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(239, 68, 68); // Red 500
        this.lblStatus.Location = new System.Drawing.Point(40, 305);
        this.lblStatus.Size = new System.Drawing.Size(380, 25);
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // Login Button
        this.btnLogin.Text = "Secure Login";
        this.btnLogin.Location = new System.Drawing.Point(40, 340);
        this.btnLogin.Size = new System.Drawing.Size(380, 46);
        this.btnLogin.BackColor = System.Drawing.Color.FromArgb(37, 99, 235); // Blue 600
        this.btnLogin.ForeColor = System.Drawing.Color.White;
        this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
        this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnLogin.FlatAppearance.BorderSize = 0;
        this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnLogin.Click += this.btnLogin_Click;

        // Quick Admin
        this.btnQuickAdmin.Text = "Bypass (Dev Only)";
        this.btnQuickAdmin.Location = new System.Drawing.Point(40, 400);
        this.btnQuickAdmin.Size = new System.Drawing.Size(380, 36);
        this.btnQuickAdmin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnQuickAdmin.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184); // Slate 400
        this.btnQuickAdmin.FlatAppearance.BorderSize = 1;
        this.btnQuickAdmin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240); // Slate 200
        this.btnQuickAdmin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnQuickAdmin.Click += this.btnQuickAdmin_Click;

        this._loginCard.Controls.Add(this._lblBrand);
        this._loginCard.Controls.Add(this._lblSubtitle);
        this._loginCard.Controls.Add(this._lblUsername);
        this._loginCard.Controls.Add(this.txtUsername);
        this._loginCard.Controls.Add(this._lblPassword);
        this._loginCard.Controls.Add(this.txtPassword);
        this._loginCard.Controls.Add(this.lblStatus);
        this._loginCard.Controls.Add(this.btnLogin);
        this._loginCard.Controls.Add(this.btnQuickAdmin);

        this._mainPanel.Controls.Add(this._loginCard);

        // LoginForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600);
        this.WindowState = System.Windows.Forms.FormWindowState.Normal;
        this.Controls.Add(this._mainPanel);
        this.Name = "LoginForm";
        this.Text = "ClinicFlow - Secure Authentication";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        
        this.Resize += (s, e) => {
            this._loginCard.Location = new System.Drawing.Point(
                (this._mainPanel.Width - this._loginCard.Width) / 2,
                (this._mainPanel.Height - this._loginCard.Height) / 2
            );
        };
        this.Load += (s, e) => {
            this._loginCard.Location = new System.Drawing.Point(
                (this._mainPanel.Width - this._loginCard.Width) / 2,
                (this._mainPanel.Height - this._loginCard.Height) / 2
            );
        };

        this.ResumeLayout(false);
    }

    #endregion
}
