# computer_project

Blazor app with a mica/acrylic theme. Frontend, backend API, and shared defaults live in a single repo.

> ⚠️ Requires .NET 10 SDK. Earlier SDKs are not supported.

## Prerequisites
- .NET 10 SDK
- Git

## Projects
- `computer_project.Web` — Blazor UI, layout, routing, SmartBite API client.
- `computer_project.ApiService` — backend API and shared models.
- `computer_project.ServiceDefaults` — common service wiring/extensions.
- `computer_project.AppHost` — hosting/bootstrap for the solution.

## Quickstart
```bash
# Clone
git clone https://github.com/ZeroTrace0245/computer_project.git
cd computer_project

# Restore
dotnet restore

# Run API (adjust URL/port as needed)
dotnet run --project computer_project.ApiService

# Run UI with hot reload
dotnet watch --project computer_project.Web
```

## File structure (key paths)
```
computer_project.Web/            # Blazor UI
  Components/
    Layout/
      MainLayout.razor
      NavMenu.razor
    Pages/
      Home.razor
      Dashboard.razor
      Feedback.razor
      Settings.razor
      ShoppingList.razor
  Services/
    UserSession.cs
  SmartBiteApiClient.cs
  Models.cs
  wwwroot/
    app.css

computer_project.ApiService/     # Backend API
  Program.cs
  Models.cs
  Data/
  Services/

computer_project.ServiceDefaults/ # Shared service wiring
  Extensions.cs

computer_project.AppHost/         # Host/bootstrap
  Program.cs
```

## Tech stack
| Area | Technology | Notes |
| --- | --- | --- |
| UI | Blazor (`.razor` components) | Layout, routing, role-aware UI (admin badge, quick actions). |
| Styling | Bootstrap 5 + custom CSS (`wwwroot/app.css`) | Mica/acrylic-inspired surfaces, responsive sidebar/header. |
| Client interop | `IJSRuntime` | Theme toggle and other JS hooks. |
| State/Auth | `UserSession` service | Tracks login, roles, theme, and change notifications. |
| API client | `SmartBiteApiClient` (`HttpClient`) | Calls backend endpoints for data and feedback. |
| Backend | ASP.NET Core (`computer_project.ApiService`) | API + shared models. |
| Hosting | .NET 10 | CLI-driven build/run. |

## Screenshots (placeholders)
| View | Path |
| --- | --- |
| Startup | ![Startup](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Startup%20.png) |
| Dashboard | ![Dashboard Dashboard](https://github.com/ZeroTrace0245/computer_project/blob/ad9d1a3e722af95c084b06bd998b6195083238ee/screenshots/Dashboard.gif) |
| Home | ![Home](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Home%20page.png) |
| Log History | ![Log history](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Log%20History.png) |
| Performance | ![Performance](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Performance.png) |
| Hydration | ![Hydration](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Hydration.png) |
| Groceries | ![Groceries](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Groceries.gif) |
| Settings (when logged as an admin) | ![Settings](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Setting%20page%20admin%20athu.png) |
| Settings (when logged as consumer) | ![Setting](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Settings%20only%20for%20ADMINS.png) |
| Support | ![Support](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Support.png) |
| Labs | ![Labs](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Labs.png) |
| Connection/Rejoin error | ![Error](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/severnot%20connected.png) |
| Role-based access (admin vs consumer) | ![Role-based](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Admin%20logging%20warning%20consumers%20only.png) | ![Role-based](https://github.com/ZeroTrace0245/computer_project/blob/a63f15c996b2e7d53140f1349c60286cf1d756f5/screenshots/Settings%20only%20for%20ADMINS.png) |

_Add captures to `docs/screenshots/` matching the above filenames._

## ER diagram
```mermaid
erDiagram
    %% TODO: replace with actual entities/relations
    User ||--o{ Session : maintains
    User ||--o{ Feedback : submits
    User ||--o{ Meal : logs
    Meal ||--o{ WaterIntake : includes
    User ||--o{ Share : publishes
    Share }o--|| Feedback : can_reference
    FeedbackButton ||--|| Feedback : triggers
```

## Contributing focus areas
- Layout/theming consistency (mica/acrylic, dark/light toggle)
- Nav/menu and sticky header behavior
- Session/auth and role-aware UI (admin badge, quick actions)
- Feedback form and SmartBite API client hardening
- Role-based access: admin-only Settings page; consumers are blocked from Settings.
