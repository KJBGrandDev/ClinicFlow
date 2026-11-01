#nullable enable

using System;
using System.Collections.Generic;

namespace ClinicFlow_Unified.Models
{
    public sealed class PendingCheckoutItem
    {
        public int AppointmentId { get; set; }
        public string AppointmentNumber { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string ScheduledTime { get; set; } = string.Empty;
        public int? ConsultationId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string PrescriptionStatus { get; set; } = "None";
        public decimal TotalEstimated { get; set; }
    }

    public sealed class BillableMedicineItem
    {
        public int PrescriptionItemId { get; set; }
        public int MedicineId { get; set; }
        public string GenericName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public string? Strength { get; set; }
        public string DosageForm { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public string PatientChoice { get; set; } = "Clinic Pharmacy";

        public string DisplayName => string.IsNullOrWhiteSpace(BrandName)
            ? $"{GenericName} {Strength}".Trim()
            : $"{BrandName} ({GenericName} {Strength})".Trim();
    }

    public sealed class BillDetails
    {
        public int AppointmentId { get; set; }
        public string AppointmentNumber { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int? ConsultationId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; } = 500.00m;
        public List<BillableMedicineItem> Medicines { get; set; } = new();
        public decimal PharmacyTotal { get; set; }
        public decimal GrossTotal => ConsultationFee + PharmacyTotal;
    }

    public sealed class ProcessPaymentRequest
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int? ConsultationId { get; set; }
        public decimal ConsultationFee { get; set; }
        public decimal PharmacyTotal { get; set; }
        public string DiscountType { get; set; } = "None";
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmountDue { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public int? ProcessedByUserId { get; set; }
        public string? Notes { get; set; }
    }

    public sealed class PaymentReceiptResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public decimal TotalAmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }

    public sealed class PaymentHistoryItem
    {
        public int PaymentId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ProcessedBy { get; set; } = "Secretary";
    }

    public sealed class ReceiptItemLine
    {
        public string Description { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public sealed class ReceiptData
    {
        public string ReceiptNumber { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string PatientName { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public List<ReceiptItemLine> Items { get; set; } = new();
        public decimal ConsultationFee { get; set; }
        public decimal PharmacyTotal { get; set; }
        public decimal Subtotal { get; set; }
        public string DiscountType { get; set; } = "None";
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmountDue { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public decimal AmountPaid { get; set; }
        public decimal ChangeAmount { get; set; }
        public string CashierName { get; set; } = "Reception Desk";
        public string Notes { get; set; } = string.Empty;
    }
}

