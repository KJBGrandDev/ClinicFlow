USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   007: Add MiddleName and Suffix to Patients Table
   ========================================================================== */

IF COL_LENGTH(N'dbo.Patients', N'MiddleName') IS NULL
BEGIN
    ALTER TABLE dbo.Patients ADD MiddleName NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH(N'dbo.Patients', N'Suffix') IS NULL
BEGIN
    ALTER TABLE dbo.Patients ADD Suffix NVARCHAR(20) NULL;
END;
GO