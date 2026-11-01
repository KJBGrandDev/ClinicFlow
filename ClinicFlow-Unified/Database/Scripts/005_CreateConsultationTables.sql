USE ClinicFlowUnifiedDb;
GO

/* ==========================================================================
   005: Patients, Consultations, Pharmacy & Laboratory Tables
   ========================================================================== */

-- 1. Patients Table
IF OBJECT_ID(N'dbo.Patients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Patients
    (
        PatientId INT IDENTITY(1, 1) NOT NULL,
        PatientNumber AS (CONVERT(NVARCHAR(30), CONCAT(N'PT-', RIGHT(N'000000' + CONVERT(VARCHAR(6), PatientId), 6)))) PERSISTED,
        FirstName NVARCHAR(100) NOT NULL,
        MiddleName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NOT NULL,
        Suffix NVARCHAR(20) NULL,
        Sex NVARCHAR(20) NOT NULL,
        BirthDate DATE NOT NULL,
        ContactNumber NVARCHAR(30) NOT NULL,
        BlockAddress NVARCHAR(80) NULL,
        StreetAddress NVARCHAR(120) NOT NULL,
        City NVARCHAR(100) NOT NULL,
        Municipality NVARCHAR(100) NOT NULL,
        EmergencyContactName NVARCHAR(120) NULL,
        EmergencyContactNumber NVARCHAR(30) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Patients_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Patients_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,

        CONSTRAINT PK_Patients PRIMARY KEY (PatientId),
        CONSTRAINT UQ_Patients_PatientNumber UNIQUE (PatientNumber)
    );
END;
GO

-- 2. Appointments Table
IF OBJECT_ID(N'dbo.Appointments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments
    (
        AppointmentId INT IDENTITY(1, 1) NOT NULL,
        AppointmentNumber NVARCHAR(30) NOT NULL,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        AppointmentDate DATE NOT NULL,
        ScheduledStart TIME(0) NOT NULL,
        ScheduledEnd TIME(0) NOT NULL,
        QueueNumber INT NULL,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT (N'Confirmed'),
        Reason NVARCHAR(250) NULL,
        CreatedByUserId INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Appointments_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL,

        CONSTRAINT PK_Appointments PRIMARY KEY (AppointmentId),
        CONSTRAINT UQ_Appointments_Number UNIQUE (AppointmentNumber),
        CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES dbo.Patients(PatientId),
        CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorId) REFERENCES dbo.Doctors(DoctorId)
    );
END;
GO

-- 3. Appointment Queue Table
IF OBJECT_ID(N'dbo.AppointmentQueue', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppointmentQueue
    (
        QueueId INT IDENTITY(1, 1) NOT NULL,
        AppointmentId INT NOT NULL,
        QueueDate DATE NOT NULL,
        QueueNumber INT NOT NULL,
        QueueStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_AppointmentQueue_Status DEFAULT (N'Waiting'),
        CalledAt DATETIME2(0) NULL,
        CompletedAt DATETIME2(0) NULL,

        CONSTRAINT PK_AppointmentQueue PRIMARY KEY (QueueId),
        CONSTRAINT UQ_AppointmentQueue_Appointment UNIQUE (AppointmentId),
        CONSTRAINT FK_AppointmentQueue_Appointments FOREIGN KEY (AppointmentId) REFERENCES dbo.Appointments(AppointmentId)
    );
END;
GO

-- 4. Patient Visits Table
IF OBJECT_ID(N'dbo.PatientVisits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PatientVisits
    (
        VisitId INT IDENTITY(1, 1) NOT NULL,
        AppointmentId INT NOT NULL,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        VisitStart DATETIME2(0) NULL,
        VisitEnd DATETIME2(0) NULL,
        VisitStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_PatientVisits_Status DEFAULT (N'Open'),

        CONSTRAINT PK_PatientVisits PRIMARY KEY (VisitId),
        CONSTRAINT UQ_PatientVisits_Appointment UNIQUE (AppointmentId),
        CONSTRAINT FK_PatientVisits_Appointments FOREIGN KEY (AppointmentId) REFERENCES dbo.Appointments(AppointmentId),
        CONSTRAINT FK_PatientVisits_Patients FOREIGN KEY (PatientId) REFERENCES dbo.Patients(PatientId),
        CONSTRAINT FK_PatientVisits_Doctors FOREIGN KEY (DoctorId) REFERENCES dbo.Doctors(DoctorId)
    );
END;
GO

-- 5. Consultations Table
IF OBJECT_ID(N'dbo.Consultations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consultations
    (
        ConsultationId INT IDENTITY(1, 1) NOT NULL,
        VisitId INT NOT NULL,
        ChiefComplaint NVARCHAR(500) NOT NULL,
        ClinicalNotes NVARCHAR(MAX) NULL,
        Diagnosis NVARCHAR(500) NULL,
        DoctorRecommendation NVARCHAR(MAX) NULL,
        RecordedByUserId INT NULL,
        RecordedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Consultations_RecordedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Consultations PRIMARY KEY (ConsultationId),
        CONSTRAINT UQ_Consultations_Visit UNIQUE (VisitId),
        CONSTRAINT FK_Consultations_Visits FOREIGN KEY (VisitId) REFERENCES dbo.PatientVisits(VisitId)
    );
END;
GO

-- 6. Medicines & Inventory Tables
IF OBJECT_ID(N'dbo.Medicines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Medicines
    (
        MedicineId INT IDENTITY(1, 1) NOT NULL,
        GenericName NVARCHAR(150) NOT NULL,
        BrandName NVARCHAR(150) NULL,
        DosageForm NVARCHAR(60) NOT NULL,
        Strength NVARCHAR(60) NULL,
        UnitPrice DECIMAL(12, 2) NOT NULL,
        ReorderLevel INT NOT NULL CONSTRAINT DF_Medicines_ReorderLevel DEFAULT (10),
        IsActive BIT NOT NULL CONSTRAINT DF_Medicines_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Medicines_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Medicines PRIMARY KEY (MedicineId)
    );
END;
GO

IF OBJECT_ID(N'dbo.MedicineInventory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MedicineInventory
    (
        InventoryId INT IDENTITY(1, 1) NOT NULL,
        MedicineId INT NOT NULL,
        BatchNumber NVARCHAR(60) NOT NULL,
        ExpiryDate DATE NOT NULL,
        QuantityOnHand INT NOT NULL CONSTRAINT DF_MedicineInventory_Quantity DEFAULT (0),
        LastUpdated DATETIME2(0) NOT NULL CONSTRAINT DF_MedicineInventory_LastUpdated DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_MedicineInventory PRIMARY KEY (InventoryId),
        CONSTRAINT FK_MedicineInventory_Medicines FOREIGN KEY (MedicineId) REFERENCES dbo.Medicines(MedicineId)
    );
END;
GO

-- 7. Prescriptions & Prescription Items
IF OBJECT_ID(N'dbo.Prescriptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Prescriptions
    (
        PrescriptionId INT IDENTITY(1, 1) NOT NULL,
        PrescriptionNumber NVARCHAR(40) NOT NULL,
        ConsultationId INT NOT NULL,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        PatientFulfillmentChoice NVARCHAR(30) NULL,
        PrescriptionStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_Prescriptions_Status DEFAULT (N'Recommended'),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Prescriptions_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Prescriptions PRIMARY KEY (PrescriptionId),
        CONSTRAINT UQ_Prescriptions_Number UNIQUE (PrescriptionNumber),
        CONSTRAINT FK_Prescriptions_Consultations FOREIGN KEY (ConsultationId) REFERENCES dbo.Consultations(ConsultationId),
        CONSTRAINT FK_Prescriptions_Patients FOREIGN KEY (PatientId) REFERENCES dbo.Patients(PatientId),
        CONSTRAINT FK_Prescriptions_Doctors FOREIGN KEY (DoctorId) REFERENCES dbo.Doctors(DoctorId)
    );
END;
GO

IF OBJECT_ID(N'dbo.PrescriptionItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PrescriptionItems
    (
        PrescriptionItemId INT IDENTITY(1, 1) NOT NULL,
        PrescriptionId INT NOT NULL,
        MedicineId INT NOT NULL,
        QuantityPrescribed INT NOT NULL,
        Instructions NVARCHAR(300) NOT NULL,
        AvailabilityStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_PrescriptionItems_Availability DEFAULT (N'Pending'),
        PatientChoice NVARCHAR(30) NULL,

        CONSTRAINT PK_PrescriptionItems PRIMARY KEY (PrescriptionItemId),
        CONSTRAINT FK_PrescriptionItems_Prescriptions FOREIGN KEY (PrescriptionId) REFERENCES dbo.Prescriptions(PrescriptionId),
        CONSTRAINT FK_PrescriptionItems_Medicines FOREIGN KEY (MedicineId) REFERENCES dbo.Medicines(MedicineId)
    );
END;
GO

-- 8. Laboratory Tests & Requests
IF OBJECT_ID(N'dbo.LaboratoryTests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LaboratoryTests
    (
        LaboratoryTestId INT IDENTITY(1, 1) NOT NULL,
        TestCode NVARCHAR(30) NOT NULL,
        TestName NVARCHAR(150) NOT NULL,
        Description NVARCHAR(300) NULL,
        Fee DECIMAL(12, 2) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_LaboratoryTests_IsActive DEFAULT (1),

        CONSTRAINT PK_LaboratoryTests PRIMARY KEY (LaboratoryTestId),
        CONSTRAINT UQ_LaboratoryTests_TestCode UNIQUE (TestCode)
    );
END;
GO

IF OBJECT_ID(N'dbo.LaboratoryRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LaboratoryRequests
    (
        LaboratoryRequestId INT IDENTITY(1, 1) NOT NULL,
        RequestNumber NVARCHAR(40) NOT NULL,
        ConsultationId INT NOT NULL,
        PatientId INT NOT NULL,
        DoctorId INT NOT NULL,
        LaboratoryTestId INT NOT NULL,
        PatientChoice NVARCHAR(30) NOT NULL CONSTRAINT DF_LaboratoryRequests_Choice DEFAULT (N'Pending Decision'),
        RequestStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_LaboratoryRequests_Status DEFAULT (N'Recommended'),
        PaymentId INT NULL,
        RequestedAt DATETIME2(0) NOT NULL CONSTRAINT DF_LaboratoryRequests_RequestedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_LaboratoryRequests PRIMARY KEY (LaboratoryRequestId),
        CONSTRAINT UQ_LaboratoryRequests_Number UNIQUE (RequestNumber),
        CONSTRAINT FK_LaboratoryRequests_Consultations FOREIGN KEY (ConsultationId) REFERENCES dbo.Consultations(ConsultationId),
        CONSTRAINT FK_LaboratoryRequests_Patients FOREIGN KEY (PatientId) REFERENCES dbo.Patients(PatientId),
        CONSTRAINT FK_LaboratoryRequests_Doctors FOREIGN KEY (DoctorId) REFERENCES dbo.Doctors(DoctorId),
        CONSTRAINT FK_LaboratoryRequests_Tests FOREIGN KEY (LaboratoryTestId) REFERENCES dbo.LaboratoryTests(LaboratoryTestId)
    );
END;
GO

-- 9. Seed Catalog Data & Sample Visit
IF NOT EXISTS (SELECT 1 FROM dbo.Medicines)
BEGIN
    INSERT INTO dbo.Medicines (GenericName, BrandName, DosageForm, Strength, UnitPrice, ReorderLevel)
    VALUES 
    (N'Paracetamol', N'Biogesic', N'Tablet', N'500 mg', 3.50, 20),
    (N'Amoxicillin', N'Amoxil', N'Capsule', N'500 mg', 8.50, 15),
    (N'Cetirizine', N'Alnix', N'Tablet', N'10 mg', 12.00, 15),
    (N'Mefenamic Acid', N'Ponstan', N'Capsule', N'500 mg', 14.00, 10);

    INSERT INTO dbo.MedicineInventory (MedicineId, BatchNumber, ExpiryDate, QuantityOnHand)
    SELECT MedicineId, CONCAT(N'BATCH-', MedicineId, N'-2026'), DATEADD(MONTH, 24, CAST(GETDATE() AS DATE)), 100
    FROM dbo.Medicines;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LaboratoryTests)
BEGIN
    INSERT INTO dbo.LaboratoryTests (TestCode, TestName, Description, Fee)
    VALUES 
    (N'CBC', N'Complete Blood Count', N'Full blood profile with platelets', 350.00),
    (N'UA', N'Urinalysis', N'Routine physical and chemical urine exam', 150.00),
    (N'FBS', N'Fasting Blood Sugar', N'Diabetes and blood glucose screening', 220.00),
    (N'LIPID', N'Lipid Profile', N'Cholesterol, HDL, LDL, and Triglycerides', 650.00);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Patients)
BEGIN
    INSERT INTO dbo.Patients (FirstName, LastName, Sex, BirthDate, ContactNumber, StreetAddress, City, Municipality)
    VALUES (N'Maria', N'Dela Cruz', N'Female', '1998-05-14', '09171234567', N'Km 5 Buhangin', N'Davao City', N'Davao');

    -- Ensure at least one doctor exists
    IF NOT EXISTS (SELECT 1 FROM dbo.Doctors)
    BEGIN
        INSERT INTO dbo.Doctors (DoctorCode, FirstName, LastName, LicenseNumber, Specialty, ContactNumber, EmailAddress, IsActive)
        VALUES (N'DOC-001', N'Juan', N'Santos', N'LIC-10001', N'Family Medicine', '09180001122', 'dr.santos@clinic.local', 1);
    END;

    INSERT INTO dbo.Appointments (AppointmentNumber, PatientId, DoctorId, AppointmentDate, ScheduledStart, ScheduledEnd, QueueNumber, Status, Reason)
    VALUES (N'APT-20260901-0001', 1, 1, CAST(GETDATE() AS DATE), '08:30', '09:00', 1, N'Confirmed', N'Fever and sore throat for 3 days');

    INSERT INTO dbo.AppointmentQueue (AppointmentId, QueueDate, QueueNumber, QueueStatus)
    VALUES (1, CAST(GETDATE() AS DATE), 1, N'Waiting');

    INSERT INTO dbo.PatientVisits (AppointmentId, PatientId, DoctorId, VisitStart, VisitStatus)
    VALUES (1, 1, 1, SYSUTCDATETIME(), N'Open');
END;
GO