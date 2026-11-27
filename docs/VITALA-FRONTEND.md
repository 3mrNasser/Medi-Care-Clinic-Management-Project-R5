# Vitala UI — Frontend handoff (MediCare.Web)

Frontend only: Razor views + CSS + JS. **No backend logic**: no models, no services, no DB.

## Files added

| File (under `MediCare.Web/`) | Purpose |
|---|---|
| `wwwroot/css/vitala.css` | All styles. Every class starts with `vt-`, so nothing clashes with Bootstrap or `site.css`. |
| `wwwroot/js/vitala.js` | UI behaviour only (password eye, OTP boxes and timer, role cards, mobile menus, tabs, calendar, booking times). |
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

### Controllers
`Controllers/AccountController.cs` and `Controllers/PatientController.cs` contain **GET actions only** (`return View();`)
so the screens can be opened. The backend work goes into the same files:
- `AccountController`: Login, Signup, Logout, AccessDenied, password recovery (`ForgotPassword`, `VerifyOtp`, `ResendOtp`, `ResetPassword`) and Google sign-in (`ExternalLogin`, `ExternalLoginCallback`).
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
Every link points to a page that exists. **Removed from scope (team decision):** AI Assistant, Pharmacy, Labs, Rewards and
e-Consultation: their pages, links, dashboard tiles, the floating robot button, chat CSS/JS and images are gone. Cart and Notifications stay in the sidebar (as in Figma)
and open an empty-state page (`Patient/Cart`, `Patient/Notifications`).

> **Roles:** Select Role shows **Patient** and **Doctor** only (team decision). Pharmacist and Admin accounts are not created
> from the sign-up page, and `Signup` ignores those values (falls back to Patient). The backend rejects them as well.
- "Find Specialist", "See all", "Check Now", the dashboard search (`?name=`) and the **Doctor** tile go to `Doctors/Index`.
- **Medical History** opens `MyMedicalRecords/Index`. The dashboard shows two tiles: Doctor and Medical History.
  until they are designed. (`Patient/MedicalHistory` still exists but nothing links to it.)
- Topbar (as in Figma): bell → `Patient/Notifications`; the "indent" button collapses the sidebar (opens it on mobile).
- Sidebar (patient): Home, Appointments, Medical Records, Cart, Notifications.
- "My Appointments", sidebar "Appointments" and "View Details" go to `MyAppointments/Index`.
- The landing "Medical Records" card goes to `MyMedicalRecords/Index` (it replaced "Get Connected").

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
| MedicalRecords/Edit (doctor) | `POST MedicalRecords/Save` | `appointmentId` (hidden), `diagnosis`, `notes` (optional) |
| Login / Sign up (Google button) | `POST Account/ExternalLogin` | `provider` = `Google` (standard Identity external login) |

- POST forms use `asp-controller` / `asp-action`, so the anti-forgery token is added automatically.
- **Client-side validation:** `_AuthLayout` loads jQuery + `_ValidationScriptsPartial`, and the inputs carry `data-val-*` rules
  (required, email, min length 6, max length 100, passwords match). Keep server-side validation too, since client rules can be bypassed.
  The OTP code is checked by `vitala.js` (all 4 digits required). The "Resend code" button skips that check.
- Every field has `<span data-valmsg-for="...">`, so ModelState errors from the server show up in the same place.
  You can switch `name="Email"` to `asp-for="Email"` once a `@model` exists. The `data-val-*` attributes are then generated from the model.
- The patient dashboard shows the real first name (`FullName` claim) and the next Pending/Confirmed appointment (`ViewBag.NextAppointment`, `ViewBag.NextDoctor`). The landing page "featured doctor" is still static marketing content.


## Images
Exported from the Figma file into `wwwroot/images/vitala/`:

| File | Used in |
|---|---|
| `logo.png` | Landing header, sidebar |
| `doctor-sarah-jenkins.png` | Landing featured doctor |
| `feature-find-specialist.png` | Landing feature card |
| `role-patient.svg`, `role-doctor.svg` | Select Role cards |
| `eye.png` | Password show/hide button |
| `patient-avatar.png` | Dashboard avatar |
| `banner-doctor.png` | Dashboard "Medical Checks" banner |
| `icon-home.png`, `icon-cart.png`, `icon-notifications.png` | Sidebar items |
| `google.png` | Google sign-in button on Login / Sign up |
| `svc-doctor.png`, `svc-medical-history.png` | Dashboard service tiles |

Inline SVG (`_VtIcon.cshtml`) is used where Figma has no exported icon: sidebar items without a Figma icon,
the landing feature cards, and the topbar bell / indent buttons.

