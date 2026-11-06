# 4.2 Database Design & Data Modeling

**Project Name:** MediCare - Clinic Management & Appointment System  
**DEPI Track:** Full Stack .NET Web Developer  
**Submission Deadline:** 6 November 2026  

[$\leftarrow$ Back to Main README](../README.md)

---

## 2.1 Entity-Relationship Diagram (ERD)

```mermaid
erDiagram
    AspNetUsers ||--o| DoctorProfiles : "1 : 0..1 (UserId)"
    DoctorProfiles ||--o{ AvailabilitySlots : "1 : N (DoctorId)"
    DoctorProfiles ||--o{ Appointments : "1 : N (DoctorId)"
    AspNetUsers ||--o{ Appointments : "1 : N (PatientId)"
    AvailabilitySlots ||--o{ Appointments : "1 : N (SlotId)"
    Appointments ||--o{ Prescriptions : "1 : 0..N (AppointmentId)"

    AspNetUsers {
        nvarchar_450 Id PK
        nvarchar_100 FullName
        nvarchar_256 Email UQ
        nvarchar_20 PhoneNumber
        nvarchar_50 UserRole
    }

    DoctorProfiles {
        int DoctorId PK
        nvarchar_450 UserId FK,UQ
        nvarchar_100 Specialty IX
        nvarchar_250 ClinicAddress
        decimal_18_2 ConsultationFee
    }

    AvailabilitySlots {
        int SlotId PK
        int DoctorId FK
        int DayOfWeek
        time_7 StartTime
        time_7 EndTime
    }

    Appointments {
        int AppointmentId PK
        nvarchar_450 PatientId FK
        int DoctorId FK
        int SlotId FK
        date AppointmentDate
        time_7 StartTime
        nvarchar_30 Status
        nvarchar_500 ReasonForVisit
    }

    Prescriptions {
        int PrescriptionId PK
        int AppointmentId FK,IX
        nvarchar_150 MedicationName
        nvarchar_100 Dosage
        nvarchar_100 Frequency
        int DurationDays
    }
```

### Relationship Matrix
| Primary Entity | Related Entity | Cardinality | Relationship Description |
| :--- | :--- | :--- | :--- |
| `AspNetUsers` | `DoctorProfiles` | $1 : 0..1$ | A user account may have one doctor profile; each doctor profile belongs to one user. |
| `DoctorProfiles` | `AvailabilitySlots` | $1 : N$ | A doctor can define multiple recurring weekly availability slots. |
| `DoctorProfiles` | `Appointments` | $1 : N$ | A doctor can have multiple appointments; each appointment references one doctor. |
| `AspNetUsers (Patient)` | `Appointments` | $1 : N$ | A patient can book multiple appointments; each appointment references one patient account. |
| `AvailabilitySlots` | `Appointments` | $1 : N$ | A recurring availability slot can be referenced by multiple dated appointments. |
| `Appointments` | `Prescriptions` | $1 : 0..N$ | An appointment may have zero or multiple prescribed medication rows. |

---

## 2.2 Logical & Physical Schema

### Normalization & Integrity Rules
* **First Normal Form (1NF):** Each column stores a single atomic value. Medication details are stored in individual `Prescriptions` rows rather than a delimited list.
* **Second Normal Form (2NF):** Every non-key attribute depends on the whole primary key (all tables use single-column primary keys).
* **Third Normal Form (3NF):** Non-key attributes are stored only with the entity they describe, avoiding duplication of user or doctor profile attributes inside appointment records.
* **Doctor Double-Booking Prevention:** A composite unique index is enforced on `UNIQUE (DoctorId, AppointmentDate, StartTime)`.

### Physical Database Table Specifications

#### Table 1: `AspNetUsers` (Identity Base User Table)
| Column Name | Data Type | Keys / Indexes | Null? | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `nvarchar(450)` | `PK` | No | Unique user identifier (ASP.NET Core Identity key). |
| `FullName` | `nvarchar(100)` | — | No | User's full display name. |
| `Email` | `nvarchar(256)` | `UQ` | No | Account email used as login identifier. |
| `PhoneNumber` | `nvarchar(20)` | — | Yes | Contact phone number. |
| `UserRole` | `nvarchar(50)` | `IX` | No | Account role: `Patient`, `Doctor`, or `Admin`. |

#### Table 2: `DoctorProfiles`
| Column Name | Data Type | Keys / Indexes | Null? | Description |
| :--- | :--- | :--- | :--- | :--- |
| `DoctorId` | `int` | `PK, Identity` | No | Primary key. |
| `UserId` | `nvarchar(450)` | `FK, UQ` | No | References `AspNetUsers(Id)`; unique to enforce $1:0..1$ relationship. |
| `Specialty` | `nvarchar(100)` | `IX` | No | Medical specialty (e.g., Cardiology). |
| `ClinicAddress` | `nvarchar(250)` | — | No | Clinic physical location. |
| `ConsultationFee` | `decimal(18,2)` | — | No | Consultation price. |

#### Table 3: `AvailabilitySlots`
| Column Name | Data Type | Keys / Indexes | Null? | Description |
| :--- | :--- | :--- | :--- | :--- |
| `SlotId` | `int` | `PK, Identity` | No | Primary key. |
| `DoctorId` | `int` | `FK` | No | References `DoctorProfiles(DoctorId)`. |
| `DayOfWeek` | `int` | `CHECK: 0-6` | No | Day of week ($0 = \text{Sunday}$, $6 = \text{Saturday}$). |
| `StartTime` | `time(7)` | — | No | Availability start time. |
| `EndTime` | `time(7)` | `CHECK: > StartTime` | No | Availability end time. |

#### Table 4: `Appointments`
| Column Name | Data Type | Keys / Indexes | Null? | Description |
| :--- | :--- | :--- | :--- | :--- |
| `AppointmentId` | `int` | `PK, Identity` | No | Primary key. |
| `PatientId` | `nvarchar(450)` | `FK` | No | References `AspNetUsers(Id)` for the patient account. |
| `DoctorId` | `int` | `FK, Composite UQ` | No | References `DoctorProfiles(DoctorId)`. |
| `SlotId` | `int` | `FK` | No | References `AvailabilitySlots(SlotId)`. |
| `AppointmentDate` | `date` | `Composite UQ` | No | Calendar date of the appointment. |
| `StartTime` | `time(7)` | `Composite UQ` | No | Booked start time within the referenced slot. |
| `Status` | `nvarchar(30)` | — | No | `Pending`, `Confirmed`, `Rejected`, `Cancelled`, or `Completed`. |
| `ReasonForVisit` | `nvarchar(500)` | — | Yes | Optional patient notes or reason for visit. |

#### Table 5: `Prescriptions`
| Column Name | Data Type | Keys / Indexes | Null? | Description |
| :--- | :--- | :--- | :--- | :--- |
| `PrescriptionId` | `int` | `PK, Identity` | No | Primary key. |
| `AppointmentId` | `int` | `FK, IX` | No | References `Appointments(AppointmentId)`. |
| `MedicationName` | `nvarchar(150)` | — | No | Name of the prescribed medication. |
| `Dosage` | `nvarchar(100)` | — | No | Dosage instructions (e.g., 500 mg). |
| `Frequency` | `nvarchar(100)` | — | No | Frequency instructions (e.g., twice daily). |
| `DurationDays` | `int` | `CHECK: > 0` | No | Treatment duration in days ($\text{DurationDays} > 0$). |