# MediCare - Clinic Management & Appointment System

**Initiative:** Digital Egypt Pioneers Initiative (DEPI)  
**Track:** Full Stack .NET Web Developer  
**Architecture Style:** Layered MVC Monolith / N-Tier Architecture (`MediCare.Web`, `MediCare.Services`, `MediCare.Data`)  

---

## Project Overview
**MediCare** is a comprehensive web-based clinic management and appointment booking platform that connects patients with healthcare providers. It enables patients to search for doctors by specialization, location, and availability, book conflict-free appointments, and access their medical history and digital prescriptions. Simultaneously, it empowers doctors to manage their weekly schedules, confirm or reject appointment requests, record clinical visits, and issue printable prescriptions, while providing administrators with full system oversight and visual analytics.

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

## Technology Stack Summary
* **Presentation Layer (`MediCare.Web`):** ASP.NET Core MVC, Razor Views, Bootstrap 5, JavaScript, FullCalendar.js, Chart.js, SignalR.
* **Business Logic Layer (`MediCare.Services`):** C# Services, Dependency Injection, Factory Pattern, Conflict Detection Engine, Scheduled Reminder Jobs.
* **Data Access Layer (`MediCare.Data`):** Entity Framework Core (Code-First Migrations), Microsoft SQL Server / Azure SQL Database, Generic & Specific Repositories, Unit of Work Pattern.
* **Security & Cloud Integrations:** ASP.NET Core Identity (Role-Based Access Control), Twilio API (SMS), SendGrid API (Email), Microsoft Azure App Service, Azure SignalR Service, GitHub Actions / Azure DevOps (CI/CD).
