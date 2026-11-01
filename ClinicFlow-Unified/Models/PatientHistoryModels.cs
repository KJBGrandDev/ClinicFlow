#nullable enable

using System;
using System.Collections.Generic;

namespace ClinicFlow_Unified.Models;

public sealed class PatientSummaryHeader
{
    public int PatientId { get; set; }
    public string PatientNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int TotalVisitsCount { get; set; }
}

public sealed class PatientEncounterRecord
{
    public int ConsultationId { get; set; }
    public int VisitId { get; set; }
    public DateTime RecordedAt { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string ClinicalNotes { get; set; } = string.Empty;
    public string DoctorRecommendation { get; set; } = string.Empty;

    public string FormattedDate => RecordedAt.ToString("MMM dd, yyyy  hh:mm tt");

    public List<PastPrescriptionItem> Prescriptions { get; set; } = [];
    public List<PastLabRequestItem> LabRequests { get; set; } = [];
}

public sealed class PastPrescriptionItem
{
    public int ConsultationId { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public string FulfillmentChoice { get; set; } = string.Empty;
    public string PrescriptionStatus { get; set; } = string.Empty;
}

public sealed class PastLabRequestItem
{
    public int ConsultationId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public string RequestStatus { get; set; } = string.Empty;
    public string ResultText { get; set; } = string.Empty;
}