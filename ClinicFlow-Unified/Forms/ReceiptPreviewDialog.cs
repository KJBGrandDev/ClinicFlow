#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public partial class ReceiptPreviewDialog : Form
    {
        private readonly ReceiptData _data;
        private Panel _pnlReceiptCard = null!;

        public ReceiptPreviewDialog(ReceiptData data)
        {
            _data = data;
            InitializeComponent();
            PopulateReceiptContent();
        }

        private void InitializeComponent()
        {
            this.Text = $"Official Receipt - {_data.ReceiptNumber}";
            this.Size = new Size(670, 880);
            this.MinimumSize = new Size(640, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(241, 245, 249); // Slate 100

            // Bottom action panel
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.White,
                Padding = new Padding(24, 12, 24, 12)
            };
            bottomPanel.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawLine(p, 0, 0, bottomPanel.Width, 0);
            };

            var btnPrint = new Button
            {
                Text = "🖨️ Print Receipt",
                Size = new Size(160, 40),
                Location = new Point(24, 12),
                BackColor = Color.FromArgb(15, 118, 110), // Medical Teal
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            var btnClose = new Button
            {
                Text = "Done / Close",
                Size = new Size(130, 40),
                Location = new Point(bottomPanel.ClientSize.Width - 130 - 24, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.Click += (_, _) => this.Close();

            bottomPanel.Resize += (s, e) =>
            {
                btnClose.Left = bottomPanel.ClientSize.Width - btnClose.Width - 24;
            };

            bottomPanel.Controls.Add(btnPrint);
            bottomPanel.Controls.Add(btnClose);

            // Scroll container
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            // Receipt Paper Card (580px width for spacious, unclipped layout)
            _pnlReceiptCard = new Panel
            {
                Width = 580,
                BackColor = Color.White,
                Padding = new Padding(24, 20, 24, 20),
                Location = new Point(20, 20)
            };
            _pnlReceiptCard.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(203, 213, 225), 1);
                e.Graphics.DrawRectangle(p, 0, 0, _pnlReceiptCard.Width - 1, _pnlReceiptCard.Height - 1);
                // Top medical teal accent bar
                using var b = new SolidBrush(Color.FromArgb(15, 118, 110));
                e.Graphics.FillRectangle(b, 0, 0, _pnlReceiptCard.Width, 5);
            };

            // Keep card centered horizontally in scroll container
            scrollPanel.Resize += (s, e) =>
            {
                int left = Math.Max(20, (scrollPanel.ClientSize.Width - _pnlReceiptCard.Width) / 2);
                _pnlReceiptCard.Left = left;
            };

            scrollPanel.Controls.Add(_pnlReceiptCard);
            this.Controls.Add(scrollPanel);
            this.Controls.Add(bottomPanel);
        }

        private void PopulateReceiptContent()
        {
            _pnlReceiptCard.Controls.Clear();
            const int cardWidth = 580;
            const int leftMargin = 24;
            const int usableWidth = cardWidth - (leftMargin * 2); // 532px
            int curY = 16;

            // 1. Clinic Header (Davao City Location)
            var lblClinicTitle = new Label
            {
                Text = "CLINICFLOW HEALTH MEDICAL CLINIC",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(usableWidth, 26),
                Location = new Point(leftMargin, curY)
            };
            _pnlReceiptCard.Controls.Add(lblClinicTitle);
            curY += 28;

            var lblClinicSub = new Label
            {
                Text = "Outpatient Consultation & Integrated Pharmacy Services\nJ.P. Laurel Ave., Bajada, Davao City, 8000 Davao del Sur\nVAT Reg. TIN: 432-876-543-000  •  Tel: (082) 228-FLOW / 0917-888-FLOW",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(usableWidth, 56),
                Location = new Point(leftMargin, curY)
            };
            _pnlReceiptCard.Controls.Add(lblClinicSub);
            curY += 62;

            _pnlReceiptCard.Controls.Add(CreateDivider(curY, usableWidth, leftMargin));
            curY += 12;

            // 2. Receipt Badge & Meta Box (Spacious 3-tier layout with guaranteed non-overlapping columns)
            var pnlMeta = new Panel
            {
                Location = new Point(leftMargin, curY),
                Size = new Size(usableWidth, 74),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlMeta.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawRectangle(p, 0, 0, pnlMeta.Width - 1, pnlMeta.Height - 1);
            };

            // Row 1: Receipt Title & OR Number
            var lblOrTitle = new Label
            {
                Text = "OFFICIAL RECEIPT",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 118, 110),
                Location = new Point(10, 8),
                Size = new Size(220, 20)
            };
            var lblOrNumber = new Label
            {
                Text = $"OR No.: {_data.ReceiptNumber}",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 118, 110),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(usableWidth - 210, 8),
                Size = new Size(200, 20)
            };

            // Row 2: Invoice Number & Transaction Date
            var lblMetaInvoice = new Label
            {
                Text = $"Inv. Ref: {_data.InvoiceNumber}",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(10, 30),
                Size = new Size(250, 18)
            };
            var lblMetaDate = new Label
            {
                Text = $"Date: {_data.PaymentDate:yyyy-MM-dd hh:mm tt}",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(usableWidth - 250, 30),
                Size = new Size(240, 18)
            };

            // Row 3: Cashier Name & Payment Method
            var lblMetaCashier = new Label
            {
                Text = $"Cashier: {_data.CashierName}",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(10, 50),
                Size = new Size(250, 18)
            };
            var lblMetaMethod = new Label
            {
                Text = $"Pay Mode: {_data.PaymentMethod}",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(usableWidth - 250, 50),
                Size = new Size(240, 18)
            };

            pnlMeta.Controls.Add(lblOrTitle);
            pnlMeta.Controls.Add(lblOrNumber);
            pnlMeta.Controls.Add(lblMetaInvoice);
            pnlMeta.Controls.Add(lblMetaDate);
            pnlMeta.Controls.Add(lblMetaCashier);
            pnlMeta.Controls.Add(lblMetaMethod);

            _pnlReceiptCard.Controls.Add(pnlMeta);
            curY += 82;

            // 3. Patient & Doctor Information Box (Dedicated 2-tier display preserving full names)
            var pnlPatDoc = new Panel
            {
                Location = new Point(leftMargin, curY),
                Size = new Size(usableWidth, 54),
                BackColor = Color.FromArgb(248, 250, 252)
            };
            pnlPatDoc.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawRectangle(p, 0, 0, pnlPatDoc.Width - 1, pnlPatDoc.Height - 1);
            };

            var lblPatientName = new Label
            {
                Text = $"Patient:  {_data.PatientName}",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(10, 7),
                Size = new Size(380, 18),
                AutoEllipsis = true
            };
            var lblPatientId = new Label
            {
                Text = $"[ID: {_data.PatientNumber}]",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(usableWidth - 140, 7),
                Size = new Size(130, 18)
            };
            var lblDoctorName = new Label
            {
                Text = $"Attending MD:  {_data.DoctorName}",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(10, 28),
                Size = new Size(340, 18),
                AutoEllipsis = true
            };
            var lblDoctorSpec = new Label
            {
                Text = $"({_data.Specialty})",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(usableWidth - 180, 28),
                Size = new Size(170, 18),
                AutoEllipsis = true
            };

            pnlPatDoc.Controls.Add(lblPatientName);
            pnlPatDoc.Controls.Add(lblPatientId);
            pnlPatDoc.Controls.Add(lblDoctorName);
            pnlPatDoc.Controls.Add(lblDoctorSpec);

            _pnlReceiptCard.Controls.Add(pnlPatDoc);
            curY += 62;

            // 4. Itemized Charges Table
            // Layout (total 532px):
            // Particulars (260px) | Type (64px) | Qty (36px) | Price (72px) | Total (72px)
            var pnlTableHdr = new Panel
            {
                Location = new Point(leftMargin, curY),
                Size = new Size(usableWidth, 26),
                BackColor = Color.FromArgb(241, 245, 249)
            };
            pnlTableHdr.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(203, 213, 225));
                e.Graphics.DrawLine(p, 0, 0, pnlTableHdr.Width, 0);
                e.Graphics.DrawLine(p, 0, pnlTableHdr.Height - 1, pnlTableHdr.Width, pnlTableHdr.Height - 1);
            };

            var lblColDesc = new Label
            {
                Text = "PARTICULARS / ITEM",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(8, 5),
                Size = new Size(260, 16)
            };
            var lblColType = new Label
            {
                Text = "TYPE",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(270, 5),
                Size = new Size(64, 16)
            };
            var lblColQty = new Label
            {
                Text = "QTY",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.TopCenter,
                Location = new Point(338, 5),
                Size = new Size(36, 16)
            };
            var lblColPrice = new Label
            {
                Text = "PRICE",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(378, 5),
                Size = new Size(72, 16)
            };
            var lblColTotal = new Label
            {
                Text = "TOTAL",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(454, 5),
                Size = new Size(72, 16)
            };

            pnlTableHdr.Controls.Add(lblColDesc);
            pnlTableHdr.Controls.Add(lblColType);
            pnlTableHdr.Controls.Add(lblColQty);
            pnlTableHdr.Controls.Add(lblColPrice);
            pnlTableHdr.Controls.Add(lblColTotal);

            _pnlReceiptCard.Controls.Add(pnlTableHdr);
            curY += 28;

            // Line items
            bool isAlt = false;
            foreach (var item in _data.Items)
            {
                var rowPnl = new Panel
                {
                    Location = new Point(leftMargin, curY),
                    Size = new Size(usableWidth, 26),
                    BackColor = isAlt ? Color.FromArgb(248, 250, 252) : Color.White
                };
                rowPnl.Paint += (s, e) =>
                {
                    using var p = new Pen(Color.FromArgb(241, 245, 249));
                    e.Graphics.DrawLine(p, 0, rowPnl.Height - 1, rowPnl.Width, rowPnl.Height - 1);
                };

                var lblDesc = new Label
                {
                    Text = item.Description,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    Location = new Point(8, 4),
                    Size = new Size(260, 18),
                    AutoEllipsis = true
                };
                var lblType = new Label
                {
                    Text = AbbreviateItemType(item.ItemType),
                    Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    TextAlign = ContentAlignment.TopCenter,
                    Location = new Point(270, 4),
                    Size = new Size(64, 18)
                };
                var lblQty = new Label
                {
                    Text = item.Quantity.ToString(),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    TextAlign = ContentAlignment.TopCenter,
                    Location = new Point(338, 4),
                    Size = new Size(36, 18)
                };
                var lblPrice = new Label
                {
                    Text = $"₱{item.UnitPrice:N2}",
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(71, 85, 105),
                    TextAlign = ContentAlignment.TopRight,
                    Location = new Point(378, 4),
                    Size = new Size(72, 18)
                };
                var lblTot = new Label
                {
                    Text = $"₱{item.TotalPrice:N2}",
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    TextAlign = ContentAlignment.TopRight,
                    Location = new Point(454, 4),
                    Size = new Size(72, 18)
                };

                rowPnl.Controls.Add(lblDesc);
                rowPnl.Controls.Add(lblType);
                rowPnl.Controls.Add(lblQty);
                rowPnl.Controls.Add(lblPrice);
                rowPnl.Controls.Add(lblTot);

                _pnlReceiptCard.Controls.Add(rowPnl);
                curY += 26;
                isAlt = !isAlt;
            }

            _pnlReceiptCard.Controls.Add(CreateDivider(curY, usableWidth, leftMargin));
            curY += 12;

            // 5. Totals & Settlement Summary
            AddSummaryRow("Gross Subtotal Charges", $"₱{_data.Subtotal:N2}", ref curY, usableWidth, leftMargin, isBold: false);

            if (_data.DiscountAmount > 0)
            {
                string discLabel = $"Disc. ({_data.DiscountType})";
                AddSummaryRow(discLabel, $"-₱{_data.DiscountAmount:N2}", ref curY, usableWidth, leftMargin, isBold: false, foreColor: Color.FromArgb(220, 38, 38));
            }

            // Net Total Due Highlight Banner
            curY += 4;
            var pnlNetBanner = new Panel
            {
                Location = new Point(leftMargin, curY),
                Size = new Size(usableWidth, 38),
                BackColor = Color.FromArgb(240, 253, 250) // Soft Teal
            };
            pnlNetBanner.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(153, 246, 228));
                e.Graphics.DrawRectangle(p, 0, 0, pnlNetBanner.Width - 1, pnlNetBanner.Height - 1);
            };

            var lblNetLabel = new Label
            {
                Text = "NET AMOUNT DUE",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 118, 110),
                Location = new Point(12, 9),
                Size = new Size(220, 20)
            };
            var lblNetValue = new Label
            {
                Text = $"₱{_data.TotalAmountDue:N2}",
                Font = new Font("Segoe UI", 13.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 118, 110),
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(usableWidth - 210, 6),
                Size = new Size(198, 26)
            };
            pnlNetBanner.Controls.Add(lblNetLabel);
            pnlNetBanner.Controls.Add(lblNetValue);

            _pnlReceiptCard.Controls.Add(pnlNetBanner);
            curY += 44;

            // Cashier Tender & Change
            AddSummaryRow($"Tendered ({_data.PaymentMethod})", $"₱{_data.AmountPaid:N2}", ref curY, usableWidth, leftMargin, isBold: false);
            AddSummaryRow("Change Returned", $"₱{_data.ChangeAmount:N2}", ref curY, usableWidth, leftMargin, isBold: true, foreColor: Color.FromArgb(5, 150, 105));

            curY += 10;
            _pnlReceiptCard.Controls.Add(CreateDivider(curY, usableWidth, leftMargin));
            curY += 14;

            // 6. Footer Notice & Official Verification Code
            var lblFootNote = new Label
            {
                Text = "OFFICIAL CLINIC CASHIER ACKNOWLEDGMENT\nPayment settlement confirmed for outpatient clinical services and pharmacy.\nPrescribed clinic medications may now be claimed at the Pharmacy dispensary.\nThank you for choosing ClinicFlow Davao!",
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(usableWidth, 60),
                Location = new Point(leftMargin, curY)
            };
            _pnlReceiptCard.Controls.Add(lblFootNote);
            curY += 64;

            var lblRefCode = new Label
            {
                Text = $"DOC REF: CF-DVO-{_data.ReceiptNumber}  •  SEC-ACK",
                Font = new Font("Consolas", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.TopCenter,
                Size = new Size(usableWidth, 18),
                Location = new Point(leftMargin, curY)
            };
            _pnlReceiptCard.Controls.Add(lblRefCode);
            curY += 26;

            // Resize panel height dynamically
            _pnlReceiptCard.Height = curY + 16;
        }

        private static string AbbreviateItemType(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return "-";
            if (type.Contains("Consult", StringComparison.OrdinalIgnoreCase)) return "Consult";
            if (type.Contains("Pharm", StringComparison.OrdinalIgnoreCase) || type.Contains("Med", StringComparison.OrdinalIgnoreCase)) return "Rx / Med";
            if (type.Contains("Lab", StringComparison.OrdinalIgnoreCase)) return "Lab";
            return type.Length > 8 ? type.Substring(0, 8) + "." : type;
        }

        private void AddSummaryRow(string label, string val, ref int curY, int usableWidth, int leftMargin, bool isBold = false, Color? foreColor = null)
        {
            var c = foreColor ?? (isBold ? Color.FromArgb(15, 23, 42) : Color.FromArgb(71, 85, 105));
            var font = new Font("Segoe UI", 9F, isBold ? FontStyle.Bold : FontStyle.Regular);

            var lblL = new Label
            {
                Text = label,
                Font = font,
                ForeColor = c,
                Size = new Size(350, 20),
                Location = new Point(leftMargin + 8, curY),
                AutoEllipsis = true
            };
            var lblR = new Label
            {
                Text = val,
                Font = font,
                ForeColor = c,
                TextAlign = ContentAlignment.TopRight,
                Size = new Size(150, 20),
                Location = new Point(leftMargin + usableWidth - 156, curY)
            };
            _pnlReceiptCard.Controls.Add(lblL);
            _pnlReceiptCard.Controls.Add(lblR);
            curY += 22;
        }

        private static Control CreateDivider(int y, int width, int leftMargin)
        {
            var pnl = new Panel
            {
                Location = new Point(leftMargin, y),
                Size = new Size(width, 1),
                BackColor = Color.FromArgb(226, 232, 240)
            };
            return pnl;
        }

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            try
            {
                var doc = new PrintDocument();
                doc.PrintPage += (s, ev) =>
                {
                    using var bmp = new Bitmap(_pnlReceiptCard.Width, _pnlReceiptCard.Height);
                    _pnlReceiptCard.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    ev.Graphics?.DrawImage(bmp, 40, 40);
                };

                using var dlg = new PrintPreviewDialog
                {
                    Document = doc,
                    Width = 850,
                    Height = 700,
                    StartPosition = FormStartPosition.CenterParent
                };
                dlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to start printing: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

