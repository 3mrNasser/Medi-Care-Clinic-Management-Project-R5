# 4.4 UI/UX Design & Prototyping

**Project Name:** MediCare - Clinic Management & Appointment System  
**DEPI Track:** Full Stack .NET Web Developer  
**Submission Deadline:** 6 November 2026  

[$\leftarrow$ Back to Main README](../README.md)

---

## 1. Design Objective & Experience Principles

### Design Objective
Create a clear, reassuring clinic experience that helps patients find doctors and book appointments effortlessly, while giving doctors and administrators efficient tools for their daily workflows.

### Core Experience Principles
* **Make the next step obvious:** Search $\rightarrow$ Choose a time $\rightarrow$ Confirm.
* **Use plain language:** Present appointment statuses, validation alerts, and error messages clearly.
* **Role-distinct workflows:** Keep Patient, Doctor, and Administrator views tailored to their specific responsibilities.
* **Progressive disclosure & privacy:** Show only information needed for the current task and protect sensitive medical details.
* **Mobile-first responsiveness:** Support mobile-first access via **Bootstrap 5** and maintain consistent top navigation.

---

## 2. Color Palette & Typography

### Brand Color Palette
| Token | Hex Code | Recommended Usage |
| :--- | :--- | :--- |
| **Primary Burgundy** | `#56060D` | Brand header, top navigation bar, footer |
| **Action Crimson** | `#A81420` | Primary CTA buttons, active links, key actions |
| **Warm Gold** | `#D9AA45` | Small icons, borders, active markers (not for small body text) |
| **Champagne** | `#F5D98A` | Soft highlight fills and selected calendar/slot states |
| **Warm Ivory** | `#FFF9F1` | Page background and subtle section panels |
| **White** | `#FFFFFF` | Cards, forms, and primary content surfaces |
| **Charcoal** | `#292526` | Primary body text and form input labels |
| **Muted Gray** | `#756D6A` | Secondary labels and helper text |

### Typography & Components
* **Interface Font Family:** Legible sans-serif (`Inter`, `Segoe UI`, or `system-ui`).
* **Hierarchy & Controls:** Clear heading levels, consistent button styles, persistent form labels above inputs, and visible keyboard focus rings.

---

## 3. Primary Screen Wireframes Summary

1. **Patient Landing Page:** Features one prominent doctor search bar (`Specialty | Location | Date`), concise hero copy (*"Care that fits your day"*), and quick links to popular medical specialties.
2. **Doctor Search Results:** Keeps `Specialty`, `Area`, and `Availability` filters visible at the top, displaying clean doctor cards with specialty, rating, consultation fee, and a primary `View profile` CTA.
3. **Appointment Booking Screen:** Displays the selected doctor's summary card alongside interactive date and time slot selectors (`FullCalendar.js`), disclosing clinic cancellation rules prior to final confirmation.
4. **Patient Dashboard:** Greets the patient (*"Your care at a glance"*), prioritizing the next upcoming appointment, its confirmation status, active prescriptions count, and quick links to details/history.
5. **Doctor Dashboard:** Displays today's schedule count, time-ordered upcoming appointments with status badges (`Confirmed`, `Pending`), and quick actions to confirm/reject requests or inspect patient records.
6. **Administrator Dashboard:** Presents summary KPI cards (`Appointments`, `Patients`, `Doctors`), weekly appointment trend charts (`Chart.js`), and recent system activity logs.

---

## 4. UI/UX & Accessibility Guidelines (`WCAG 2.2 AA`)

* **Navigation & Layout:** Centered content container with generous spacing; group related items into clean cards; collapse navigation into an accessible menu on mobile screens.
* **Forms & Feedback:** Persistent `<label>` above every input field; server-side validation messages in plain language next to the relevant field; explicit visual and text states (`Available`, `Selected`, `Booked`, `Cancelled`, `Unavailable`) without relying on color alone.
* **Accessibility Compliance:** Maintain **WCAG 2.2 AA** contrast ratios (Charcoal `#292526` text on Ivory/White surfaces; White `#FFFFFF` text on Burgundy/Crimson buttons); minimum $44 \times 44\text{ CSS pixels}$ touch targets; full keyboard tab navigation.

### Prototype Validation Checklist
| Task | Validation Check |
| :--- | :--- |
| **Find a doctor** | Can a patient enter a specialty/location and immediately understand the filtered results? |
| **Book an appointment** | Can a patient choose an open time slot, review details, and recognize booking confirmation? |
| **Manage schedule** | Can a doctor identify upcoming appointments and manage weekly availability controls? |
| **Review activity** | Can an administrator find appointment totals, charts, and recent account/schedule changes? |
| **Accessibility** | Can users navigate entirely by keyboard and identify labels, focus states, errors, and statuses? |