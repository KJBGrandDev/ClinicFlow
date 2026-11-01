# ClinicFlow — Comprehensive Data Dictionary

> **Module:** Database Architecture & Schema Specification  
> **Target Database:** `ClinicFlowUnifiedDb` (Microsoft SQL Server)  
> **Schema Compliance:** Third Normal Form (3NF)  
> **Requirements Addressed:** `REQ-007` (Normalized Schema, ERD & Data Dictionary)  
> **Last Verified Against Database:** October 26, 2026  

---

## Table of Contents
1. [Security & Administration](#1-security--administration)
   - [Roles](#11-dbo-roles)
   - [Users](#12-dbo-users)
   - [AuditLogs](#13-dbo-auditlogs)
   - [ErrorLogs](#14-dbo-errorlogs)
2. [Clinic Staff & Scheduling](#2-clinic-staff--scheduling)
   - [Doctors](#21-dbo-doctors)
   - [Secretaries](#22-dbo-secretaries)
   - [Pharmacists](#23-dbo-pharmacists)
   - [DoctorSchedules](#24-dbo-doctorschedules)
   - [DoctorWeeklySchedules](#25-dbo-doctorweeklyschedules)
   - [DoctorScheduleExceptions](#26-dbo-doctorscheduleexceptions)
3. [Patient Care & Appointment Flow](#3-patient-care--appointment-flow)
   - [Patients](#31-dbo-patients)
   - [Appointments](#32-dbo-appointments)
   - [AppointmentQueue](#33-dbo-appointmentqueue)
   - [PatientVisits](#34-dbo-patientvisits)
4. [Clinical Consultations & Diagnostics](#4-clinical-consultations--diagnostics)
   - [Consultations](#41-dbo-consultations)
   - [Prescriptions](#42-dbo-prescriptions)
   - [PrescriptionItems](#43-dbo-prescriptionitems)
   - [LaboratoryTests](#44-dbo-laboratorytests)
   - [LaboratoryRequests](#45-dbo-laboratoryrequests)
   - [LaboratoryResults](#46-dbo-laboratoryresults)
5. [Pharmacy & Inventory Control](#5-pharmacy--inventory-control)
   - [Medicines](#51-dbo-medicines)
   - [MedicineInventory](#52-dbo-medicineinventory)
   - [MedicineDisposalLogs](#53-dbo-medicinedisposallogs)
6. [Billing & Financial Settlement](#6-billing--financial-settlement)
   - [Invoices](#61-dbo-invoices)
   - [Payments](#62-dbo-payments)
7. [Analytical Views (Stretch Bonus)](#7-analytical-views-stretch-bonus)
   - [vw_DailyRevenueReport](#71-dbo-vw_dailyrevenuereport)
   - [vw_PharmacyInventoryExpiryReport](#72-dbo-vw_pharmacyinventoryexpiryreport)
   - [vw_DoctorConsultationReport](#73-dbo-vw_doctorconsultationreport)

---

## 1. Security & Administration

### 1.1 `dbo.Roles`
- **Description:** System role catalog governing role-based access control (RBAC).
- **Primary Key:** `RoleId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `RoleId` | `int` | NO | IDENTITY(1,1) | PK | Unique surrogate identifier for system role. |
| `RoleName` | `nvarchar(50)` | NO | | UK (`UQ_Roles_RoleName`) | Role name: `Admin`, `Doctor`, `Secretary`, `Pharmacist`. |
| `Description` | `nvarchar(200)` | NO | | | Functional scope and permissions summary. |
| `IsActive` | `bit` | NO | `1` | | Status flag indicating if the role is active in the system. |
| `CreatedAT` | `datetime2` | NO | `SYSUTCDATETIME()` | | Timestamp when role was initialized. |

---

### 1.2 `dbo.Users`
- **Description:** Central user authentication and security account repository.
- **Primary Key:** `UserId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `UserId` | `int` | NO | IDENTITY(1,1) | PK | Unique identifier for staff user account. |
| `Username` | `nvarchar(50)` | NO | | UK (`UQ_Users_Username`) | Unique login name (case-insensitive in SQL collation). |
| `PasswordHash` | `nvarchar(512)` | NO | | | Cryptographic salted PBKDF2-SHA256 password hash. |
| `PasswordSalt` | `nvarchar(200)` | YES | | | Base64-encoded cryptographic salt (128-bit). |
| `FullName` | `nvarchar(150)` | NO | | | User's complete legal/professional display name. |
| `RoleId` | `int` | NO | | FK -> `Roles(RoleId)` | Role identifier determining UI and service access level. |
| `FailedLoginAttempts`| `int` | NO | `0` | | Consecutive failed auth attempts (triggers lockout at 3). |
| `LockoutUntil` | `datetime2` | YES | | | Expiration timestamp for temporary lockout (15 minutes). |
| `IsActive` | `bit` | NO | `1` | | Active flag (deactivated users cannot authenticate). |
| `CreatedAt` | `datetime2` | NO | `SYSUTCDATETIME()` | | Audit creation timestamp (UTC). |
| `UpdatedAt` | `datetime2` | YES | | | Audit modification timestamp (UTC). |

---

### 1.3 `dbo.AuditLogs`
- **Description:** Append-only clinical and administrative compliance audit trail.
- **Primary Key:** `AuditLogId` (Auto-incrementing BigInt Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `AuditLogId` | `bigint` | NO | IDENTITY(1,1) | PK | Unique sequence number for audit event. |
| `UserId` | `int` | YES | | FK -> `Users(UserId)` | ID of actor executing action (NULL if anonymous/pre-auth). |
| `ActionType` | `nvarchar(50)` | NO | | | Action verb: `LOGIN`, `LOGOUT`, `CREATE`, `UPDATE`, `ARCHIVE`, `RESTORE`, `BACKUP`, `PASSWORD_RESET`. |
| `EntityName` | `nvarchar(100)` | NO | | | Affected domain model/table (`Patient`, `User`, `Invoice`). |
| `RecordId` | `nvarchar(50)` | YES | | | Primary identifier of affected target record. |
| `Description` | `nvarchar(500)` | NO | | | Human-readable audit narrative and parameter change log. |
| `LoggedAt` | `datetime2` | NO | `SYSUTCDATETIME()` | | Tamper-evident UTC event timestamp. |

---

### 1.4 `dbo.ErrorLogs`
- **Description:** Centralized diagnostic exception and incident repository.
- **Primary Key:** `ErrorLogId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `ErrorLogId` | `int` | NO | IDENTITY(1,1) | PK | Unique identifier for error incident. |
| `LoggedAt` | `datetime2` | NO | `SYSUTCDATETIME()` | | UTC timestamp when exception was caught. |
| `SourceModule` | `nvarchar(100)` | NO | | | Class/method origin (e.g. `PatientService.SavePatientAsync`). |
| `ExceptionType` | `nvarchar(200)` | NO | | | .NET exception class name (`SqlException`, `TimeoutException`). |
| `Message` | `nvarchar(1000)` | NO | | | Technical exception message. |
| `StackTrace` | `nvarchar(max)` | YES | | | Full stack trace for developer diagnosis. |
| `MachineName` | `nvarchar(50)` | YES | `HOST_NAME()` | | Client machine hostname. |
| `WindowsUser` | `nvarchar(100)` | YES | `SUSER_SNAME()` | | Windows user or service account executing process. |

---

## 2. Clinic Staff & Scheduling

### 2.1 `dbo.Doctors`
- **Description:** Master directory of clinical physicians and attending doctors.
- **Primary Key:** `DoctorId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `DoctorId` | `int` | NO | IDENTITY(1,1) | PK | Unique identifier for doctor profile. |
| `DoctorCode` | `nvarchar(20)` | NO | | UK (`UQ_Doctors_DoctorCode`) | Alphanumeric clinic code (e.g., `DOC-001`). |
| `UserId` | `int` | NO | | FK -> `Users(UserId)`, UK | User account link for login access. |
| `FirstName` | `nvarchar(100)` | NO | | | Doctor's first name. |
| `LastName` | `nvarchar(100)` | NO | | | Doctor's last name. |
| `Specialty` | `nvarchar(100)` | NO | | | Medical field (Pediatrics, Cardiology, General Medicine). |
| `LicenseNumber` | `nvarchar(50)` | NO | | UK (`UX_Doctors_LicenseNumber`)| Professional PRC / Medical license number. |
| `ContactNumber` | `nvarchar(30)` | YES | | | Primary contact phone number. |
| `EmailAddress` | `nvarchar(150)` | YES | | | Official email contact. |
| `RoomAssignment` | `nvarchar(50)` | YES | | | Assigned consultation room or clinic suite number. |
| `ConsultationFee`| `decimal(12,2)` | NO | `500.00` | | Standard physician consultation fee. |
| `IsActive` | `bit` | NO | `1` | | Active clinical practice status. |
| `CreatedAt` | `datetime2` | NO | `SYSUTCDATETIME()` | | Creation timestamp. |
| `UpdatedAt` | `datetime2` | YES | | | Modification timestamp. |

---

### 2.2 `dbo.Secretaries`
- **Description:** Master directory of administrative front-desk secretaries and triage staff.
- **Primary Key:** `SecretaryId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `SecretaryId` | `int` | NO | IDENTITY(1,1) | PK | Unique identifier for secretary profile. |
| `SecretaryCode` | `nvarchar(20)` | NO | | UK (`UQ_Secretaries_Code`) | Administrative staff code (e.g., `SEC-001`). |
| `UserId` | `int` | YES | | FK -> `Users(UserId)` | Associated system user account. |
| `FirstName` | `nvarchar(100)` | NO | | | Secretary first name. |
| `LastName` | `nvarchar(100)` | NO | | | Secretary last name. |
| `ContactNumber` | `nvarchar(30)` | YES | | | Contact phone number. |
| `AssignedDesk` | `nvarchar(100)` | YES | | | Reception desk station (e.g. `Front Desk Station 1`). |
| `EmailAddress` | `nvarchar(150)` | YES | | | Contact email address. |
| `IsActive` | `bit` | NO | `1` | | Active employment status. |
| `CreatedAt` | `datetime2` | NO | `SYSUTCDATETIME()` | | Record creation timestamp. |
| `UpdatedAt` | `datetime2` | YES | | | Record update timestamp. |

---

### 2.3 `dbo.Pharmacists`
- **Description:** Licensed pharmacy dispensary staff and pharmacists.
- **Primary Key:** `PharmacistId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `PharmacistId` | `int` | NO | IDENTITY(1,1) | PK | Unique identifier for pharmacist. |
| `PharmacistCode`| `nvarchar(20)` | NO | | UK (`UQ_Pharmacists_Code`) | Staff identifier code (e.g., `PHARM-001`). |
| `UserId` | `int` | NO | | FK -> `Users(UserId)` | User login account. |
| `FirstName` | `nvarchar(100)` | NO | | | Pharmacist first name. |
| `LastName` | `nvarchar(100)` | NO | | | Pharmacist last name. |
| `LicenseNumber` | `nvarchar(50)` | NO | | | Professional Board pharmacy license number. |
| `ContactNumber` | `nvarchar(30)` | YES | | | Contact phone number. |
| `EmailAddress` | `nvarchar(150)` | YES | | | Official email address. |
| `IsActive` | `bit` | NO | `1` | | Active dispensing authorization status. |
| `CreatedAt` | `datetime2` | NO | `SYSUTCDATETIME()` | | Creation timestamp. |
| `UpdatedAt` | `datetime2` | YES | | | Update timestamp. |

---

### 2.4 `dbo.DoctorSchedules`
- **Description:** Specific calendar date schedules and availability slots for doctors.
- **Primary Key:** `DoctorScheduleId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `DoctorScheduleId`| `int` | NO | IDENTITY(1,1) | PK | Schedule entry ID. |
| `DoctorId` | `int` | NO | | FK -> `Doctors(DoctorId)` | Scheduled physician. |
| `ScheduleDate` | `date` | NO | | | Specific calendar date of duty. |
| `StartTime` | `time` | NO | | | Shift start time. |
| `EndTime` | `time` | NO | | | Shift end time. |
| `MaxPatients` | `int` | NO | `20` | | Patient appointment ceiling for shift. |
| `IsAvailable` | `bit` | NO | `1` | | Availability flag. |
| `Notes` | `nvarchar(200)` | YES | | | Shift notes or clinic instructions. |

---

### 2.5 `dbo.DoctorWeeklySchedules`
- **Description:** Recurring template schedules by day of week for automated booking.
- **Primary Key:** `ScheduleId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `ScheduleId` | `int` | NO | IDENTITY(1,1) | PK | Template schedule ID. |
| `DoctorId` | `int` | NO | | FK -> `Doctors(DoctorId)` | Physician. |
| `DayOfWeek` | `tinyint` | NO | | UK (`UQ_Doctor_Day` composite)| Day of week (0=Sunday ... 6=Saturday). |
| `StartTime` | `time` | NO | | | Template start time. |
| `EndTime` | `time` | NO | | | Template end time. |
| `SlotDurationMinutes`| `int` | NO | `20` | | Duration per consultation appointment slot. |
| `MaxPatients` | `int` | NO | `20` | | Capacity limit per recurring day. |
| `IsActive` | `bit` | NO | `1` | | Active template rule status. |

---

### 2.6 `dbo.DoctorScheduleExceptions`
- **Description:** Physician leaves, holidays, conferences, and off-duty overrides.
- **Primary Key:** `ExceptionId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `ExceptionId` | `int` | NO | IDENTITY(1,1) | PK | Unique exception ID. |
| `DoctorId` | `int` | NO | | FK -> `Doctors(DoctorId)` | Attending physician. |
| `ExceptionDate` | `date` | NO | | | Date of exception/absence. |
| `ExceptionType` | `nvarchar(50)` | NO | | | Type: `Leave`, `Conference`, `Emergency`, `Holiday`. |
| `Reason` | `nvarchar(250)` | YES | | | Stated justification. |
| `IsApproved` | `bit` | NO | `1` | | Administrative approval flag. |

---

## 3. Patient Care & Appointment Flow

### 3.1 `dbo.Patients`
- **Description:** Master patient health records directory with demographic data and concurrency tracking.
- **Primary Key:** `PatientId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `PatientId` | `int` | NO | IDENTITY(1,1) | PK | Unique patient system ID. |
| `PatientNumber` | `nvarchar(30)` | YES | | UK (`UQ_Patients_PatientNumber`)| Unique hospital/clinic MRN (e.g., `PAT-2026-0001`). |
| `FirstName` | `nvarchar(80)` | NO | | | Patient given name. |
| `MiddleName` | `nvarchar(100)` | YES | | | Patient middle name. |
| `LastName` | `nvarchar(80)` | NO | | | Patient surname. |
| `Suffix` | `nvarchar(20)` | YES | | | Generational suffix (Jr., Sr., III). |
| `Sex` | `nvarchar(20)` | NO | | | Biological sex (`Male`, `Female`). |
| `BirthDate` | `date` | NO | | | Date of birth for age calculation and pediatric triage. |
| `ContactNumber` | `nvarchar(30)` | NO | | | Primary contact phone/mobile. |
| `BlockAddress` | `nvarchar(80)` | YES | | | House/Unit/Block address line. |
| `StreetAddress` | `nvarchar(120)` | NO | | | Street address line. |
| `City` | `nvarchar(100)` | NO | | | City/Barangay address. |
| `Municipality` | `nvarchar(100)` | NO | | | Municipality or Province. |
| `EmergencyContactName`| `nvarchar(120)` | YES | | | Emergency contact person full name. |
| `EmergencyContactNumber`| `nvarchar(30)`| YES | | | Emergency contact telephone number. |
| `IsActive` | `bit` | NO | `1` | | Soft-delete status flag (1=Active, 0=Archived). |
| `CreatedAt` | `datetime2` | NO | `SYSDATETIME()` | | Initial patient registration timestamp. |
| `UpdatedAt` | `datetime2` | YES | | | Demographic modification timestamp. |
| `RowVersion` | `timestamp` (`ROWVERSION`)| NO | | Automatic 8-byte token | Concurrency token verifying record freshness before updates. |

---

### 3.2 `dbo.Appointments`
- **Description:** Scheduled and walk-in clinical patient bookings.
- **Primary Key:** `AppointmentId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `AppointmentId` | `int` | NO | IDENTITY(1,1) | PK | Appointment ID. |
| `AppointmentNumber`| `nvarchar(30)`| NO | | UK (`UQ_Appointments_Number`)| Unique booking reference code (e.g., `APT-20261026-001`). |
| `PatientId` | `int` | NO | | FK -> `Patients(PatientId)` | Patient booking appointment. |
| `DoctorId` | `int` | NO | | FK -> `Doctors(DoctorId)` | Requested/assigned doctor. |
| `AppointmentDate`| `date` | NO | | | Scheduled appointment date. |
| `AppointmentTime`| `time` | NO | | | Scheduled slot time. |
| `AppointmentType`| `nvarchar(30)`| NO | | | Type: `Consultation`, `FollowUp`, `LabReview`, `Emergency`. |
| `Status` | `nvarchar(30)` | NO | `Scheduled` | | Status: `Scheduled`, `CheckedIn`, `InConsultation`, `Completed`, `Cancelled`. |
| `ReasonForVisit` | `nvarchar(250)`| YES | | | Patient's presenting symptoms or complaint. |
| `IsWalkIn` | `bit` | NO | `0` | | 1 if walk-in triage; 0 if pre-booked. |
| `QueueNumber` | `int` | YES | | | Assigned sequential queue ticket number. |
| `CreatedAt` | `datetime2` | NO | `SYSDATETIME()` | | Booking creation timestamp. |
| `UpdatedAt` | `datetime2` | YES | | | Status change timestamp. |
| `RowVersion` | `timestamp` (`ROWVERSION`)| NO | | Automatic 8-byte token | Concurrency token preventing race conditions during triage. |

---

### 3.3 `dbo.AppointmentQueue`
- **Description:** Real-time lobby waiting queue tracking check-ins and calling state.
- **Primary Key:** `QueueId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `QueueId` | `int` | NO | IDENTITY(1,1) | PK | Unique lobby queue entry ID. |
| `AppointmentId` | `int` | NO | | FK -> `Appointments(AppointmentId)`, UK | 1-to-1 link to checked-in appointment. |
| `QueueNumber` | `int` | NO | | | Daily sequential queue ticket number. |
| `QueueStatus` | `nvarchar(30)` | NO | `Waiting` | | Queue state: `Waiting`, `Called`, `InService`, `Deferred`, `Completed`. |
| `CheckInTime` | `datetime2` | NO | `SYSDATETIME()` | | Timestamp when patient arrived in clinic lobby. |
| `CalledTime` | `datetime2` | YES | | | Timestamp when doctor called ticket into clinic room. |
| `CompletedTime` | `datetime2` | YES | | | Timestamp when consultation finished. |

---

### 3.4 `dbo.PatientVisits`
- **Description:** Clinical encounter tracking the official visit session duration.
- **Primary Key:** `VisitId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `VisitId` | `int` | NO | IDENTITY(1,1) | PK | Unique clinical encounter ID. |
| `AppointmentId` | `int` | NO | | FK -> `Appointments(AppointmentId)`, UK | Originating appointment. |
| `PatientId` | `int` | NO | | FK -> `Patients(PatientId)` | Patient. |
| `DoctorId` | `int` | NO | | FK -> `Doctors(DoctorId)` | Physician conducting encounter. |
| `VisitStart` | `datetime2` | YES | | | Encounter start timestamp. |
| `VisitEnd` | `datetime2` | YES | | | Encounter completion timestamp. |
| `VisitStatus` | `nvarchar(30)` | NO | `Open` | | Encounter status: `Open`, `Closed`, `Referred`. |

---

## 4. Clinical Consultations & Diagnostics

### 4.1 `dbo.Consultations`
- **Description:** Comprehensive medical documentation of physician consultation.
- **Primary Key:** `ConsultationId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `ConsultationId`| `int` | NO | IDENTITY(1,1) | PK | Unique consultation encounter ID. |
| `VisitId` | `int` | NO | | FK -> `PatientVisits(VisitId)`, UK | Link to active patient visit. |
| `ChiefComplaint`| `nvarchar(max)`| NO | | | Primary subjective reason for visit. |
| `HistoryOfPresentIllness`| `nvarchar(max)`| YES | | | Chronological narrative of symptoms. |
| `PhysicalExamination`| `nvarchar(max)`| YES | | | Objective vital signs and clinical findings. |
| `Diagnosis` | `nvarchar(max)`| NO | | | ICD / clinical assessment and diagnosis. |
| `TreatmentPlan` | `nvarchar(max)`| YES | | | Recommended therapeutic regime and patient advice. |
| `Notes` | `nvarchar(500)`| YES | | | Internal physician notes. |
| `ConsultationDate`| `datetime2`| NO | `SYSDATETIME()` | | Date and time consultation was logged. |

---

### 4.2 `dbo.Prescriptions`
- **Description:** Prescription header document issued by attending physician.
- **Primary Key:** `PrescriptionId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `PrescriptionId`| `int` | NO | IDENTITY(1,1) | PK | Unique prescription ID. |
| `PrescriptionNumber`| `nvarchar(40)`| NO | | UK (`UQ_Prescriptions_Number`)| Clinical Rx document serial number (e.g., `RX-20261026-001`). |
| `ConsultationId`| `int` | NO | | FK -> `Consultations(ConsultationId)` | Parent consultation. |
| `PatientId` | `int` | NO | | FK -> `Patients(PatientId)` | Patient prescribed. |
| `DoctorId` | `int` | NO | | FK -> `Doctors(DoctorId)` | Prescribing physician. |
| `PatientFulfillmentChoice`| `nvarchar(30)`| YES | `ClinicPharmacy` | | Patient preference: `ClinicPharmacy` or `OutsideDispensary`. |
| `PrescriptionStatus`| `nvarchar(30)`| NO | `Recommended` | | Status: `Recommended`, `PartiallyDispensed`, `Dispensed`, `Cancelled`. |
| `CreatedAt` | `datetime2` | NO | `SYSDATETIME()` | | Timestamp when Rx was generated. |

---

### 4.3 `dbo.PrescriptionItems`
- **Description:** Child detail table containing line-item medication orders.
- **Primary Key:** `PrescriptionItemId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `PrescriptionItemId`| `int` | NO | IDENTITY(1,1) | PK | Line item ID. |
| `PrescriptionId`| `int` | NO | | FK -> `Prescriptions(PrescriptionId)` | Parent Rx header. |
| `MedicineId` | `int` | NO | | FK -> `Medicines(MedicineId)` | Catalog drug prescribed. |
| `QuantityPrescribed`| `int` | NO | | | Ordered item unit count (must be > 0). |
| `Instructions` | `nvarchar(300)`| NO | | | Dosage frequency, route, and timing instructions. |
| `AvailabilityStatus`| `nvarchar(30)`| NO | `Pending` | | Dispensing status: `Pending`, `InStock`, `Dispensed`, `OutOfStock`. |
| `PatientChoice`| `nvarchar(30)`| YES | | | Patient item choice: `Take` or `Decline`. |

---

### 4.4 `dbo.LaboratoryTests`
- **Description:** Standard catalog of available clinic diagnostic and laboratory tests.
- **Primary Key:** `LaboratoryTestId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `LaboratoryTestId`| `int` | NO | IDENTITY(1,1) | PK | Diagnostic test ID. |
| `TestCode` | `nvarchar(30)` | NO | | UK (`UQ_LabTests_TestCode`) | Standard lab code (e.g., `CBC`, `URIN`, `LIPID`). |
| `TestName` | `nvarchar(150)`| NO | | | Complete diagnostic test title. |
| `Description` | `nvarchar(300)`| YES | | | Diagnostic purpose and preparation notes. |
| `Fee` | `decimal(12,2)`| NO | | | Standard diagnostic billing fee. |
| `IsActive` | `bit` | NO | `1` | | Test catalog availability status. |

---

### 4.5 `dbo.LaboratoryRequests`
- **Description:** Physician laboratory diagnostic test orders.
- **Primary Key:** `LaboratoryRequestId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `LaboratoryRequestId`| `int`| NO | IDENTITY(1,1) | PK | Lab order ID. |
| `RequestNumber`| `nvarchar(30)`| NO | | UK (`UQ_LabRequests_Number`)| Unique diagnostic requisition code. |
| `ConsultationId`| `int` | NO | | FK -> `Consultations(ConsultationId)` | Originating consultation encounter. |
| `PatientId` | `int` | NO | | FK -> `Patients(PatientId)` | Patient. |
| `DoctorId` | `int` | NO | | FK -> `Doctors(DoctorId)` | Ordering doctor. |
| `LaboratoryTestId`| `int` | NO | | FK -> `LaboratoryTests(TestId)`| Ordered diagnostic test. |
| `ClinicalNotes` | `nvarchar(250)`| YES | | | Pertinent clinical indications for lab staff. |
| `PatientChoice`| `nvarchar(30)`| NO | `Pending Decision` | | Patient election: `InHouse`, `External`, `Declined`. |
| `RequestStatus` | `nvarchar(30)`| NO | `Recommended` | | Status: `Recommended`, `Paid`, `SpecimenCollected`, `Completed`, `Cancelled`. |
| `PaymentId` | `int` | YES | | | Settled billing payment link. |
| `RequestedAt` | `datetime2` | NO | `SYSDATETIME()` | | Order placement timestamp. |

---

### 4.6 `dbo.LaboratoryResults`
- **Description:** Diagnostic findings and attachments produced by laboratory technicians.
- **Primary Key:** `LaboratoryResultId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `LaboratoryResultId`| `int`| NO | IDENTITY(1,1) | PK | Unique result ID. |
| `LaboratoryRequestId`| `int`| NO | | FK -> `LaboratoryRequests(RequestId)`, UK | 1-to-1 link to lab order. |
| `ResultText` | `nvarchar(max)`| NO | | | Formatted diagnostic analysis, values, and reference ranges. |
| `ResultAttachmentPath`| `nvarchar(500)`| YES | | | File path to scanned PDF/image lab report. |
| `ResultStatus` | `nvarchar(30)`| NO | `Final` | | Result state: `Preliminary`, `Final`, `Amended`. |
| `PerformedByUserId`| `int` | YES | | FK -> `Users(UserId)` | Medical technologist or pathologist account. |
| `ResultDate` | `datetime2` | NO | `SYSUTCDATETIME()` | | Date and time result was released. |

---

## 5. Pharmacy & Inventory Control

### 5.1 `dbo.Medicines`
- **Description:** Master formulary catalog of pharmaceutical drugs and medical supplies.
- **Primary Key:** `MedicineId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `MedicineId` | `int` | NO | IDENTITY(1,1) | PK | Unique formulary drug ID. |
| `GenericName` | `nvarchar(150)`| NO | | | Official generic active ingredient name. |
| `BrandName` | `nvarchar(150)`| YES | | | Commercial brand or trade name. |
| `Category` | `nvarchar(80)` | YES | `General` | | Classification: `Antibiotic`, `Analgesic`, `Antihypertensive`, etc. |
| `DosageForm` | `nvarchar(60)` | NO | | | Form: `Tablet`, `Capsule`, `Syrup`, `Vial`, `Ointment`. |
| `Strength` | `nvarchar(60)` | YES | | | Dosage strength (e.g. `500 mg`, `10 mg/5 mL`). |
| `CostPrice` | `decimal(12,2)`| NO | `0.00` | | Acquisition cost per unit. |
| `UnitPrice` | `decimal(12,2)`| NO | | | Retail dispensing price per unit to patient. |
| `ReorderLevel` | `int` | NO | `10` | | Minimum safety threshold triggering low-stock alert. |
| `IsActive` | `bit` | NO | `1` | | Active formulary listing flag. |

---

### 5.2 `dbo.MedicineInventory`
- **Description:** Physical batch inventory with individual batch tracking, FEFO management, and expiration monitoring.
- **Primary Key:** `InventoryId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `InventoryId` | `int` | NO | IDENTITY(1,1) | PK | Unique stock batch entry ID. |
| `MedicineId` | `int` | NO | | FK -> `Medicines(MedicineId)` | Catalog medicine item. |
| `BatchNumber` | `nvarchar(60)` | NO | | | Manufacturer batch or lot tracking code. |
| `ExpiryDate` | `date` | NO | | | Manufacturer expiration date (governs First-Expired First-Out). |
| `QuantityOnHand`| `int` | NO | | | Available stock units remaining in batch. |
| `CostPrice` | `decimal(12,2)`| YES | | | Batch-specific acquisition unit cost. |
| `LastUpdated` | `datetime2` | NO | `SYSDATETIME()` | | Stock level update timestamp. |

---

### 5.3 `dbo.MedicineDisposalLogs`
- **Description:** Hazardous waste, expired medicine disposal, and inventory loss accounting.
- **Primary Key:** `DisposalId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `DisposalId` | `int` | NO | IDENTITY(1,1) | PK | Unique disposal event ID. |
| `MedicineId` | `int` | NO | | FK -> `Medicines(MedicineId)` | Disposed medicine. |
| `BatchNumber` | `nvarchar(60)` | NO | | | Batch number of disposed stock. |
| `ExpiryDate` | `date` | YES | | | Expiration date of discarded batch. |
| `QuantityDisposed`| `int` | NO | | | Quantity destroyed/disposed. |
| `UnitPrice` | `decimal(12,2)`| NO | `0` | | Unit value at disposal time. |
| `TotalLossAmount`| `decimal(12,2)`| NO | `0` | | Computed loss amount (`Quantity * UnitPrice`). |
| `Reason` | `nvarchar(100)`| NO | | | Reason: `Expired`, `Damaged`, `Recalled`, `Contaminated`. |
| `DisposalMethod`| `nvarchar(150)`| YES | | | Method: `Incineration`, `Chemical Neutralization`, `Returned to Supplier`. |
| `Notes` | `nvarchar(500)`| YES | | | Regulatory compliance notes. |
| `DisposedByUserId`| `int` | YES | | | Pharmacist user executing disposal. |
| `DisposedBy` | `nvarchar(100)`| NO | `Central Pharmacist` | | Responsible person title. |
| `DisposedAt` | `datetime2` | NO | `SYSUTCDATETIME()` | | UTC timestamp of disposal event. |

---

## 6. Billing & Financial Settlement

### 6.1 `dbo.Invoices`
- **Description:** Aggregated patient billing statements detailing consultations, drugs, and lab charges.
- **Primary Key:** `InvoiceId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `InvoiceId` | `int` | NO | IDENTITY(1,1) | PK | Unique billing statement ID. |
| `InvoiceNumber` | `nvarchar(30)` | NO | | UK (`UQ_Invoices_InvoiceNumber`)| Official billing invoice serial number. |
| `PatientId` | `int` | NO | | | Billed patient ID. |
| `VisitId` | `int` | NO | | | Billed patient clinical visit ID. |
| `ConsultationFee`| `decimal(18,2)`| NO | `0.00` | | Professional physician consultation charge. |
| `PharmacyTotal` | `decimal(18,2)`| NO | `0.00` | | Dispensed medications subtotal. |
| `LaboratoryTotal`| `decimal(18,2)`| NO | `0.00` | | Performed laboratory diagnostics subtotal. |
| `MiscellaneousTotal`| `decimal(18,2)`| NO | `0.00` | | Additional clinic supplies or administration fee. |
| `SubTotal` | `decimal(18,2)`| NO | | | Gross statement total before discounts. |
| `DiscountType` | `nvarchar(50)` | NO | `None` | | Applied concession: `None`, `Senior Citizen (20%)`, `PWD (20%)`, `Employee`. |
| `DiscountPercentage`| `decimal(18,2)`| NO | `0.00` | | Concession discount percentage. |
| `DiscountAmount`| `decimal(18,2)`| NO | `0.00` | | Computed deducted discount amount. |
| `TotalAmount` | `decimal(18,2)`| NO | | | Final net payable amount after discount. |
| `PaymentStatus` | `nvarchar(30)` | NO | `Unpaid` | | Status: `Unpaid`, `PartiallyPaid`, `Paid`, `Voided`. |
| `CreatedAt` | `datetime2` | NO | `SYSDATETIME()` | | Statement generation timestamp. |
| `DueDate` | `datetime2` | YES | | | Payment due date. |

---

### 6.2 `dbo.Payments`
- **Description:** Official receipts and monetary transaction settlement records.
- **Primary Key:** `PaymentId` (Auto-incrementing Identity)

| Column Name | Data Type | Nullable | Default | Constraints | Description |
|---|---|:---:|---|---|---|
| `PaymentId` | `int` | NO | IDENTITY(1,1) | PK | Unique transaction payment ID. |
| `ReceiptNumber` | `nvarchar(30)` | NO | | UK (`UQ_Payments_ReceiptNumber`)| Official Receipt (OR) document number. |
| `InvoiceId` | `int` | NO | | FK -> `Invoices(InvoiceId)` | Settled statement reference. |
| `PaymentMethod` | `nvarchar(50)` | NO | `Cash` | | Settlement method: `Cash`, `GCash`, `Credit Card`, `Debit Card`. |
| `AmountPaid` | `decimal(18,2)`| NO | | | Cash tendered or digital authorization amount. |
| `ChangeAmount` | `decimal(18,2)`| NO | `0.00` | | Change returned to patient (Cash). |
| `ProcessedByUserId`| `int` | YES | | | Cashier or administrative staff user account. |
| `PaymentDate` | `datetime2` | NO | `SYSUTCDATETIME()` | | Transaction completion timestamp (UTC). |
| `Notes` | `nvarchar(250)`| YES | | | Transaction reference code or cashier notes. |

---

## 7. Analytical Views (Stretch Bonus)

### 7.1 `dbo.vw_DailyRevenueReport`
- **Description:** Transaction-level aggregated revenue view joining `Payments`, `Invoices`, `Patients`, and `Doctors`.
- **Primary Use:** Powers Daily Clinic Revenue & Collections report, payment method breakdowns, and accounting reconciliations.

| Column Name | Data Type | Nullable | Description |
|---|---|:---:|---|
| `PaymentId` | `int` | NO | Primary payment ID. |
| `ReceiptNumber` | `nvarchar(30)` | NO | Official Receipt number. |
| `InvoiceNumber` | `nvarchar(30)` | NO | Associated invoice number. |
| `PaymentDate` | `datetime2` | NO | Payment completion timestamp. |
| `PatientId` | `int` | NO | Billed patient ID. |
| `PatientName` | `nvarchar(162)` | NO | Computed patient full name (`LastName, FirstName`). |
| `DoctorId` | `int` | NO | Attending physician ID. |
| `DoctorName` | `nvarchar(205)` | NO | Formatted doctor title and name (`Dr. FirstName LastName`). |
| `PaymentMethod` | `nvarchar(50)` | NO | Method of settlement (`Cash`, `GCash`, `Credit Card`). |
| `SubTotal` | `decimal(18,2)`| NO | Statement gross amount before discount. |
| `DiscountType` | `nvarchar(50)` | NO | Concession discount type. |
| `DiscountAmount`| `decimal(18,2)`| NO | Total discounted deduction. |
| `AmountPaid` | `decimal(18,2)`| NO | Net tendered transaction amount. |
| `ChangeAmount` | `decimal(18,2)`| NO | Change returned to patient. |
| `Notes` | `nvarchar(250)`| YES | Reference number or cashier remarks. |

---

### 7.2 `dbo.vw_PharmacyInventoryExpiryReport`
- **Description:** Pharmacy inventory health and expiry risk analysis joining `MedicineInventory` with `Medicines`.
- **Primary Use:** Powers Drug Inventory & Expiry Watch report, stock valuation, and FEFO inventory reordering.

| Column Name | Data Type | Nullable | Description |
|---|---|:---:|---|
| `InventoryId` | `int` | NO | Unique batch stock record ID. |
| `MedicineId` | `int` | NO | Master formulary catalog ID. |
| `GenericName` | `nvarchar(150)`| NO | Generic drug title. |
| `BrandName` | `nvarchar(150)`| NO | Commercial brand name. |
| `DosageForm` | `nvarchar(60)` | NO | Physical drug formulation (`Tablet`, `Syrup`). |
| `Strength` | `nvarchar(60)` | NO | Dosage strength. |
| `BatchNumber` | `nvarchar(60)` | NO | Manufacturer production lot number. |
| `ExpiryDate` | `date` | NO | Batch expiration date. |
| `QuantityOnHand`| `int` | NO | Physical units in stock. |
| `ReorderLevel` | `int` | NO | Minimum safety stock threshold. |
| `UnitPrice` | `decimal(12,2)`| NO | Retail dispensing unit price. |
| `TotalInventoryValue`| `decimal(18,2)`| YES | Computed inventory valuation (`QuantityOnHand * UnitPrice`). |
| `DaysUntilExpiry`| `int` | YES | Computed calendar days remaining until expiration (`DATEDIFF`). |
| `StockStatus` | `nvarchar(18)` | NO | Evaluated stock health: `Expired`, `Near Expiry (<60d)`, `Low Stock`, `Optimal`. |

---

### 7.3 `dbo.vw_DoctorConsultationReport`
- **Description:** Clinical consultation productivity view joining `Consultations`, `PatientVisits`, `Doctors`, `Patients`, and aggregate prescription counts.
- **Primary Use:** Powers Physician Consultations & Diagnostics Activity report, physician revenue analysis, and patient diagnostic summaries.

| Column Name | Data Type | Nullable | Description |
|---|---|:---:|---|
| `ConsultationId`| `int` | NO | Encounter consultation record ID. |
| `ConsultationDate`| `datetime2`| NO | Date and time consultation took place. |
| `DoctorId` | `int` | NO | Attending physician ID. |
| `DoctorName` | `nvarchar(205)`| NO | Attending physician full name. |
| `Specialty` | `nvarchar(100)`| NO | Physician medical specialty. |
| `PatientId` | `int` | NO | Patient ID. |
| `PatientNumber` | `nvarchar(30)` | YES | Patient hospital MRN code. |
| `PatientName` | `nvarchar(162)`| NO | Patient full name. |
| `PatientAge` | `int` | YES | Computed patient age in years at consultation time. |
| `PatientSex` | `nvarchar(20)` | NO | Biological sex. |
| `ChiefComplaint`| `nvarchar(500)`| NO | Clinical presenting complaint. |
| `Diagnosis` | `nvarchar(500)`| NO | Medical assessment and diagnosis. |
| `PrescribedItemsCount`| `int` | NO | Aggregated total count of prescribed drug items. |
| `ConsultationFee`| `decimal(18,2)`| NO | Doctor consultation fee charged. |
