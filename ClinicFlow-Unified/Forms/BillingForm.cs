#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Forms
{
    public sealed class DisplayBillItem
    {
        public string Description { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }

    public partial class BillingForm : Form
    {
        private readonly BillingRepository _billingRepo = new();
        private List<PendingCheckoutItem> _pendingQueue = new();
        private List<PaymentHistoryItem> _settledHistory = new();

        private BillDetails? _currentBill = null;
        private decimal _currentConsultFee = 500.00m;
        private decimal _currentPharmacyTotal = 0.00m;
        private decimal _currentSubtotal = 500.00m;
        private decimal _currentDiscountAmount = 0.00m;
        private decimal _currentNetTotalDue = 500.00m;
        private decimal _previousNetTotalDue = 500.00m;

        private bool _isHistoryMode = false;
        private bool _suppressCalculation = false;
        private bool _userAdjustedSplitter = false;
        private int? _viewingSettledPaymentId = null;

        public BillingForm()
        {
            InitializeComponent();

            EnableDoubleBuffer(_gridQueue);
            EnableDoubleBuffer(_gridHistory);
            EnableDoubleBuffer(_gridBillItems);

            ClinicTheme.ApplyForm(this);
            ClinicTheme.SetupMnemonicSafeguard(this);

            ConfigureGrids();
            PopulateDropdowns();
            WireEvents();
        }

        private static void EnableDoubleBuffer(DataGridView grid)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, grid, new object[] { true });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyResponsiveLayout();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyResponsiveLayout();
            _mainSplitContainer.SplitterMoved += (_, _) => _userAdjustedSplitter = true;
            await LoadPendingQueueAsync();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!_userAdjustedSplitter)
            {
                ApplyResponsiveLayout();
            }
        }

        private void ApplyResponsiveLayout()
        {
            try
            {
                if (_mainSplitContainer.Width > 500)
                {
                    // Balance screen real estate: 52% left queue list, 48% right checkout ledger
                    int target = (int)(_mainSplitContainer.Width * 0.52);
                    int minLeft = Math.Max(300, _mainSplitContainer.Panel1MinSize);
                    int maxLeft = _mainSplitContainer.Width - Math.Max(380, _mainSplitContainer.Panel2MinSize);

                    if (maxLeft > minLeft)
                    {
                        if (target < minLeft) target = minLeft;
                        if (target > maxLeft) target = maxLeft;

                        if (target >= minLeft && target <= maxLeft)
                        {
                            _mainSplitContainer.SplitterDistance = target;
                        }
                    }
                }
            }
            catch
            {
                // Guard against transient WinForms docking calculation quirks
            }
        }

        private void PopulateDropdowns()
        {
            // Statutory discounts commonly used in Philippine clinics
            _cboDiscountType.Items.Clear();
            _cboDiscountType.Items.Add("None (0%)");
            _cboDiscountType.Items.Add("Senior Citizen (20% - RA 9994)");
            _cboDiscountType.Items.Add("Person with Disability / PWD (20% - RA 10754)");
            _cboDiscountType.Items.Add("Solo Parent (10%)");
            _cboDiscountType.SelectedIndex = 0;

            // Payment methods
            _cboPaymentMethod.Items.Clear();
            _cboPaymentMethod.Items.Add("Cash");
            _cboPaymentMethod.Items.Add("GCash");
            _cboPaymentMethod.Items.Add("Maya / E-Wallet");
            _cboPaymentMethod.Items.Add("Credit / Debit Card");
            _cboPaymentMethod.SelectedIndex = 0;
        }

        private void ConfigureGrids()
        {
            // -------------------------------------------------------------
            // 1. PENDING CHECKOUT QUEUE GRID
            // -------------------------------------------------------------
            _gridQueue.AutoGenerateColumns = false;
            _gridQueue.Columns.Clear();
            _gridQueue.EnableHeadersVisualStyles = false;
            _gridQueue.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridQueue.ColumnHeadersHeight = 34;
            _gridQueue.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _gridQueue.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridQueue.GridColor = Color.FromArgb(226, 232, 240);

            _gridQueue.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            _gridQueue.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridQueue.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            _gridQueue.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _gridQueue.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            _gridQueue.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _gridQueue.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            _gridQueue.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            _gridQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PendingCheckoutItem.AppointmentNumber),
                HeaderText = "Ticket",
                Width = 70,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) }
            });

            _gridQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PendingCheckoutItem.ScheduledTime),
                HeaderText = "Time",
                Width = 60,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F) }
            });

            _gridQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PendingCheckoutItem.PatientName),
                HeaderText = "Patient Full Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 130,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _gridQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PendingCheckoutItem.PatientNumber),
                HeaderText = "Patient #",
                Width = 85,
                DefaultCellStyle = { ForeColor = Color.FromArgb(100, 116, 139) }
            });

            _gridQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PendingCheckoutItem.DoctorName),
                HeaderText = "Attending Physician",
                Width = 125
            });

            _gridQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PendingCheckoutItem.PrescriptionStatus),
                HeaderText = "Rx Status",
                Width = 85,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F) }
            });

            _gridQueue.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PendingCheckoutItem.TotalEstimated),
                HeaderText = "Est. Total",
                Width = 90,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleRight } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Format = "₱#,##0.00" }
            });

            AttachHeaderPainter(_gridQueue);

            // -------------------------------------------------------------
            // 2. SETTLED RECEIPTS HISTORY GRID
            // -------------------------------------------------------------
            _gridHistory.AutoGenerateColumns = false;
            _gridHistory.Columns.Clear();
            _gridHistory.EnableHeadersVisualStyles = false;
            _gridHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridHistory.ColumnHeadersHeight = 34;
            _gridHistory.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _gridHistory.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridHistory.GridColor = Color.FromArgb(226, 232, 240);

            _gridHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 118, 110);
            _gridHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridHistory.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            _gridHistory.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            _gridHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            _gridHistory.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            _gridHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
            _gridHistory.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.ReceiptNumber),
                HeaderText = "Receipt (OR)",
                Width = 115,
                DefaultCellStyle = { Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(15, 118, 110) }
            });

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.InvoiceNumber),
                HeaderText = "Invoice #",
                Width = 120,
                DefaultCellStyle = { ForeColor = Color.FromArgb(100, 116, 139) }
            });

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.PatientName),
                HeaderText = "Patient Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 120,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            });

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.DoctorName),
                HeaderText = "Physician",
                Width = 125
            });

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.PaymentMethod),
                HeaderText = "Method",
                Width = 75,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8F) }
            });

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.AmountPaid),
                HeaderText = "Paid",
                Width = 85,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleRight } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Format = "₱#,##0.00" }
            });

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.PaymentDate),
                HeaderText = "Settled Date",
                Width = 135,
                DefaultCellStyle = { Font = new Font("Segoe UI", 8F), Format = "yyyy-MM-dd hh:mm tt" }
            });

            _gridHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(PaymentHistoryItem.ProcessedBy),
                HeaderText = "Cashier",
                Width = 90,
                DefaultCellStyle = { Font = new Font("Segoe UI", 8F) }
            });

            var btnCol = new DataGridViewButtonColumn
            {
                Name = "ColViewReceipt",
                HeaderText = "Action",
                Text = "👁️ View",
                UseColumnTextForButtonValue = true,
                Width = 80,
                FlatStyle = FlatStyle.Flat
            };
            btnCol.DefaultCellStyle.BackColor = Color.FromArgb(240, 253, 250);
            btnCol.DefaultCellStyle.ForeColor = Color.FromArgb(15, 118, 110);
            btnCol.DefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 118, 110);
            btnCol.DefaultCellStyle.SelectionForeColor = Color.White;
            btnCol.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _gridHistory.Columns.Add(btnCol);

            AttachHeaderPainter(_gridHistory);

            // -------------------------------------------------------------
            // 3. BILL ITEMS BREAKDOWN GRID (Right Card)
            // -------------------------------------------------------------
            _gridBillItems.AutoGenerateColumns = false;
            _gridBillItems.Columns.Clear();
            _gridBillItems.EnableHeadersVisualStyles = false;
            _gridBillItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _gridBillItems.ColumnHeadersHeight = 28;
            _gridBillItems.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _gridBillItems.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            _gridBillItems.GridColor = Color.FromArgb(241, 245, 249);

            _gridBillItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            _gridBillItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            _gridBillItems.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            _gridBillItems.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(71, 85, 105);
            _gridBillItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);

            _gridBillItems.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            _gridBillItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            _gridBillItems.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            _gridBillItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplayBillItem.Description),
                HeaderText = "Description / Medication",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 140,
                DefaultCellStyle = { Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) }
            });

            _gridBillItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplayBillItem.ItemType),
                HeaderText = "Type",
                Width = 85,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 7.5F) }
            });

            _gridBillItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplayBillItem.Quantity),
                HeaderText = "Qty",
                Width = 45,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            _gridBillItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplayBillItem.UnitPrice),
                HeaderText = "Price",
                Width = 75,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleRight } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "₱#,##0.00" }
            });

            _gridBillItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(DisplayBillItem.LineTotal),
                HeaderText = "Total",
                Width = 80,
                HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleRight } },
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Format = "₱#,##0.00" }
            });
        }

        private static void AttachHeaderPainter(DataGridView grid)
        {
            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                {
                    e.Handled = true;

                    using var bgBrush = new SolidBrush(Color.FromArgb(15, 118, 110));
                    e.Graphics?.FillRectangle(bgBrush, e.CellBounds);

                    using var borderPen = new Pen(Color.FromArgb(13, 98, 91));
                    if (e.Graphics != null)
                    {
                        e.Graphics.DrawLine(borderPen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(borderPen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);

                        string text = grid.Columns[e.ColumnIndex].HeaderText;
                        using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                        var align = grid.Columns[e.ColumnIndex].HeaderCell.Style.Alignment;

                        TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                        if (align == DataGridViewContentAlignment.MiddleCenter)
                            flags |= TextFormatFlags.HorizontalCenter;
                        else if (align == DataGridViewContentAlignment.MiddleRight)
                            flags |= TextFormatFlags.Right;
                        else
                            flags |= TextFormatFlags.Left;

                        var textRect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y, e.CellBounds.Width - 12, e.CellBounds.Height);
                        TextRenderer.DrawText(e.Graphics, text, font, textRect, Color.White, flags);
                    }
                }
            };
        }

        private void WireEvents()
        {
            // Tab switching
            _btnTabPending.Click += async (_, _) =>
            {
                _isHistoryMode = false;
                UpdateTabUi();
                await LoadPendingQueueAsync();
            };

            _btnTabHistory.Click += async (_, _) =>
            {
                _isHistoryMode = true;
                UpdateTabUi();
                await LoadPaymentHistoryAsync();
            };

            // Search and Refresh
            _btnSearch.Click += async (_, _) =>
            {
                if (_isHistoryMode)
                    await LoadPaymentHistoryAsync();
                else
                    await LoadPendingQueueAsync();
            };

            _txtSearch.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    if (_isHistoryMode)
                        await LoadPaymentHistoryAsync();
                    else
                        await LoadPendingQueueAsync();
                }
            };

            _btnRefresh.Click += async (_, _) =>
            {
                _txtSearch.Clear();
                if (_isHistoryMode)
                    await LoadPaymentHistoryAsync();
                else
                    await LoadPendingQueueAsync();
            };

            // Queue row selection
            _gridQueue.SelectionChanged += GridQueue_SelectionChanged;

            // History grid interactions
            _gridHistory.SelectionChanged += GridHistory_SelectionChanged;
            _gridHistory.CellClick += GridHistory_CellClick;
            _gridHistory.CellDoubleClick += GridHistory_CellDoubleClick;

            // Discount dropdown change
            _cboDiscountType.SelectedIndexChanged += (_, _) => RecalculateTotals();

            // Payment method change
            _cboPaymentMethod.SelectedIndexChanged += (_, _) =>
            {
                string method = _cboPaymentMethod.SelectedItem?.ToString() ?? "Cash";
                if (!method.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                {
                    // For e-wallets and card, default amount tendered = net total due
                    _txtAmountTendered.Text = _currentNetTotalDue.ToString("N2");
                }
                RecalculateChange();
            };

            // Tender input change
            _txtAmountTendered.TextChanged += (_, _) => RecalculateChange();

            // Quick cash buttons
            _btnQuickExact.Click += (_, _) => { if (!_viewingSettledPaymentId.HasValue) _txtAmountTendered.Text = _currentNetTotalDue.ToString("N2"); };
            _btnQuick500.Click += (_, _) => { if (!_viewingSettledPaymentId.HasValue) _txtAmountTendered.Text = "500.00"; };
            _btnQuick1000.Click += (_, _) => { if (!_viewingSettledPaymentId.HasValue) _txtAmountTendered.Text = "1000.00"; };
            _btnQuick2000.Click += (_, _) => { if (!_viewingSettledPaymentId.HasValue) _txtAmountTendered.Text = "2000.00"; };

            // Process payment
            _btnProcessPayment.Click += BtnProcessPayment_Click;

            // Clear
            _btnClearLedger.Click += (_, _) => ClearLedgerCard();
        }

        private void UpdateTabUi()
        {
            if (!_isHistoryMode)
            {
                // Queue Tab Active
                _btnTabPending.BackColor = Color.FromArgb(15, 118, 110);
                _btnTabPending.ForeColor = Color.White;
                _btnTabPending.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

                _btnTabHistory.BackColor = Color.White;
                _btnTabHistory.ForeColor = Color.FromArgb(71, 85, 105);
                _btnTabHistory.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

                _gridQueue.Visible = true;
                _gridHistory.Visible = false;
                _lblEmptyHistory.Visible = false;

                // Reset ledger controls to active payment mode
                _viewingSettledPaymentId = null;
                _cboDiscountType.Enabled = true;
                _cboPaymentMethod.Enabled = true;
                _txtAmountTendered.ReadOnly = false;
                _btnQuickExact.Enabled = true;
                _btnQuick500.Enabled = true;
                _btnQuick1000.Enabled = true;
                _btnQuick2000.Enabled = true;
                _txtPaymentNotes.ReadOnly = false;
                _btnClearLedger.Enabled = true;
                _btnProcessPayment.Text = "💳 Process Payment & Issue Receipt (OR)";
                _btnProcessPayment.BackColor = Color.FromArgb(15, 118, 110);

                if (_gridQueue.CurrentRow?.DataBoundItem is PendingCheckoutItem item)
                {
                    LoadBillForAppointment(item.AppointmentId);
                }
                else
                {
                    ClearLedgerCard();
                }
            }
            else
            {
                // History Tab Active
                _btnTabHistory.BackColor = Color.FromArgb(15, 118, 110);
                _btnTabHistory.ForeColor = Color.White;
                _btnTabHistory.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

                _btnTabPending.BackColor = Color.White;
                _btnTabPending.ForeColor = Color.FromArgb(71, 85, 105);
                _btnTabPending.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

                _gridQueue.Visible = false;
                _gridHistory.Visible = true;
                _lblEmptyQueue.Visible = false;

                if (_gridHistory.CurrentRow?.DataBoundItem is PaymentHistoryItem hist)
                {
                    LoadSettledReceiptToLedger(hist.PaymentId);
                }
                else
                {
                    ClearLedgerCard();
                }
            }
        }

        private async Task LoadPendingQueueAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                string search = _txtSearch.Text.Trim();
                _pendingQueue = await Task.Run(() => _billingRepo.GetPendingCheckouts(search));

                _gridQueue.DataSource = null;
                if (_pendingQueue.Count > 0)
                {
                    _gridQueue.DataSource = _pendingQueue;
                    _gridQueue.Visible = true;
                    _lblEmptyQueue.Visible = false;
                    _btnTabPending.Text = $"⚡ Pending Queue ({_pendingQueue.Count})";
                }
                else
                {
                    _gridQueue.Visible = false;
                    _lblEmptyQueue.Visible = true;
                    _btnTabPending.Text = "⚡ Pending Queue (0)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load pending checkouts: {ex.Message}", "Billing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async Task LoadPaymentHistoryAsync()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _settledHistory = await Task.Run(() => _billingRepo.GetRecentPayments(50));

                string search = _txtSearch.Text.Trim();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    _settledHistory = _settledHistory.Where(h =>
                        h.PatientName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        h.ReceiptNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        h.InvoiceNumber.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                _gridHistory.DataSource = null;
                if (_settledHistory.Count > 0)
                {
                    _gridHistory.DataSource = _settledHistory;
                    _gridHistory.Visible = true;
                    _lblEmptyHistory.Visible = false;
                    if (_gridHistory.Rows.Count > 0)
                    {
                        LoadSettledReceiptToLedger(_settledHistory[0].PaymentId);
                    }
                }
                else
                {
                    _gridHistory.Visible = false;
                    _lblEmptyHistory.Visible = true;
                    ClearLedgerCard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settled history: {ex.Message}", "Billing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void GridHistory_SelectionChanged(object? sender, EventArgs e)
        {
            if (!_isHistoryMode) return;
            if (_gridHistory.CurrentRow?.DataBoundItem is PaymentHistoryItem item)
            {
                LoadSettledReceiptToLedger(item.PaymentId);
            }
        }

        private void GridHistory_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _gridHistory.Rows.Count) return;
            if (_gridHistory.Rows[e.RowIndex].DataBoundItem is PaymentHistoryItem item)
            {
                if (e.ColumnIndex >= 0 && _gridHistory.Columns[e.ColumnIndex].Name == "ColViewReceipt")
                {
                    OpenReceiptForPayment(item.PaymentId);
                }
                else
                {
                    LoadSettledReceiptToLedger(item.PaymentId);
                }
            }
        }

        private void GridHistory_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _gridHistory.Rows.Count) return;
            if (_gridHistory.Rows[e.RowIndex].DataBoundItem is PaymentHistoryItem item)
            {
                OpenReceiptForPayment(item.PaymentId);
            }
        }

        private void OpenReceiptForPayment(int paymentId)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var data = _billingRepo.GetReceiptData(paymentId);
                if (data == null)
                {
                    MessageBox.Show("Receipt details could not be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var dlg = new ReceiptPreviewDialog(data);
                dlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to display receipt: {ex.Message}", "Receipt Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LoadSettledReceiptToLedger(int paymentId)
        {
            try
            {
                var receipt = _billingRepo.GetReceiptData(paymentId);
                if (receipt == null) return;

                _viewingSettledPaymentId = paymentId;

                _lblPatientCardName.Text = $"{receipt.PatientName} ({receipt.PatientNumber})";
                _lblPatientCardMeta.Text = $"Physician: {receipt.DoctorName}  •  Specialty: {receipt.Specialty}";
                _lblPatientCardDiagnosis.Text = $"Receipt No.: {receipt.ReceiptNumber}  •  Invoice: {receipt.InvoiceNumber}";
                _lblLedgerBadge.Text = "SETTLED & PAID";
                _lblLedgerBadge.BackColor = Color.FromArgb(209, 250, 229); // Green 100
                _lblLedgerBadge.ForeColor = Color.FromArgb(5, 150, 105); // Green 600

                var items = new List<DisplayBillItem>();
                foreach (var line in receipt.Items)
                {
                    items.Add(new DisplayBillItem
                    {
                        Description = line.Description,
                        ItemType = line.ItemType,
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice
                    });
                }

                _gridBillItems.DataSource = null;
                _gridBillItems.DataSource = items;

                _currentConsultFee = receipt.ConsultationFee;
                _currentPharmacyTotal = receipt.PharmacyTotal;
                _currentSubtotal = receipt.Subtotal;
                _currentDiscountAmount = receipt.DiscountAmount;
                _currentNetTotalDue = receipt.TotalAmountDue;

                _lblConsultFeeValue.Text = $"₱{_currentConsultFee:N2}";
                _lblPharmacyFeeValue.Text = $"₱{_currentPharmacyTotal:N2}";
                _lblSubtotalValue.Text = $"₱{_currentSubtotal:N2}";
                _lblDiscountAmountValue.Text = _currentDiscountAmount > 0 ? $"-₱{_currentDiscountAmount:N2}" : "₱0.00";
                _lblTotalDueValue.Text = $"₱{_currentNetTotalDue:N2}";

                _suppressCalculation = true;
                try
                {
                    _cboDiscountType.SelectedIndex = -1;
                    for (int i = 0; i < _cboDiscountType.Items.Count; i++)
                    {
                        string itemStr = _cboDiscountType.Items[i]?.ToString() ?? "";
                        if (itemStr.StartsWith(receipt.DiscountType, StringComparison.OrdinalIgnoreCase) ||
                            receipt.DiscountType.StartsWith(itemStr.Split(' ')[0], StringComparison.OrdinalIgnoreCase))
                        {
                            _cboDiscountType.SelectedIndex = i;
                            break;
                        }
                    }
                    if (_cboDiscountType.SelectedIndex < 0)
                    {
                        _cboDiscountType.Text = receipt.DiscountType;
                    }

                    _cboPaymentMethod.SelectedIndex = -1;
                    for (int i = 0; i < _cboPaymentMethod.Items.Count; i++)
                    {
                        if (string.Equals(_cboPaymentMethod.Items[i]?.ToString(), receipt.PaymentMethod, StringComparison.OrdinalIgnoreCase))
                        {
                            _cboPaymentMethod.SelectedIndex = i;
                            break;
                        }
                    }
                    if (_cboPaymentMethod.SelectedIndex < 0)
                    {
                        _cboPaymentMethod.Text = receipt.PaymentMethod;
                    }

                    _cboDiscountType.Enabled = false;
                    _cboPaymentMethod.Enabled = false;
                    _txtAmountTendered.ReadOnly = true;
                    _btnQuickExact.Enabled = false;
                    _btnQuick500.Enabled = false;
                    _btnQuick1000.Enabled = false;
                    _btnQuick2000.Enabled = false;
                    _txtPaymentNotes.ReadOnly = true;
                    _txtPaymentNotes.Text = receipt.Notes;
                    _btnClearLedger.Enabled = false;

                    _txtAmountTendered.Text = receipt.AmountPaid.ToString("N2");
                    _lblChangeValue.Text = $"₱{receipt.ChangeAmount:N2}";
                    _lblChangeValue.ForeColor = Color.FromArgb(5, 150, 105);
                }
                finally
                {
                    _suppressCalculation = false;
                }

                _btnProcessPayment.Text = "🖨️ View & Reprint Official Receipt";
                _btnProcessPayment.BackColor = Color.FromArgb(15, 118, 110);
                _btnProcessPayment.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settled receipt details: {ex.Message}", "Billing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridQueue_SelectionChanged(object? sender, EventArgs e)
        {
            if (_gridQueue.CurrentRow?.DataBoundItem is PendingCheckoutItem item)
            {
                LoadBillForAppointment(item.AppointmentId);
            }
        }

        private void LoadBillForAppointment(int appointmentId)
        {
            try
            {
                _currentBill = _billingRepo.GetBillDetails(appointmentId);
                if (_currentBill == null)
                {
                    ClearLedgerCard();
                    return;
                }

                _lblPatientCardName.Text = $"{_currentBill.PatientName} ({_currentBill.PatientNumber})";
                _lblPatientCardMeta.Text = $"Physician: {_currentBill.DoctorName}  •  Specialty: {_currentBill.Specialty}";
                _lblPatientCardDiagnosis.Text = $"Diagnosis: {_currentBill.Diagnosis}";
                _lblLedgerBadge.Text = "READY FOR PAYMENT";
                _lblLedgerBadge.BackColor = Color.FromArgb(209, 250, 229); // Green 100
                _lblLedgerBadge.ForeColor = Color.FromArgb(5, 150, 105);

                // Build line items
                var items = new List<DisplayBillItem>
                {
                    new DisplayBillItem
                    {
                        Description = "Doctor Consultation / Professional Fee",
                        ItemType = "Consultation",
                        Quantity = 1,
                        UnitPrice = _currentBill.ConsultationFee
                    }
                };

                foreach (var med in _currentBill.Medicines)
                {
                    items.Add(new DisplayBillItem
                    {
                        Description = med.DisplayName,
                        ItemType = "Medication",
                        Quantity = med.Quantity,
                        UnitPrice = med.UnitPrice
                    });
                }

                _gridBillItems.DataSource = null;
                _gridBillItems.DataSource = items;

                _currentConsultFee = _currentBill.ConsultationFee;
                _currentPharmacyTotal = _currentBill.PharmacyTotal;
                _currentSubtotal = _currentConsultFee + _currentPharmacyTotal;

                _lblConsultFeeValue.Text = $"₱{_currentConsultFee:N2}";
                _lblPharmacyFeeValue.Text = $"₱{_currentPharmacyTotal:N2}";
                _lblSubtotalValue.Text = $"₱{_currentSubtotal:N2}";

                RecalculateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load bill details: {ex.Message}", "Billing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RecalculateTotals()
        {
            if (_suppressCalculation || _currentBill == null) return;

            string discountType = _cboDiscountType.SelectedItem?.ToString() ?? "None (0%)";
            decimal discountPercent = 0.00m;

            if (discountType.Contains("Senior", StringComparison.OrdinalIgnoreCase) ||
                discountType.Contains("PWD", StringComparison.OrdinalIgnoreCase))
            {
                discountPercent = 0.20m;
            }
            else if (discountType.Contains("Solo Parent", StringComparison.OrdinalIgnoreCase))
            {
                discountPercent = 0.10m;
            }

            decimal oldTotal = _currentNetTotalDue;

            _currentDiscountAmount = Math.Round(_currentSubtotal * discountPercent, 2);
            _currentNetTotalDue = Math.Max(0.00m, _currentSubtotal - _currentDiscountAmount);

            _lblDiscountAmountValue.Text = _currentDiscountAmount > 0 ? $"-₱{_currentDiscountAmount:N2}" : "₱0.00";
            _lblTotalDueValue.Text = $"₱{_currentNetTotalDue:N2}";

            // Smart Tender Preservation:
            // Only update amount tendered to the new exact net total if:
            // 1. The field was empty, "0", or "0.00", OR
            // 2. The field was previously auto-matched to the previous net total.
            // If the user entered a custom amount (e.g. 1000.00), preserve it and just recalculate change!
            string tenderStr = _txtAmountTendered.Text.Trim();
            bool isCurrentTenderAutoMatched = false;
            if (decimal.TryParse(tenderStr, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out decimal curTender) ||
                decimal.TryParse(tenderStr, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out curTender))
            {
                isCurrentTenderAutoMatched = (curTender == oldTotal);
            }

            if (string.IsNullOrWhiteSpace(tenderStr) || curTender <= 0 || isCurrentTenderAutoMatched)
            {
                _txtAmountTendered.Text = _currentNetTotalDue.ToString("N2");
            }

            _previousNetTotalDue = _currentNetTotalDue;
            RecalculateChange();
        }

        private void RecalculateChange()
        {
            if (_viewingSettledPaymentId.HasValue) return;

            if (_currentBill == null)
            {
                _btnProcessPayment.Enabled = false;
                _lblChangeValue.Text = "₱0.00";
                return;
            }

            string tenderStr = _txtAmountTendered.Text.Trim();
            if (decimal.TryParse(tenderStr, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out decimal tendered) ||
                decimal.TryParse(tenderStr, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out tendered))
            {
                decimal diff = tendered - _currentNetTotalDue;
                if (diff >= 0)
                {
                    _lblChangeValue.Text = $"₱{diff:N2}";
                    _lblChangeValue.ForeColor = Color.FromArgb(5, 150, 105); // Green
                    _btnProcessPayment.Enabled = true;
                }
                else
                {
                    _lblChangeValue.Text = $"Short: -₱{Math.Abs(diff):N2}";
                    _lblChangeValue.ForeColor = Color.FromArgb(220, 38, 38); // Red
                    _btnProcessPayment.Enabled = false;
                }
            }
            else
            {
                _lblChangeValue.Text = "Invalid Amount";
                _lblChangeValue.ForeColor = Color.FromArgb(220, 38, 38);
                _btnProcessPayment.Enabled = false;
            }
        }

        private async void BtnProcessPayment_Click(object? sender, EventArgs e)
        {
            if (_viewingSettledPaymentId.HasValue)
            {
                OpenReceiptForPayment(_viewingSettledPaymentId.Value);
                return;
            }

            if (_currentBill == null)
            {
                MessageBox.Show("Please select a pending patient checkout from the queue.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string tenderStr = _txtAmountTendered.Text.Trim();
            if (!decimal.TryParse(tenderStr, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out decimal tendered) &&
                !decimal.TryParse(tenderStr, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out tendered))
            {
                MessageBox.Show("Please enter a valid amount tendered.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtAmountTendered.Focus();
                return;
            }

            if (tendered < _currentNetTotalDue)
            {
                MessageBox.Show("Amount tendered cannot be less than the net total due.", "Insufficient Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtAmountTendered.Focus();
                return;
            }

            decimal change = tendered - _currentNetTotalDue;
            string discountType = _cboDiscountType.SelectedItem?.ToString() ?? "None";
            string paymentMethod = _cboPaymentMethod.SelectedItem?.ToString() ?? "Cash";
            string notes = _txtPaymentNotes.Text.Trim();

            // QA: Non-cash payment audit trail validation
            if (!string.Equals(paymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(notes) || notes.Length < 6)
                {
                    MessageBox.Show(
                        $"A valid transaction reference number or approval code (minimum 6 characters) is required for {paymentMethod} payment auditing.\n\nPlease enter the transaction reference in the 'Payment Notes / Remarks' field.",
                        "Cashier QA Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _txtPaymentNotes.Focus();
                    return;
                }
            }

            // QA: Statutory Discount ID Verification (RA 9994 / RA 10754)
            if (discountType.Contains("Senior", StringComparison.OrdinalIgnoreCase) || discountType.Contains("PWD", StringComparison.OrdinalIgnoreCase))
            {
                if (!notes.Contains("ID", StringComparison.OrdinalIgnoreCase) && !notes.Contains("OSCA", StringComparison.OrdinalIgnoreCase))
                {
                    using var idDlg = new Form
                    {
                        Text = "Discount Verification",
                        Size = new Size(420, 185),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false,
                        BackColor = Color.White
                    };

                    var lblPrompt = new Label
                    {
                        Text = "Enter Senior Citizen (OSCA) or PWD ID Number:",
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = Color.FromArgb(30, 41, 59),
                        Location = new Point(16, 16),
                        AutoSize = true
                    };

                    var txtId = new TextBox
                    {
                        Font = new Font("Segoe UI", 10F),
                        Location = new Point(16, 46),
                        Width = 370,
                        PlaceholderText = "e.g. OSCA-2024-1234 or PWD-5678"
                    };

                    var btnOk = new Button
                    {
                        Text = "Confirm ID",
                        UseMnemonic = false,
                        DialogResult = DialogResult.OK,
                        Location = new Point(266, 88),
                        Size = new Size(120, 36),
                        BackColor = Color.FromArgb(15, 118, 110),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        Cursor = Cursors.Hand
                    };
                    btnOk.FlatAppearance.BorderSize = 0;

                    var btnCancel = new Button
                    {
                        Text = "Cancel",
                        UseMnemonic = false,
                        DialogResult = DialogResult.Cancel,
                        Location = new Point(160, 88),
                        Size = new Size(95, 36),
                        BackColor = Color.White,
                        ForeColor = Color.FromArgb(71, 85, 105),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9F),
                        Cursor = Cursors.Hand
                    };
                    btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);

                    idDlg.Controls.AddRange(new Control[] { lblPrompt, txtId, btnOk, btnCancel });
                    idDlg.AcceptButton = btnOk;
                    idDlg.CancelButton = btnCancel;

                    if (idDlg.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(txtId.Text))
                    {
                        MessageBox.Show("Discount verification cancelled. A 20% discount requires the patient's ID number.", "Verification Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string verifiedId = txtId.Text.Trim();
                    notes = string.IsNullOrWhiteSpace(notes) ? $"ID: {verifiedId}" : $"{notes} | ID: {verifiedId}";
                    _txtPaymentNotes.Text = notes;
                }
            }

            var confirm = MessageBox.Show(
                $"Proceed with payment settlement?\n\n" +
                $"Patient: {_currentBill.PatientName}\n" +
                $"Total Due: ₱{_currentNetTotalDue:N2}\n" +
                $"Tendered: ₱{tendered:N2}\n" +
                $"Change: ₱{change:N2}\n" +
                $"Payment Method: {paymentMethod}",
                "Confirm Payment & Official Receipt Issuance",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                _btnProcessPayment.Enabled = false;

                var req = new ProcessPaymentRequest
                {
                    AppointmentId = _currentBill.AppointmentId,
                    PatientId = _currentBill.PatientId,
                    DoctorId = _currentBill.DoctorId,
                    ConsultationId = _currentBill.ConsultationId,
                    ConsultationFee = _currentConsultFee,
                    PharmacyTotal = _currentPharmacyTotal,
                    DiscountType = discountType,
                    DiscountAmount = _currentDiscountAmount,
                    TotalAmountDue = _currentNetTotalDue,
                    PaymentMethod = paymentMethod,
                    AmountPaid = tendered,
                    ChangeAmount = change,
                    ProcessedByUserId = UserSession.UserId,
                    Notes = _txtPaymentNotes.Text.Trim()
                };

                var (success, message, receipt) = await Task.Run(() => _billingRepo.ProcessPayment(req));

                if (!success || receipt == null)
                {
                    MessageBox.Show(message, "Payment Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Build receipt items
                var receiptItems = new List<ReceiptItemLine>
                {
                    new ReceiptItemLine
                    {
                        Description = "Doctor Consultation Fee",
                        ItemType = "Consultation",
                        Quantity = 1,
                        UnitPrice = _currentConsultFee
                    }
                };

                foreach (var med in _currentBill.Medicines)
                {
                    receiptItems.Add(new ReceiptItemLine
                    {
                        Description = med.DisplayName,
                        ItemType = "Pharmacy Medication",
                        Quantity = med.Quantity,
                        UnitPrice = med.UnitPrice
                    });
                }

                var receiptData = new ReceiptData
                {
                    ReceiptNumber = receipt.ReceiptNumber,
                    InvoiceNumber = receipt.InvoiceNumber,
                    PaymentDate = receipt.PaymentDate,
                    PatientName = _currentBill.PatientName,
                    PatientNumber = _currentBill.PatientNumber,
                    DoctorName = _currentBill.DoctorName,
                    Specialty = _currentBill.Specialty,
                    Items = receiptItems,
                    ConsultationFee = _currentConsultFee,
                    PharmacyTotal = _currentPharmacyTotal,
                    Subtotal = _currentSubtotal,
                    DiscountType = discountType,
                    DiscountAmount = _currentDiscountAmount,
                    TotalAmountDue = _currentNetTotalDue,
                    PaymentMethod = paymentMethod,
                    AmountPaid = tendered,
                    ChangeAmount = change,
                    CashierName = !string.IsNullOrWhiteSpace(UserSession.FullName) ? UserSession.FullName : "Reception Desk"
                };

                // Open receipt preview modal dialog
                using (var dlg = new ReceiptPreviewDialog(receiptData))
                {
                    dlg.ShowDialog(this);
                }

                // Clear current ledger and reload queue
                ClearLedgerCard();
                await LoadPendingQueueAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment processing error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnProcessPayment.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void ClearLedgerCard()
        {
            _currentBill = null;
            _viewingSettledPaymentId = null;
            _cboDiscountType.Enabled = true;
            _cboPaymentMethod.Enabled = true;
            _txtAmountTendered.ReadOnly = false;
            _btnQuickExact.Enabled = true;
            _btnQuick500.Enabled = true;
            _btnQuick1000.Enabled = true;
            _btnQuick2000.Enabled = true;
            _txtPaymentNotes.ReadOnly = false;
            _btnClearLedger.Enabled = true;
            _btnProcessPayment.Text = "💳 Process Payment & Issue Receipt (OR)";
            _lblPatientCardName.Text = "No patient selected";
            _lblPatientCardMeta.Text = "Select a pending patient from the queue to view itemized fees.";
            _lblPatientCardDiagnosis.Text = "";
            _lblLedgerBadge.Text = "Awaiting Patient Selection";
            _lblLedgerBadge.BackColor = Color.FromArgb(241, 245, 249);
            _lblLedgerBadge.ForeColor = Color.FromArgb(100, 116, 139);

            _gridBillItems.DataSource = null;

            _lblConsultFeeValue.Text = "₱0.00";
            _lblPharmacyFeeValue.Text = "₱0.00";
            _lblSubtotalValue.Text = "₱0.00";
            _lblDiscountAmountValue.Text = "₱0.00";
            _lblTotalDueValue.Text = "₱0.00";
            _txtAmountTendered.Text = "0.00";
            _lblChangeValue.Text = "₱0.00";
            _txtPaymentNotes.Clear();

            _btnProcessPayment.Enabled = false;
        }
    }
}

