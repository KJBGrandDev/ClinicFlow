#nullable enable

using System;
using System.Collections.Generic;

namespace ClinicFlow_Unified.Models;

public sealed class PharmacyQueueItem
{
    public int PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorLicense { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public DateTime PaidAt { get; set; }
    public string PrescriptionStatus { get; set; } = string.Empty;
}

public sealed class PharmacyPrescriptionItem
{
    public int PrescriptionItemId { get; set; }
    public int PrescriptionId { get; set; }
    public int MedicineId { get; set; }
    public string GenericName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string DosageForm { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string Category { get; set; } = string.Empty;
    public int QuantityPrescribed { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * QuantityPrescribed;
    public string PatientChoice { get; set; } = "Clinic Pharmacy";
    public string AvailabilityStatus { get; set; } = "Paid";
    public int InHouseStock { get; set; }

    public bool HasSufficientStock => InHouseStock >= QuantityPrescribed;

    public string MedicineDisplayName => string.IsNullOrWhiteSpace(BrandName)
        ? $"{GenericName} ({Strength} {DosageForm})"
        : $"{GenericName} [{BrandName}] ({Strength} {DosageForm})";

    public string StockStatusText
    {
        get
        {
            if (AvailabilityStatus == "Dispensed") return "✅ Dispensed";
            if (AvailabilityStatus == "Out of Stock / External") return "⚠️ External Rx";
            if (InHouseStock >= QuantityPrescribed) return $"🟢 Available ({InHouseStock} in stock)";
            if (InHouseStock > 0) return $"🟡 Partial Stock ({InHouseStock} available)";
            return "🔴 Out of Stock (0 in clinic)";
        }
    }
}

public sealed class PharmacyInventoryItem
{
    public int MedicineId { get; set; }
    public int? InventoryId { get; set; }
    public string GenericName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string Category { get; set; } = string.Empty;
    public string DosageForm { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int QuantityOnHand { get; set; } // Quantity in this specific lot / batch
    public int TotalStock { get; set; } // Aggregate quantity across all active lots of this medication
    public int ReorderLevel { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int BatchSeq { get; set; } = 1;
    public int ActiveBatchCount { get; set; } = 0;

    public bool IsFirstBatch => BatchSeq <= 1;
    public bool IsLastBatch => BatchSeq >= ActiveBatchCount;

    public bool IsLowStock => TotalStock <= ReorderLevel;

    public decimal EstimatedMarkupPct => CostPrice > 0 ? ((UnitPrice - CostPrice) / CostPrice) * 100m : 0m;
    public decimal EstimatedMarginPct => UnitPrice > 0 ? ((UnitPrice - CostPrice) / UnitPrice) * 100m : 0m;

    public string StockStatus
    {
        get
        {
            if (TotalStock <= 0) return "Out of Stock";
            if (TotalStock <= ReorderLevel) return "Low Stock";
            return "Optimal";
        }
    }

    public string DisplayName => string.IsNullOrWhiteSpace(BrandName)
        ? $"{GenericName} ({Strength})"
        : $"{GenericName} [{BrandName}] ({Strength})";
}

public sealed class PrescriptionSlipData
{
    public int PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public DateTime DatePrescribed { get; set; } = DateTime.Now;

    // Doctor Credentials
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string PtrNumber { get; set; } = string.Empty;
    public string S2Number { get; set; } = string.Empty;

    // Clinic Davao Address
    public string ClinicName { get; set; } = "ClinicFlow Health Network — Davao Medical Plaza";
    public string ClinicAddress { get; set; } = "J.P. Laurel Avenue, Bajada, Davao City, Davao del Sur, 8000";
    public string ClinicContact { get; set; } = "Tel: (082) 299-1234 | Mobile: 0917-555-FLOW";

    // Patient Info
    public int PatientId { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string PatientAddress { get; set; } = "Davao City";

    // Medicines
    public List<PrescriptionSlipItem> Items { get; set; } = new();

    // Notes
    public string Notes { get; set; } = string.Empty;
}

public sealed class PrescriptionSlipItem
{
    public string GenericName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string DosageForm { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public int Quantity { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public bool IsExternalUnavailable { get; set; }
}

public sealed class PharmacistRecord
{
    public int PharmacistId { get; set; }
    public string PharmacistCode { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}, RPh";
    public string LicenseNumber { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string StatusDisplay => IsActive ? "Active" : "Archived";
    public DateTime CreatedAt { get; set; }
}

public sealed class DispensedAuditItem
{
    public int PrescriptionId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string MedicineSummary { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime DispensedAt { get; set; }
    public string DispensedBy { get; set; } = string.Empty;
    public string Status { get; set; } = "Dispensed";
}

public sealed class MedicineDisposalLogItem
{
    public int DisposalId { get; set; }
    public int MedicineId { get; set; }
    public string GenericName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string DosageForm { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int QuantityDisposed { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalLossAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string DisposalMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? DisposedByUserId { get; set; }
    public string DisposedBy { get; set; } = string.Empty;
    public DateTime DisposedAt { get; set; }

    public string MedicineDisplayName => string.IsNullOrWhiteSpace(BrandName)
        ? $"{GenericName} ({Strength} {DosageForm})"
        : $"{GenericName} [{BrandName}] ({Strength} {DosageForm})";
}

public sealed class DisposalCandidateItem
{
    public bool IsSelected { get; set; } = false;
    public int MedicineId { get; set; }
    public int InventoryId { get; set; }
    public string GenericName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string DosageForm { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityToDispose { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal EstLossAmount => QuantityToDispose * UnitPrice;

    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.Today;
    public bool IsCritical => ExpiryDate.HasValue && ExpiryDate.Value.Date <= DateTime.Today.AddDays(30);

    public string MedicineDisplayName => string.IsNullOrWhiteSpace(BrandName)
        ? $"{GenericName} ({Strength} {DosageForm})"
        : $"{GenericName} [{BrandName}] ({Strength} {DosageForm})";

    public string ExpiryStatusText
    {
        get
        {
            if (!ExpiryDate.HasValue) return "No Expiry";
            if (ExpiryDate.Value.Date < DateTime.Today)
            {
                int days = (DateTime.Today - ExpiryDate.Value.Date).Days;
                return $"🔴 Expired ({days}d ago)";
            }
            int rem = (ExpiryDate.Value.Date - DateTime.Today).Days;
            if (rem <= 30) return $"⚠️ Critical ({rem}d left)";
            if (rem <= 60) return $"🟡 Near Expiry ({rem}d left)";
            return $"Active ({rem}d left)";
        }
    }
}

public sealed class PharmacyFinancialSummary
{
    public decimal GrossSalesRevenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit => GrossSalesRevenue - CostOfGoodsSold;
    public decimal ProfitMarginPct => GrossSalesRevenue > 0 ? (GrossProfit / GrossSalesRevenue) * 100m : 0m;

    public decimal TotalDisposalLoss { get; set; }
    public int DisposalCount { get; set; }
    public decimal ShrinkageRatioPct => (GrossSalesRevenue + TotalDisposalLoss) > 0 
        ? (TotalDisposalLoss / (GrossSalesRevenue + TotalDisposalLoss)) * 100m 
        : 0m;
    public string ShrinkageHealthStatus => ShrinkageRatioPct switch
    {
        <= 2.0m => "🟢 Healthy (< 2.0%)",
        <= 4.0m => "🟡 Moderate (2.0% - 4.0%)",
        _ => "🔴 High Waste (> 4.0%)"
    };

    public decimal MissedSalesOpportunity { get; set; } // Out of Stock tagged items
    public int MissedRxItemsCount { get; set; }

    public decimal ActiveInventoryCapital { get; set; } // Current shelf stock value
    public decimal CapitalAtRisk30Days { get; set; }    // Lots expiring <= 30 days
    public int FormulationsCount { get; set; }
    public int PrescriptionsDispensedCount { get; set; }

    public List<TopRevenueDrugItem> TopRevenueDrugs { get; set; } = new();
    public List<AtRiskExpiryItem> ImminentExpiryRisks { get; set; } = new();
    public List<DisposalLossReasonItem> LossReasons { get; set; } = new();
}

public sealed class TopRevenueDrugItem
{
    public int MedicineId { get; set; }
    public string MedicineDisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int UnitsDispensed { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal EstimatedProfit { get; set; }
    public decimal ProfitMarginPct => GrossRevenue > 0 ? (EstimatedProfit / GrossRevenue) * 100m : 0m;
    public decimal SalesSharePct { get; set; }
}

public sealed class AtRiskExpiryItem
{
    public int MedicineId { get; set; }
    public string MedicineDisplayName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int QuantityOnHand { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ValueAtRisk => QuantityOnHand * UnitPrice;
    public int DaysRemaining => ExpiryDate.HasValue ? Math.Max(0, (ExpiryDate.Value.Date - DateTime.Today).Days) : 0;
    public string StatusText => DaysRemaining switch
    {
        0 => "🔴 Expired Today or Past",
        <= 15 => $"⚠️ Urgent ({DaysRemaining}d left)",
        _ => $"⚠️ Critical ({DaysRemaining}d left)"
    };
}

public sealed class DisposalLossReasonItem
{
    public string Reason { get; set; } = string.Empty;
    public int BatchesDestroyed { get; set; }
    public int UnitsDestroyed { get; set; }
    public decimal TotalLossAmount { get; set; }
    public decimal LossSharePct { get; set; }
}


