USE ClinicFlowUnifiedDb;
GO

CREATE TABLE dbo.Roles(
	RoleId INT IDENTITY(1,1) NOT NULL,
	RoleName NVARCHAR(50) NOT NULL,
	Description NVARCHAR(200) NOT NULL,
	IsActive BIT NOT NULL
		CONSTRAINT DF_Roles_IsActive DEFAULT (1),
	CreatedAT DATETIME2(0) NOT NULL
		CONSTRAINT DF_Roles_CreatedAt DEFAULT (SYSUTCDATETIME()),

	CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
	CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
GO

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1, 1) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(512) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt DATETIME2(0) NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt DATETIME2(0) NULL,

    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

CREATE TABLE dbo.AuditLogs
(
    AuditLogId BIGINT IDENTITY(1, 1) NOT NULL,
    UserId INT NULL,
    ActionType NVARCHAR(50) NOT NULL,
    EntityName NVARCHAR(100) NOT NULL,
    RecordId NVARCHAR(50) NULL,
    Description NVARCHAR(500) NOT NULL,
    MachineName NVARCHAR(100) NULL,
    BeforeJson NVARCHAR(MAX) NULL,
    AfterJson NVARCHAR(MAX) NULL,
    LoggedAt DATETIME2(0) NOT NULL
        CONSTRAINT DF_AuditLogs_LoggedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditLogId),
    CONSTRAINT FK_AuditLogs_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE INDEX IX_Users_RoleId
    ON dbo.Users(RoleId);
GO

CREATE INDEX IX_AuditLogs_LoggedAt
    ON dbo.AuditLogs(LoggedAt DESC);
GO