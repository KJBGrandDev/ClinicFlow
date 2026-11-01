#nullable enable

using System;
using System.Collections.Generic;

namespace ClinicFlow_Unified.Models;

public sealed class DailyRevenueReportItem
{
    public int PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Cash";
    public decimal SubTotal { get; set; }
    public string DiscountType { get; set; } = "None";
    public decimal DiscountAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? Notes { get; set; }
}

public sealed class RevenueReportSummary
{
    public int TotalTransactions { get; set; }
    public decimal GrossSubtotal { get; set; }
    public decimal TotalDiscountsGranted { get; set; }
    public decimal NetRevenueCollected { get; set; }
    public decimal CashCollections { get; set; }
    public decimal GCashCollections { get; set; }
    public decimal CardCollections { get; set; }
    public decimal OtherCollections { get; set; }
}

public sealed class PharmacyInventoryReportItem
{
    public int InventoryId { get; set; }
    public int MedicineId { get; set; }
    public string GenericName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string DosageForm { get; set; } = string.Empty;
    public string Strength { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string StockStatus { get; set; } = "Optimal";

    public string MedicineDisplayName => string.IsNullOrWhiteSpace(BrandName)
        ? $"{GenericName} ({Strength} {DosageForm})"
        : $"{GenericName} [{BrandName}] ({Strength} {DosageForm})";
}

public sealed class InventoryReportSummary
{
    public int TotalBatchesMonitored { get; set; }
    public int TotalUnitsInStock { get; set; }
    public decimal TotalValuation { get; set; }
    public int ExpiredBatchCount { get; set; }
    public int NearExpiryBatchCount { get; set; }
    public int LowStockBatchCount { get; set; }
    public int OptimalBatchCount { get; set; }
}

public sealed class DoctorConsultationReportItem
{
    public int ConsultationId { get; set; }
    public DateTime ConsultationDate { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public int PatientAge { get; set; }
    public string PatientSex { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public int PrescribedItemsCount { get; set; }
    public decimal ConsultationFee { get; set; }
}

public sealed class ConsultationReportSummary
{
    public int TotalConsultations { get; set; }
    public int UniquePatientsCount { get; set; }
    public int TotalPrescriptionItems { get; set; }
    public decimal TotalConsultationFees { get; set; }
    public Dictionary<string, int> ConsultationsByDoctor { get; set; } = new();
}
