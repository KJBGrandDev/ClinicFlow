#nullable enable

using System.Drawing;
using System.Windows.Forms;

namespace ClinicFlow_Unified.Utilities
{
    public static class ClinicTheme
    {
        // Surfaces & Backgrounds
        public static readonly Color FormBackground = Color.FromArgb(248, 250, 252);     // #F8FAFC
        public static readonly Color CardBackground = Color.White;
        public static readonly Color BorderColor    = Color.FromArgb(203, 213, 225);     // #CBD5E1

        // Typography
        public static readonly Color TextPrimary   = Color.FromArgb(15, 23, 42);         // #0F172A
        public static readonly Color TextBody      = Color.FromArgb(30, 41, 59);         // #1E293B
        public static readonly Color TextLabel     = Color.FromArgb(71, 85, 105);        // #475569
        public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);      // #64748B

        // Action Buttons
        public static readonly Color PrimaryCta    = Color.FromArgb(16, 185, 129);       // #10B981
        public static readonly Color WarningAction = Color.FromArgb(245, 158, 11);       // #F59E0B
        public static readonly Color DangerAction  = Color.FromArgb(239, 68, 68);        // #EF4444

        // DataGridView (Row & Header Uniformity)
        public static readonly Color GridHeaderBg    = Color.FromArgb(241, 245, 249);    // #F1F5F9
        public static readonly Color GridHeaderFg    = Color.FromArgb(71, 85, 105);      // #475569
        public static readonly Color GridRowBg       = Color.White;
        public static readonly Color GridRowFg       = Color.FromArgb(30, 41, 59);      // #1E293B
        public static readonly Color GridSelectionBg = Color.FromArgb(226, 232, 240);    // #E2E8F0 (Soft selection)
        public static readonly Color GridSelectionFg = Color.FromArgb(15, 23, 42);       // #0F172A

        public static void ApplyForm(Form form)
        {
            form.BackColor = FormBackground;
            form.ForeColor = TextBody;
            form.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            form.UseWaitCursor = false;
            form.Cursor = Cursors.Default;
        }

        public static void ApplyPrimaryButton(Button btn, bool small = false)
        {
            btn.BackColor = PrimaryCta;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", small ? 9F : 9.5F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyOutlinedButton(Button btn)
        {
            btn.BackColor = Color.White;
            btn.ForeColor = TextBody;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyOutlinedButtons(params Button[] buttons)
        {
            foreach (var b in buttons) if (b != null) ApplyOutlinedButton(b);
        }

        public static void ApplyGrid(DataGridView grid, int rowHeight = 34)
        {
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.RowTemplate.Height = rowHeight;
            grid.Font = new Font("Segoe UI", 9F);
            grid.Cursor = Cursors.Default;
            grid.UseWaitCursor = false;

            grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = GridHeaderFg;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderBg;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = GridHeaderFg;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            grid.DefaultCellStyle.BackColor = GridRowBg;
            grid.DefaultCellStyle.ForeColor = GridRowFg;
            grid.DefaultCellStyle.SelectionBackColor = GridSelectionBg;
            grid.DefaultCellStyle.SelectionForeColor = GridSelectionFg;
        }
    }
}
