#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;

namespace ClinicFlow_Unified.Repositories
{
    public sealed class WeeklyScheduleItem
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public bool IsActive { get; set; } = true;
    }

    public sealed class DoctorScheduleRepository
    {
        private static bool _tablesInitialized = false;
        private static readonly object _initLock = new();

        public DoctorScheduleRepository()
        {
            EnsureTablesCreated();
        }

        private static void EnsureTablesCreated()
        {
            if (_tablesInitialized) return;
            lock (_initLock)
            {
                if (_tablesInitialized) return;
                try
                {
                    using var conn = DatabaseConnection.CreateConnection();
                    conn.Open();
                    const string sql = @"
IF OBJECT_ID(N'dbo.DoctorScheduleExceptions', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DoctorScheduleExceptions') AND name = 'IsFullDayOff')
        ALTER TABLE dbo.DoctorScheduleExceptions ADD IsFullDayOff BIT NOT NULL DEFAULT 0;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DoctorScheduleExceptions') AND name = 'IsBlocked')
        ALTER TABLE dbo.DoctorScheduleExceptions ADD IsBlocked BIT NOT NULL DEFAULT 1;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DoctorScheduleExceptions') AND name = 'StartTime')
        ALTER TABLE dbo.DoctorScheduleExceptions ADD StartTime TIME NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DoctorScheduleExceptions') AND name = 'EndTime')
        ALTER TABLE dbo.DoctorScheduleExceptions ADD EndTime TIME NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.DoctorScheduleExceptions') AND name = 'Reason')
        ALTER TABLE dbo.DoctorScheduleExceptions ADD Reason NVARCHAR(200) NULL;
END
ELSE
BEGIN
    CREATE TABLE dbo.DoctorScheduleExceptions (
        ExceptionId INT IDENTITY(1,1) PRIMARY KEY,
        DoctorId INT NOT NULL,
        ExceptionDate DATE NOT NULL,
        IsFullDayOff BIT NOT NULL DEFAULT 0,
        StartTime TIME NULL,
        EndTime TIME NULL,
        IsBlocked BIT NOT NULL DEFAULT 1,
        Reason NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DoctorWeeklySchedules')
BEGIN
    CREATE TABLE dbo.DoctorWeeklySchedules (
        ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
        DoctorId INT NOT NULL,
        DayOfWeek INT NOT NULL,
        StartTime TIME NOT NULL DEFAULT '09:00:00',
        EndTime TIME NOT NULL DEFAULT '14:00:00',
        SlotDurationMinutes INT NOT NULL DEFAULT 30,
        IsActive BIT NOT NULL DEFAULT 1,
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_Doctor_Day UNIQUE (DoctorId, DayOfWeek)
    );
END";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    _tablesInitialized = true;
                }
                catch { }
            }
        }

        public List<WeeklyScheduleItem> GetWeeklySchedule(int doctorId)
        {
            var list = new List<WeeklyScheduleItem>();
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = @"
SELECT DayOfWeek, StartTime, EndTime, SlotDurationMinutes, IsActive
FROM dbo.DoctorWeeklySchedules
WHERE DoctorId = @DoctorId
ORDER BY DayOfWeek ASC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new WeeklyScheduleItem
                    {
                        DayOfWeek = rdr.GetInt32(0),
                        StartTime = (TimeSpan)rdr.GetValue(1),
                        EndTime = (TimeSpan)rdr.GetValue(2),
                        SlotDurationMinutes = rdr.GetInt32(3),
                        IsActive = rdr.GetBoolean(4)
                    });
                }
            }
            catch { }
            return list;
        }

        public void SaveWeeklySchedule(int doctorId, List<int> activeDays, TimeSpan startTime, TimeSpan endTime, int slotMinutes)
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                for (int day = 0; day <= 6; day++)
                {
                    bool isActive = activeDays.Contains(day);

                    const string sqlWeekly = @"
MERGE dbo.DoctorWeeklySchedules AS target
USING (SELECT @DoctorId AS DoctorId, @DayOfWeek AS DayOfWeek) AS source
ON (target.DoctorId = source.DoctorId AND target.DayOfWeek = source.DayOfWeek)
WHEN MATCHED THEN
    UPDATE SET StartTime = @StartTime, EndTime = @EndTime, SlotDurationMinutes = @SlotMinutes, IsActive = @IsActive, UpdatedAt = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes, IsActive)
    VALUES (@DoctorId, @DayOfWeek, @StartTime, @EndTime, @SlotMinutes, @IsActive);";

                    using var cmd = new SqlCommand(sqlWeekly, conn, tx);
                    cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmd.Parameters.Add("@DayOfWeek", SqlDbType.Int).Value = day;
                    cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = startTime;
                    cmd.Parameters.Add("@EndTime", SqlDbType.Time).Value = endTime;
                    cmd.Parameters.Add("@SlotMinutes", SqlDbType.Int).Value = slotMinutes;
                    cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public (int TransportedCount, List<string> Details) AutoTransportAffectedPatients(int doctorId, DateTime date, TimeSpan newStart, TimeSpan newEnd, int slotMinutes)
        {
            var details = new List<string>();
            int transported = 0;
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();

            const string sqlAppts = @"
SELECT a.AppointmentId, a.ScheduledStart, a.ScheduledEnd, a.AppointmentNumber, (p.LastName + ', ' + p.FirstName) AS PatientName
FROM dbo.Appointments a
INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
WHERE a.DoctorId = @DoctorId AND a.AppointmentDate = @Date AND a.Status NOT IN ('Cancelled', 'No-Show')
ORDER BY a.ScheduledStart ASC, a.AppointmentId ASC;";

            var allAppts = new List<(int ApptId, TimeSpan Start, TimeSpan End, string Number, string Name)>();
            using (var cmd = new SqlCommand(sqlAppts, conn))
            {
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    allAppts.Add((rdr.GetInt32(0), (TimeSpan)rdr.GetValue(1), (TimeSpan)rdr.GetValue(2), rdr.GetString(3), rdr.GetString(4)));
                }
            }

            var blocked = GetBlockedSlots(doctorId, date);
            var affected = allAppts.Where(a => a.Start < newStart || a.Start >= newEnd || blocked.Contains(a.Start)).ToList();
            if (affected.Count == 0) return (0, details);

            var occupied = allAppts.Except(affected).Select(a => a.Start).ToHashSet();
            var availableSlots = new List<TimeSpan>();
            TimeSpan cur = newStart;
            while (cur < newEnd)
            {
                TimeSpan nxt = cur.Add(TimeSpan.FromMinutes(slotMinutes));
                if (nxt > newEnd) break;
                if (!occupied.Contains(cur) && !blocked.Contains(cur))
                {
                    availableSlots.Add(cur);
                }
                cur = nxt;
            }

            using var tx = conn.BeginTransaction();
            try
            {
                int slotIdx = 0;
                foreach (var pat in affected)
                {
                    if (slotIdx < availableSlots.Count)
                    {
                        TimeSpan targetStart = availableSlots[slotIdx];
                        TimeSpan targetEnd = targetStart.Add(TimeSpan.FromMinutes(slotMinutes));

                        const string sqlUpdate = @"
UPDATE dbo.Appointments
SET ScheduledStart = @NewStart, ScheduledEnd = @NewEnd
WHERE AppointmentId = @ApptId;";

                        using var cmdUp = new SqlCommand(sqlUpdate, conn, tx);
                        cmdUp.Parameters.Add("@NewStart", SqlDbType.Time).Value = targetStart;
                        cmdUp.Parameters.Add("@NewEnd", SqlDbType.Time).Value = targetEnd;
                        cmdUp.Parameters.Add("@ApptId", SqlDbType.Int).Value = pat.ApptId;
                        cmdUp.ExecuteNonQuery();

                        details.Add($"{pat.Name}: Shifted {pat.Start:hh\\:mm} -> {targetStart:hh\\:mm}");
                        transported++;
                        slotIdx++;
                    }
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
            return (transported, details);
        }

        public bool IsDayOff(int doctorId, DateTime date)
        {
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = @"
SELECT TOP 1 1 FROM dbo.DoctorScheduleExceptions
WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND IsFullDayOff = 1;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                var res = cmd.ExecuteScalar();
                return res != null && res != DBNull.Value;
            }
            catch { return false; }
        }

        public void ToggleDayOff(int doctorId, DateTime date, bool isDayOff)
        {
            EnsureTablesCreated();
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            if (isDayOff)
            {
                const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND IsFullDayOff = 1)
BEGIN
    INSERT INTO dbo.DoctorScheduleExceptions (DoctorId, ExceptionDate, IsFullDayOff, IsBlocked, Reason)
    VALUES (@DoctorId, @Date, 1, 1, 'Marked Day Off');
END";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.ExecuteNonQuery();
            }
            else
            {
                const string sql = @"
DELETE FROM dbo.DoctorScheduleExceptions
WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND IsFullDayOff = 1;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.ExecuteNonQuery();
            }
        }

        public int GetBookedPatientCount(int doctorId, DateTime date, TimeSpan? slotTime = null)
        {
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                string sql = @"
SELECT COUNT(*) FROM dbo.Appointments
WHERE DoctorId = @DoctorId AND AppointmentDate = @Date AND Status NOT IN ('Cancelled', 'No-Show')";

                if (slotTime.HasValue) sql += " AND ScheduledStart = @ScheduledStart";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                if (slotTime.HasValue) cmd.Parameters.Add("@ScheduledStart", SqlDbType.Time).Value = slotTime.Value;
                var count = cmd.ExecuteScalar();
                return count != null && count != DBNull.Value ? Convert.ToInt32(count) : 0;
            }
            catch { return 0; }
        }

        public HashSet<TimeSpan> GetBlockedSlots(int doctorId, DateTime date)
        {
            EnsureTablesCreated();
            var set = new HashSet<TimeSpan>();
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                const string sql = @"
SELECT StartTime FROM dbo.DoctorScheduleExceptions
WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND IsFullDayOff = 0 AND IsBlocked = 1 AND StartTime IS NOT NULL;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0)) set.Add((TimeSpan)rdr.GetValue(0));
                }
            }
            catch { }
            return set;
        }

        public void ToggleSlotBlock(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, bool isBlocked)
        {
            EnsureTablesCreated();
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            if (isBlocked)
            {
                const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime = @StartTime)
BEGIN
    INSERT INTO dbo.DoctorScheduleExceptions (DoctorId, ExceptionDate, IsFullDayOff, StartTime, EndTime, IsBlocked, Reason)
    VALUES (@DoctorId, @Date, 0, @StartTime, @EndTime, 1, 'Doctor Blockout');
END";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = startTime;
                cmd.Parameters.Add("@EndTime", SqlDbType.Time).Value = endTime;
                cmd.ExecuteNonQuery();
            }
            else
            {
                const string sql = @"
DELETE FROM dbo.DoctorScheduleExceptions
WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime = @StartTime;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = startTime;
                cmd.ExecuteNonQuery();
            }
        }

        public List<(int AppointmentId, string AppointmentNumber, string PatientName, string PatientNumber, string ContactNumber, TimeSpan Start, TimeSpan End)> GetBookedAppointments(int doctorId, DateTime date, TimeSpan? slotTime = null)
        {
            var list = new List<(int, string, string, string, string, TimeSpan, TimeSpan)>();
            try
            {
                using var conn = DatabaseConnection.CreateConnection();
                conn.Open();
                string sql = @"
SELECT a.AppointmentId, a.AppointmentNumber, (p.LastName + ', ' + p.FirstName) AS PatientName, p.PatientNumber, ISNULL(p.ContactNumber, 'N/A') AS ContactNumber, a.ScheduledStart, a.ScheduledEnd
FROM dbo.Appointments a
INNER JOIN dbo.Patients p ON p.PatientId = a.PatientId
WHERE a.DoctorId = @DoctorId AND a.AppointmentDate = @Date AND a.Status NOT IN ('Cancelled', 'Displaced', 'No-Show')";

                if (slotTime.HasValue) sql += " AND a.ScheduledStart = @ScheduledStart";
                sql += " ORDER BY a.ScheduledStart ASC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                if (slotTime.HasValue) cmd.Parameters.Add("@ScheduledStart", SqlDbType.Time).Value = slotTime.Value;

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add((
                        rdr.GetInt32(0),
                        rdr.GetString(1),
                        rdr.GetString(2),
                        rdr.GetString(3),
                        rdr.GetString(4),
                        (TimeSpan)rdr.GetValue(5),
                        (TimeSpan)rdr.GetValue(6)
                    ));
                }
            }
            catch { }
            return list;
        }
    }
}