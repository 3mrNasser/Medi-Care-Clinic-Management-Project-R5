# MediCare - Clinic Management & Appointment System

[![CI](https://github.com/3mrNasser/Medi-Care-Clinic-Management-Project-R5/actions/workflows/ci.yml/badge.svg)](https://github.com/3mrNasser/Medi-Care-Clinic-Management-Project-R5/actions/workflows/ci.yml)

**Initiative:** Digital Egypt Pioneers Initiative (DEPI)  
**Track:** Full Stack .NET Web Developer  
**Architecture Style:** Layered MVC Monolith / N-Tier Architecture (`MediCare.Web`, `MediCare.Services`, `MediCare.Data`)  

---

## Project Overview
**MediCare** is a comprehensive web-based clinic management and appointment booking platform that connects patients with healthcare providers. It enables patients to search for doctors by specialization, location, and availability, book conflict-free appointments, and access their medical history and digital prescriptions. Simultaneously, it empowers doctors to manage their weekly schedules, confirm or reject appointment requests, record clinical visits, and issue printable prescriptions, while providing administrators with full system oversight and visual analytics.

---

## Getting Started

### System requirements
| Requirement | Version / notes |
| :--- | :--- |
| **.NET SDK** | **10.0** (`dotnet --version` → `10.0.x`) |
| **SQL Server** | **LocalDB** (comes with Visual Studio "ASP.NET and web development") or any SQL Server 2019+ |
| **IDE** (optional) | Visual Studio 2022 17.14+, Rider, or VS Code with the C# Dev Kit |
| **EF Core CLI** | `dotnet tool install --global dotnet-ef` (to create/update the database) |
| Hardware | Any machine that runs the .NET 10 SDK (4 GB RAM is enough) |

### Installation and first run
```bash
# 1. Get the code
git clone https://github.com/3mrNasser/Medi-Care-Clinic-Management-Project-R5.git
cd Medi-Care-Clinic-Management-Project-R5

# 2. Restore and build
dotnet restore MediCare.slnx
dotnet build MediCare.slnx

# 3. Create / update the database (LocalDB "MediCareDb" from appsettings.json)
dotnet ef database update --project MediCare.Data --startup-project MediCare.Web

# 4. Run (Development seeds sample doctors, slots, a patient and the dev accounts below)
dotnet run --project MediCare.Web --environment Development
```
Open the URL printed in the console (for example `https://localhost:7xxx`).

> To use another SQL Server, change `ConnectionStrings:DefaultConnection` in `MediCare.Web/appsettings.json`
> (or set it with user-secrets / an environment variable `ConnectionStrings__DefaultConnection`).

### Development accounts (Development environment only)
All use the password **`Dev123456`**. They are created by `MediCare.Data/Seed/IdentitySeeder.cs` and never in Production.

| Role | Email | Starts at |
| :--- | :--- | :--- |
| Patient | `patient.test@medicare.test` | `/Patient/Home` |
| Doctor | `ahmed.hassan@medicare.test` (also `sara.mostafa@`, `omar.khaled@`, `mona.adel@`) | `/DoctorAppointments` |
| Admin | `admin@medicare.test` | `/Admin/Dashboard` |

### Configuration (optional services)
Secrets are **never** stored in `appsettings.json` or in git. Use user-secrets on your machine
(the project already has a `UserSecretsId`) and App Service settings on Azure.
Every service below is optional: without it the app still runs, and in Development the message is written to the console log instead.

| Feature | Settings | How to set them locally |
| :--- | :--- | :--- |
| **Email** (password-reset codes, appointment emails, reminders) | `Email:Host`, `Email:Port`, `Email:UserName`, `Email:Password`, `Email:From`, `Email:EnableSsl` | `dotnet user-secrets set "Email:Host" "smtp.gmail.com" --project MediCare.Web` (Gmail: use an *App Password*; SendGrid SMTP also works: host `smtp.sendgrid.net`, user `apikey`) |
| **SMS** (24-hour reminders, Twilio) | `Twilio:AccountSid`, `Twilio:AuthToken`, `Twilio:From` | `dotnet user-secrets set "Twilio:AccountSid" "AC..." --project MediCare.Web` (a trial account can only text verified numbers) |
| **Google sign-in** | `Authentication:Google:ClientId`, `Authentication:Google:ClientSecret` | `dotnet user-secrets set "Authentication:Google:ClientId" "..." --project MediCare.Web` (redirect URI: `https://<host>/signin-google`) |
| **Reminder job** | `Reminders:Enabled` (default `true`), `Reminders:IntervalMinutes` (default `10`) | `appsettings.json` or user-secrets |

---

## Features

| Role | What they can do |
| :--- | :--- |
| **Patient** | Sign up / log in (or Google), reset a forgotten password with an emailed code, search doctors by specialty, name, location and **a free day**, book a 30-minute slot, cancel or reschedule, see the next appointment on the dashboard, keep a **medical profile** (date of birth, gender, blood type, allergies, chronic conditions, medications, history, emergency contact), **upload test results / X-rays / reports** (PDF, JPG, PNG up to 10 MB), read prescriptions (printable) and medical records, choose **email / SMS notification preferences**, get live notifications and reminders, switch **dark / light mode**. |
| **Doctor** | Complete the profile (specialty from the admin's list, clinic, fee), set weekly working hours and **days off** (no bookings on those dates), see appointments as a list or a FullCalendar calendar, confirm / reject / complete / mark no-show, open a **patient's details** (allergies first, visits, documents) and add files to their record, write prescriptions and medical records, get a live notification for every new booking, change or cancellation. |
| **Admin** | **Dashboard with Chart.js**: monthly appointments, revenue, most popular doctors and specialties, status breakdown, patient gender and age groups, new patients; **export to PDF (QuestPDF) and Excel (ClosedXML)** for any date range. Approve doctors, **manage patients** (search, block / unblock sign-in), **see and cancel any appointment** (patient and doctor are notified), **manage specializations** (add, rename, hide). |

### Notifications and reminders
* **Factory Pattern:** `MediCare.Services/Notifications/AppointmentNotifications.cs`. `NotificationFactory.Create(type)` returns one class per event (booked, rescheduled, cancelled, confirmed, rejected, completed, reminder). Each class decides the recipient, the text, the link, and whether email/SMS are also sent.
* **In-app:** saved in the `Notifications` table and listed at `/Notifications` (patients and doctors). The sidebar shows an unread badge.
* **Real-time (SignalR):** `MediCare.Web/Hubs/NotificationHub.cs` at `/hubs/notifications`. A new notification appears immediately as a toast and the badge updates, without a page reload.
* **Email:** confirmations, rejections and reminders go to the patient (`SmtpEmailService`).
* **24-hour reminders:** `ReminderBackgroundService` runs every 10 minutes and sends one reminder (in-app + email + SMS) for each **confirmed** appointment that starts within 24 hours. `Appointment.ReminderSentAtUtc` makes sure it's sent only once; rescheduling clears it.
* A failed email/SMS/push never breaks the booking itself; it is logged.

### Security
ASP.NET Core Identity with role-based authorization (`[Authorize(Roles = "...")]` on every controller), anti-forgery tokens on every POST form, local-only redirects (`Url.IsLocalUrl`), server-side validation, EF Core parameterized queries, hashed one-time password-reset codes (10 minutes, 5 tries, one use), and no secrets in the repository.

---

## Testing
```bash
dotnet test MediCare.slnx
```
`MediCare.Tests` (xUnit, **46 tests**):
* **Unit tests:** `NotificationFactory` (right type, recipient, channels and text for every event) and `ReminderRules` (due only within the next 24 hours).
* **Integration tests** (real services, repositories and Unit of Work on an in-memory SQLite database): booking creates a pending appointment and notifies the doctor; the same time can't be booked twice; times outside working hours are refused; booked times disappear from the free times; confirming notifies the patient in-app and by email; another doctor can't confirm; cancelling frees the time and notifies the doctor; a reminder is sent once by every channel; no reminder for pending or far appointments; "mark all as read"; a user can't open someone else's notification.
* **Clinic features** (same in-memory database): a day off removes every free time and blocks booking; a day off is refused while patients are booked; only real PDF/JPG/PNG files up to 10 MB are accepted (a renamed file is rejected); only the patient and doctors they booked with can open a file; doctors upload only for their own patients; duplicate specializations are refused and renaming moves the doctors; hidden specializations are not offered; the profile is validated; email is skipped when the patient turned it off (in-app stays).

## CI/CD and deployment
GitHub Actions workflow: [`.github/workflows/ci.yml`](.github/workflows/ci.yml).
* **Every pull request and push to `main`:** restore → build (Release) → run all tests → upload the test results.
* **Deploy to Azure App Service** (push to `main`): runs only after the Azure setup exists. Add, in *Settings → Secrets and variables → Actions*:
  * variable `AZURE_WEBAPP_NAME` (the App Service name)
  * secret `AZURE_WEBAPP_PUBLISH_PROFILE` (Azure portal → App Service → *Download publish profile*)
* On Azure, set `ConnectionStrings__DefaultConnection` (Azure SQL) and the optional secrets above as **App Service → Configuration** settings, then apply the migrations once:
  `dotnet ef database update --project MediCare.Data --startup-project MediCare.Web --connection "<Azure SQL connection string>"`.

---

## Project Team (Team 1)

| # | Team Member | Role & Title | Key Responsibilities |
| :--- | :--- | :--- | :--- |
| **1** | **Amr Nasser** | **Team Leader, Software Architect & Core Backend Lead** | Project leadership, GitHub repository management, N-Tier architecture setup (`MediCare.Web`, `MediCare.Services`, `MediCare.Data`), EF Core schema design, Generic & Specific Repositories, Unit of Work, and server-side booking conflict detection. |
| **2** | **Basmala Mohamed** | **Identity, Security & Admin Dashboard Developer** | Multi-role authentication and authorization (`Patient`, `Doctor`, `Admin`) using ASP.NET Core Identity, role-specific registration workflows, and Admin Dashboard management (doctor approvals, user accounts, and specializations). |
| **3** | **Ali Mohamed** | **Real-Time, Notifications & Cloud DevOps Engineer** | Factory Pattern (`NotificationFactory`) implementation, real-time alerts via SignalR (`AppointmentHub`), automated 24-hour reminders via Twilio (SMS) and SendGrid (Email), and Microsoft Azure deployment with CI/CD pipelines. |
| **4** | **Sara Elsaeid** | **Frontend, Interactive Calendar & UI/UX Lead** | UI/UX wireframing, responsive views using Bootstrap 5, WCAG 2.2 AA accessibility compliance, doctor search/filter UI, FullCalendar.js integration, and Doctor Dashboard schedule views. |
| **5** | **Youssef Elazrk** | **Medical Modules & Analytics Developer** | Patient Profile dashboard, authorized Medical Records and Digital Prescription System (print-friendly format), medical document/X-ray uploads, and Chart.js administrative analytics with PDF/Excel export. |

---

## Repository Folder Structure

```text
├── MediCare.Web/          # Presentation: MVC controllers, Razor views, SignalR hub, background jobs, wwwroot (vitala.css/js)
├── MediCare.Services/     # Business logic: services, DTOs, NotificationFactory, booking & reminder rules
├── MediCare.Data/         # Data access: EF Core DbContext, entities, migrations, Generic Repository, Unit of Work, seeders
├── MediCare.Tests/        # xUnit unit + integration tests
├── .github/workflows/     # CI/CD (build, test, deploy to Azure)
│
├── docs/                                                   # Official PDF Deliverables
│   ├── 01_Project Planning & Management.pdf
│   ├── 02_ Literature Review.pdf
│   ├── 03_Requirements Gathering.pdf
│   ├── 04_1_Problem Statement & Software Architecture.pdf
│   ├── 04_2_Database Design & Data Modeling.pdf
│   ├── 04_3_Data Flow & System Behavior.pdf
│   ├── 04_4_UI UX Design & Prototyping.pdf
│   ├── 04_5_System Deployment & Integration.pdf
│   └── 04_6_Additional Deliverables.pdf
│
├── mdFiles/                                                # Markdown Documentation & Mermaid Diagrams
│   ├── 1. Project Planning and Management.md
│   ├── 2. Literature Review.md
│   ├── 3. Requirements Gathering.md
│   ├── 4.1 Problem Statement and Software Architecture.md
│   ├── 4.2 Database Design and Data Modeling.md
│   ├── 4.3 Data Flow and System Behavior.md
│   ├── 4.4 UI UX Design and Prototyping.md
│   ├── 4.5 System Deployment and Integration.md
│   └── 4.6 Additional Deliverables.md
│
└── README.md                                               # Main Project Documentation Index
```

---

## Project Documentation Index (DEPI Deliverables)

All project deliverables are available in both **interactive Markdown format (`mdFiles/`)** and **printable PDF format (`docs/`)**:

### Phase 1: Planning, Literature Review & Requirements (Deadline: 16 Oct 2026)

| Section | Deliverable Details | Markdown File (`mdFiles/`) | PDF Document (`docs/`) |
| :--- | :--- | :--- | :--- |
| **1. Project Planning & Management** | Proposal, Timeline, Gantt Chart, Roles, Risk Plan & KPIs | [1. Project Planning and Management.md](mdFiles/1.%20Project%20Planning%20and%20Management.md) | [01_Project Planning & Management.pdf](docs/01_Project%20Planning%20%26%20Management.pdf) |
| **2. Literature Review** | Lecturer Evaluation, Suggested Improvements & Grading Criteria | [2. Literature Review.md](mdFiles/2.%20Literature%20Review.md) | [02_ Literature Review.pdf](docs/02_%20Literature%20Review.pdf) |
| **3. Requirements Gathering** | Stakeholders, User Stories, Use Cases, FRs (`FR-01`–`FR-17`) & NFRs (`NFR-01`–`NFR-10`) | [3. Requirements Gathering.md](mdFiles/3.%20Requirements%20Gathering.md) | [03_Requirements Gathering.pdf](docs/03_Requirements%20Gathering.pdf) |

---

### Phase 2: System Analysis & Design (Deadline: 6 Nov 2026)

| Section | Deliverable Details | Markdown File (`mdFiles/`) | PDF Document (`docs/`) |
| :--- | :--- | :--- | :--- |
| **4.1 Problem Statement & Architecture** | Objectives (`O1`–`O7`), Scope, Use Case Diagram (`UC-01`–`UC-15`) & N-Tier Architecture | [4.1 Problem Statement and Software Architecture.md](mdFiles/4.1%20Problem%20Statement%20and%20Software%20Architecture.md) | [04_1_Problem Statement & Software Architecture.pdf](docs/04_1_Problem%20Statement%20%26%20Software%20Architecture.pdf) |
| **4.2 Database Design & Data Modeling** | Corrected ERD, Relationship Matrix, `3NF` Rules & Physical SQL Schema | [4.2 Database Design and Data Modeling.md](mdFiles/4.2%20Database%20Design%20and%20Data%20Modeling.md) | [04_2_Database Design & Data Modeling.pdf](docs/04_2_Database%20Design%20%26%20Data%20Modeling.pdf) |
| **4.3 Data Flow & System Behavior** | Level 0/1 DFD, Sequence, Activity, State & Class Diagrams | [4.3 Data Flow and System Behavior.md](mdFiles/4.3%20Data%20Flow%20and%20System%20Behavior.md) | [04_3_Data Flow & System Behavior.pdf](docs/04_3_Data%20Flow%20%26%20System%20Behavior.pdf) |
| **4.4 UI/UX Design & Prototyping** | Wireframes, Color Palette (`#56060D`), Typography & `WCAG 2.2 AA` Guidelines | [4.4 UI UX Design and Prototyping.md](mdFiles/4.4%20UI%20UX%20Design%20and%20Prototyping.md) | [04_4_UI UX Design & Prototyping.pdf](docs/04_4_UI%20UX%20Design%20%26%20Prototyping.pdf) |
| **4.5 System Deployment & Integration** | Technology Stack, Cloud Deployment Diagram & N-Tier Component Diagram | [4.5 System Deployment and Integration.md](mdFiles/4.5%20System%20Deployment%20and%20Integration.md) | [04_5_System Deployment & Integration.pdf](docs/04_5_System%20Deployment%20%26%20Integration.pdf) |
| **4.6 Additional Deliverables** | Internal Endpoints/API Docs, Testing & Validation Plan & Azure CI/CD Strategy | [4.6 Additional Deliverables.md](mdFiles/4.6%20Additional%20Deliverables.md) | [04_6_Additional Deliverables.pdf](docs/04_6_Additional%20Deliverables.pdf) |

---

### Phase 3: Implementation, Testing & Final Presentation (Deadlines: 30 Nov / 4 Dec 2026)

| Section | Deliverable Details | Markdown File (`mdFiles/`) | PDF / File (`docs/`) |
| :--- | :--- | :--- | :--- |
| **5. Implementation** | Code structure & standards, branching and pull requests, execution, requirements traceability | [5. Implementation (Source Code and Execution).md](mdFiles/5.%20Implementation%20(Source%20Code%20and%20Execution).md) | [05_Implementation (Source Code & Execution).pdf](docs/05_Implementation%20(Source%20Code%20%26%20Execution).pdf) |
| **6. Testing & QA** | Test plan, 42 test cases, 46 automated tests, bug report | [6. Testing and Quality Assurance.md](mdFiles/6.%20Testing%20and%20Quality%20Assurance.md) | [06_Testing & Quality Assurance.pdf](docs/06_Testing%20%26%20Quality%20Assurance.pdf) |
| **7.1 User Manual** | Step-by-step guide with screenshots for patients, doctors and admins | [7.1 User Manual.md](mdFiles/7.1%20User%20Manual.md) | [07_1_User Manual.pdf](docs/07_1_User%20Manual.pdf) |
| **7.2 Technical Documentation** | Architecture, database schema (ERD), endpoints, flows, security, configuration, CI/CD | [7.2 Technical Documentation.md](mdFiles/7.2%20Technical%20Documentation.md) | [07_2_Technical Documentation.pdf](docs/07_2_Technical%20Documentation.pdf) |
| **Final Presentation** | 15 slides with speaker notes | — | [08_Final Presentation.pptx](docs/08_Final%20Presentation.pptx) · [PDF](docs/08_Final%20Presentation.pdf) |
| **Project Infographic** | One 16:9 roadmap slide (DEPI template) | — | [09_Project Infographic.png](docs/09_Project%20Infographic.png) |

Screenshots used in the manual: [`docs/screenshots/`](docs/screenshots/).

---

## Technology Stack Summary
* **Presentation Layer (`MediCare.Web`):** ASP.NET Core MVC, Razor Views, Bootstrap 5, JavaScript, FullCalendar.js, Chart.js, SignalR.
* **Business Logic Layer (`MediCare.Services`):** C# Services, Dependency Injection, Factory Pattern, Conflict Detection Engine, Scheduled Reminder Jobs.
* **Data Access Layer (`MediCare.Data`):** Entity Framework Core (Code-First Migrations), Microsoft SQL Server / Azure SQL Database, Generic & Specific Repositories, Unit of Work Pattern.
* **Security & Cloud Integrations:** ASP.NET Core Identity (Role-Based Access Control), Twilio API (SMS), SendGrid API (Email), Microsoft Azure App Service, Azure SignalR Service, GitHub Actions / Azure DevOps (CI/CD).
