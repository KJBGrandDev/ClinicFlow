#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;
using ClinicFlow_Unified.Models;

namespace ClinicFlow_Unified.Repositories;

public sealed class AppointmentRepository
{
    /// <summary>
    /// Atomically generates the next unique appointment number (APT-YYYYMMDD-XXXX)
    /// and saves the appointment with status 'Booked'. Queue ticket is NOT assigned yet.
    /// </summary>
    public (int AppointmentId, string AppointmentNumber) InsertAppointment(
        int patientId,
        int doctorId,
        DateTime appointmentDate,
        TimeSpan startTime,
        TimeSpan endTime,
        string? reason,
        int createdByUserId)
    {
        const string sql = @"
            BEGIN TRANSACTION;

            DECLARE @DatePrefix VARCHAR(8) = CONVERT(VARCHAR(8), @AppointmentDate, 112);
            DECLARE @Pattern NVARCHAR(30) = 'APT-' + @DatePrefix + '-%';
            DECLARE @MaxSeq INT = 0;

            -- Locks the prefix pattern across all appointments (even if rescheduled) to guarantee uniqueness
            SELECT @MaxSeq = ISNULL(MAX(CAST(RIGHT(AppointmentNumber, 4) AS INT)), 0)
            FROM dbo.Appointments WITH (UPDLOCK, HOLDLOCK)
            WHERE AppointmentNumber LIKE @Pattern;

            DECLARE @NextSeq INT = @MaxSeq + 1;
            DECLARE @NewAppointmentNumber NVARCHAR(30) = 'APT-' + @DatePrefix + '-' + RIGHT('0000' + CAST(@NextSeq AS VARCHAR(4)), 4);

            INSERT INTO dbo.Appointments 
                (AppointmentNumber, PatientId, DoctorId, AppointmentDate, 
                 ScheduledStart, ScheduledEnd, Status, Reason, CreatedByUserId, CreatedAt)
            OUTPUT INSERTED.AppointmentId
            VALUES 
                (@NewAppointmentNumber, @PatientId, @DoctorId, @AppointmentDate, 
                 @StartTime, @EndTime, N'Booked', @Reason, @CreatedByUserId, SYSDATETIME());

            COMMIT TRANSACTION;
            SELECT @NewAppointmentNumber;";

        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
        command.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
        command.Parameters.Add("@AppointmentDate", SqlDbType.Date).Value = appointmentDate.Date;
        command.Parameters.Add("@StartTime", SqlDbType.Time).Value = startTime;
        command.Parameters.Add("@EndTime", SqlDbType.Time).Value = endTime;
        command.Parameters.Add("@Reason", SqlDbType.NVarChar, 250).Value = (object?)reason ?? DBNull.Value;
        command.Parameters.Add("@CreatedByUserId", SqlDbType.Int).Value = createdByUserId > 0 ? createdByUserId : 1;

        connection.Open();
        using var reader = command.ExecuteReader();
        int appointmentId = 0;
        string appointmentNumber = string.Empty;

        if (reader.Read())
        {
            appointmentId = reader.GetInt32(0);
        }
        if (reader.NextResult() && reader.Read())
        {
            appointmentNumber = reader.GetString(0);
        }

        return (appointmentId, appointmentNumber);
    }

    private static bool _schemaInitialized = false;
    private static readonly object _schemaLock = new();

    public AppointmentRepository()
    {
        EnsureSchema();
    }

