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
- `PatientController`: pass real data (patient name, upcoming appointment) to `Home`.

### Flows
- **Sign up:** Landing "Get Started" / Login "sign up" → `SelectRole` → `GET Account/Signup?role=Patient|Doctor|Admin`.
  The role is shown on the Sign up page and posted as a hidden `Role` field (unknown values fall back to `Patient`).
- **Password recovery:** Login → "Forget Password?" → `ForgotPassword` (email) → `VerifyOtp` → `ResetPassword`.
  `ForgotPassword` isn't in the Figma file. It's built from the same components so the flow has no dead end.
- **Log out:** button at the bottom of the sidebar, `POST Account/Logout` with the anti-forgery token.

> **Security:** `Role` comes from the browser, so it can be edited. The `Signup` POST must **not** create an `Admin`
> from this value. Reject it, or require an existing admin to create/approve admin accounts.

### Links
Every link points to a page that exists. Links to pages that weren't built (Pharmacy, Labs, Cart, Rewards,
e-Consultation, Medical History, Profile, Notifications) were removed.
- "Find Specialist", "See all", "Check Now", the dashboard search (`?name=`) and the specialty tiles (`?specialty=`) go to `Doctors/Index`.
  Specialty values must match `Doctor.Specialty` exactly (`Cardiology`, `Dermatology` and `Pediatrics` are seeded).
- "My Appointments", sidebar "Appointments" and "View Details" go to `MyAppointments/Index`.
- "AI Assistant" and "Get Connected" go to `Patient/AiAssistant`.

## Form field names (for the backend team's ViewModels)

| Page | Request | Fields |
|---|---|---|
| SelectRole | `GET Account/Signup` | `role` (Patient / Doctor / Admin), query string |
| Signup | `POST Account/Signup` | `Role` (hidden), `FullName`, `Email`, `Password`, `ConfirmPassword` |
| Login | `POST Account/Login` | `Email`, `Password` |
| ForgotPassword | `POST Account/ForgotPassword` | `Email` |
| VerifyOtp | `POST Account/VerifyOtp` (resend: `POST Account/ResendOtp`) | `Code` (4 digits) |
| ResetPassword | `POST Account/ResetPassword` | `NewPassword`, `ConfirmPassword` |
| Sidebar | `POST Account/Logout` | anti-forgery token only |

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
The Figma PNGs (logo, doctor/feature illustrations, specialty icons) weren't sent, so inline SVG icons stand in for them.
To use the real images, put them in `wwwroot/images/...` and uncomment the `<img>` lines left in the views.
