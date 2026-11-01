#nullable enable

using System;
using System.Collections.Generic;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Services;

public sealed class DoctorConsultationService
{
    private readonly DoctorConsultationRepository _repository = new();

    public (bool Success, string Message, List<DoctorQueueItem> Queue) LoadQueue(int? doctorId = null)
    {
        try { return (true, string.Empty, _repository.GetDoctorQueue(doctorId)); }
        catch (Exception ex) { return (false, $"Could not load consultation queue: {ex.Message}", []); }
    }

    public void StartConsultation(int queueId)
    {
        UserSession.RequireRole("Doctor", "Admin");
        try { _repository.SetQueueInService(queueId); }
        catch { }
    }

    public (bool Success, string Message, List<MedicineItem> Medicines) LoadCatalogs()
    {
        try { return (true, string.Empty, _repository.GetMedicines()); }
        catch (Exception ex) { return (false, $"Could not load catalogs: {ex.Message}", []); }
    }

    public (bool Success, string Message) DeferPatient(int currentQueueId, int nextQueueId)
    {
        UserSession.RequireRole("Doctor", "Admin");
        try
        {
            _repository.DeferPatient(currentQueueId, nextQueueId);
            AuditLogger.Log("Update", "AppointmentQueue", currentQueueId, "Doctor deferred patient to next in line");
            return (true, "Patient deferred. Swapped with the next patient in line.");
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "DoctorConsultationService.DeferPatient");
            return (false, $"Could not defer patient: {msg}");
        }
    }

    public (bool Success, string Message, DateTime NextDate, int Count) EmergencyPostpone(int doctorId)
    {
        UserSession.RequireRole("Doctor", "Admin");
        try
        {
            var (nextDate, count) = _repository.EmergencyPostponeDoctor(doctorId);
            AuditLogger.Log("Update", "Appointments", doctorId, $"Emergency leave: rescheduled {count} patients to {nextDate:yyyy-MM-dd}");
            return (true, $"Emergency leave processed. Rescheduled {count} patient(s) to {nextDate:MMM dd, yyyy}.", nextDate, count);
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "DoctorConsultationService.EmergencyPostpone");
            return (false, $"Emergency postpone failed: {msg}", DateTime.Today, 0);
        }
    }

    public (bool Success, string Message, int ConsultationId) CompleteConsultation(
        int appointmentId,
        int patientId,
        int doctorId,
        string chiefComplaint,
        string clinicalNotes,
        string diagnosis,
        string recommendation,
        List<PrescriptionLineItem> prescriptions)
    {
        UserSession.RequireRole("Doctor", "Admin");

        if (appointmentId <= 0 || patientId <= 0)
            return (false, "A valid active patient appointment must be selected.", 0);

        if (string.IsNullOrWhiteSpace(chiefComplaint))
            return (false, "Chief Complaint is required.", 0);

        if (string.IsNullOrWhiteSpace(diagnosis))
            return (false, "Diagnosis is required before completing the consultation.", 0);

        foreach (var rx in prescriptions)
        {
            if (rx.Quantity <= 0)
                return (false, $"Prescription for '{rx.MedicineName}' must have a quantity greater than 0.", 0);
            if (string.IsNullOrWhiteSpace(rx.Instructions))
                return (false, $"Dosage instructions for '{rx.MedicineName}' are required.", 0);
        }

        try
        {
            int? currentUserId = UserSession.IsAuthenticated && UserSession.UserId > 0 ? UserSession.UserId : null;

            int consultationId = _repository.SaveConsultation(
                appointmentId,
                patientId,
                doctorId,
                chiefComplaint,
                clinicalNotes,
                diagnosis,
                recommendation,
                prescriptions,
                currentUserId
            );

            AuditLogger.Log("Insert", "Consultations", consultationId, $"Completed consultation for Patient #{patientId}");
            return (true, "Consultation completed and saved successfully.", consultationId);
        }
        catch (Exception ex)
        {
            string msg = ErrorLogger.Log(ex, "DoctorConsultationService.CompleteConsultation");
            return (false, $"Failed to save consultation: {msg}", 0);
        }
    }
}