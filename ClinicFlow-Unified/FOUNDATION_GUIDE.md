# ClinicFlow Foundation Guide

## What this covers

This is the **working foundation** of ClinicFlow: database connection, first Admin setup, sign-in, session, dashboard shell, and audit logging.

It does **not** yet include patients, appointments, cashier work, doctors, pharmacy, laboratory, reports, or ETL.

## The big picture

```text
Person uses a form
        ↓
Service checks the rules
        ↓
Repository runs safe SQL
        ↓
SQL Server stores or reads data
```

Simple rule: **Forms do not contain SQL. Repositories do not decide business rules.**

## What happens when the app starts

```text
Program.cs
  ├─ checks that SQL Server can be reached
  ├─ checks whether any user exists
  ├─ no users  → InitialSetupForm
  └─ users exist → LoginForm
```

## Normal sign-in flow

```text
LoginForm
  → AuthenticationService
  → UserRepository reads the user and role
  → PasswordHasher checks the password hash
  → AuditService records success or failure
  → UserSession remembers the signed-in user
  → MainDashboardForm opens
```

## Folder map

| Folder | Plain-English job | What belongs here |
|---|---|---|
| `Forms` | Screens the staff can see and use. | Buttons, text boxes, labels, short UI feedback. |
| `Services` | The rule keeper. | Login rules, validation, workflow rules, authorization. |
| `Repositories` | The database messenger. | Parameterized SQL and mapping SQL rows to models. |
| `Models` | Simple data containers. | `User`, `Role`, and future `Patient`, `Appointment`, etc. |
| `Data` | Database setup helpers. | Configuration and connection creation. |
| `Utilities` | Small reusable helpers. | Password hashing and the current-user session. |
| `Database/Scripts` | Rebuildable database instructions. | Numbered SQL schema and seed scripts. |

## Files and their duties

### App startup and settings

| File | Job | Used by |
|---|---|---|
| `Program.cs` | Starts WinForms, checks the database, then chooses Initial Setup or Login. | The whole application. |
| `appsettings.json` | Holds the local development connection string under `ConnectionStrings:ClinicFlowDatabase`. | `AppConfiguration`. |
| `Data/AppConfiguration.cs` | Reads `appsettings.json` and exposes the connection string. | `SqlConnectionFactory`. |
| `Data/SqlConnectionFactory.cs` | Creates a new closed SQL Server connection when needed. | Every repository. |

**Important:** `appsettings.json` is configuration, not a place for passwords or business rules. The current setup uses Windows Authentication. Do not commit real production secrets to source control.

### Models (data shapes)

| File | Represents | Important detail |
|---|---|---|
| `Models/Role.cs` | One row from `dbo.Roles`. | Holds role name, description, active state, and creation time. |
| `Models/User.cs` | One row from `dbo.Users` plus the joined role name. | `PasswordHash` is stored; a real password is never stored. |
| `Models/AuditLogEntry.cs` | One row from `dbo.AuditLogs`. | Can record who, what happened, which record, which computer, and when. |

### Repositories (database access only)

| File | Reads/writes | Main methods |
|---|---|---|
| `Repositories/RoleRepository.cs` | `dbo.Roles` | Finds an active role by name. |
| `Repositories/UserRepository.cs` | `dbo.Users` and `dbo.Roles` | Finds a user by username, creates a user, checks whether any user exists. |
| `Repositories/AuditLogRepository.cs` | `dbo.AuditLogs` | Inserts an audit record. |

All repository input is sent with SQL parameters such as `@Username`. This protects against SQL injection and keeps text values safe.

### Services (rules and workflows)

| File | Rule it owns |
|---|---|
| `Services/AuthenticationService.cs` | Rejects missing/inactive/incorrect credentials; logs `LOGIN_SUCCESS` or `LOGIN_FAILED`. |
| `Services/UserManagementService.cs` | Creates the first Admin only when no user exists; requires name, username, and a 12+ character password; hashes the password; logs the creation. |
| `Services/AuditService.cs` | Builds a consistent audit entry and adds the computer name before saving it. |

