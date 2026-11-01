#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;
using ClinicFlow_Unified.Repositories;
using ClinicFlow_Unified.Utilities;

namespace ClinicFlow_Unified.Services
{
    public sealed class AppointmentService
    {
        private readonly AppointmentRepository _repository = new();

        public (bool Success, string Message, List<DoctorLookupItem> Doctors) LoadDoctors()
        {
            try { return (true, string.Empty, _repository.GetActiveDoctors()); }
            catch (Exception ex) { return (false, $"Could not load doctors: {ex.Message}", []); }
        }

        public (bool Success, string Message, List<PatientLookupItem> Patients) SearchPatients(string search, int top = 20)
        {
            try { return (true, string.Empty, _repository.SearchPatients(search, top)); }
            catch (Exception ex) { return (false, $"Could not search patients: {ex.Message}", []); }
        }

        public (bool Success, string Message, List<PatientBookingConflictItem> Bookings) GetPatientBookingsForDate(int patientId, DateTime date)
        {
            try { return (true, string.Empty, _repository.GetPatientBookingsForDate(patientId, date)); }
            catch (Exception ex) { return (false, $"Could not check existing bookings: {ex.Message}", []); }
        }

        public (bool Success, string Message, List<TimeSlotItem> Slots) GenerateAvailableSlots(int doctorId, DateTime date)
        {
            try
            {
                // 1. Check if Doctor is on Full Day Off / Leave
                if (IsDoctorDayOff(doctorId, date))
                {
                    return (true, "Doctor is on leave / day off for this date.", new List<TimeSlotItem>());
                }

                // 2. Fetch Doctor's active practice hours
                int dayOfWeek = (int)date.DayOfWeek;
                var schedule = QueryDoctorSchedule(doctorId, dayOfWeek);

                TimeSpan start = schedule.StartTime != TimeSpan.Zero ? schedule.StartTime : new TimeSpan(8, 0, 0);
                TimeSpan end = schedule.EndTime != TimeSpan.Zero ? schedule.EndTime : new TimeSpan(17, 0, 0);
                if (end <= start) end = start.Add(TimeSpan.FromHours(5));
                int slotMinutes = schedule.SlotMinutes > 0 ? schedule.SlotMinutes : 30;

                var bookedStarts = QueryBookedStarts(doctorId, date);
                var blockedStarts = QueryBlockedStarts(doctorId, date);
                var slots = new List<TimeSlotItem>();

                // 3. Generate slots strictly within the doctor's practice hours
                var current = start;
                while (current < end)
                {
                    var next = current.Add(TimeSpan.FromMinutes(slotMinutes));
                    if (next > end) break;

                    bool isBooked = bookedStarts.Any(b => b.Hours == current.Hours && b.Minutes == current.Minutes);
                    bool isBlocked = blockedStarts.Any(b => b.Hours == current.Hours && b.Minutes == current.Minutes);

                    slots.Add(new TimeSlotItem
                    {
                        StartTime = current,
                        EndTime = next,
                        IsAvailable = !isBooked && !isBlocked
                    });

                    current = next;
                }

                // 4. Mark earliest available slot as [NEXT OPEN]
                var firstAvailable = slots.FirstOrDefault(s => s.IsAvailable);
                if (firstAvailable != null)
                {
                    firstAvailable.IsNextAvailable = true;
                }

                return (true, string.Empty, slots);
            }
            catch (Exception ex)
            {
                return (false, $"Could not compute time slots: {ex.Message}", []);
            }
        }

        private static (bool HasSchedule, TimeSpan StartTime, TimeSpan EndTime, int SlotMinutes) QueryDoctorSchedule(int doctorId, int dayOfWeek)
        {
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();

                // 1. Check dbo.DoctorWeeklySchedules (where Doctor Practice Schedule form saves)
                // Priority ranking:
                // 1) Today's day of week if Active
                // 2) Today's day of week
                // 3) Doctor's configured practice routine (e.g. 11:00 AM - 02:00 PM)
                const string sqlWeekly = @"
IF OBJECT_ID(N'dbo.DoctorWeeklySchedules', N'U') IS NOT NULL
BEGIN
    SELECT TOP 1 StartTime, EndTime, SlotDurationMinutes
    FROM dbo.DoctorWeeklySchedules
    WHERE DoctorId = @DoctorId AND EndTime > StartTime
    ORDER BY 
        CASE 
            WHEN DayOfWeek = @DayOfWeek AND IsActive = 1 THEN 1
            WHEN DayOfWeek = @DayOfWeek THEN 2
            WHEN IsActive = 1 THEN 3
            ELSE 4 
        END ASC,
        UpdatedAt DESC;
END";
                using (var cmd = new SqlCommand(sqlWeekly, conn))
                {
                    cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmd.Parameters.Add("@DayOfWeek", SqlDbType.Int).Value = dayOfWeek;
                    using var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        return (true, (TimeSpan)rdr.GetValue(0), (TimeSpan)rdr.GetValue(1), rdr.GetInt32(2));
                    }
                }