## App pages (backend endpoints)
Restyled in the Vitala look; controller actions, form field names and `@model` types are unchanged:
`Doctors/Index`, `Appointments/Book`, `MyAppointments/Index`, `MyPrescriptions/Index` (patient),
`MyMedicalRecords/Index` (patient), `DoctorAppointments/Index`, `Availability/Index`, `Prescriptions/Index`, `MedicalRecords/Edit` (doctor). No Figma frames exist for these screens.
- `_VtAlerts.cshtml` shows `TempData["Success"]` / `TempData["Error"]` on every page with a form.
- `_VtStatusBadge.cshtml` colors an `AppointmentStatus`.
- `Account/AccessDenied` (403, Identity `AccessDeniedPath`): shows the signed-in user and role, a button to that role's home
  (Doctor → `DoctorAppointments`, Patient → `Patient/Home`) and "Use another account" (POST `Account/Logout`).
- Doctor pages set `ViewData["SidebarRole"] = "Doctor"` (sidebar: Appointments, Working Hours). No `doctorId` is sent:
  the backend reads the signed-in doctor. Patient pages are Patient-only and doctor pages Doctor-only (`[Authorize(Roles)]`).
- Booking: changing the date reloads times from `GET /Appointments/AvailableTimes` (JSON) without a page reload.
- Doctor calendar: FullCalendar 6 (CDN) reads `GET /DoctorAppointments/Events`, colored by status.
- Cancel / Reject / No-show / Remove ask for confirmation (`<form data-vt-confirm="...">`).
- Medical records: a completed appointment on the doctor's page has **Prescription** and **Medical record** links
  (`MedicalRecords/Edit?appointmentId=`); the two pages link to each other. Patients open `MyMedicalRecords`
  from the sidebar ("Medical Records"); the page is printable and links to My Prescriptions.
- My Prescriptions is printable (sidebar, buttons and alerts are hidden when printing). The dashboard "Medical History" tile opens it.
- Book and Reschedule receive `ViewBag.Doctor` (`DoctorDto`) and show it with `_VtDoctorSummary` (name, specialty, clinic, fee). Booking an unknown or unapproved doctor redirects to Doctors with an error.


## Backend features added (feature/backend-gaps)

### Password recovery
`ForgotPassword` (email) → `VerifyOtp` (4-digit code) → `ResetPassword` → `Login`.
- The code is random, stored **hashed** in `AspNetUserTokens` (no migration), valid for **10 minutes**, one use only,
  **5 wrong tries** max, and a new one can be requested at most once a minute.
- The same answer is shown whether the email exists or not, so nobody can find out who has an account.
- The email and the Identity reset token move between the steps in TempData (an encrypted cookie).
- Email is sent by `Infrastructure/SmtpEmailService.cs`. Settings (password in user-secrets, not in git):
  `Email:Host`, `Email:Port`, `Email:UserName`, `Email:Password`, `Email:From`, `Email:EnableSsl`.
  **Without `Email:Host` in Development the email (with the code) is written to the server log** so the flow can be tested.

### Google sign-in
Turned on only when the keys exist (never commit them):
```
dotnet user-secrets set "Authentication:Google:ClientId" "..." --project MediCare.Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "..." --project MediCare.Web
```
Google Cloud Console → OAuth client (Web) → redirect URI `https://localhost:<port>/signin-google`.
New Google users become **Patients**; an existing account with the same (verified) email is linked.
Without keys, the button shows "Google sign-in isn't set up yet".

### Roles and start pages
- After login: Patient → `Patient/Home`, Doctor → `DoctorAppointments`, Admin → `Admin/Doctors` (a local `returnUrl` wins).
- `FullName` is added to the login cookie (`Security/FullNameClaimsFactory.cs`): `User.FindFirst("FullName")`.
- `PatientController` is Patient-only.

### Doctor profile and approval
- A doctor who signs up gets an empty profile. `DoctorProfile` (sidebar "My Profile") lets them set specialty, clinic address and fee.
- `Admin/Doctors` lists all doctors (pending first). **Approve** works only when the profile is complete; **Remove approval** hides the doctor.
- Only approved doctors appear in search and can be booked.

## Notifications (live)
- `/Notifications` (patients and doctors): list with unread highlight, "Mark all as read", and each item opens its page (`POST Notifications/Open`).
- Sidebar "Notifications" shows an unread badge (`[data-vt-unread]`); the dashboard bell dot (`[data-vt-unread-dot]`) only shows when there's something unread.
- `_DashboardLayout` loads the SignalR client (10.0.11, SRI-pinned) for signed-in patients/doctors; `vitala.js` connects to `/hubs/notifications`,
  shows a toast for each new notification and updates the badge without reloading. Text is inserted with `textContent` (never as HTML).