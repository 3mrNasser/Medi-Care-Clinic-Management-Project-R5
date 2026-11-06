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

## Project Documentation Index (DEPI Deliverables)

Every section of the DEPI Project Documentation Guidelines is organized into its own dedicated file inside the `docs/` folder:

### Phase 1: Planning, Literature Review & Requirements (Deadline: 16 Oct 2026)
1. **[1. Project Planning & Management](docs/01_Project_Planning_and_Management.md)**
   * Project Proposal (Overview, Objectives, Scope)
   * Project Plan (Master Timeline, Mermaid Gantt Chart, Milestones & Resource Allocation)
   * Task Assignment & Roles
   * Risk Assessment & Mitigation Plan
   * Key Performance Indicators (KPIs)
2. **[2. Literature Review](docs/02_Literature_Review.md)**
   * Feedback & Evaluation (Lecturer’s Assessment)
   * Suggested Improvements (Future Enhancements)
   * Final Grading Criteria
3. **[3. Requirements Gathering](docs/03_Requirements_Gathering.md)**
   * Stakeholder Analysis
   * User Stories & Use Cases
   * Functional Requirements (`FR-01` to `FR-17`)
   * Non-Functional Requirements (`NFR-01` to `NFR-10`)

### Phase 2: System Analysis & Design (Deadline: 6 Nov 2026)
4. **[4.1 Problem Statement, Use Cases & Software Architecture](docs/04_1_Problem_Statement_and_Architecture.md)**
   * Problem Statement, Stakeholders, Objectives (`O1`–`O7`) & Scope Boundary
   * Visual Use Case Diagram & Detailed Use Case Specifications (`UC-01` to `UC-15`)
   * Functional & Non-Functional Requirements Mapping
   * Software Architecture (`Layered MVC Monolith / N-Tier`) & Architectural Decisions (ADR)
5. **[4.2 Database Design & Data Modeling](docs/04_2_Database_Design_and_Data_Modeling.md)**
   * Entity-Relationship Diagram (ERD) & Relationship Matrix
   * Normalization Rules (`1NF`, `2NF`, `3NF`) & Double-Booking Prevention Index
   * Physical Database Schema (`AspNetUsers`, `DoctorProfiles`, `AvailabilitySlots`, `Appointments`, `Prescriptions`)
6. **[4.3 Data Flow & System Behavior](docs/04_3_Data_Flow_and_System_Behavior.md)**
   * Data Flow Diagrams (DFD Level 0 Context & Level 1 Processes `1.0` to `7.0`)
   * Sequence Diagrams (Booking, Confirm/Reject, Automated 24h Reminder)
   * Activity Diagram (End-to-End Patient Visit Workflow)
   * State Diagrams (Appointment States & Doctor Account States)
   * Class Diagrams (Domain Model & N-Tier Services/Repositories)
7. **[4.4 UI/UX Design & Prototyping](docs/04_4_UI_UX_Design_and_Prototyping.md)**
   * Design Objective & Experience Principles
   * Brand Color Palette (`#56060D` Burgundy & `#D9AA45` Warm Gold) & Typography
   * Primary Screen Wireframes Overview
   * UI/UX Guidelines, Accessibility (`WCAG 2.2 AA`) & Prototype Validation Checklist
8. **[4.5 System Deployment & Integration](docs/04_5_System_Deployment_and_Integration.md)**
   * Technology Stack (Frontend, Backend, Database, Cloud & External Services)
   * Deployment Diagram
   * Component Diagram
9. **[4.6 Additional Deliverables](docs/04_6_Additional_Deliverables.md)**
   * API & Internal Service Endpoints Documentation
   * Testing & Validation Plan (Unit, Integration/Concurrency & UAT)
   * Deployment Strategy (Azure Hosting, CI/CD Pipelines & Scaling Considerations)

---

## Technology Stack Summary
* **Presentation Layer (`MediCare.Web`):** ASP.NET Core MVC, Razor Views, Bootstrap 5, JavaScript, FullCalendar.js, Chart.js, SignalR.
* **Business Logic Layer (`MediCare.Services`):** C# Services, Dependency Injection, Factory Pattern, Conflict Detection Engine, Scheduled Reminder Jobs.
* **Data Access Layer (`MediCare.Data`):** Entity Framework Core (Code-First Migrations), Microsoft SQL Server / Azure SQL Database, Generic & Specific Repositories, Unit of Work Pattern.
* **Security & Cloud Integrations:** ASP.NET Core Identity (Role-Based Access Control), Twilio API (SMS), SendGrid API (Email), Microsoft Azure App Service, Azure SignalR Service, GitHub Actions / Azure DevOps (CI/CD).
