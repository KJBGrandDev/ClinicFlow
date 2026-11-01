USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   008: Add PasswordSalt column to Users and seed Admin account
   ========================================================================== */

-- 1. Add PasswordSalt column if it does not exist
IF COL_LENGTH(N'dbo.Users', N'PasswordSalt') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PasswordSalt NVARCHAR(200) NULL;
    PRINT 'Added PasswordSalt column to dbo.Users.';
END;
GO

-- 2. Ensure the Admin role exists in Roles table
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO dbo.Roles (RoleName) VALUES ('Admin');
    PRINT 'Created Admin role.';
END;
GO

-- 3. Reset or insert the admin user with PBKDF2 hash for 'Admin@123'
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'admin')
BEGIN
    UPDATE dbo.Users 
    SET PasswordHash = N'IhEqoEHVPRShGf0aQlsPvC4Mt98HZLgoWASkKoR0K2E=',
        PasswordSalt = N'wZRwUdX0cE/SbtlXfuNyww==',
        IsActive = 1
    WHERE Username = 'admin';
    PRINT 'Admin user password reset to: Admin@123';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (Username, FullName, RoleId, PasswordHash, PasswordSalt, IsActive)
    SELECT N'admin', N'System Administrator', RoleId,
           N'IhEqoEHVPRShGf0aQlsPvC4Mt98HZLgoWASkKoR0K2E=',
           N'wZRwUdX0cE/SbtlXfuNyww==', 1
    FROM dbo.Roles
    WHERE RoleName = 'Admin';
    PRINT 'Admin user created successfully: admin / Admin@123';
END;
GO