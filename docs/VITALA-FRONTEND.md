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
| `Views/Account/Signup.cshtml` | 04 Sign up |
| `Views/Account/VerifyOtp.cshtml` | 05 Verify OTP |
| `Views/Account/ResetPassword.cshtml` | 06 Reset password |
| `Views/Patient/Home.cshtml` | 1.2 Patient dashboard |
| `Views/Patient/AiAssistant.cshtml` | AI assistant |

### Controllers still needed (backend)
These views are ready, but they have no controller yet in this repo:
- `AccountController`: `SelectRole`, `Login`, `Signup`, `VerifyOtp`, `ResendOtp`, `ResetPassword` (Identity task).
- `PatientController`: `Home`, `AiAssistant` (each one only needs `return View();` to show the page).

Links that already point to the team's existing pages:
- "Find Specialist", the Doctor tile and the "Book ..." buttons go to `Doctors/Index` (with `?specialty=`).
- Appointments in the sidebar and "View Details" go to `MyAppointments/Index`.

Note: the Figma role screen has 4 roles (Patient, Doctor, Pharmacist, Admin), while the README lists 3. Remove the Pharmacist card from `SelectRole.cshtml` if it's not in scope.

## Form field names (for the backend team's ViewModels)

| Page | POST action | Fields |
|---|---|---|
| SelectRole | `Account/SelectRole` | `Role` (Patient / Doctor / Pharmacist / Admin) |
| Login | `Account/Login` | `Email`, `Password` |
| Signup | `Account/Signup` | `Email`, `Password`, `ConfirmPassword` |
| VerifyOtp | `Account/VerifyOtp` (resend: `Account/ResendOtp`) | `Code` (4 digits) |
| ResetPassword | `Account/ResetPassword` | `NewPassword`, `ConfirmPassword` |

- Forms use `asp-controller` / `asp-action`, so the anti-forgery token is added automatically.
- Every field has `<span data-valmsg-for="...">`, so jQuery unobtrusive validation and ModelState errors show up as-is.
  You can also switch `name="Email"` to `asp-for="Email"` once a `@model` exists.
- Static text (Sara, Dr. Sarah Jenkins, the appointment, ...) is placeholder content to replace with Model data.

## AI chat hook
`vitala.js` fires a `vitala:chat-send` event when the user sends a message. Usage is documented in the comment inside the file.
Remove `data-vt-chat-demo` from `AiAssistant.cshtml` once the real API is connected.

## Images
The Figma PNGs (logo, doctor/feature illustrations, specialty icons) weren't sent, so inline SVG icons stand in for them.
To use the real images, put them in `wwwroot/images/...` and uncomment the `<img>` lines left in the views.