                // 2. Check dbo.DoctorSchedules (master table fallback for doctors who haven't customized routine)
                int dayOfWeekAlt = dayOfWeek == 0 ? 7 : dayOfWeek;
                const string sqlSchedules = @"
IF OBJECT_ID(N'dbo.DoctorSchedules', N'U') IS NOT NULL
BEGIN
    DECLARE @ColSlot NVARCHAR(50) = 
        CASE WHEN COL_LENGTH(N'dbo.DoctorSchedules', N'SlotDurationMinutes') IS NOT NULL 
             THEN N'SlotDurationMinutes' 
             ELSE N'SlotMinutes' END;

    DECLARE @Sql NVARCHAR(MAX) = N'
        SELECT TOP 1 StartTime, EndTime, ' + @ColSlot + N' AS SlotMinutes
        FROM dbo.DoctorSchedules
        WHERE DoctorId = @DocId AND EndTime > StartTime
        ORDER BY 
            CASE 
                WHEN (DayOfWeek = @Day1 OR DayOfWeek = @Day2) THEN 1 
                ELSE 2 
            END ASC;';

    EXEC sp_executesql @Sql, 
        N'@DocId INT, @Day1 INT, @Day2 INT', 
        @DocId = @DoctorId, @Day1 = @DayOfWeek, @Day2 = @DayOfWeekAlt;
END";
                using (var cmd2 = new SqlCommand(sqlSchedules, conn))
                {
                    cmd2.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmd2.Parameters.Add("@DayOfWeek", SqlDbType.Int).Value = dayOfWeek;
                    cmd2.Parameters.Add("@DayOfWeekAlt", SqlDbType.Int).Value = dayOfWeekAlt;
                    using var rdr2 = cmd2.ExecuteReader();
                    if (rdr2.Read())
                    {
                        return (true, (TimeSpan)rdr2.GetValue(0), (TimeSpan)rdr2.GetValue(1), Convert.ToInt32(rdr2.GetValue(2)));
                    }
                }
            }
            catch { }

            // 3. Fallback to standard clinic practice hours: 08:00 AM to 05:00 PM
            return (true, new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0), 30);
        }

        private static bool IsDoctorDayOff(int doctorId, DateTime date)
        {
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = @"
IF OBJECT_ID(N'dbo.DoctorScheduleExceptions', N'U') IS NOT NULL
BEGIN
    SELECT TOP 1 1 FROM dbo.DoctorScheduleExceptions 
    WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND IsFullDayOff = 1;
END";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                var res = cmd.ExecuteScalar();
                return res != null && res != DBNull.Value;
            }
            catch { return false; }
        }

        private static List<TimeSpan> QueryBookedStarts(int doctorId, DateTime date)
        {
            var list = new List<TimeSpan>();
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = @"
SELECT ScheduledStart
FROM dbo.Appointments
WHERE DoctorId = @DoctorId AND AppointmentDate = @Date AND Status NOT IN ('Cancelled', 'No-Show');";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add((TimeSpan)rdr.GetValue(0));
                }
            }
            catch { }
            return list;
        }

        private static HashSet<TimeSpan> QueryBlockedStarts(int doctorId, DateTime date)
        {
            var set = new HashSet<TimeSpan>();
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = @"
IF OBJECT_ID(N'dbo.DoctorScheduleExceptions', N'U') IS NOT NULL
BEGIN
    SELECT StartTime, EndTime FROM dbo.DoctorScheduleExceptions 
    WHERE DoctorId = @DoctorId 
      AND ExceptionDate = @Date 
      AND IsFullDayOff = 0 
      AND (IsBlocked = 1 OR IsBlocked IS NULL)
      AND StartTime IS NOT NULL;
END";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0))
                    {
                        var bStart = (TimeSpan)rdr.GetValue(0);
                        var bEnd = !rdr.IsDBNull(1) ? (TimeSpan)rdr.GetValue(1) : bStart.Add(TimeSpan.FromMinutes(30));

                        var cur = bStart;
                        while (cur < bEnd)
                        {
                            set.Add(cur);
                            cur = cur.Add(TimeSpan.FromMinutes(30));
                        }
                    }
                }
            }
            catch { }
            return set;
        }

        public (bool Success, string Message, int AppointmentId) BookAppointment(
            int patientId,
            int doctorId,
            DateTime date,
            TimeSpan start,
            TimeSpan end,
            string reason)
        {
            UserSession.RequireRole("Admin", "Secretary", "Doctor");
            if (patientId <= 0) return (false, "Please select a patient.", 0);
            if (doctorId <= 0) return (false, "Please select a doctor.", 0);
            if (date.Date < DateTime.Today) return (false, "Appointment date cannot be in the past.", 0);

            try
            {
                // Quality Assurance: Prevent duplicate booking with the same doctor on the same date
                var existingBookings = _repository.GetPatientBookingsForDate(patientId, date);
                var sameDoctorBooking = existingBookings.FirstOrDefault(b => b.DoctorId == doctorId);
                if (sameDoctorBooking != null)
                {
                    string queueInfo = sameDoctorBooking.QueueNumber > 0 ? $" (Queue Ticket #{sameDoctorBooking.QueueNumber:D2})" : "";
                    return (false, $"Patient already has an active appointment with {sameDoctorBooking.DoctorName} on {date:MMM dd, yyyy}{queueInfo}. Duplicate booking with the same attending physician is restricted.", 0);
                }

                int? userId = UserSession.IsAuthenticated && UserSession.UserId > 0 ? UserSession.UserId : null;
                int appointmentId = ExecuteCreateAppointment(patientId, doctorId, date, start, end, reason, userId);
                AuditLogger.Log("Insert", "Appointments", appointmentId, $"Booked appointment for Patient #{patientId} with Doctor #{doctorId} (Status: Booked)");
                return (true, "Appointment booked successfully. Status: Booked (Awaiting check-in upon arrival).", appointmentId);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to book appointment: {ex.Message}", 0);
            }
        }

        private static int ExecuteCreateAppointment(int patientId, int doctorId, DateTime date, TimeSpan start, TimeSpan end, string reason, int? userId)
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                const string sqlSeq = @"
DECLARE @DatePrefix VARCHAR(8) = CONVERT(VARCHAR(8), @Date, 112);
DECLARE @Pattern NVARCHAR(30) = 'APT-' + @DatePrefix + '-%';
SELECT ISNULL(MAX(CAST(RIGHT(AppointmentNumber, 4) AS INT)), 0) + 1 
FROM dbo.Appointments WITH (UPDLOCK, HOLDLOCK)
WHERE AppointmentNumber LIKE @Pattern;";

                int nextSeq;
                using (var cmdSeq = new SqlCommand(sqlSeq, conn, tx))
                {
                    cmdSeq.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                    nextSeq = Convert.ToInt32(cmdSeq.ExecuteScalar() ?? 1);
                }

                string apptNumber = $"APT-{date:yyyyMMdd}-{nextSeq:0000}";

                const string sqlAppt = @"
