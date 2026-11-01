USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   009: Add UpdatedAt column to Patients table
   ========================================================================== */

IF COL_LENGTH(N'dbo.Patients', N'UpdatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.Patients ADD UpdatedAt DATETIME2(0) NULL;
    PRINT 'Added UpdatedAt column to dbo.Patients.';
END;
GO