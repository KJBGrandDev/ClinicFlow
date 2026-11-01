USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   011: Seed All Role Default User Accounts & Clear Lockouts (REQ-031)
   ========================================================================== */

-- 1. Ensure Roles Exist
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Admin')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Admin', 'Full administrative system access');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Doctor')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Doctor', 'Attending physician clinical portal');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Secretary')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Secretary', 'Front-desk triage, appointments, and cashier');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Pharmacist')
    INSERT INTO dbo.Roles (RoleName, Description) VALUES ('Pharmacist', 'Pharmacy medication dispensing and stock');
GO

-- 2. Seed or Update Default Admin: admin / Admin@123
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'8RUVL4J2n5tFmkGqTAogh/cBMRggjm5YoC+IHF9tnvcUd67stusNdWM0yTl9IZ4U',
        PasswordSalt = NULL,
        FailedLoginAttempts = 0,
        LockoutUntil = NULL,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Username = 'admin';
    PRINT 'Updated admin account: admin / Admin@123';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, IsActive, FailedLoginAttempts)
    SELECT N'admin', N'System Administrator', RoleId,
           N'8RUVL4J2n5tFmkGqTAogh/cBMRggjm5YoC+IHF9tnvcUd67stusNdWM0yTl9IZ4U', 1, 0
    FROM dbo.Roles WHERE RoleName = 'Admin';
    PRINT 'Created admin account: admin / Admin@123';
END;
GO

-- 3. Seed or Update Default Doctor: drsantos / Doctor@123
DECLARE @DocUserId INT;
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'drsantos')
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'MTRQXsEL9teg0Exg5qQu1pPhDKcmoiTESYyUn933WQqTGjvslW22PGVxhS9EK+p8',
        PasswordSalt = NULL,
        FailedLoginAttempts = 0,
        LockoutUntil = NULL,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Username = 'drsantos';
    SELECT @DocUserId = UserId FROM dbo.Users WHERE Username = 'drsantos';
    PRINT 'Updated doctor account: drsantos / Doctor@123';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, IsActive, FailedLoginAttempts)
    SELECT N'drsantos', N'Dr. Juan Santos', RoleId,
           N'MTRQXsEL9teg0Exg5qQu1pPhDKcmoiTESYyUn933WQqTGjvslW22PGVxhS9EK+p8', 1, 0
    FROM dbo.Roles WHERE RoleName = 'Doctor';
    SET @DocUserId = SCOPE_IDENTITY();
    PRINT 'Created doctor account: drsantos / Doctor@123';
END;

-- Ensure Doctor record is linked
IF NOT EXISTS (SELECT 1 FROM dbo.Doctors WHERE DoctorCode = 'DOC-001')
BEGIN
    INSERT INTO dbo.Doctors (DoctorCode, UserId, FirstName, LastName, Specialty, LicenseNumber, ContactNumber, EmailAddress, IsActive)
    VALUES ('DOC-001', @DocUserId, 'Juan', 'Santos', 'Family Medicine', 'LIC-10001', '09180001122', 'dr.santos@clinic.local', 1);
END
ELSE
BEGIN
    UPDATE dbo.Doctors SET UserId = @DocUserId, IsActive = 1 WHERE DoctorCode = 'DOC-001';
END;
GO

-- 4. Seed or Update Default Secretary: secretary / Staff@123
DECLARE @SecUserId INT;
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'secretary')
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'yes/vCGQl4PdbuwH2N7XcqU59YQTKR/0ettcJnv3soAgJLdAtb4F7HEtY51Evt/1',
        PasswordSalt = NULL,
        FailedLoginAttempts = 0,
        LockoutUntil = NULL,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Username = 'secretary';
    SELECT @SecUserId = UserId FROM dbo.Users WHERE Username = 'secretary';
    PRINT 'Updated secretary account: secretary / Staff@123';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, IsActive, FailedLoginAttempts)
    SELECT N'secretary', N'Elena Ramos', RoleId,
           N'yes/vCGQl4PdbuwH2N7XcqU59YQTKR/0ettcJnv3soAgJLdAtb4F7HEtY51Evt/1', 1, 0
    FROM dbo.Roles WHERE RoleName = 'Secretary';
    SET @SecUserId = SCOPE_IDENTITY();
    PRINT 'Created secretary account: secretary / Staff@123';
END;

-- Ensure Secretary profile is linked
IF NOT EXISTS (SELECT 1 FROM dbo.Secretaries WHERE SecretaryCode = 'SEC-001')
BEGIN
    INSERT INTO dbo.Secretaries (SecretaryCode, UserId, FirstName, LastName, AssignedDesk, ContactNumber, EmailAddress, IsActive)
    VALUES ('SEC-001', @SecUserId, 'Elena', 'Ramos', 'Front Reception Desk 1', '09170002233', 'secretary@clinic.local', 1);
END
ELSE
BEGIN
    UPDATE dbo.Secretaries SET UserId = @SecUserId, IsActive = 1 WHERE SecretaryCode = 'SEC-001';
END;
GO

-- 5. Seed or Update Default Pharmacist: pharmacist / Staff@123
DECLARE @PharUserId INT;
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'pharmacist')
BEGIN
    UPDATE dbo.Users
    SET PasswordHash = N'yes/vCGQl4PdbuwH2N7XcqU59YQTKR/0ettcJnv3soAgJLdAtb4F7HEtY51Evt/1',
        PasswordSalt = NULL,
        FailedLoginAttempts = 0,
        LockoutUntil = NULL,
        IsActive = 1,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Username = 'pharmacist';
    SELECT @PharUserId = UserId FROM dbo.Users WHERE Username = 'pharmacist';
    PRINT 'Updated pharmacist account: pharmacist / Staff@123';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, IsActive, FailedLoginAttempts)
    SELECT N'pharmacist', N'Maria Santos, RPh', RoleId,
           N'yes/vCGQl4PdbuwH2N7XcqU59YQTKR/0ettcJnv3soAgJLdAtb4F7HEtY51Evt/1', 1, 0
    FROM dbo.Roles WHERE RoleName = 'Pharmacist';
    SET @PharUserId = SCOPE_IDENTITY();
    PRINT 'Created pharmacist account: pharmacist / Staff@123';
END;

-- Ensure Pharmacist profile is linked
IF NOT EXISTS (SELECT 1 FROM dbo.Pharmacists WHERE PharmacistCode = 'PHAR-001')
BEGIN
    INSERT INTO dbo.Pharmacists (PharmacistCode, UserId, FirstName, LastName, LicenseNumber, ContactNumber, EmailAddress, IsActive)
    VALUES ('PHAR-001', @PharUserId, 'Maria', 'Santos', 'RPH-99210', '09190003344', 'pharmacy@clinic.local', 1);
END
ELSE
BEGIN
    UPDATE dbo.Pharmacists SET UserId = @PharUserId, IsActive = 1 WHERE PharmacistCode = 'PHAR-001';
END;
GO

PRINT '011_SeedAllRoleAccounts script completed successfully.';
GO
