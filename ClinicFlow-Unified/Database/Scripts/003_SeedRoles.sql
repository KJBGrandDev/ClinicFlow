USE ClinicFlowUnifiedDb;
GO

INSERT INTO dbo.Roles (RoleName, Description)
SELECT N'Admin', N'Manages users, settings, master data, reports, and audit logs.'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE RoleName = N'Admin'
);
GO

INSERT INTO dbo.Roles (RoleName, Description)
SELECT N'Receptionist', N'Registers patients, manages appointments, and handles check-in.'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE RoleName = N'Receptionist'
);
GO

INSERT INTO dbo.Roles (RoleName, Description)
SELECT N'Cashier', N'Records payments, issues receipts, and manages payment clearance.'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE RoleName = N'Cashier'
);
GO

INSERT INTO dbo.Roles (RoleName, Description)
SELECT N'Doctor', N'Records consultations, diagnoses, prescriptions, and laboratory requests.'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE RoleName = N'Doctor'
);
GO

INSERT INTO dbo.Roles (RoleName, Description)
SELECT N'Pharmacist', N'Dispenses medicines and records controlled inventory movements.'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE RoleName = N'Pharmacist'
);
GO

INSERT INTO dbo.Roles (RoleName, Description)
SELECT N'Laboratory Staff', N'Processes laboratory requests and records results.'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE RoleName = N'Laboratory Staff'
);
GO

SELECT RoleId, RoleName, Description, IsActive, CreatedAt
FROM dbo.Roles
ORDER BY RoleId;
GO