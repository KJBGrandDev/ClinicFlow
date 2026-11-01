namespace ClinicFlow_Unified.Forms;

partial class UserManagementForm
{
    private System.ComponentModel.IContainer components = null!;
    private Panel _topBannerPanel = null!;
    private Label _lblTitle = null!;
    private Label _lblSubtitle = null!;
    private SplitContainer _splitContainer = null!;
    private Panel _leftPanel = null!;
    private Panel _searchBarPanel = null!;
    private TextBox _txtSearch = null!;
    private ComboBox _cboRoleFilter = null!;
    private CheckBox _chkShowInactive = null!;
    private DataGridView _gridUsers = null!;
    private Label _lblUserCount = null!;
    private Panel _rightPanel = null!;
    private GroupBox _grpUserDetails = null!;
    private TableLayoutPanel _tlpInputs = null!;
    private Label _lblUsername = null!;
    private TextBox _txtUsername = null!;
    private Label _lblFullName = null!;
    private TextBox _txtFullName = null!;
    private Label _lblRole = null!;
    private ComboBox _cboRole = null!;
    private Label _lblPassword = null!;
    private TextBox _txtPassword = null!;
    private Label _lblPasswordHint = null!;
    private FlowLayoutPanel _actionFlow = null!;
    private Button _btnCreateUser = null!;
    private Button _btnUpdateUser = null!;
    private Button _btnToggleStatus = null!;
    private Button _btnResetPassword = null!;
    private Button _btnClear = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _topBannerPanel = new Panel();
        _lblTitle = new Label();
        _lblSubtitle = new Label();
        _splitContainer = new SplitContainer();
        _leftPanel = new Panel();
        _gridUsers = new DataGridView();
        _lblUserCount = new Label();
        _searchBarPanel = new Panel();
        _txtSearch = new TextBox();
        _cboRoleFilter = new ComboBox();
        _chkShowInactive = new CheckBox();
        _rightPanel = new Panel();
        _grpUserDetails = new GroupBox();
        _tlpInputs = new TableLayoutPanel();
        _lblUsername = new Label();
        _txtUsername = new TextBox();
        _lblFullName = new Label();
        _txtFullName = new TextBox();
        _lblRole = new Label();
        _cboRole = new ComboBox();
        _lblPassword = new Label();
        _txtPassword = new TextBox();
        _lblPasswordHint = new Label();
        _actionFlow = new FlowLayoutPanel();
        _btnCreateUser = new Button();
        _btnUpdateUser = new Button();
        _btnToggleStatus = new Button();
        _btnResetPassword = new Button();
        _btnClear = new Button();

