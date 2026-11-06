# 4.5 System Deployment & Integration

**Project Name:** MediCare - Clinic Management & Appointment System  
**DEPI Track:** Full Stack .NET Web Developer  
**Submission Deadline:** 6 November 2026  

[$\leftarrow$ Back to Main README](../README.md)

---

## 5.1 Technology Stack

The MediCare platform integrates frontend, backend, database, and cloud technologies structured around a **Layered MVC Monolith / N-Tier Architecture**:

| Layer / Category | Selected Technologies | Architectural Role & Implementation Purpose |
| :--- | :--- | :--- |
| **Frontend Technologies** | `Razor Views`, `Bootstrap 5`, `JavaScript`, `FullCalendar.js`, `Chart.js` | Renders responsive, mobile-first interfaces adhering to the brand palette (`#56060D` burgundy, `#A81420` crimson, `#FFF9F1` ivory) and `WCAG 2.2 AA` accessibility guidelines; displays interactive booking calendars and administrative analytics charts. |
| **Backend Technologies** | `C#`, `ASP.NET Core MVC`, `ASP.NET Core Identity`, `SignalR` | Implements thin MVC controllers (`MediCare.Web`), role-based authorization policies (`Patient`, `Doctor`, `Admin`), real-time WebSocket hubs (`AppointmentHub`), and decoupled business services (`MediCare.Services`) using **Dependency Injection** and the **Factory Pattern** (`NotificationFactory`). |
| **Database Technologies** | `Microsoft SQL Server` / `Azure SQL Database`, `Entity Framework Core (Code-First)` | Manages normalized (`3NF`) relational tables (`AspNetUsers`, `DoctorProfiles`, `AvailabilitySlots`, `Appointments`, `Prescriptions`) and extended clinical entities via `IUnitOfWork`, `IGenericRepository<T>`, and specific repositories (`IDoctorRepository`, `IPatientRepository`, `IAppointmentRepository`, `IMedicalRecordRepository`). |
| **Cloud & External Integrations** | `Microsoft Azure App Service`, `Azure SignalR Service`, `Twilio (SMS)`, `SendGrid (Email)`, `GitHub Actions / Azure DevOps` | Hosts the web application and real-time hubs in the cloud, dispatches SMS/Email confirmations and 24-hour reminders via `INotificationSender` (with In-App/Mock fallback), and automates CI/CD pipelines. |

---

## 5.2 Deployment Diagram

The Deployment Diagram illustrates how MediCare software components are distributed across client devices, Microsoft Azure cloud infrastructure, and external notification providers:

```mermaid
flowchart TB
    subgraph Client_Device ["Client Tier (Desktop & Mobile Devices)"]
        Browser["Web Browser (Chrome, Edge, Firefox, Safari)<br/>Razor Views, Bootstrap 5, FullCalendar.js, Chart.js"]
    end

    subgraph CI_CD ["Source Control & CI/CD Pipeline"]
        GitHub["GitHub Repository (Source Code & Docs)"]
        Pipeline["GitHub Actions / Azure DevOps Pipeline"]
        GitHub --> Pipeline
    end

    subgraph Azure_Cloud ["Microsoft Azure Cloud Environment"]
        subgraph App_Service ["Azure App Service (ASP.NET Core Runtime)"]
            WebApp["MediCare Layered Application<br/>(MediCare.Web + MediCare.Services + MediCare.Data)"]
        end
        AzureSignalR["Azure SignalR Service<br/>(AppointmentHub Real-Time WebSockets)"]
        AzureSQL[("Azure SQL Database / MS SQL Server<br/>(AspNetUsers, DoctorProfiles, AvailabilitySlots, Appointments, Prescriptions)")]
    end

    subgraph External_Services ["External Notification Providers (INotificationSender)"]
        TwilioAPI["Twilio REST API<br/>(SmsSender - 24h SMS Reminders)"]
        SendGridAPI["SendGrid REST API<br/>(EmailSender - Email Notifications)"]
    end

    Browser <-->|"HTTPS (TLS 1.2+)"| WebApp
    Browser <-->|"WSS (WebSockets)"| AzureSignalR
    Pipeline ==>|"Automated Build, Test & Deploy"| App_Service
    WebApp <-->|"EF Core / TDS (Atomic Transactions)"| AzureSQL
    WebApp <-->|"SignalR Hub Backplane"| AzureSignalR
    WebApp -->|"HTTPS REST API"| TwilioAPI
    WebApp -->|"HTTPS REST API"| SendGridAPI
```

---

## 5.3 Component Diagram

The Component Diagram depicts the high-level software components across the three N-Tier layers (`MediCare.Web`, `MediCare.Services`, `MediCare.Data`) and their interface dependencies:

```mermaid
flowchart TB
    subgraph MediCare_Web ["Presentation Layer (MediCare.Web)"]
        IdentityMW["Identity & Authorization Middleware<br/>(ASP.NET Core Identity & RBAC)"]
        ApptCtrl["MVC Controllers<br/>(AppointmentsController, DoctorController, AdminController)"]
        SignalRHub["AppointmentHub (SignalR)<br/>+NotifyDoctor() / +NotifyPatient()"]
    end

    subgraph MediCare_Services ["Business Logic Layer (MediCare.Services)"]
        ApptSvc["AppointmentService<br/>+BookAppointment() / +CancelAppointment()<br/>+RescheduleAppointment() / +ConfirmAppointment() / +RejectAppointment()"]
        NotifFact["NotificationFactory<br/>+Create(channel)"]
        INotif["<<interface>> INotificationSender<br/>+Send(message)"]
        Senders["Concrete Senders<br/>SmsSender (Twilio) | EmailSender (SendGrid) | MockProvider"]
    end

    subgraph MediCare_Data ["Data Access Layer (MediCare.Data)"]
        IUoW["<<interface>> IUnitOfWork<br/>+Doctors / +Patients / +Appointments / +MedicalRecords / +SaveChangesAsync()"]
        SpecRepos["Specific Repositories<br/>IDoctorRepository (+Search)<br/>IAppointmentRepository (+HasConflict, +GetByDoctor)<br/>IPatientRepository (+GetWithHistory)<br/>IMedicalRecordRepository (+GetByPatient)"]
        GenRepo["<<interface>> IGenericRepository<T><br/>+GetById() / +GetAll() / +Add() / +Update() / +Delete()"]
        DbCtx["Entity Framework Core DbContext"]
    end

    IdentityMW --> ApptCtrl
    ApptCtrl -->|"uses (DI)"| ApptSvc
    ApptSvc -->|"notifies"| SignalRHub
    ApptSvc -->|"uses"| NotifFact
    NotifFact -->|"creates"| INotif
    Senders -.->|"implements"| INotif
    ApptSvc -->|"uses (DI)"| IUoW
    IUoW --> SpecRepos
    SpecRepos --> GenRepo
    GenRepo --> DbCtx
```