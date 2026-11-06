# 1. Project Planning & Management

**Project Name:** MediCare - Clinic Management & Appointment System  
**DEPI Track:** Full Stack .NET Web Developer  
**Submission Deadline:** 16 October 2026  

[$\leftarrow$ Back to Main README](../README.md)

---

## 1.1 Project Proposal
* **Project Overview:** MediCare is a full-featured web-based healthcare and clinic management platform that enables patients to search for doctors by specialization, location, and availability, book appointments, and access their medical history and prescriptions. Doctors can manage their recurring weekly schedules, confirm or reject appointment requests, record clinical visits, and issue digital prescriptions, while administrators oversee user accounts, clinic settings, and operational analytics.
* **Project Objectives:**
  * **O1:** Enable patients to search for doctors filtered by specialty, location, and available time slots.
  * **O2:** Minimize schedule conflicts and prevent double bookings using server-side validation, composite unique database constraints, and atomic Unit of Work transactions.
  * **O3:** Empower doctors to manage working hours and vacation days, review daily/weekly appointment lists, and inspect relevant patient history.
  * **O4:** Provide patients secure, role-authorized access to their appointment history, medical records, and printable digital prescriptions.
  * **O5:** Deliver real-time alerts via **SignalR** and automated confirmations/24-hour reminders via **Twilio (SMS)** and **SendGrid (Email)**.
  * **O6 & O7:** Equip administrators with tools for doctor onboarding, account management, activity tracking, and **Chart.js** visual analytics with PDF/Excel export.
* **Project Scope:**
  * **In-Scope:** Multi-role authentication (`Patient`, `Doctor`, `Admin`) via ASP.NET Core Identity; doctor profiles and multi-criteria search; weekly slot availability management; appointment booking, cancellation, and rescheduling with server-side conflict detection; interactive calendars (`FullCalendar.js`); patient and doctor dashboards; authorized medical records, document uploads, and digital prescriptions; real-time (`SignalR`) and external/mock notifications (`Twilio`, `SendGrid`); admin management and `Chart.js` analytics; responsive `Bootstrap 5` UI; and `Microsoft Azure` cloud deployment.
  * **Out-of-Scope:** Clinical medical diagnosis decision support, emergency triage handling, integrated online payment/insurance processing, direct external Hospital Information System (HIS) API integration, and storage of real identifiable patient data (100% synthetic mock data is used).

---

## 1.2 Project Plan (Timeline, Gantt Chart, Milestones & Resource Allocation)

| Phase / Milestone | Start Date | End Date | Key Deliverables & Resource Allocation |
| :--- | :--- | :--- | :--- |
| **1. Project Planning & Management** | 2 Oct 2026 | 16 Oct 2026 | Project Proposal, Project Plan, Task Assignment, Risk Plan, and KPIs uploaded to GitHub. |
| **2 & 3. Literature Review & Requirements** | 9 Oct 2026 | 16 Oct 2026 | Lecturer Evaluation section, Suggested Improvements, Grading Criteria, Stakeholder Analysis, User Stories, Use Cases, FRs (`FR-01`–`FR-17`), and NFRs (`NFR-01`–`NFR-10`). |
| **4. System Analysis & Design** | 9 Oct 2026 | 6 Nov 2026 | Problem Statement, Use Cases, Layered MVC Architecture, ERD & Physical Schema, Level 0/1 DFD, Sequence, Activity, State & Class Diagrams, UI/UX Wireframes, Deployment & Component Diagrams, Testing & Deployment Strategy. |
| **5. Implementation - Week 1 (Setup & Auth)** | 16 Oct 2026 | 26 Oct 2026 | N-Tier setup (`MediCare.Web`, `MediCare.Services`, `MediCare.Data`), EF Core Code-First schema, Generic & Specific Repositories, Unit of Work, Multi-role ASP.NET Identity, and Doctor listing/search page. |
| **5. Implementation - Week 2 (Booking & Doctor UI)** | 27 Oct 2026 | 7 Nov 2026 | Interactive booking calendar (`FullCalendar.js`), cancel/reschedule workflows, conflict detection, Doctor Dashboard, and `SignalR` (`AppointmentHub`) real-time alerts. |
| **5. Implementation - Week 3 (EMR, Prescriptions & SMS)** | 8 Nov 2026 | 18 Nov 2026 | Medical records management, document/X-ray uploads, printable digital prescriptions, `Twilio` SMS & `SendGrid` Email integration via `NotificationFactory`, and Patient Profile. |
| **5. Implementation - Week 4 (Admin, Analytics & Azure)** | 19 Nov 2026 | 30 Nov 2026 | Admin Dashboard, `Chart.js` analytics with PDF/Excel export, `Bootstrap 5` UI enhancements (Dark/Light mode & accessibility), and deployment to Microsoft Azure with CI/CD pipelines. |
| **6 & 7. Testing, Final Presentation & Reports** | 16 Oct 2026 | 4 Dec 2026 | Test Plan & Test Cases, Bug Reports, User Manual, Technical Documentation, Final Presentation Deck, and DEPI Single-Slide Project Infographic. |

