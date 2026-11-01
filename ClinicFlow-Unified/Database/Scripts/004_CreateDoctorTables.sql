USE ClinicFlowUnifiedDb;
GO

CREATE TABLE dbo.Doctors
(
    DoctorId INT IDENTITY(1, 1) NOT NULL,
    DoctorCode NVARCHAR(20) NOT NULL,
    UserId INT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NOT NULL,
    Suffix NVARCHAR(20) NULL,
    LicenseNumber NVARCHAR(50) NULL,
    Specialty NVARCHAR(100) NULL,
    ContactNumber NVARCHAR(30) NULL,
    EmailAddress NVARCHAR(150) NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_Doctors_IsActive DEFAULT (1),
    CreatedAt DATETIME2(0) NOT NULL
        CONSTRAINT DF_Doctors_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2(0) NULL,

    CONSTRAINT PK_Doctors PRIMARY KEY (DoctorId),
    CONSTRAINT UQ_Doctors_DoctorCode UNIQUE (DoctorCode),
    CONSTRAINT FK_Doctors_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE UNIQUE INDEX UX_Doctors_UserId
    ON dbo.Doctors(UserId)
    WHERE UserId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_Doctors_LicenseNumber
    ON dbo.Doctors(LicenseNumber)
    WHERE LicenseNumber IS NOT NULL;
GO

CREATE TABLE dbo.DoctorSchedules
(
    DoctorScheduleId INT IDENTITY(1, 1) NOT NULL,
    DoctorId INT NOT NULL,
    DayOfWeek TINYINT NOT NULL,
    StartTime TIME(0) NOT NULL,
    EndTime TIME(0) NOT NULL,
    SlotDurationMinutes SMALLINT NOT NULL,
    MaxPatientsPerSlot SMALLINT NOT NULL
        CONSTRAINT DF_DoctorSchedules_MaxPatientsPerSlot DEFAULT (1),
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_DoctorSchedules_IsActive DEFAULT (1),
    CreatedAt DATETIME2(0) NOT NULL
        CONSTRAINT DF_DoctorSchedules_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2(0) NULL,

    CONSTRAINT PK_DoctorSchedules PRIMARY KEY (DoctorScheduleId),
    CONSTRAINT FK_DoctorSchedules_Doctors
        FOREIGN KEY (DoctorId) REFERENCES dbo.Doctors(DoctorId),

    CONSTRAINT CK_DoctorSchedules_DayOfWeek
        CHECK (DayOfWeek BETWEEN 0 AND 6),

    CONSTRAINT CK_DoctorSchedules_TimeRange
        CHECK (EndTime > StartTime),

    CONSTRAINT CK_DoctorSchedules_SlotDuration
        CHECK (SlotDurationMinutes BETWEEN 5 AND 240),

    CONSTRAINT CK_DoctorSchedules_Capacity
        CHECK (MaxPatientsPerSlot >= 1),

    CONSTRAINT CK_DoctorSchedules_EffectiveDates
        CHECK (EffectiveTo IS NULL OR EffectiveTo >= EffectiveFrom)
);
GO

CREATE INDEX IX_DoctorSchedules_DoctorId_DayOfWeek
    ON dbo.DoctorSchedules(DoctorId, DayOfWeek);
GO