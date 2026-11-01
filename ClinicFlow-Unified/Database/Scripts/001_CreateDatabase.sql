USE master;
GO

IF DB_ID(N'ClinicFlowUnifiedDb') IS NULL
BEGIN
	CREATE DATABASE ClinicFlowUnifiedDb;
END;
GO

USE ClinicFlowUnifiedDb;
GO

SELECT DB_NAME() AS CurrentDatabase;
GO