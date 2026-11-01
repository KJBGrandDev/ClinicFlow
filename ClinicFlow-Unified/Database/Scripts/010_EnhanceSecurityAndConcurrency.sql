USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   010: Enhance Security (Account Lockout & Roles) and Concurrency (RowVersion)
   ========================================================================== */

-- 1. Add FailedLoginAttempts and LockoutUntil to dbo.Users
IF COL_LENGTH(N'dbo.Users', N'FailedLoginAttempts') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD FailedLoginAttempts INT NOT NULL CONSTRAINT DF_Users_FailedLoginAttempts DEFAULT (0);
    PRINT 'Added FailedLoginAttempts column to dbo.Users.';
END;
GO

IF COL_LENGTH(N'dbo.Users', N'LockoutUntil') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD LockoutUntil DATETIME2(0) NULL;
    PRINT 'Added LockoutUntil column to dbo.Users.';
END;
GO

IF COL_LENGTH(N'dbo.Users', N'PasswordSalt') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PasswordSalt NVARCHAR(200) NULL;
    PRINT 'Added PasswordSalt column to dbo.Users.';
END;
GO

-- 2. Add RowVersion to dbo.Patients for optimistic concurrency checking (REQ-025)
IF COL_LENGTH(N'dbo.Patients', N'RowVersion') IS NULL
BEGIN
    ALTER TABLE dbo.Patients ADD RowVersion ROWVERSION;
    PRINT 'Added RowVersion column to dbo.Patients.';
END;
GO

-- 3. Add RowVersion to dbo.Appointments for optimistic concurrency checking (REQ-025)
IF COL_LENGTH(N'dbo.Appointments', N'RowVersion') IS NULL
BEGIN
    ALTER TABLE dbo.Appointments ADD RowVersion ROWVERSION;
    PRINT 'Added RowVersion column to dbo.Appointments.';
END;
GO

-- 4. Ensure Central ErrorLogs table exists (REQ-003, REQ-024)
IF OBJECT_ID(N'dbo.ErrorLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ErrorLogs
    (
        ErrorLogId BIGINT IDENTITY(1, 1) NOT NULL,
        UserId INT NULL,
        MachineName NVARCHAR(100) NULL,
        Source NVARCHAR(200) NOT NULL,
        ErrorMessage NVARCHAR(MAX) NOT NULL,
        StackTrace NVARCHAR(MAX) NULL,
        LoggedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ErrorLogs_LoggedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ErrorLogs PRIMARY KEY (ErrorLogId)
    );

    CREATE INDEX IX_ErrorLogs_LoggedAt ON dbo.ErrorLogs(LoggedAt DESC);
    PRINT 'Created dbo.ErrorLogs table.';
END;
GO

-- 5. Ensure core roles exist in dbo.Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Admin')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Admin', 'Full system administration');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Doctor')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Doctor', 'Licensed physician consultation and prescription portal');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Secretary')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Secretary', 'Front-desk reception, appointments, triage, and cashier');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Receptionist')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Receptionist', 'Front-desk patient lobby and appointment check-in');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Pharmacist')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Pharmacist', 'Pharmacy medication dispensing and stock inventory control');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Laboratory Staff')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Laboratory Staff', 'Clinical diagnostic laboratory test fulfillment and result recording');
GO

PRINT '010_EnhanceSecurityAndConcurrency script completed successfully.';
GO