### Project Gantt Chart
```mermaid
gantt
    title MediCare Master Project Plan (DEPI 2026)
    dateFormat  YYYY-MM-DD
    section Documentation Phases
    Project Planning & Management       :done, p1, 2026-10-02, 2026-10-16
    Literature Review & Requirements    :done, p2, 2026-10-09, 2026-10-16
    System Analysis & Design            :done, p3, 2026-10-09, 2026-11-06
    section 4-Week Implementation
    Week 1: NTier, EF Core, Auth & Search      :c1, 2026-10-16, 2026-10-26
    Week 2: Booking, FullCalendar & SignalR    :c2, 2026-10-27, 2026-11-07
    Week 3: Medical Records, Prescriptions & SMS :c3, 2026-11-08, 2026-11-18
    Week 4: Admin, Chart.js & Azure Deployment :c4, 2026-11-19, 2026-11-30
    section QA & Final Presentation
    Testing, User Manual & Final Presentation  :t1, 2026-10-16, 2026-12-04
```

### Resource Allocation
* **Human Resources:** 5 Full Stack .NET Developers led by the Team Leader, with clear division across Architecture/Data, Identity/Admin, Real-Time/Cloud DevOps, Frontend/UI, and Medical/Analytics modules.
* **Development & Database Tools:** Visual Studio 2022 / VS Code, SQL Server Management Studio (SSMS), Git & GitHub, Postman.
* **Cloud & External Services:** Microsoft Azure App Service, Azure SQL Database, Azure SignalR Service, Twilio API (SMS), SendGrid API (Email), GitHub Actions / Azure DevOps.

---

## 1.3 Task Assignment & Roles
| Team Member | Assigned Role | Core Responsibilities Across Documentation & Implementation |
| :--- | :--- | :--- |
| **[Your Name]** | **Team Leader, Software Architect & Core Backend Lead** | Coordinates project planning, task tracking, and GitHub repository management; establishes the **N-Tier / Layered MVC Architecture** (`MediCare.Web`, `MediCare.Services`, `MediCare.Data`), designs the **EF Core** database schema, implements Generic & Specific Repositories and **Unit of Work**, and builds server-side booking & conflict detection logic (`FR-05`, `FR-06`, `FR-08`). |
| **Basmala Mohamed** | **Identity, Security & Admin Dashboard Developer** | Implements multi-role authentication and authorization (`FR-01`, `FR-02`) using **ASP.NET Core Identity**, role-specific registration workflows, and develops the **Admin Dashboard** for doctor approvals, user permissions, and specializations (`FR-15`, `FR-16`). |
| **Ali Mohamed** | **Real-Time, Notifications & Cloud DevOps Engineer** | Manages risk mitigation and cloud deployment; implements the **Factory Pattern** (`NotificationFactory`), real-time notifications via **SignalR** (`AppointmentHub`), 24-hour reminders via **Twilio** & **SendGrid** (`FR-14`), and **Microsoft Azure** deployment with CI/CD pipelines. |
| **Sara Elsaeid** | **Frontend, Calendar & UI/UX Lead** | Leads UI/UX prototyping and accessibility compliance (`WCAG 2.2 AA`); develops responsive **Bootstrap 5** views, doctor search/filter pages (`FR-03`, `FR-04`), interactive **FullCalendar.js** booking views, and the **Doctor Dashboard** (`FR-09`, `FR-10`). |
| **Youssef Elazrk** | **Medical Modules & Analytics Developer** | Develops the **Patient Profile** (`FR-07`), authorized **Medical Records** and **Digital Prescription System** (`FR-11`, `FR-12`), medical document/X-ray uploads (`FR-13`), and **Chart.js** administrative analytics with PDF/Excel export (`FR-17`). |

---

## 1.4 Risk Assessment & Mitigation Plan
| Risk Category | Risk Description | Mitigation Plan |
| :--- | :--- | :--- |
| **Concurrent Double Booking** | Two patients attempt to book the exact same time slot for the same doctor simultaneously. | Re-verify slot availability inside an atomic **Unit of Work** transaction (`UC-15`) and enforce a database-level composite constraint `UNIQUE (DoctorId, AppointmentDate, StartTime)`. |
| **Unauthorized Medical Data Access** | Unauthorized users attempt to view or modify private patient records, prescriptions, or uploaded lab documents. | Enforce strict Role-Based Access Control (RBAC), server-side ownership checks (`FR-12`), secure file extension validation, and 100% synthetic mock data in demos (`NFR-03`, `NFR-06`). |
| **External Notification Service Outages** | Third-party SMS (`Twilio`) or Email (`SendGrid`) quotas or network failures disrupt reminder delivery. | Abstract notification dispatch behind `INotificationSender` and `NotificationFactory` so the system logs delivery failures gracefully and falls back to **SignalR** in-app or mock providers (`NFR-09`). |
| **Cloud Deployment & Integration Delays** | Configuration issues during migration to Azure App Service, Azure SQL Database, and Azure SignalR Service. | Maintain a clean **Layered MVC Monolith** structure and automate build/deployment early via **GitHub Actions / Azure DevOps** CI/CD pipelines. |

---

## 1.5 Key Performance Indicators (KPIs)
* **Search & Booking Response Time:** Median response time $\le 2.0\text{ seconds}$ for doctor search queries and appointment booking operations under standard test loads (`NFR-01`).
* **Concurrency & Conflict Prevention:** $0$ overlapping appointment conflicts accepted during concurrent booking tests (`NFR-03`).
* **System Uptime & Availability:** $\ge 99.0\%$ operational uptime maintained on Microsoft Azure during evaluation and pilot windows (`NFR-04`, `NFR-09`).
* **Usability & Task Completion Rate:** $\ge 80\%$ of test users complete core tasks (searching doctors, booking slots, managing schedules) without external guidance (`NFR-05`).
* **Test Suite & Notification Reliability:** $\ge 95\%$ pass rate across unit/integration test suites (`NFR-08`) and $\ge 95\%$ successful notification delivery rate in the test environment (`NFR-09`).