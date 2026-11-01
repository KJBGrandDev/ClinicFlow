# 🏥 ClinicFlow-Unified

[![.NET 10.0](https://img.shields.io/badge/.NET-10.0%20WindowsForms-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/Database-Microsoft%20SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![License: Academic](https://img.shields.io/badge/License-Academic%20Evaluation-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing%20(100%25%20QA)-10B981)](#-automated-testing--qa-verification)

> **ClinicFlow-Unified** is an enterprise-grade outpatient electronic medical record (EMR), queue command board, point-of-sale cashier billing, and FEFO pharmaceutical inventory desktop system built with **C# .NET 10** and **Microsoft SQL Server**.

---

## 🌟 Key Functional Modules & Features

```
                                  ┌───────────────────────────┐
                                  │      ClinicFlow Core      │
                                  └─────────────┬─────────────┘
                                                │
         ┌──────────────────┬───────────────────┼───────────────────┬──────────────────┐
         ▼                  ▼                   ▼                   ▼                  ▼
┌─────────────────┐ ┌───────────────┐ ┌───────────────────┐ ┌───────────────┐ ┌───────────────┐
│  1. Front-Desk  │ │  2. Physician │ │ 3. Emergency      │ │  4. Cashier   │ │ 5. Central    │
│  Reception &    │ │  Consultation │ │    Triage Center  │ │  Billing &    │ │    Pharmacy   │
│  Lobby Queue    │ │  & EMR (SOAP) │ │    & Resolution   │ │  Settlement   │ │    (FEFO)     │
└─────────────────┘ └───────────────┘ └───────────────────┘ └───────────────┘ └───────────────┘
```

### 1. 📋 Front-Desk Reception & Queue Command Board
- **Patient Registration & Lookup:** Fast autocomplete search across Name, Patient Number (`PT-YYYY-XXXXX`), and Contact Number.
- **Defensive Boundary Validation:** Enforces legal Philippine phone numbers and validates Birth Dates (blocking unconfigured `1/1/1800` defaults and future dates).
- **Conflict Guard Engine:** Evaluates attending doctor schedules in real-time, detecting and warning of double-bookings or concurrent consultations.
- **Lobby Queue Ticket Issuance:** Atomically assigns sequential tickets (`Q-01`, `Q-02`, etc.) upon patient physical check-in using SQL hold locks.
- **Room Status Station Cards:** Live color-coded room status cards (`AVAILABLE`, `AWAITING DOCTOR`, `IN SERVICE`, `EMERGENCY LEAVE`).

### 2. 🩺 Physician Consultation Hub & Electronic Medical Records (EMR)
- **Queue Synchronization:** Transition patient status with one click (`Waiting` $\rightarrow$ `Called` $\rightarrow$ `In Service`).
- **Clinical SOAP Documentation:** Track Vitals (BP, Temp, Pulse, Weight), Chief Concern, Physical Examination, and Longitudinal History.
- **ICD-10 Diagnostic Engine:** Mandatory clinical diagnostic enforcement before consultation completion.
- **Electronic Prescribing:** Integrated formulary selector with automatic dosage, route, frequency, and duration specifications.
- **Doctor Practice Routine:** Visual 30-minute timeline slot manager with recurring weekly routines and slot unblock/block toggles.

### 3. 🚨 Emergency Schedule Blockout & Front-Desk Triage Center
- **Doctor Emergency Declaration:** One-click STAT emergency blockout for acute physician illness, emergency surgeries, or ward calls.
- **Automated Patient Displacement:** Atomically detaches affected appointments from the active lobby queue, changes status to `Displaced`, and logs clinical justifications.
- **Front-Desk Emergency Alert Banner:** High-visibility banner (`[⚡ Open Emergency Triage Center]`) alerts the front desk immediately upon doctor displacement.
- **Triage Center Resolution Hub (`EmergencyTriageDialog`):**
  - **Option 1 (Peer Doctor Transfer):** Queries other active physicians on duty today with open slots and transfers patient with 1 click.
  - **Option 2 (Fast-Track Reschedule):** Queries attending doctor's earliest return working days and priority-reschedules patient.
  - **Option 3 (Outreach Call Logger):** Records patient phone outreach status (`Contacted - Confirmed`, `Left Voicemail`, `Unreachable`).
- **Displaced Check-in Safety Guard:** Prevents front-desk staff from accidentally checking in displaced patients into unattended rooms.

### 4. 💳 Cashier Point-of-Sale Billing & Settlement
- **Automated Bill Compilation:** Gathers consultation fee and itemized pharmacy prescription lines into one unified invoice.
- **Statutory Discount Engine:** Seamless application of Republic Act 9994 (Senior Citizen 20% VAT-exempt), PWD 20%, PhilHealth, Employee, and HMO discounts.
- **Underpayment Safety Guard:** Prevents receipt issuance if tendered cash is less than the net amount due.
- **Official Receipt Generation:** Generates unique, tamper-proof receipts (`OR-YYYY-XXXXX`) and logs transaction audits.

### 5. 💊 Central Pharmacy Hub & FEFO Inventory Management
- **Dispensing Station Paid Queue:** Dedicated station for dispensing paid prescriptions with real-time stock availability verification.
- **FEFO Batch Allocation:** Dispenses from the earliest expiring batch lot first (First-Expiry, First-Out).
- **Formulary Duplicate Guard:** Blocks insertion of identical generic formulation, form, and strength entries.
- **Safe Medicine Condemnation & Disposal Log:** Dedicated destruction dialog for expired or compromised lots (Chemical Inactivation & Encapsulation or Incineration) with financial loss write-off tracking.
- **Pharmacy Financial Intelligence:** Real-time analytics tracking Gross Sales, Cost of Goods Sold (COGS), Gross Profit %, Disposal Loss, and Shrinkage Benchmarks.

---

## 🏛️ System Architecture

ClinicFlow-Unified implements a clean **4-Tier Relational Architecture**:

```
┌────────────────────────────────────────────────────────┐
│            Presentation Tier (Windows Forms)           │
│  ClinicTheme • Custom Double-Buffered Grids • Task.Run │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│              Business Logic & Service Layer            │
│  AppointmentService • PatientService • BillingService  │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│             Data Access Layer (Repository Pattern)     │
│   Raw ADO.NET • Parameterized SQL • Explicit Locks     │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│             Database Tier (Microsoft SQL Server)       │
│  25 Normalized Tables • Foreign Keys • Check Constraints│
└────────────────────────────────────────────────────────┘
```

- **Direct ADO.NET with Explicit Locks:** Avoids ORM lock contention during high-throughput queue ticketing using `UPDLOCK, HOLDLOCK`.
- **Zero SQL Injection:** 100% of SQL statements utilize strictly typed, bounded `SqlParameter` parameters.
- **BCrypt Password Security:** Work factor 11 password hashing with automated 15-minute brute-force lockout after 5 failed login attempts.
- **Tamper-Evident Audit Trails:** Critical events log directly to `dbo.AuditLogs` with actor identity, action type, table, and timestamp.

---

## 🧪 Automated Testing & QA Verification

The system includes a comprehensive automated integration test suite (`TestRunner/Program.cs`) covering all clinical roles:

```text
================================================================================
    CLINICFLOW INTEGRATED SYSTEM & QA WORKFLOW AUTOMATED TEST SUITE (.NET 10)  
================================================================================
Total Test Scenarios : 44
Passed               : 44
Failed               : 0
Success Rate         : 100%
================================================================================
```

### Coverage Categories:
- ✅ **Valid Input:** Standard patient registration, slot reservations, SOAP consultations.
- ✅ **Invalid & Empty Input:** Empty clinical diagnoses, missing patient names.
- ✅ **Boundary Value Testing:** Rejection of legacy birth dates (`<= 1/1/1800`), cashier underpayment validation.
- ✅ **SQL Injection Defense:** Injection attacks in login and patient search boxes (`admin' OR '1'='1 --`).
- ✅ **Duplicate Entry Guards:** Formulary drug duplicate prevention.
- ✅ **Insufficient Stock Guards:** Halting dispenses exceeding active lot quantities.
- ✅ **Rollback on Failure:** Transaction rollback upon mid-settlement disconnection.
- ✅ **Role-Based Access Control:** Blocking unauthorized roles from invoking doctor consultation APIs.
- ✅ **Brute-Force Account Lockout:** Triggering 15-minute lockouts after 5 consecutive bad passwords.
- ✅ **Foreign Key Referential Integrity:** Blocking deletion of patients with existing clinical visits.
- ✅ **Edge-Case Sequence Locks:** Resolving appointment number sequence collisions across multi-date reschedules.
- ✅ **Doctor Emergency Triage:** Validating STAT emergency displacement, peer transfer, and front-desk check-in guards.

---

## 💻 Tech Stack & Prerequisites

- **Language & Runtime:** C# 13 / Microsoft .NET 10.0 SDK & Desktop Runtime
- **UI Framework:** Windows Forms (WinForms) with custom modern UI styling (`ClinicTheme.cs`)
- **Database Engine:** Microsoft SQL Server 2019 / 2022 (Developer, Standard, Express) or LocalDB
- **Database Driver:** `Microsoft.Data.SqlClient` (v5.2.2)
- **Security:** `BCrypt.Net-Next` (v4.0.3)

---

## 🚀 Installation & Getting Started

### 1. Database Setup
1. Open **SQL Server Management Studio (SSMS)** and connect to your SQL Server instance (`localhost` or `.\SQLEXPRESS`).
2. Create the clinic database:
   ```sql
   CREATE DATABASE ClinicFlowUnifiedDb;
   ```
3. Execute the migration script located at `ClinicFlow-Unified/Database/Schema_Migration_v2.sql` to initialize all 25 tables, check constraints, and default indexes.

### 2. Verify Connection String
Verify or edit `ClinicFlow-Unified/Data/DatabaseConnection.cs`:
```csharp
private const string DefaultConnectionString = 
    "Server=localhost;Database=ClinicFlowUnifiedDb;Trusted_Connection=True;TrustServerCertificate=True;";
```

### 3. Build & Run
Open PowerShell in the repository root:
```powershell
# Restore & build solution
dotnet build -c Release

# Run automated integration test runner
dotnet run --project "..\scratch\TestRunner"

# Launch ClinicFlow-Unified Application
cd ClinicFlow-Unified
dotnet run
```

---

## 🔑 Default System Seed Accounts

| Username | Role | Full Name | Default Password | Access Scope |
| :--- | :--- | :--- | :--- | :--- |
| `admin` | **Admin** | Kent Jason Basalan | `Admin@123` | Full System Administration, Audit Logs, Staff Rosters |
| `drsantos` | **Doctor** | Dr. Juan Santos | `Doc@123` | Physician Consultation Room, EMR, Prescriptions, Routine |
| `secretary` | **Secretary** | Ana Reyes | `Sec@123` | Patient Registration, Scheduling, Lobby Queue, Triage Hub |
| `pharmacist`| **Pharmacist**| Maria Santos, RPh | `Pharm@123` | Dispensing Station, Inventory Restock, Batch Disposal |

---

## 👥 Authors & Academic Contributors

- **Kent Jason Basalan** — *Lead Software Architect & Database Administrator* ([@KJBGrandDev](https://github.com/KJBGrandDev))
- **ClinicFlow Development Team** — *BSIT / BSCS Capstone Project 2025–2026*

---

## 📄 License
This repository is developed for academic evaluation, software architecture defense, and clinical demonstration purposes. All rights reserved.
