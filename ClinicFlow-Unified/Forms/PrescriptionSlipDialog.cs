#nullable enable

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;

namespace ClinicFlow_Unified.Forms;

public sealed class PrescriptionSlipDialog : Form
{
    private readonly PrescriptionSlipData _rx;
    private readonly Panel _cardPanel;
    private readonly Button _btnPrint;
    private readonly Button _btnClose;

    public PrescriptionSlipDialog(PrescriptionSlipData rx)
    {
        _rx = rx ?? throw new ArgumentNullException(nameof(rx));

        Text = $"Official Prescription Slip (Rx) — {_rx.PrescriptionNumber}";
        Size = new Size(720, 920);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(241, 245, 249); // Slate-100

        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.White,
            Padding = new Padding(20, 10, 20, 10)
        };
        topBar.Paint += (s, e) =>
            e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);

        var lblTop = new Label
        {
            Text = "📋 Official Physician Prescription Slip (Rx)",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Dock = DockStyle.Left,
            AutoSize = true
        };

        var pnlActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        _btnClose = new Button
        {
            Text = "Close",
            Size = new Size(88, 36),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(71, 85, 105),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        _btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        _btnClose.Click += (_, _) => Close();

        _btnPrint = new Button
        {
            Text = "🖨️ Print Rx Slip",
            Size = new Size(130, 36),
            BackColor = Color.FromArgb(37, 99, 235), // Blue-600
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };
        _btnPrint.FlatAppearance.BorderSize = 0;
        _btnPrint.Click += BtnPrint_Click;

        pnlActions.Controls.Add(_btnClose);
        pnlActions.Controls.Add(_btnPrint);

        topBar.Controls.Add(lblTop);
        topBar.Controls.Add(pnlActions);

        var scrollContainer = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(24, 20, 24, 20)
        };

        _cardPanel = new Panel
        {
            Width = 630,
            Height = 820,
            BackColor = Color.White,
            Location = new Point(25, 16)
        };
        _cardPanel.Paint += DrawPrescriptionSlip;

        scrollContainer.Controls.Add(_cardPanel);

        Controls.Add(scrollContainer);
        Controls.Add(topBar);
    }

    private void DrawPrescriptionSlip(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int width = _cardPanel.Width;
        int height = _cardPanel.Height;

        // Border
        using (var borderPen = new Pen(Color.FromArgb(203, 213, 225), 1))
        {
            g.DrawRectangle(borderPen, 0, 0, width - 1, height - 1);
        }

        // Inner decorative Rx frame
        using (var innerPen = new Pen(Color.FromArgb(226, 232, 240), 1))
        {
            g.DrawRectangle(innerPen, 8, 8, width - 17, height - 17);
        }

        float y = 24;

        // Clinic Header
        using var fontClinic = new Font("Segoe UI", 12F, FontStyle.Bold);
        using var fontSub = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        using var fontDoctorName = new Font("Segoe UI", 13F, FontStyle.Bold);
        using var fontDocCreds = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        using var fontPatient = new Font("Segoe UI", 9F, FontStyle.Regular);
        using var fontPatientBold = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var fontRxSymbol = new Font("Georgia", 28F, FontStyle.Bold | FontStyle.Italic);
        using var fontMedName = new Font("Segoe UI", 10F, FontStyle.Bold);
        using var fontSig = new Font("Segoe UI", 9F, FontStyle.Regular);
        using var fontBadge = new Font("Segoe UI", 8F, FontStyle.Bold);
        using var fontFooter = new Font("Segoe UI", 8F, FontStyle.Italic);

        using var brushDark = new SolidBrush(Color.FromArgb(15, 23, 42));
        using var brushMuted = new SolidBrush(Color.FromArgb(100, 116, 139));
        using var brushBlue = new SolidBrush(Color.FromArgb(37, 99, 235));
        using var brushAmber = new SolidBrush(Color.FromArgb(217, 119, 6));

        // Clinic Branding
        string clinicName = _rx.ClinicName.ToUpperInvariant();
        var sizeClinic = g.MeasureString(clinicName, fontClinic);
        g.DrawString(clinicName, fontClinic, brushDark, (width - sizeClinic.Width) / 2, y);
        y += 22;

        var sizeAddr = g.MeasureString(_rx.ClinicAddress, fontSub);
        g.DrawString(_rx.ClinicAddress, fontSub, brushMuted, (width - sizeAddr.Width) / 2, y);
        y += 16;

        var sizeCont = g.MeasureString(_rx.ClinicContact, fontSub);
        g.DrawString(_rx.ClinicContact, fontSub, brushMuted, (width - sizeCont.Width) / 2, y);
        y += 24;

        // Divider
        using (var divPen = new Pen(Color.FromArgb(203, 213, 225), 1.5f))
        {
            g.DrawLine(divPen, 24, y, width - 24, y);
        }
        y += 12;

        // Doctor Header Block
        g.DrawString(_rx.DoctorName, fontDoctorName, brushBlue, 28, y);
        y += 22;
        g.DrawString($"Specialty: {_rx.Specialty}", fontDocCreds, brushDark, 28, y);
        y += 16;
        g.DrawString($"PRC License No.: {_rx.LicenseNumber}   |   {_rx.PtrNumber}   |   {_rx.S2Number}", fontDocCreds, brushMuted, 28, y);
        y += 24;

        // Patient Metadata Panel Box
        RectangleF patientBox = new RectangleF(24, y, width - 48, 68);
        using (var fillBrush = new SolidBrush(Color.FromArgb(248, 250, 252)))
        {
            g.FillRectangle(fillBrush, patientBox);
        }
        using (var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1))
        {
            g.DrawRectangle(borderPen, patientBox.X, patientBox.Y, patientBox.Width, patientBox.Height);
        }

        float py = y + 10;
        g.DrawString("Patient:", fontPatientBold, brushDark, 34, py);
        g.DrawString($"{_rx.PatientName} ({_rx.PatientNumber})", fontPatient, brushDark, 120, py);

        g.DrawString("Date:", fontPatientBold, brushDark, width - 260, py);
        g.DrawString(_rx.DatePrescribed.ToString("MMMM dd, yyyy"), fontPatient, brushDark, width - 195, py);

        py += 24;
        g.DrawString("Age / Sex:", fontPatientBold, brushDark, 34, py);
        g.DrawString($"{_rx.Age} yrs  /  {_rx.Sex}", fontPatient, brushDark, 120, py);

        g.DrawString("Rx No.:", fontPatientBold, brushDark, width - 260, py);
        g.DrawString(_rx.PrescriptionNumber, fontPatient, brushBlue, width - 195, py);

        y += 82;

        // Rx Symbol
        g.DrawString("℞", fontRxSymbol, brushBlue, 28, y);
        y += 42;

        // Medicine Items
        if (_rx.Items.Count == 0)
        {
            g.DrawString("No prescription items recorded.", fontSig, brushMuted, 36, y);
            y += 30;
        }
        else
        {
            int index = 1;
            foreach (var item in _rx.Items)
            {
                string title = string.IsNullOrWhiteSpace(item.BrandName)
                    ? $"{index}.  {item.GenericName} ({item.Strength} {item.DosageForm})"
                    : $"{index}.  {item.GenericName} [{item.BrandName}] ({item.Strength} {item.DosageForm})";

                g.DrawString(title, fontMedName, brushDark, 36, y);

                // Quantity on the right
                string qtyText = $"# {item.Quantity} {item.DosageForm}";
                var sizeQty = g.MeasureString(qtyText, fontMedName);
                g.DrawString(qtyText, fontMedName, brushBlue, width - 40 - sizeQty.Width, y);
                y += 20;

                // Instructions (Sig:)
                g.DrawString($"     Sig: {item.Instructions}", fontSig, brushDark, 36, y);
                y += 18;

                // If external/unavailable, badge it clearly
                if (item.IsExternalUnavailable)
                {
                    RectangleF badgeRect = new RectangleF(60, y, 380, 20);
                    using (var bBrush = new SolidBrush(Color.FromArgb(254, 243, 199))) // Amber-100
                    using (var bPen = new Pen(Color.FromArgb(251, 191, 36))) // Amber-400
                    {
                        g.FillRectangle(bBrush, badgeRect);
                        g.DrawRectangle(bPen, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height);
                    }
                    g.DrawString("⚠️ Out of Clinic Stock — Approved for External Pharmacy Purchase", fontBadge, brushAmber, 64, y + 2);
                    y += 26;
                }
                else
                {
                    y += 10;
                }

                index++;
            }
        }

        // Bottom Signature Section (Fixed to bottom)
        float sigY = height - 160;

        using (var divPen = new Pen(Color.FromArgb(226, 232, 240), 1))
        {
            g.DrawLine(divPen, 24, sigY - 15, width - 24, sigY - 15);
        }

        // Left note
        RectangleF noteBox = new RectangleF(32, sigY, 260, 90);
        g.DrawString("NOTE:\nThis is an official physician prescription issued by ClinicFlow Davao Central Facility.\nValid across all licensed retail pharmacies nationwide.", fontSub, brushMuted, noteBox);

        // Right Doctor Signature Line
        float sigLineX = width - 280;
        using (var sigPen = new Pen(Color.FromArgb(15, 23, 42), 1.5f))
        {
            g.DrawLine(sigPen, sigLineX, sigY + 50, width - 36, sigY + 50);
        }

        g.DrawString(_rx.DoctorName, fontPatientBold, brushDark, sigLineX + 15, sigY + 55);
        g.DrawString($"PRC Lic. No. {_rx.LicenseNumber}", fontDocCreds, brushMuted, sigLineX + 15, sigY + 73);
        g.DrawString(_rx.PtrNumber, fontDocCreds, brushMuted, sigLineX + 15, sigY + 89);

        // Footer timestamp
        string footerNotice = "ClinicFlow Unified • Davao City Health System • Dispensing & Out-of-Stock Fulfillment";
        var sizeFoot = g.MeasureString(footerNotice, fontFooter);
        g.DrawString(footerNotice, fontFooter, brushMuted, (width - sizeFoot.Width) / 2, height - 26);
    }

    private void BtnPrint_Click(object? sender, EventArgs e)
    {
        try
        {
            using var printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            printDoc.PrintPage += (s, ev) =>
            {
                if (ev.Graphics == null) return;
                using var bmp = new Bitmap(_cardPanel.Width, _cardPanel.Height);
                _cardPanel.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                ev.Graphics.DrawImage(bmp, 80, 80);
                ev.HasMorePages = false;
            };

            using var preview = new PrintPreviewDialog
            {
                Document = printDoc,
                Width = 800,
                Height = 900,
                StartPosition = FormStartPosition.CenterParent
            };
            preview.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Printing error: " + ex.Message, "Print Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

