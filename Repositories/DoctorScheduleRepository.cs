#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicFlow_Unified.Data;

namespace ClinicFlow_Unified.Repositories
{
    public sealed class WeeklyDaySchedule
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; } = new(8, 0, 0);
        public TimeSpan EndTime { get; set; } = new(17, 0, 0);
        public int SlotDurationMinutes { get; set; } = 30;
        public bool IsActive { get; set; } = true;
    }

    public sealed class DoctorScheduleRepository
    {
        public List<WeeklyDaySchedule> GetWeeklySchedule(int doctorId)
        {
            const string sql = "SELECT DayOfWeek, StartTime, EndTime, SlotDurationMinutes, IsActive FROM dbo.DoctorSchedules WHERE DoctorId = @DoctorId;";
            var list = new List<WeeklyDaySchedule>();
            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new WeeklyDaySchedule
                {
                    DayOfWeek = Convert.ToInt32(rdr.GetByte(0)),
                    StartTime = rdr.GetTimeSpan(1),
                    EndTime = rdr.GetTimeSpan(2),
                    SlotDurationMinutes = Convert.ToInt32(rdr.GetInt16(3)),
                    IsActive = rdr.GetBoolean(4)
                });
            }
            return list;
        }

        public void SaveWeeklySchedule(int doctorId, List<int> activeDays, TimeSpan start, TimeSpan end, int duration)
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                const string delSql = "DELETE FROM dbo.DoctorSchedules WHERE DoctorId = @DoctorId;";
                using (var cmdDel = new SqlCommand(delSql, conn, tran))
                {
                    cmdDel.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmdDel.ExecuteNonQuery();
                }

                const string insSql = @"INSERT INTO dbo.DoctorSchedules (DoctorId, DayOfWeek, StartTime, EndTime, SlotDurationMinutes, MaxPatientsPerSlot, EffectiveFrom, IsActive, CreatedAt)
VALUES (@DoctorId, @DayOfWeek, @StartTime, @EndTime, @Duration, 1, CAST(GETDATE() AS DATE), 1, SYSUTCDATETIME());";

                foreach (int day in activeDays)
                {
                    using var cmdIns = new SqlCommand(insSql, conn, tran);
                    cmdIns.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                    cmdIns.Parameters.Add("@DayOfWeek", SqlDbType.TinyInt).Value = (byte)day;
                    cmdIns.Parameters.Add("@StartTime", SqlDbType.Time).Value = start;
                    cmdIns.Parameters.Add("@EndTime", SqlDbType.Time).Value = end;
                    cmdIns.Parameters.Add("@Duration", SqlDbType.SmallInt).Value = (short)duration;
                    cmdIns.ExecuteNonQuery();
                }
                tran.Commit();
            }
            catch { tran.Rollback(); throw; }
        }

        public bool IsDayOff(int doctorId, DateTime date)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime IS NULL;";
            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
            cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public HashSet<TimeSpan> GetBlockedSlots(int doctorId, DateTime date)
        {
            const string sql = "SELECT StartTime FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime IS NOT NULL;";
            var set = new HashSet<TimeSpan>();
            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
            cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read()) { set.Add(rdr.GetTimeSpan(0)); }
            return set;
        }

        public void ToggleDayOff(int doctorId, DateTime date, bool block)
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            if (block)
            {
                const string insSql = @"IF NOT EXISTS (SELECT 1 FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime IS NULL)
INSERT INTO dbo.DoctorScheduleExceptions (DoctorId, ExceptionDate, StartTime, EndTime, Reason, CreatedAt)
VALUES (@DoctorId, @Date, NULL, NULL, N'Day Off / Leave', SYSUTCDATETIME());";
                using var cmd = new SqlCommand(insSql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.ExecuteNonQuery();
            }
            else
            {
                const string delSql = "DELETE FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime IS NULL;";
                using var cmd = new SqlCommand(delSql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.ExecuteNonQuery();
            }
        }

        public void ToggleSlotBlock(int doctorId, DateTime date, TimeSpan slotStart, TimeSpan slotEnd, bool block)
        {
            using var conn = DatabaseConnection.CreateConnection();
            conn.Open();
            if (block)
            {
                const string insSql = @"IF NOT EXISTS (SELECT 1 FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime = @StartTime)
INSERT INTO dbo.DoctorScheduleExceptions (DoctorId, ExceptionDate, StartTime, EndTime, Reason, CreatedAt)
VALUES (@DoctorId, @Date, @StartTime, @EndTime, N'Slot Blocked', SYSUTCDATETIME());";
                using var cmd = new SqlCommand(insSql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = slotStart;
                cmd.Parameters.Add("@EndTime", SqlDbType.Time).Value = slotEnd;
                cmd.ExecuteNonQuery();
            }
            else
            {
                const string delSql = "DELETE FROM dbo.DoctorScheduleExceptions WHERE DoctorId = @DoctorId AND ExceptionDate = @Date AND StartTime = @StartTime;";
                using var cmd = new SqlCommand(delSql, conn);
                cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
                cmd.Parameters.Add("@StartTime", SqlDbType.Time).Value = slotStart;
                cmd.ExecuteNonQuery();
            }
        }

        public int GetBookedPatientCount(int doctorId, DateTime date)
        {
            const string sql = "SELECT COUNT(1) FROM dbo.Appointments WHERE DoctorId = @DoctorId AND AppointmentDate = @Date AND Status NOT IN ('Cancelled', 'No-Show');";
            using var conn = DatabaseConnection.CreateConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@DoctorId", SqlDbType.Int).Value = doctorId;
            cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}