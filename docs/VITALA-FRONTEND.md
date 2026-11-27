# Vitala UI — Frontend handoff (MediCare.Web)

Frontend only: Razor views + CSS + JS. **No backend logic**: no models, no services, no DB.

## Files added

| File (under `MediCare.Web/`) | Purpose |
|---|---|
| `wwwroot/css/vitala.css` | All styles. Every class starts with `vt-`, so nothing clashes with Bootstrap or `site.css`. |
| `wwwroot/js/vitala.js` | UI behaviour only (password eye, OTP boxes and timer, role cards, mobile menus, chat UI). |
| `Views/Shared/_VitalaLayout.cshtml` | Base layout (fonts + css/js). Independent of the team's `_Layout.cshtml`. |
| `Views/Shared/_AuthLayout.cshtml` | Split-screen layout for the auth pages. Left panel text comes from `ViewData["AuthPanelTitle"]`. |
| `Views/Shared/_DashboardLayout.cshtml` + `_Sidebar.cshtml` | Patient workspace with sidebar. Active link comes from `ViewData["ActiveNav"]`. |
| `Views/Shared/_VtIcon.cshtml`, `_PasswordToggle.cshtml` | Reusable partials (inline SVG icons, eye button). |
| `Views/Home/Index.cshtml` | 01 Landing |
| `Views/Account/SelectRole.cshtml` | 02 Select role |
| `Views/Account/Login.cshtml` | 03 Login |
| `Views/Account/ForgotPassword.cshtml` | Forgot password (email step) |
| `Views/Account/Signup.cshtml` | 04 Sign up |
| `Views/Account/VerifyOtp.cshtml` | 05 Verify OTP |
| `Views/Account/ResetPassword.cshtml` | 06 Reset password |
| `Views/Patient/Home.cshtml` | 1.2 Patient dashboard |
| `Views/Patient/AiAssistant.cshtml` | AI assistant |

### Controllers
`Controllers/AccountController.cs` and `Controllers/PatientController.cs` contain **GET actions only** (`return View();`)
so the screens can be opened. The backend work goes into the same files:
- `AccountController`: POST actions for `Login`, `Signup`, `ForgotPassword`, `VerifyOtp`, `ResendOtp`, `ResetPassword`, `Logout` (Identity task).
- `PatientController`: pass real data (patient name, upcoming appointment) to `Home`. `Cart` and `Notifications` show an empty state until they are designed.

### Flows
- **Sign up:** Landing "Get Started" / Login "sign up" → `SelectRole` → `GET Account/Signup?role=Patient|Doctor`.
  The role is shown on the Sign up page and posted as a hidden `Role` field (unknown values fall back to `Patient`).
- **Password recovery:** Login → "Forget Password?" → `ForgotPassword` (email) → `VerifyOtp` → `ResetPassword`.
  `ForgotPassword` isn't in the Figma file. It's built from the same components so the flow has no dead end.
- **Log out:** `POST Account/Logout` with the anti-forgery token, never a plain link. It is at the bottom of the sidebar and in the landing header when signed in.
- **Signed-in state:** when `User.Identity.IsAuthenticated`, the landing header shows `User.Identity.Name`, the role
  (`User.IsInRole("Patient" | "Doctor" | "Admin")`) and **Log Out** instead of Log In / Get Started. The hero shows
  "Go to Dashboard" (Doctor → `DoctorAppointments/Index`, others → `Patient/Home`). The sidebar shows the name and role above Log out.

> **Security:** `Role` comes from the browser, so it can be edited. The `Signup` POST must **not** create an `Admin`
> from this value. Reject it, or require an existing admin to create/approve admin accounts.

### Links
Every link points to a page that exists. Links to pages that weren't built (Pharmacy, Labs, Rewards,
e-Consultation, Medical History, Profile) were removed. Cart and Notifications are back in the sidebar (as in Figma)
and open an empty-state page (`Patient/Cart`, `Patient/Notifications`).

> **Roles:** Select Role shows **Patient** and **Doctor** only (team decision). Pharmacist and Admin accounts are not created
> from the sign-up page, and `Signup` ignores those values (falls back to Patient). The backend rejects them as well.
- "Find Specialist", "See all", "Check Now", the dashboard search (`?name=`) and the **Doctor** tile go to `Doctors/Index`.
- **Medical History** opens `MyPrescriptions/Index`. The other dashboard tiles from Figma (Pharmacy, Reward, Labs, e-Consultation) open
  `Patient/Pharmacy`, `Patient/Rewards`, `Patient/Labs`, `Patient/Consultation`: a shared "Coming soon" page (`Views/Patient/ComingSoon.cshtml`)
  until they are designed. (`Patient/MedicalHistory` still exists but nothing links to it.)
- Topbar (as in Figma): bell → `Patient/Notifications`; the "indent" button collapses the sidebar (opens it on mobile).
- Sidebar (as in Figma): the AI Assistant screen shows Home + AI Assistant; every other screen shows Home, Appointments, Medical Records, Cart, Notifications.
  The dashboard reaches the AI Assistant through the floating robot button.
- "My Appointments", sidebar "Appointments" and "View Details" go to `MyAppointments/Index`.
- "AI Assistant" and "Get Connected" go to `Patient/AiAssistant`.

## Form field names (for the backend team's ViewModels)