INSERT INTO dbo.Appointments 
    (AppointmentNumber, PatientId, DoctorId, AppointmentDate, ScheduledStart, ScheduledEnd, 
     QueueNumber, Status, Reason, CreatedByUserId, CreatedAt)
VALUES 
    (@AppointmentNumber, @PatientId, @DoctorId, @Date, @Start, @End, 
     NULL, N'Booked', @Reason, ISNULL(@UserId, 1), SYSDATETIME());
SELECT SCOPE_IDENTITY();";

                int appointmentId;
                using (var cmdAppt = new SqlCommand(sqlAppt, conn, tx))
                {
                    cmdAppt.Parameters.Add("@AppointmentNumber", SqlDbType.NVarChar, 30).Value = apptNumber;
                    cmdAppt.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
                    cmdAppt.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmdAppt.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                    cmdAppt.Parameters.Add("@Start", SqlDbType.Time).Value = start;
                    cmdAppt.Parameters.Add("@End", SqlDbType.Time).Value = end;
                    cmdAppt.Parameters.Add("@Reason", SqlDbType.NVarChar, 250).Value = (object?)reason ?? DBNull.Value;
                    cmdAppt.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)userId ?? DBNull.Value;

                    var res = cmdAppt.ExecuteScalar();
                    appointmentId = Convert.ToInt32(res);
                }

                tx.Commit();
                return appointmentId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public (bool Success, string Message, List<QueueBoardItem> Queue) LoadTodayQueue(int? doctorId = null)
        {
            try { return (true, string.Empty, _repository.GetTodayQueue(doctorId)); }
            catch (Exception ex) { return (false, $"Could not load queue: {ex.Message}", []); }
        }

        public (bool Success, string Message) CheckInPatient(int appointmentId, int patientId, int doctorId)
        {
            UserSession.RequireRole("Admin", "Secretary");
            try
            {
                var result = _repository.CheckInPatient(appointmentId, patientId, doctorId);
                if (result.Success)
                {
                    AuditLogger.Log("Update", "AppointmentQueue", appointmentId, $"Patient checked in. Assigned ticket Q-{result.QueueNumber:D2}");
                }
                return (result.Success, result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to check in patient: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateStatus(int queueId, string newStatus)
        {
            UserSession.RequireRole("Admin", "Secretary", "Doctor");
            try
            {
                _repository.UpdateQueueStatus(queueId, newStatus);
                AuditLogger.Log("Update", "AppointmentQueue", queueId, $"Queue status changed to '{newStatus}'");
                return (true, $"Patient status updated to '{newStatus}'.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update queue status: {ex.Message}");
            }
        }

        public (bool Success, string Message, List<DisplacedAppointmentItem> Items) GetDisplacedAppointments(DateTime? date = null, int? doctorId = null)
        {
            try
            {
                var list = _repository.GetDisplacedAppointments(date, doctorId);
                return (true, string.Empty, list);
            }
            catch (Exception ex)
            {
                return (false, $"Could not load displaced appointments: {ex.Message}", []);
            }
        }

        public (bool Success, string Message, int DisplacedCount) DisplaceAppointments(List<int> appointmentIds, string emergencyReason, int userId)
        {
            UserSession.RequireRole("Admin", "Secretary", "Doctor");
            if (appointmentIds == null || appointmentIds.Count == 0)
                return (false, "No appointments selected for displacement.", 0);

            if (string.IsNullOrWhiteSpace(emergencyReason))
                emergencyReason = "Doctor Emergency Blockout";

            try
            {
                int count = _repository.DisplaceAppointments(appointmentIds, emergencyReason, userId);
                foreach (int apptId in appointmentIds)
                {
                    AuditLogger.Log("Update", "Appointments", apptId, $"Emergency triage displacement: {emergencyReason}");
                }
                return (true, $"Successfully displaced {count} patient appointment(s) to emergency triage.", count);
            }
            catch (Exception ex)
            {
                return (false, $"Displacement failed: {ex.Message}", 0);
            }
        }

        public (bool Success, string Message) TransferToPeerDoctor(int appointmentId, int targetDoctorId, TimeSpan newStart, TimeSpan newEnd, string notes, int userId)
        {
            UserSession.RequireRole("Admin", "Secretary", "Doctor");
            try
            {
                bool ok = _repository.TransferToPeerDoctor(appointmentId, targetDoctorId, newStart, newEnd, notes, userId);
                if (ok)
                {
                    AuditLogger.Log("Update", "Appointments", appointmentId, $"Transferred to peer doctor ID {targetDoctorId} at {newStart}. Notes: {notes}");
                    return (true, "Appointment successfully transferred to peer doctor!");
                }
                return (false, "Target slot is no longer available. Please select another slot.");
            }
            catch (Exception ex)
            {
                return (false, $"Peer transfer failed: {ex.Message}");
            }
        }

        public (bool Success, string Message) RescheduleAppointment(int appointmentId, DateTime newDate, TimeSpan newStart, TimeSpan newEnd, string notes, int userId)
        {
            UserSession.RequireRole("Admin", "Secretary", "Doctor");
            try
            {
                bool ok = _repository.RescheduleAppointment(appointmentId, newDate, newStart, newEnd, notes, userId);
                if (ok)
                {
                    AuditLogger.Log("Update", "Appointments", appointmentId, $"Fast-track rescheduled to {newDate:yyyy-MM-dd} at {newStart}. Notes: {notes}");
                    return (true, "Appointment successfully rescheduled!");
                }
                return (false, "Failed to reschedule appointment.");
            }
            catch (Exception ex)
            {
                return (false, $"Reschedule failed: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateContactStatus(int appointmentId, string contactStatus, int userId)
        {
            UserSession.RequireRole("Admin", "Secretary");
            try
            {
                bool ok = _repository.UpdateContactStatus(appointmentId, contactStatus, userId);
                if (ok)
                {
                    AuditLogger.Log("Update", "Appointments", appointmentId, $"Outreach contact status updated: {contactStatus}");
                    return (true, "Contact status updated successfully.");
                }
                return (false, "Failed to update contact status.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating contact status: {ex.Message}");
            }
        }

        public (bool Success, string Message, List<PeerDoctorAvailableSlotItem> Slots) GetAvailablePeerDoctorSlots(int currentDoctorId, DateTime date)
        {
            try
            {
                var slots = _repository.GetAvailablePeerDoctorSlots(currentDoctorId, date);
                return (true, string.Empty, slots);
            }
            catch (Exception ex)
            {
                return (false, $"Could not retrieve peer doctor slots: {ex.Message}", []);
            }
        }

        public (bool Success, string Message, List<DoctorNextAvailableSlotItem> Slots) GetDoctorNextAvailableSlots(int doctorId, DateTime fromDate)
        {
            try
            {
                var slots = _repository.GetDoctorNextAvailableSlots(doctorId, fromDate);
                return (true, string.Empty, slots);
            }
            catch (Exception ex)
            {
                return (false, $"Could not retrieve next available slots: {ex.Message}", []);
            }
        }
    }
}