        ((System.ComponentModel.ISupportInitialize)_splitContainer).BeginInit();
        _splitContainer.Panel1.SuspendLayout();
        _splitContainer.Panel2.SuspendLayout();
        _splitContainer.SuspendLayout();
        _leftPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_gridUsers).BeginInit();
        _searchBarPanel.SuspendLayout();
        _rightPanel.SuspendLayout();
        _grpUserDetails.SuspendLayout();
        _tlpInputs.SuspendLayout();
        _actionFlow.SuspendLayout();
        _topBannerPanel.SuspendLayout();
        SuspendLayout();

        // Banner Panel
        _topBannerPanel.Dock = DockStyle.Top;
        _topBannerPanel.Height = 70;
        _topBannerPanel.Padding = new Padding(20, 12, 20, 10);
        _topBannerPanel.BackColor = Color.FromArgb(248, 250, 252);
        _topBannerPanel.Controls.Add(_lblSubtitle);
        _topBannerPanel.Controls.Add(_lblTitle);

        _lblTitle.Text = "👥 User Account Management";
        _lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        _lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        _lblTitle.Dock = DockStyle.Top;
        _lblTitle.Height = 28;

        _lblSubtitle.Text = "Create, edit, assign roles, deactivate accounts, and reset credentials with administrative authorization.";
        _lblSubtitle.Font = new Font("Segoe UI", 9F);
        _lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
        _lblSubtitle.Dock = DockStyle.Top;
        _lblSubtitle.Height = 20;

        // Split Container
        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.SplitterDistance = 620;
        _splitContainer.Panel1.Controls.Add(_leftPanel);
        _splitContainer.Panel2.Controls.Add(_rightPanel);

        // Left Panel (Grid + Filters)
        _leftPanel.Dock = DockStyle.Fill;
        _leftPanel.Padding = new Padding(16);
        _leftPanel.Controls.Add(_gridUsers);
        _leftPanel.Controls.Add(_lblUserCount);
        _leftPanel.Controls.Add(_searchBarPanel);

        // Search Bar Panel
        _searchBarPanel.Dock = DockStyle.Top;
        _searchBarPanel.Height = 38;
        _searchBarPanel.Controls.Add(_chkShowInactive);
        _searchBarPanel.Controls.Add(_cboRoleFilter);
        _searchBarPanel.Controls.Add(_txtSearch);

        _txtSearch.Dock = DockStyle.Left;
        _txtSearch.Width = 240;
        _txtSearch.PlaceholderText = "Search by name or username...";
        _txtSearch.Font = new Font("Segoe UI", 9.5F);

        _cboRoleFilter.Dock = DockStyle.Left;
        _cboRoleFilter.Width = 140;
        _cboRoleFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboRoleFilter.Font = new Font("Segoe UI", 9.5F);

        _chkShowInactive.Dock = DockStyle.Right;
        _chkShowInactive.Width = 140;
        _chkShowInactive.Text = "Show Deactivated";
        _chkShowInactive.Font = new Font("Segoe UI", 9F);
        _chkShowInactive.Cursor = Cursors.Hand;

        // Grid
        _gridUsers.Dock = DockStyle.Fill;
        _gridUsers.BackgroundColor = Color.White;
        _gridUsers.BorderStyle = BorderStyle.FixedSingle;
        _gridUsers.RowHeadersVisible = false;
        _gridUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridUsers.MultiSelect = false;
        _gridUsers.ReadOnly = true;
        _gridUsers.AllowUserToAddRows = false;
        _gridUsers.AllowUserToDeleteRows = false;

        _lblUserCount.Dock = DockStyle.Bottom;
        _lblUserCount.Height = 24;
        _lblUserCount.Text = "Total Users: 0";
        _lblUserCount.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        _lblUserCount.ForeColor = Color.FromArgb(100, 116, 139);

        // Right Panel (Form & Actions)
        _rightPanel.Dock = DockStyle.Fill;
        _rightPanel.Padding = new Padding(16);
        _rightPanel.Controls.Add(_actionFlow);
        _rightPanel.Controls.Add(_grpUserDetails);

        // GroupBox
        _grpUserDetails.Dock = DockStyle.Top;
        _grpUserDetails.Height = 310;
        _grpUserDetails.Text = "User Credentials & Role Assignment";
        _grpUserDetails.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _grpUserDetails.Padding = new Padding(14);
        _grpUserDetails.Controls.Add(_tlpInputs);

        _tlpInputs.Dock = DockStyle.Fill;
        _tlpInputs.ColumnCount = 2;
        _tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        _tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
        _tlpInputs.RowCount = 5;
        _tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        _tlpInputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

        _lblUsername.Text = "Username *";
        _lblUsername.Dock = DockStyle.Fill;
        _lblUsername.TextAlign = ContentAlignment.MiddleLeft;
        _lblUsername.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

        _txtUsername.Dock = DockStyle.Fill;
        _txtUsername.Font = new Font("Segoe UI", 9.5F);

        _lblFullName.Text = "Full Name *";
        _lblFullName.Dock = DockStyle.Fill;
        _lblFullName.TextAlign = ContentAlignment.MiddleLeft;
        _lblFullName.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

        _txtFullName.Dock = DockStyle.Fill;
        _txtFullName.Font = new Font("Segoe UI", 9.5F);

        _lblRole.Text = "Assigned Role *";
        _lblRole.Dock = DockStyle.Fill;
        _lblRole.TextAlign = ContentAlignment.MiddleLeft;
        _lblRole.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

        _cboRole.Dock = DockStyle.Fill;
        _cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboRole.Font = new Font("Segoe UI", 9.5F);

        _lblPassword.Text = "Password *";
        _lblPassword.Dock = DockStyle.Fill;
        _lblPassword.TextAlign = ContentAlignment.MiddleLeft;
        _lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

        _txtPassword.Dock = DockStyle.Fill;
        _txtPassword.UseSystemPasswordChar = true;
        _txtPassword.Font = new Font("Segoe UI", 9.5F);

        _lblPasswordHint.Text = "(Min 6 characters, salted PBKDF2 hash)";
        _lblPasswordHint.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
        _lblPasswordHint.ForeColor = Color.FromArgb(100, 116, 139);
        _lblPasswordHint.Dock = DockStyle.Fill;

        _tlpInputs.Controls.Add(_lblUsername, 0, 0);
        _tlpInputs.Controls.Add(_txtUsername, 1, 0);
        _tlpInputs.Controls.Add(_lblFullName, 0, 1);
        _tlpInputs.Controls.Add(_txtFullName, 1, 1);
        _tlpInputs.Controls.Add(_lblRole, 0, 2);
        _tlpInputs.Controls.Add(_cboRole, 1, 2);
        _tlpInputs.Controls.Add(_lblPassword, 0, 3);
        _tlpInputs.Controls.Add(_txtPassword, 1, 3);
        _tlpInputs.Controls.Add(_lblPasswordHint, 1, 4);

        // Action Buttons Bar
        _actionFlow.Dock = DockStyle.Top;
        _actionFlow.Height = 110;
        _actionFlow.Padding = new Padding(0, 12, 0, 0);
        _actionFlow.Controls.Add(_btnCreateUser);
        _actionFlow.Controls.Add(_btnUpdateUser);
        _actionFlow.Controls.Add(_btnToggleStatus);
        _actionFlow.Controls.Add(_btnResetPassword);
        _actionFlow.Controls.Add(_btnClear);

        _btnCreateUser.Size = new Size(135, 38);
        _btnCreateUser.Text = "➕ Add User";
        _btnCreateUser.Cursor = Cursors.Hand;

        _btnUpdateUser.Size = new Size(130, 38);
        _btnUpdateUser.Text = "✏ Update";
        _btnUpdateUser.Enabled = false;
        _btnUpdateUser.Cursor = Cursors.Default;

        _btnToggleStatus.Size = new Size(140, 38);
        _btnToggleStatus.Text = "🚫 Deactivate";
        _btnToggleStatus.Enabled = false;
        _btnToggleStatus.Cursor = Cursors.Default;

        _btnResetPassword.Size = new Size(145, 38);
        _btnResetPassword.Text = "🔒 Reset Password";
        _btnResetPassword.Enabled = false;
        _btnResetPassword.Cursor = Cursors.Default;

        _btnClear.Size = new Size(110, 38);
        _btnClear.Text = "Clear Fields";
        _btnClear.Cursor = Cursors.Hand;

        // Form
        ClientSize = new Size(1100, 680);
        Controls.Add(_splitContainer);
        Controls.Add(_topBannerPanel);
        Text = "User Account Management";

        _splitContainer.Panel1.ResumeLayout(false);
        _splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_splitContainer).EndInit();
        _splitContainer.ResumeLayout(false);
        _leftPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_gridUsers).EndInit();
        _searchBarPanel.ResumeLayout(false);
        _searchBarPanel.PerformLayout();
        _rightPanel.ResumeLayout(false);
        _grpUserDetails.ResumeLayout(false);
        _tlpInputs.ResumeLayout(false);
        _tlpInputs.PerformLayout();
        _actionFlow.ResumeLayout(false);
        _topBannerPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
