#nullable enable

namespace ClinicFlow_Unified.Models;

public sealed class DoctorQueueItem
{
    public int QueueId { get; set; }
    public int VisitId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int QueueNumber { get; set; }
    public string ArrivedTime { get; set; } = string.Empty;
}

public sealed class MedicineItem
{
    public int MedicineId { get; set; }
    public string GenericName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string Category { get; set; } = "General";
    public string DosageForm { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public decimal UnitPrice { get; set; }
    public int AvailableStock { get; set; }

    public string DisplayText => string.IsNullOrWhiteSpace(BrandName)
        ? $"{GenericName} ({Strength} {DosageForm}) - Stock: {AvailableStock}"
        : $"{GenericName} [{BrandName}] ({Strength} {DosageForm}) - Stock: {AvailableStock}";
}

public sealed class MedicineCategoryOption
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsPrompt { get; set; }

    public string DisplayText => IsPrompt
        ? "-- Select Medicine Category / Type --"
        : $"📁 {CategoryName} ({Count} {(Count == 1 ? "medicine" : "medicines")})";

    public override string ToString() => DisplayText;
}

public sealed class MedicineReturnOption
{
    public string DisplayText => "⬅ [Go Back / Return to Medicine Types]";
    public override string ToString() => DisplayText;
}

public sealed class PrescriptionLineItem
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string FulfillmentChoice { get; set; } = "Clinic Pharmacy";
}