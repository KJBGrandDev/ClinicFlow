using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ClinicFlow_Unified.Forms;

partial class InitialSetupForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblHeading = null;
    private Label lblInstructions = null;
    private Label lblFullName = null;
    private Label lblUsername = null;
    private Label lblPassword = null;
    private Label lblPasswordHint = null;
    private TextBox txtFullName = null;
    private TextBox txtUsername = null;
    private TextBox txtPassword = null;
    private Button btnCreateAdmin = null;

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
        lblHeading = new Label();
        lblInstructions = new Label();
        lblFullName = new Label();
        lblUsername = new Label();
        lblPassword = new Label();
        lblPasswordHint = new Label();
        txtFullName = new TextBox();
        txtUsername = new TextBox();
        txtPassword = new TextBox();
        btnCreateAdmin = new Button();

        SuspendLayout();

        // lblHeading
        lblHeading.AutoSize = true;
        lblHeading.Font = new System.Drawing.Font(
            "Segoe UI",
            16F,
            System.Drawing.FontStyle.Bold,
            System.Drawing.GraphicsUnit.Point);
        lblHeading.Location = new Point(34, 28);
        lblHeading.Name = "lblHeading";
        lblHeading.Size = new Size(258, 37);
        lblHeading.TabIndex = 0;
        lblHeading.Text = "Initial Administrator";

        // lblInstructions
        lblInstructions.AutoSize = true;
        lblInstructions.Location = new Point(36, 77);
        lblInstructions.Name = "lblInstructions";
        lblInstructions.Size = new Size(387, 20);
        lblInstructions.TabIndex = 1;
        lblInstructions.Text =
            "Create the first account that will manage ClinicFlow.";

        // lblFullName
        lblFullName.AutoSize = true;
        lblFullName.Location = new Point(38, 124);
        lblFullName.Name = "lblFullName";
        lblFullName.Size = new Size(74, 20);
        lblFullName.TabIndex = 2;
        lblFullName.Text = "Full Name";

        // txtFullName
        txtFullName.Location = new Point(38, 148);
        txtFullName.MaxLength = 150;
        txtFullName.Name = "txtFullName";
        txtFullName.Size = new Size(400, 27);
        txtFullName.TabIndex = 3;

        // lblUsername
        lblUsername.AutoSize = true;
        lblUsername.Location = new Point(38, 194);
        lblUsername.Name = "lblUsername";
        lblUsername.Size = new Size(75, 20);
        lblUsername.TabIndex = 4;
        lblUsername.Text = "Username";

        // txtUsername
        txtUsername.Location = new Point(38, 218);
        txtUsername.MaxLength = 50;
        txtUsername.Name = "txtUsername";
        txtUsername.Size = new Size(400, 27);
        txtUsername.TabIndex = 5;

        // lblPassword
        lblPassword.AutoSize = true;
        lblPassword.Location = new Point(38, 264);
        lblPassword.Name = "lblPassword";
        lblPassword.Size = new Size(70, 20);
        lblPassword.TabIndex = 6;
        lblPassword.Text = "Password";

        // txtPassword
        txtPassword.Location = new Point(38, 288);
        txtPassword.MaxLength = 128;
        txtPassword.Name = "txtPassword";
        txtPassword.Size = new Size(400, 27);
        txtPassword.TabIndex = 7;
        txtPassword.UseSystemPasswordChar = true;

        // lblPasswordHint
        lblPasswordHint.AutoSize = true;
        lblPasswordHint.ForeColor = Color.DimGray;
        lblPasswordHint.Location = new Point(38, 322);
        lblPasswordHint.Name = "lblPasswordHint";
        lblPasswordHint.Size = new Size(214, 20);
        lblPasswordHint.TabIndex = 8;
        lblPasswordHint.Text = "Use at least 12 characters.";

        // btnCreateAdmin
        btnCreateAdmin.Location = new Point(282, 350);
        btnCreateAdmin.Name = "btnCreateAdmin";
        btnCreateAdmin.Size = new Size(156, 36);
        btnCreateAdmin.TabIndex = 9;
        btnCreateAdmin.Text = "Create Admin";
        btnCreateAdmin.UseVisualStyleBackColor = true;
        btnCreateAdmin.Click += btnCreateAdmin_Click;

        // InitialSetupForm
        this.AcceptButton = btnCreateAdmin;
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(480, 415);
        this.Controls.Add(btnCreateAdmin);
        this.Controls.Add(lblPasswordHint);
        this.Controls.Add(txtPassword);
        this.Controls.Add(lblPassword);
        this.Controls.Add(txtUsername);
        this.Controls.Add(lblUsername);
        this.Controls.Add(txtFullName);
        this.Controls.Add(lblFullName);
        this.Controls.Add(lblInstructions);
        this.Controls.Add(lblHeading);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "InitialSetupForm";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "ClinicFlow - Initial Administrator Setup";

        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}