### Utilities (small reusable tools)

| File | Job |
|---|---|
| `Utilities/PasswordHasher.cs` | Creates and checks PBKDF2 password hashes with a fresh random salt. The database stores a safe encoded hash, not the password. |
| `Utilities/UserSession.cs` | Keeps the signed-in user's ID, role ID, role name, and full name while this desktop app is running. `Clear()` removes them on logout. |

### Forms (what the staff sees)

| File pair | Job | When it appears |
|---|---|---|
| `Forms/InitialSetupForm.cs` + `.Designer.cs` | Creates the very first Admin account. | Only while `dbo.Users` has no records. |
| `Forms/LoginForm.cs` + `.Designer.cs` | Collects username and password, then starts the user session after successful authentication. | Normal startup after at least one user exists. |
| `Forms/MainDashboardForm.cs` + `.Designer.cs` | Shows the signed-in user's name and role and provides logout. | Immediately after successful login. |
| Each `.resx` file | Stores form resources used by the WinForms Designer. | Managed by Visual Studio; normally do not edit it by hand. |

`*.Designer.cs` contains visual-control setup. Keep form event/business logic in the matching `.cs` file.

## Database scripts

Run these in SSMS in number order on a new development database:

| Order | File | Creates or adds |
|---:|---|---|
| 1 | `Database/Scripts/001_CreateDatabase.sql` | The `ClinicFlowUnifiedDb` database. Safe to run again. |
| 2 | `Database/Scripts/002_CreateSecurityTables.sql` | `Roles`, `Users`, and `AuditLogs`, plus keys, foreign keys, defaults, and indexes. Run once on a new database. |
| 3 | `Database/Scripts/003_SeedRoles.sql` | Admin, Receptionist, Cashier, Doctor, Pharmacist, and Laboratory Staff roles. Safe to run again. |

### The three current tables

| Table | Purpose | Key protection |
|---|---|---|
| `dbo.Roles` | The allowed job roles. | Each role name is unique. |
| `dbo.Users` | App accounts. | Usernames are unique; each user must point to a valid role. |
| `dbo.AuditLogs` | A history of important actions. | Uses UTC timestamps and links to a user when that user is known. |

## Quick "where do I put this?" guide

| If you are adding... | Put it in... |
|---|---|
| A textbox, button, label, or DataGridView | A form's `.Designer.cs` (through the Designer when practical). |
| What happens after a button is clicked | The matching form `.cs` file, usually by calling a service. |
| A rule such as "unpaid visits cannot start consultation" | A service. |
| An SQL `SELECT`, `INSERT`, `UPDATE`, or `DELETE` | A repository. |
| A C# version of a database record | `Models`. |
| A new table, constraint, index, or seed data | A new numbered SQL file in `Database/Scripts`. |

## What has been tested

- The app reaches SQL Server through the configured connection string.
- The first Admin can be created when the user table is empty.
- The Admin can sign in with the correct password.
- An incorrect password is rejected with a general message.
- Login success and failure produce audit-log records.
- Logout clears the in-memory session and restarts at Login.

## Honest current limits

- The dashboard only displays the name and role. It does **not** yet restrict buttons or modules by role; that comes before real clinic modules are added.
- Only the initial Admin-creation path exists. A normal Admin user-management screen is still needed to create staff accounts.
- The first Admin created before audit logging was added has no creation audit entry. Do not fabricate one; future first-Admin creation is logged.
- This is a learning/development setup. Before real deployment, move environment-specific connection details out of source-controlled files and add a stricter multi-user setup/transaction strategy.

## Next planned work

1. Add reference/master data, starting with doctors and schedules.
2. Build patient records and patient number generation.
3. Add appointments, queueing, payments, clearance gates, consultation, pharmacy, laboratory, reports, and analytics in that order.

