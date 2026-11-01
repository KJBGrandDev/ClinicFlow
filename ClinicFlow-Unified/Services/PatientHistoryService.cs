#nullable enable

using System;
using System.Collections.Generic;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;

namespace ClinicFlow_Unified.Services;

public sealed class PatientHistoryService
{
    private readonly PatientHistoryRepository _repository = new();

    public (bool Success, string Message, PatientSummaryHeader? Summary, List<PatientEncounterRecord> Encounters) LoadPatientRecord(int patientId)
    {
        if (patientId <= 0)
            return (false, "Invalid patient selected.", null, []);

        try
        {
            var summary = _repository.GetPatientSummary(patientId);
            var encounters = _repository.GetEncounters(patientId);
            return (true, string.Empty, summary, encounters);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to load EHR records: {ex.Message}", null, []);
        }
    }
}