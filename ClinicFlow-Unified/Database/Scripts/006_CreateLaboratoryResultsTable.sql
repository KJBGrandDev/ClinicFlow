USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   006: Create Laboratory Results Table
   ========================================================================== */

IF OBJECT_ID(N'dbo.LaboratoryResults', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LaboratoryResults
    (
        LaboratoryResultId INT IDENTITY(1, 1) NOT NULL,
        LaboratoryRequestId INT NOT NULL,
        ResultText NVARCHAR(MAX) NOT NULL,
        ResultAttachmentPath NVARCHAR(500) NULL,
        ResultStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_LaboratoryResults_Status DEFAULT (N'Final'),
        PerformedByUserId INT NULL,
        ResultDate DATETIME2(0) NOT NULL CONSTRAINT DF_LaboratoryResults_Date DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_LaboratoryResults PRIMARY KEY (LaboratoryResultId),
        CONSTRAINT UQ_LaboratoryResults_Request UNIQUE (LaboratoryRequestId),
        CONSTRAINT FK_LaboratoryResults_Requests FOREIGN KEY (LaboratoryRequestId) REFERENCES dbo.LaboratoryRequests(LaboratoryRequestId),
        CONSTRAINT FK_LaboratoryResults_Users FOREIGN KEY (PerformedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_LaboratoryResults_Status CHECK (ResultStatus IN (N'Preliminary', N'Final', N'External'))
    );
END;
GO