#nullable enable

using System;
using System.Collections.Generic;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Services;

public sealed class PatientService
{
    private readonly PatientRepository _repository = new();

    public (bool Success, string Message, List<Patient> Patients) Search(string search, bool showArchived = false)
    {
        try
        {
            return (true, string.Empty, _repository.Search(search, showArchived));
        }
        catch (Exception ex)
        {
            return (false, $"Failed to search patients: {ex.Message}", []);
        }
    }

    public (bool Success, string Message, List<Patient> Patients) GetArchivedPatients(string search = "")
    {
        return Search(search, showArchived: true);
    }

    public (bool Success, string Message, int PatientId) Register(Patient patient)
    {
        var validation = Validate(patient);
        if (validation != null)
            return (false, validation, 0);

        try
        {
            int patientId = _repository.Insert(patient);
            AuditLogger.Log("Insert", "Patients", patientId, $"Registered patient {patient.LastName}, {patient.FirstName}");
            return (true, $"Patient registered successfully. Generated ID: PT-{patientId:000000}", patientId);
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "PatientService.Register");
            return (false, $"Failed to register patient: {msg}", 0);
        }
    }

    public (bool Success, string Message) Update(Patient patient)
    {
        if (patient.PatientId <= 0)
            return (false, "Please select a valid patient to update.");

        var validation = Validate(patient);
        if (validation != null)
            return (false, validation);

        try
        {
            _repository.Update(patient);
            AuditLogger.Log("Update", "Patients", patient.PatientId, $"Updated profile for PT-{patient.PatientId:000000}");
            return (true, "Patient profile updated successfully.");
        }
        catch (System.Data.DBConcurrencyException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "PatientService.Update");
            return (false, $"Failed to update patient: {msg}");
        }
    }

    public (bool Success, string Message) Delete(int patientId)
    {
        if (patientId <= 0)
            return (false, "Please select a valid patient.");

        try
        {
            _repository.Delete(patientId);
            AuditLogger.Log("Delete", "Patients", patientId, "Archived patient record");
            return (true, "Patient record archived successfully.");
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "PatientService.Delete");
            return (false, $"Failed to archive patient: {msg}");
        }
    }

    public (bool Success, string Message) Restore(int patientId)
    {
        if (patientId <= 0)
            return (false, "Please select a valid patient to restore.");

        try
        {
            _repository.Restore(patientId);
            return (true, "Patient record restored to active roster successfully.");
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "PatientService.Restore");
            return (false, $"Failed to restore patient: {msg}");
        }
    }

    private static string? Validate(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FirstName)) return "First Name is required.";
        if (!IsValidName(patient.FirstName)) return "First Name cannot contain numbers or special symbols.";

        if (string.IsNullOrWhiteSpace(patient.LastName)) return "Last Name is required.";
        if (!IsValidName(patient.LastName)) return "Last Name cannot contain numbers or special symbols.";

        if (!string.IsNullOrWhiteSpace(patient.MiddleName) && !IsValidName(patient.MiddleName))
            return "Middle Name cannot contain numbers or special symbols.";

        if (!string.IsNullOrWhiteSpace(patient.Suffix) && !IsValidName(patient.Suffix))
            return "Suffix cannot contain numbers or special symbols.";

        if (string.IsNullOrWhiteSpace(patient.Sex)) return "Sex is required.";

        if (string.IsNullOrWhiteSpace(patient.ContactNumber)) return "Contact Number is required.";
        string cleanContact = patient.ContactNumber.Trim();
        if (cleanContact.Length != 11 || !cleanContact.All(char.IsDigit) || !cleanContact.StartsWith("09"))
            return "Contact Number must be a valid 11-digit Philippine mobile number starting with '09' (e.g., 09171234567).";

        if (string.IsNullOrWhiteSpace(patient.StreetAddress)) return "Street Address is required.";
        if (string.IsNullOrWhiteSpace(patient.City)) return "City is required.";
        if (!IsValidName(patient.City)) return "City cannot contain numbers or special symbols.";

        if (!string.IsNullOrWhiteSpace(patient.Municipality) && !IsValidName(patient.Municipality))
            return "Municipality / Region cannot contain numbers or special symbols.";

        bool hasEmergName = !string.IsNullOrWhiteSpace(patient.EmergencyContactName);
        bool hasEmergPhone = !string.IsNullOrWhiteSpace(patient.EmergencyContactNumber);

        if (hasEmergPhone && !hasEmergName)
        {
            return "Contact Person Full Name must be stated when an Emergency Contact Phone # is provided.";
        }

        if (hasEmergName && !hasEmergPhone)
        {
            return "Emergency Contact Phone # must be stated when a Contact Person Full Name is provided.";
        }

        if (hasEmergName)
        {
            if (!IsValidName(patient.EmergencyContactName))
                return "Emergency Contact Person Name cannot contain numbers or special symbols.";
        }

        if (hasEmergPhone)
        {
            string cleanEmerg = patient.EmergencyContactNumber!.Trim();
            if (cleanEmerg.Length != 11 || !cleanEmerg.All(char.IsDigit) || !cleanEmerg.StartsWith("09"))
                return "Emergency Contact Phone must be a valid 11-digit Philippine mobile number starting with '09' (e.g., 09171234567).";
        }

        if (patient.BirthDate > DateTime.Today) return "Birth date cannot be in the future.";
        if (patient.BirthDate <= new DateTime(1800, 1, 1) || patient.BirthDate.Year < 1900)
            return "Birth Date is required and cannot be 1/1/1800 or unconfigured. Please provide a valid birth date.";

        return null;
    }

    private static bool IsValidName(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        return text.Trim().All(c => char.IsLetter(c) || c == ' ' || c == '-' || c == '.' || c == '\'');
    }
}