    public static void EnsureSchema()
    {
        if (_schemaInitialized) return;
        lock (_schemaLock)
        {
            if (_schemaInitialized) return;
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Appointments') AND name = 'DisplacementReason')
BEGIN
    ALTER TABLE dbo.Appointments ADD DisplacementReason NVARCHAR(250) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Appointments') AND name = 'ContactStatus')
BEGIN
    ALTER TABLE dbo.Appointments ADD ContactStatus NVARCHAR(100) NULL CONSTRAINT DF_Appointments_ContactStatus DEFAULT ('Pending Outreach');
END

IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Appointments_Status')
BEGIN
    ALTER TABLE dbo.Appointments DROP CONSTRAINT CK_Appointments_Status;
    ALTER TABLE dbo.Appointments WITH CHECK ADD CONSTRAINT CK_Appointments_Status CHECK 
        (Status IN (N'Booked', N'Confirmed', N'Waiting', N'Called', N'In Service', N'In Consultation', N'Completed', N'Cancelled', N'No-Show', N'No Show', N'Pending Payment', N'Paid', N'Displaced'));
END";
                using var cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                _schemaInitialized = true;
            }
            catch { }
        }
    }

    /// <summary>
    /// Checks in a patient: assigns the next sequential QueueNumber (Q-01, Q-02) for today
    /// and moves their status to 'Waiting' in both AppointmentQueue and Appointments.
    /// </summary>
    public (bool Success, string Message, int QueueNumber) CheckInPatient(int appointmentId, int patientId, int doctorId)
    {
        const string sql = @"
            BEGIN TRANSACTION;

            DECLARE @CurrentStatus NVARCHAR(50);
            SELECT @CurrentStatus = Status FROM dbo.Appointments WHERE AppointmentId = @AppointmentId;
            IF @CurrentStatus = N'Displaced'
            BEGIN
                COMMIT TRANSACTION;
                SELECT -2;
                RETURN;
            END
            IF @CurrentStatus = N'Cancelled'
            BEGIN
                COMMIT TRANSACTION;
                SELECT -3;
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM dbo.AppointmentQueue WHERE AppointmentId = @AppointmentId)
            BEGIN
                SELECT QueueNumber FROM dbo.AppointmentQueue WHERE AppointmentId = @AppointmentId;
                COMMIT TRANSACTION;
                RETURN;
            END

            DECLARE @AppDate DATE;
            SELECT @AppDate = AppointmentDate FROM dbo.Appointments WHERE AppointmentId = @AppointmentId;
            IF @AppDate IS NULL SET @AppDate = CAST(GETDATE() AS DATE);

            DECLARE @NextQueueNum INT = 1;
            SELECT @NextQueueNum = ISNULL(MAX(QueueNumber), 0) + 1
            FROM dbo.AppointmentQueue WITH (UPDLOCK, HOLDLOCK)
            WHERE QueueDate = @AppDate;

            INSERT INTO dbo.AppointmentQueue (AppointmentId, QueueDate, QueueNumber, QueueStatus)
            VALUES (@AppointmentId, @AppDate, @NextQueueNum, N'Waiting');

            UPDATE dbo.Appointments
            SET Status = N'Waiting', QueueNumber = @NextQueueNum
            WHERE AppointmentId = @AppointmentId;

            -- Initialize patient visit record if not exists
            IF NOT EXISTS (SELECT 1 FROM dbo.PatientVisits WHERE AppointmentId = @AppointmentId)
            BEGIN
                INSERT INTO dbo.PatientVisits (AppointmentId, PatientId, DoctorId, VisitStart, VisitStatus)
                VALUES (@AppointmentId, @PatientId, @DoctorId, SYSDATETIME(), N'Open');
            END

            COMMIT TRANSACTION;
            SELECT @NextQueueNum;";

        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@AppointmentId", SqlDbType.Int).Value = appointmentId;
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
        command.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;

        connection.Open();
        object? result = command.ExecuteScalar();
        int queueNumber = result != null ? Convert.ToInt32(result) : 1;

        if (queueNumber == -2)
        {
            return (false, "This appointment is currently displaced due to doctor emergency. Please use the Emergency Triage Center to transfer or reschedule the patient.", 0);
        }
        if (queueNumber == -3)
        {
            return (false, "Cannot check in a cancelled appointment.", 0);
        }

        return (true, $"Patient checked in successfully. Assigned Ticket: Q-{queueNumber:D2}", queueNumber);
    }

    /// <summary>
    /// Loads today's active appointments and queue items for the Command Board.
    /// Orders by In Service -> Called -> Waiting -> Booked.
    /// </summary>
    public List<QueueBoardItem> GetTodayQueue(int? doctorId = null)
    {
        const string sql = @"
            SELECT 
                ISNULL(q.QueueId, 0) AS QueueId,
                a.AppointmentId,
                a.PatientId,
                a.DoctorId,
                ISNULL(q.QueueNumber, 0) AS QueueNumber,
                p.PatientNumber,
                CONCAT(p.LastName, N', ', p.FirstName) AS PatientName,
                CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
                CONVERT(VARCHAR(5), a.ScheduledStart, 108) AS ScheduledTime,
                ISNULL(q.QueueStatus, a.Status) AS QueueStatus,
                ISNULL(a.Reason, N'General Consultation') AS Reason
            FROM dbo.Appointments a
            INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
            INNER JOIN dbo.Doctors d ON d.DoctorId = a.DoctorId
            LEFT JOIN dbo.AppointmentQueue q ON q.AppointmentId = a.AppointmentId
            WHERE a.AppointmentDate = CAST(GETDATE() AS DATE)
              AND a.Status NOT IN (N'Cancelled')
              AND (@DoctorId IS NULL OR a.DoctorId = @DoctorId)
            ORDER BY 
                CASE ISNULL(q.QueueStatus, a.Status)
                    WHEN N'Displaced'  THEN 1
                    WHEN N'In Service' THEN 2
                    WHEN N'Called'     THEN 3
                    WHEN N'Waiting'    THEN 4
                    WHEN N'Booked'     THEN 5
                    WHEN N'Confirmed'  THEN 5
                    ELSE 6
                END,
                a.ScheduledStart ASC;";

        var list = new List<QueueBoardItem>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DoctorId", SqlDbType.Int).Value = (object?)doctorId ?? DBNull.Value;

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new QueueBoardItem
            {
                QueueId = reader.GetInt32(reader.GetOrdinal("QueueId")),
                AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                QueueNumber = reader.GetInt32(reader.GetOrdinal("QueueNumber")),
                PatientNumber = reader.GetString(reader.GetOrdinal("PatientNumber")),
                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                DoctorName = reader.GetString(reader.GetOrdinal("DoctorName")),
                ScheduledTime = reader.GetString(reader.GetOrdinal("ScheduledTime")),
                QueueStatus = reader.GetString(reader.GetOrdinal("QueueStatus")),
                Reason = reader.GetString(reader.GetOrdinal("Reason"))
            });
        }

        return list;
    }

    public List<PatientLookupItem> SearchPatients(string search, int top = 20)
    {
        const string sql = @"
            SELECT TOP (@Top)
                p.PatientId,
                p.PatientNumber,
                CONCAT(p.LastName, N', ', p.FirstName) AS FullName,
                p.ContactNumber
            FROM dbo.Patients p
            WHERE p.IsActive = 1
              AND (@Search = N'' 
                   OR p.PatientNumber LIKE @Pattern 
                   OR p.FirstName LIKE @Pattern 
                   OR p.LastName LIKE @Pattern 
                   OR p.ContactNumber LIKE @Pattern)
            ORDER BY p.LastName, p.FirstName;";

        var list = new List<PatientLookupItem>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = search.Trim();
        command.Parameters.Add("@Pattern", SqlDbType.NVarChar, 102).Value = $"%{search.Trim()}%";
        command.Parameters.Add("@Top", SqlDbType.Int).Value = top;

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new PatientLookupItem
            {
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                PatientNumber = reader.GetString(reader.GetOrdinal("PatientNumber")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                ContactNumber = reader.GetString(reader.GetOrdinal("ContactNumber")),
                IsAlreadyBookedToday = false
            });
        }

        return list;
    }

    /// <summary>
    /// Checks all active bookings for a patient on a specific date to detect same-doctor and multi-doctor appointments.
    /// </summary>
    public List<PatientBookingConflictItem> GetPatientBookingsForDate(int patientId, DateTime date)
    {
        const string sql = @"
            SELECT 
                a.AppointmentId,
                a.DoctorId,
                CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
                ISNULL(d.Specialty, N'General Medicine') AS Specialty,
                ISNULL(q.QueueNumber, 0) AS QueueNumber,
                ISNULL(q.QueueStatus, a.Status) AS QueueStatus,
                CONVERT(VARCHAR(5), a.ScheduledStart, 108) AS ScheduledTime
            FROM dbo.Appointments a
            INNER JOIN dbo.Doctors d ON d.DoctorId = a.DoctorId
            LEFT JOIN dbo.AppointmentQueue q ON q.AppointmentId = a.AppointmentId
            WHERE a.PatientId = @PatientId
              AND a.AppointmentDate = CAST(@Date AS DATE)
              AND a.Status NOT IN (N'Cancelled');";

        var list = new List<PatientBookingConflictItem>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
        command.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new PatientBookingConflictItem
            {
                AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                DoctorName = reader.GetString(reader.GetOrdinal("DoctorName")),
                Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                QueueNumber = reader.GetInt32(reader.GetOrdinal("QueueNumber")),
                QueueStatus = reader.GetString(reader.GetOrdinal("QueueStatus")),
                ScheduledTime = reader.GetString(reader.GetOrdinal("ScheduledTime"))
            });
        }

        return list;
    }

    public List<DoctorLookupItem> GetActiveDoctors()
    {
        const string sql = @"
            SELECT 
                d.DoctorId,
                CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
                ISNULL(d.Specialty, N'Family Medicine') AS Specialty
            FROM dbo.Doctors d
            WHERE d.IsActive = 1
            ORDER BY d.LastName, d.FirstName;";

        var list = new List<DoctorLookupItem>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new DoctorLookupItem
            {
                DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                DoctorName = reader.GetString(reader.GetOrdinal("DoctorName")),
                Specialty = reader.GetString(reader.GetOrdinal("Specialty"))
            });
        }

        return list;
    }

    public HashSet<TimeSpan> GetBookedSlots(int doctorId, DateTime appointmentDate)
    {
        const string sql = @"
            SELECT ScheduledStart 
            FROM dbo.Appointments 
            WHERE DoctorId = @DoctorId 
              AND AppointmentDate = @AppointmentDate 
              AND Status NOT IN (N'Cancelled');";

        var booked = new HashSet<TimeSpan>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
        command.Parameters.Add("@AppointmentDate", SqlDbType.Date).Value = appointmentDate.Date;

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            booked.Add(reader.GetTimeSpan(0));
        }

        return booked;
    }

    public void UpdateQueueStatus(int queueId, string newStatus)
    {
        const string sql = @"
            UPDATE dbo.AppointmentQueue 
            SET QueueStatus = @Status,
                CalledAt = CASE WHEN @Status = N'Called' THEN SYSDATETIME() ELSE CalledAt END
            WHERE QueueId = @QueueId;

            UPDATE dbo.Appointments
            SET Status = @Status
            WHERE AppointmentId = (SELECT AppointmentId FROM dbo.AppointmentQueue WHERE QueueId = @QueueId);";

        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = newStatus;
        command.Parameters.Add("@QueueId", SqlDbType.Int).Value = queueId;

        connection.Open();
        command.ExecuteNonQuery();
    }

    public List<DisplacedAppointmentItem> GetDisplacedAppointments(DateTime? date = null, int? doctorId = null)
    {
        EnsureSchema();
        DateTime targetDate = date?.Date ?? DateTime.Today;
        const string sql = @"
            SELECT 
                a.AppointmentId,
                a.AppointmentNumber,
                a.PatientId,
                CONCAT(p.LastName, N', ', p.FirstName) AS PatientName,
                p.PatientNumber,
                ISNULL(p.ContactNumber, N'N/A') AS ContactNumber,
                a.DoctorId,
                CONCAT(N'Dr. ', d.FirstName, N' ', d.LastName) AS DoctorName,
                ISNULL(d.Specialty, N'Family Medicine') AS Specialty,
                a.AppointmentDate,
                a.ScheduledStart,
                a.ScheduledEnd,
                ISNULL(a.DisplacementReason, N'Doctor Emergency Blockout') AS DisplacementReason,
                ISNULL(a.ContactStatus, N'Pending Outreach') AS ContactStatus
            FROM dbo.Appointments a
            INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
            INNER JOIN dbo.Doctors d ON d.DoctorId = a.DoctorId
            WHERE a.Status = N'Displaced'
              AND a.AppointmentDate = @TargetDate
              AND (@DoctorId IS NULL OR a.DoctorId = @DoctorId)
            ORDER BY a.ScheduledStart ASC;";

        var list = new List<DisplacedAppointmentItem>();
        using var connection = DatabaseConnection.CreateConnection();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@TargetDate", SqlDbType.Date).Value = targetDate;
        command.Parameters.Add("@DoctorId", SqlDbType.Int).Value = (object?)doctorId ?? DBNull.Value;

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new DisplacedAppointmentItem
            {
                AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                AppointmentNumber = reader.GetString(reader.GetOrdinal("AppointmentNumber")),
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                PatientNumber = reader.GetString(reader.GetOrdinal("PatientNumber")),
                ContactNumber = reader.GetString(reader.GetOrdinal("ContactNumber")),
                DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                DoctorName = reader.GetString(reader.GetOrdinal("DoctorName")),
                Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                ScheduledStart = (TimeSpan)reader.GetValue(reader.GetOrdinal("ScheduledStart")),
                ScheduledEnd = (TimeSpan)reader.GetValue(reader.GetOrdinal("ScheduledEnd")),
                DisplacementReason = reader.GetString(reader.GetOrdinal("DisplacementReason")),
                ContactStatus = reader.GetString(reader.GetOrdinal("ContactStatus"))
            });
        }
        return list;
    }

    public int DisplaceAppointments(List<int> appointmentIds, string emergencyReason, int userId)
    {
        EnsureSchema();
        if (appointmentIds == null || appointmentIds.Count == 0) return 0;

        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        try
        {
            int count = 0;
            foreach (var apptId in appointmentIds)
            {
                const string sql = @"
                    -- Remove active queue entry if any
                    DELETE FROM dbo.AppointmentQueue WHERE AppointmentId = @ApptId;

                    -- Update appointment status to Displaced
                    UPDATE dbo.Appointments
                    SET Status = N'Displaced',
                        DisplacementReason = @Reason,
                        ContactStatus = N'Pending Outreach',
                        QueueNumber = NULL
                    WHERE AppointmentId = @ApptId
                      AND Status NOT IN (N'Cancelled', N'Completed');";

                using var cmd = new SqlCommand(sql, connection, tx);
                cmd.Parameters.Add("@ApptId", SqlDbType.Int).Value = apptId;
                cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 250).Value = emergencyReason;
                count += cmd.ExecuteNonQuery();
            }
            tx.Commit();
            return count;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public bool TransferToPeerDoctor(int appointmentId, int targetDoctorId, TimeSpan newStart, TimeSpan newEnd, string notes, int userId)
    {
        EnsureSchema();
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlCheck = @"
                SELECT COUNT(*) FROM dbo.Appointments
                WHERE DoctorId = @TargetDoctorId
                  AND AppointmentDate = (SELECT AppointmentDate FROM dbo.Appointments WHERE AppointmentId = @ApptId)
                  AND ScheduledStart = @NewStart
                  AND Status NOT IN (N'Cancelled', N'Displaced')
                  AND AppointmentId != @ApptId;";

            using (var cmdCheck = new SqlCommand(sqlCheck, connection, tx))
            {
                cmdCheck.Parameters.Add("@TargetDoctorId", SqlDbType.Int).Value = targetDoctorId;
                cmdCheck.Parameters.Add("@ApptId", SqlDbType.Int).Value = appointmentId;
                cmdCheck.Parameters.Add("@NewStart", SqlDbType.Time).Value = newStart;
                int conflict = Convert.ToInt32(cmdCheck.ExecuteScalar());
                if (conflict > 0)
                {
                    tx.Rollback();
                    return false;
                }
            }

            const string sqlUpdate = @"
                UPDATE dbo.Appointments
                SET DoctorId = @TargetDoctorId,
                    ScheduledStart = @NewStart,
                    ScheduledEnd = @NewEnd,
                    Status = N'Booked',
                    ContactStatus = N'Contacted - Transferred',
                    Reason = CASE 
                        WHEN @Notes IS NOT NULL AND LEN(@Notes) > 0 THEN CONCAT(Reason, N' [Transferred: ', @Notes, N']')
                        ELSE Reason 
                    END
                WHERE AppointmentId = @ApptId;";

            using (var cmdUp = new SqlCommand(sqlUpdate, connection, tx))
            {
                cmdUp.Parameters.Add("@TargetDoctorId", SqlDbType.Int).Value = targetDoctorId;
                cmdUp.Parameters.Add("@NewStart", SqlDbType.Time).Value = newStart;
                cmdUp.Parameters.Add("@NewEnd", SqlDbType.Time).Value = newEnd;
                cmdUp.Parameters.Add("@ApptId", SqlDbType.Int).Value = appointmentId;
                cmdUp.Parameters.Add("@Notes", SqlDbType.NVarChar, 250).Value = (object?)notes ?? DBNull.Value;
                cmdUp.ExecuteNonQuery();
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public bool RescheduleAppointment(int appointmentId, DateTime newDate, TimeSpan newStart, TimeSpan newEnd, string notes, int userId)
    {
        EnsureSchema();
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        try
        {
            const string sqlUpdate = @"
                UPDATE dbo.Appointments
                SET AppointmentDate = @NewDate,
                    ScheduledStart = @NewStart,
                    ScheduledEnd = @NewEnd,
                    Status = N'Booked',
                    ContactStatus = N'Contacted - Rescheduled',
                    Reason = CASE 
                        WHEN @Notes IS NOT NULL AND LEN(@Notes) > 0 THEN CONCAT(Reason, N' [Rescheduled: ', @Notes, N']')
                        ELSE Reason 
                    END
                WHERE AppointmentId = @ApptId;";

            using (var cmdUp = new SqlCommand(sqlUpdate, connection, tx))
            {
                cmdUp.Parameters.Add("@NewDate", SqlDbType.Date).Value = newDate.Date;
                cmdUp.Parameters.Add("@NewStart", SqlDbType.Time).Value = newStart;
                cmdUp.Parameters.Add("@NewEnd", SqlDbType.Time).Value = newEnd;
                cmdUp.Parameters.Add("@ApptId", SqlDbType.Int).Value = appointmentId;
                cmdUp.Parameters.Add("@Notes", SqlDbType.NVarChar, 250).Value = (object?)notes ?? DBNull.Value;
                cmdUp.ExecuteNonQuery();
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public bool UpdateContactStatus(int appointmentId, string contactStatus, int userId)
    {
        EnsureSchema();
        using var connection = DatabaseConnection.CreateConnection();
        connection.Open();
        const string sql = @"
            UPDATE dbo.Appointments
            SET ContactStatus = @ContactStatus
            WHERE AppointmentId = @ApptId;";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@ContactStatus", SqlDbType.NVarChar, 100).Value = contactStatus;
        cmd.Parameters.Add("@ApptId", SqlDbType.Int).Value = appointmentId;
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<PeerDoctorAvailableSlotItem> GetAvailablePeerDoctorSlots(int currentDoctorId, DateTime date)
    {
        EnsureSchema();
        var result = new List<PeerDoctorAvailableSlotItem>();
        try
        {
            var doctors = GetActiveDoctors().Where(d => d.DoctorId != currentDoctorId).ToList();
            var docScheduleRepo = new DoctorScheduleRepository();

            foreach (var doc in doctors)
            {
                if (docScheduleRepo.IsDayOff(doc.DoctorId, date))
                    continue;

                var routine = docScheduleRepo.GetWeeklySchedule(doc.DoctorId);
                var daySched = routine.FirstOrDefault(r => r.DayOfWeek == (int)date.DayOfWeek);
                TimeSpan start = (daySched != null && daySched.IsActive) ? daySched.StartTime : new TimeSpan(9, 0, 0);
                TimeSpan end = (daySched != null && daySched.IsActive) ? daySched.EndTime : new TimeSpan(14, 0, 0);
                int slotMinutes = (daySched != null && daySched.SlotDurationMinutes > 0) ? daySched.SlotDurationMinutes : 30;

                var booked = GetBookedSlots(doc.DoctorId, date);
                var blocked = docScheduleRepo.GetBlockedSlots(doc.DoctorId, date);

                TimeSpan cur = start;
                while (cur < end)
                {
                    TimeSpan next = cur.Add(TimeSpan.FromMinutes(slotMinutes));
                    if (next > end) break;

                    if (!booked.Contains(cur) && !blocked.Contains(cur))
                    {
                        result.Add(new PeerDoctorAvailableSlotItem
                        {
                            DoctorId = doc.DoctorId,
                            DoctorName = doc.DoctorName,
                            Specialty = doc.Specialty,
                            StartTime = cur,
                            EndTime = next
                        });
                    }
                    cur = next;
                }
            }
        }
        catch { }
        return result;
    }

    public List<DoctorNextAvailableSlotItem> GetDoctorNextAvailableSlots(int doctorId, DateTime fromDate)
    {
        EnsureSchema();
        var result = new List<DoctorNextAvailableSlotItem>();
        try
        {
            var docScheduleRepo = new DoctorScheduleRepository();
            var doctors = GetActiveDoctors();
            var doc = doctors.FirstOrDefault(d => d.DoctorId == doctorId);
            string docName = doc?.DoctorName ?? "Attending Doctor";

            var routine = docScheduleRepo.GetWeeklySchedule(doctorId);

            for (int i = 1; i <= 14 && result.Count < 20; i++)
            {
                DateTime nextDate = fromDate.Date.AddDays(i);
                if (docScheduleRepo.IsDayOff(doctorId, nextDate))
                    continue;

                var daySched = routine.FirstOrDefault(r => r.DayOfWeek == (int)nextDate.DayOfWeek);
                if (daySched != null && !daySched.IsActive)
                    continue;

                TimeSpan start = (daySched != null && daySched.IsActive) ? daySched.StartTime : new TimeSpan(9, 0, 0);
                TimeSpan end = (daySched != null && daySched.IsActive) ? daySched.EndTime : new TimeSpan(14, 0, 0);
                int slotMinutes = (daySched != null && daySched.SlotDurationMinutes > 0) ? daySched.SlotDurationMinutes : 30;

                var booked = GetBookedSlots(doctorId, nextDate);
                var blocked = docScheduleRepo.GetBlockedSlots(doctorId, nextDate);

                TimeSpan cur = start;
                while (cur < end && result.Count < 20)
                {
                    TimeSpan next = cur.Add(TimeSpan.FromMinutes(slotMinutes));
                    if (next > end) break;

                    if (!booked.Contains(cur) && !blocked.Contains(cur))
                    {
                        result.Add(new DoctorNextAvailableSlotItem
                        {
                            DoctorId = doctorId,
                            DoctorName = docName,
                            Date = nextDate,
                            StartTime = cur,
                            EndTime = next
                        });
                    }
                    cur = next;
                }
            }
        }
        catch { }
        return result;
    }
}