| Page | Request | Fields |
|---|---|---|
| SelectRole | `GET Account/Signup` | `role` (Patient / Doctor), query string |
| Signup | `POST Account/Signup` | `Role` (hidden), `FullName`, `Email`, `Password`, `ConfirmPassword` |
| Login | `POST Account/Login` | `Email`, `Password` |
| ForgotPassword | `POST Account/ForgotPassword` | `Email` |
| VerifyOtp | `POST Account/VerifyOtp` (resend: `POST Account/ResendOtp`) | `Code` (4 digits) |
| ResetPassword | `POST Account/ResetPassword` | `NewPassword`, `ConfirmPassword` |
| Sidebar, landing header | `POST Account/Logout` | anti-forgery token only |
| MedicalRecords/Edit (doctor) | `POST MedicalRecords/Save` | `doctorId`, `appointmentId` (hidden), `diagnosis`, `notes` (optional) |
| Login / Sign up (Google button) | `POST Account/ExternalLogin` | `provider` = `Google` (standard Identity external login) |

- POST forms use `asp-controller` / `asp-action`, so the anti-forgery token is added automatically.
- **Client-side validation:** `_AuthLayout` loads jQuery + `_ValidationScriptsPartial`, and the inputs carry `data-val-*` rules
  (required, email, min length 6, max length 100, passwords match). Keep server-side validation too, since client rules can be bypassed.
  The OTP code is checked by `vitala.js` (all 4 digits required). The "Resend code" button skips that check.
- Every field has `<span data-valmsg-for="...">`, so ModelState errors from the server show up in the same place.
  You can switch `name="Email"` to `asp-for="Email"` once a `@model` exists. The `data-val-*` attributes are then generated from the model.
- Static text (Sara, Dr. Sarah Jenkins, the appointment, ...) is placeholder content to replace with Model data.

## AI chat hook
`vitala.js` fires a `vitala:chat-send` event when the user sends a message. Usage is documented in the comment inside the file.
Remove `data-vt-chat-demo` from `AiAssistant.cshtml` once the real API is connected.

## Images
Exported from the Figma file into `wwwroot/images/vitala/`:

| File | Used in |
|---|---|
| `logo.png` | Landing header, sidebar |
| `doctor-sarah-jenkins.png` | Landing featured doctor |
| `feature-find-specialist.png`, `feature-get-connected.png` | Landing feature cards |
| `feature-search-pharmacy.png` | Not used (pharmacy is out of scope) |
| `role-patient.svg`, `role-doctor.svg` | Select Role cards (`role-pharmacist.png`, `role-admin.png` are no longer used) |
| `eye.png` | Password show/hide button |
| `patient-avatar.png` | Dashboard avatar |
| `banner-doctor.png` | Dashboard "Medical Checks" banner |
| `bot.png` | AI button on the dashboard, bot avatar in the chat |
| `user-avatar.png` | User avatar in the chat |
| `icon-home.png`, `icon-cart.png`, `icon-notifications.png` | Sidebar items |
| `google.png` | Google sign-in button on Login / Sign up |
| `svc-doctor.png`, `svc-pharmacy.png`, `svc-labs.png`, `svc-econsultation.png`, `svc-medical-history.png` | Dashboard service tiles |
| `svc-reward.png` | Not used yet: the export only has the badge background (re-export `reward-svgrepo-com 1` with its children) |

Inline SVG (`_VtIcon.cshtml`) is used where Figma has no exported icon: sidebar Appointments / AI Assistant,
the Reward tile, and the topbar bell / indent buttons.

## App pages (backend endpoints)
Restyled in the Vitala look; controller actions, form field names and `@model` types are unchanged:
`Doctors/Index`, `Appointments/Book`, `MyAppointments/Index`, `MyPrescriptions/Index` (patient),
`MyMedicalRecords/Index` (patient), `DoctorAppointments/Index`, `Availability/Index`, `Prescriptions/Index`, `MedicalRecords/Edit` (doctor). No Figma frames exist for these screens.
- `_VtAlerts.cshtml` shows `TempData["Success"]` / `TempData["Error"]` on every page with a form.
- `_VtStatusBadge.cshtml` colors an `AppointmentStatus`.
- Doctor pages set `ViewData["SidebarRole"] = "Doctor"` (sidebar: Appointments, Working Hours). The sidebar keeps the
  temporary `doctorId` from the URL; remove that when login is ready.
- Booking: changing the date reloads times from `GET /Appointments/AvailableTimes` (JSON) without a page reload.
- Doctor calendar: FullCalendar 6 (CDN) reads `GET /DoctorAppointments/Events`, colored by status.
- Cancel / Reject / No-show / Remove ask for confirmation (`<form data-vt-confirm="...">`).
- Medical records: a completed appointment on the doctor's page has **Prescription** and **Medical record** links
  (`MedicalRecords/Edit?doctorId=&appointmentId=`); the two pages link to each other. Patients open `MyMedicalRecords`
  from the sidebar ("Medical Records"); the page is printable and links to My Prescriptions.
- My Prescriptions is printable (sidebar, buttons and alerts are hidden when printing). The dashboard "Medical History" tile opens it.
- Request for the backend: the Book page only receives free times; a `DoctorDto` (name, specialty, fee) in `ViewBag` would let it show which doctor is being booked.

