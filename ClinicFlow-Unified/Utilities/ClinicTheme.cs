#nullable enable

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace ClinicFlow_Unified.Utilities
{
    public static class ClinicTheme
    {
        // 1. Color Palette with Medical Intent
        public static readonly Color FormBackground = Color.FromArgb(248, 250, 252); // Slate 50
        public static readonly Color CardBackground = Color.White; // Pure White
        public static readonly Color BorderSubtle = Color.FromArgb(226, 232, 240); // Slate 200
        
        public static readonly Color PrimaryCta = Color.FromArgb(37, 99, 235); // Royal Medical Blue 600
        public static readonly Color SuccessCta = Color.FromArgb(16, 185, 129); // Emerald 600
        public static readonly Color AttentionCta = Color.FromArgb(245, 158, 11); // Amber 500
        public static readonly Color DangerCta = Color.FromArgb(220, 38, 38); // Red 600
        
        public static readonly Color TextHeading = Color.FromArgb(15, 23, 42); // Slate 900
        public static readonly Color TextPrimary = Color.FromArgb(30, 41, 59); // Slate 800
        public static readonly Color TextSecondary = Color.FromArgb(71, 85, 105); // Slate 600

        public static readonly Color GridSelectionBg = Color.FromArgb(239, 246, 255); // Soft Blue
        public static readonly Color GridSelectionFg = TextHeading;
        public static readonly Color GridHeaderBg = Color.FromArgb(248, 250, 252);
        
        // 2. Typography
        public static readonly Font FontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font FontSectionHeader = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontInput = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font FontGridRow = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        public static readonly Font FontBadge = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font FontLabel = new Font("Segoe UI", 9.5F, FontStyle.Regular); // Labels default

        // 3. Helper Apply Methods
        public static void ApplyForm(Form form)
        {
            form.BackColor = FormBackground;
            form.Font = FontInput;
            form.ForeColor = TextPrimary;
            form.UseWaitCursor = false;
            form.Cursor = Cursors.Default;
            
            // Fitts's Law defaults or general layout setups could be placed here if needed.
        }

        public static void ApplyPrimaryAction(Button button)
        {
            button.BackColor = PrimaryCta;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Height = 46;
        }
        
        public static void ApplySuccessAction(Button button)
        {
            button.BackColor = SuccessCta;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Height = 46;
        }

        public static void ApplyWarningAction(Button button)
        {
            button.BackColor = AttentionCta;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Height = 40;
        }

        public static void ApplyDangerAction(Button button)
        {
            button.BackColor = DangerCta;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Height = 40;
        }

        public static void ApplyOutlinedButton(Button button)
        {
            button.BackColor = CardBackground;
            button.ForeColor = TextPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = BorderSubtle;
            button.FlatAppearance.BorderSize = 1;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            button.Cursor = Cursors.Hand;
            button.Height = 40;
        }

        public static void ApplyGrid(DataGridView grid, int rowHeight = 34)
        {
            grid.BackgroundColor = CardBackground;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.RowTemplate.Height = Math.Max(rowHeight, 34); // Minimum 34px
            grid.Font = FontGridRow;
            grid.Cursor = Cursors.Default;
            grid.UseWaitCursor = false;

            // Enable DoubleBuffered to eliminate flickering
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                grid,
                new object[] { true });

            grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderBg;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextSecondary;
            grid.ColumnHeadersDefaultCellStyle.Font = FontSectionHeader;

            grid.DefaultCellStyle.BackColor = CardBackground;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = GridSelectionBg;
            grid.DefaultCellStyle.SelectionForeColor = GridSelectionFg;
        }

        public static void SetupMnemonicSafeguard(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Label lbl)
                {
                    if (lbl.Text.Contains("&")) lbl.UseMnemonic = false;
                }
                else if (control is Button btn)
                {
                    if (btn.Text.Contains("&")) btn.UseMnemonic = false;
                }
                else if (control is CheckBox cb)
                {
                    if (cb.Text.Contains("&")) cb.UseMnemonic = false;
                }
                else if (control is RadioButton rb)
                {
                    if (rb.Text.Contains("&")) rb.UseMnemonic = false;
                }

                if (control.HasChildren)
                {
                    SetupMnemonicSafeguard(control);
                }
            }
        }
        
        // Aliases for compatibility
        public static void ApplyPrimaryButton(Button button) => ApplyPrimaryAction(button);
        public static void ApplyWarningButton(Button button) => ApplyWarningAction(button);
        public static void ApplyDangerButton(Button button) => ApplyDangerAction(button);
        
        public static Color Background => FormBackground;
        public static Color Sidebar => Color.FromArgb(241, 245, 249);
        public static Color Success => SuccessCta;
        public static Color Danger => DangerCta;
        public static Color Border => BorderSubtle;
        public static Color DangerLight => Color.FromArgb(254, 242, 242);
        public static Color Primary => PrimaryCta;
        public static Color Warning => AttentionCta;
        public static Color WarningLight => Color.FromArgb(254, 243, 199);
        public static Color SuccessLight => Color.FromArgb(209, 250, 229);
    }
}