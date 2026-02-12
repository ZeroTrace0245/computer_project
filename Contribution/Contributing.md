# Contribution Guide — SmartBite

Nine members, nine areas. Each section lists the files you own, CONTRIBUTING to work on, and the key code you'll touch.

---

## Member 1 — Layout & Responsiveness

**Goal**: Optimise the main grid, sticky header, and mobile sidebar toggle.

**Files**:
- `computer_project.Web/Components/Layout/MainLayout.razor` — page grid, sidebar toggle, sticky top bar
- `computer_project.Web/Components/Layout/MainLayout.razor.css` — flex layout, breakpoints, sidebar width
- `computer_project.Web/wwwroot/app.css` — `.page`, `.sidebar`, `@media` rules

**What to do**:
- Fix `MainLayout.razor.css` breakpoint at `641px` to match Bootstrap's `992px (lg)` used in the razor markup (`d-lg-none`).
- Ensure `.page` uses `flex-direction: row` at `lg` breakpoint, not `641px`.
- Test sidebar overlay on mobile (`sidebarShow` toggle + `.modal-backdrop`).
- Verify sticky header stays pinned during scroll on all viewports.

**Key code** (`MainLayout.razor` lines 8–90):
```razor
<div class="page">
    <div class="sidebar mica-effect @(sidebarShow ? "show" : "")">
        <NavMenu />
    </div>
    <main>
        <div class="top-row ... position: sticky; top: 0; z-index: 1000;">
            <button class="btn ... d-lg-none" @onclick="ToggleSidebar">
            ...
        </div>
        <div class="content px-3 px-md-4">@Body</div>
    </main>
    @if (sidebarShow) { <div class="modal-backdrop ..."> }
</div>
```

**Key code** (`MainLayout.razor.css` lines 49–77):
```css
@media (min-width: 641px) {   /* ← align with lg breakpoint */
    .page { flex-direction: row; }
    .sidebar { width: 250px; height: 100vh; position: sticky; top: 0; }
}
```

---

## Member 2 — Theming Pipeline

**Goal**: Refine dark/light toggle, ensure consistent CSS tokens, and mica/acrylic effects.

**Files**:
- `computer_project.Web/Components/Layout/MainLayout.razor` — `ToggleDarkMode()`, `ApplyTheme()`
- `computer_project.Web/wwwroot/app.css` — `:root` tokens, `[data-bs-theme="dark"]`, `.mica-effect`, `.acrylic-effect`
- `computer_project.Web/Services/UserSession.cs` — `IsDarkMode`, `SetTheme()`

**What to do**:
- Wire the `ToggleDarkMode()` method to a visible button in the header (currently unused).
- Ensure `ApplyTheme()` JS interop sets `data-bs-theme` on `<html>` element.
- Audit all cards/surfaces for `var(--win11-surface)` and `var(--win11-text)` usage.
- Verify `.mica-effect` and `.acrylic-effect` render correctly in both themes.

**Key code** (`MainLayout.razor` lines 101–113):
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender) { await ApplyTheme(); }
}
private async Task ToggleDarkMode()
{
    Session.SetTheme(!Session.IsDarkMode);
    await ApplyTheme();
}
private async Task ApplyTheme()
{
    await JS.InvokeVoidAsync("setTheme", Session.IsDarkMode ? "dark" : "light");
}
```

**Key code** (`app.css` `:root` and dark tokens):
```css
:root {
    --win11-accent: #0078d4;
    --win11-surface: rgba(255, 255, 255, 0.7);
    --win11-sidebar: rgba(18, 33, 65, 0.85);
}
[data-bs-theme="dark"] {
    --win11-surface: rgba(43, 43, 43, 0.7);
    --win11-sidebar: rgba(15, 23, 42, 0.9);
}
```

---

## Member 3 — Navigation & Routing

**Goal**: Harden NavMenu routes, active-link states, and deep-link handling.

**Files**:
- `computer_project.Web/Components/Layout/NavMenu.razor` — sidebar nav items, `NavLink` components
- `computer_project.Web/Components/Layout/NavMenu.razor.css` — active-link styling
- `computer_project.Web/wwwroot/app.css` — `.nav-item .nav-link.active` styles

**What to do**:
- Verify every `NavLink href` matches its page `@page` directive (e.g. `href="meals"` → `@page "/meals"`).
- Ensure `NavLinkMatch.All` is only on the Home route; all others default to prefix match.
- Test deep-linking directly to `/reports`, `/water`, `/shoppinglist` etc.
- Ensure admin-only "System Settings" vs consumer "Settings" renders the correct label.

**Key code** (`NavMenu.razor` lines 16–99):
```razor
<NavLink class="nav-link ..." href="dashboard">Dashboard</NavLink>
<NavLink class="nav-link ..." href="" Match="NavLinkMatch.All">Home</NavLink>
<NavLink class="nav-link ..." href="meals">Log History</NavLink>
<NavLink class="nav-link ..." href="reports">Performance</NavLink>
<NavLink class="nav-link ..." href="water">Hydration</NavLink>
<NavLink class="nav-link ..." href="shoppinglist">Groceries</NavLink>
@if (Session.IsAdmin) { <NavLink href="settings">System Settings</NavLink> }
else { <NavLink href="settings">Settings</NavLink> }
<NavLink class="nav-link ..." href="feedback">Support</NavLink>
<NavLink class="nav-link ..." href="labs">Labs</NavLink>
```

---

## Member 4 — Session State

**Goal**: Improve `UserSession` lifecycle, null/role guards, and change notifications.

**Files**:
- `computer_project.Web/Services/UserSession.cs` — all session logic
- `computer_project.Web/Components/Layout/MainLayout.razor` — `OnInitialized`, `Dispose` (event subscribe/unsubscribe)

**What to do**:
- Add null guard on `CurrentUser` access (e.g. `Username[0]` can throw if null).
- Consider debouncing `NotifyStateChanged()` to avoid redundant re-renders.
- Ensure `OnChange` event is unsubscribed in every component that subscribes (check `IDisposable`).
- Add `TimeZoneId` and `SearchQuery` validation.

**Key code** (`UserSession.cs` full file):
```csharp
public class UserSession
{
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == "Admin";
    public bool IsEndUser => CurrentUser?.Role == "EndUser";
    public bool IsDarkMode { get; set; }
    public string SearchQuery { get; private set; } = string.Empty;
    public event Action? OnChange;

    public void Login(User user) { CurrentUser = user; NotifyStateChanged(); }
    public void Logout() { CurrentUser = null; NotifyStateChanged(); }
    public void SetTheme(bool dark) { IsDarkMode = dark; NotifyStateChanged(); }
    public void SetSearch(string query) { SearchQuery = query; NotifyStateChanged(); }
    public void NotifyStateChanged() => OnChange?.Invoke();
}
```

**Key code** (`MainLayout.razor` subscribe/dispose):
```csharp
protected override void OnInitialized() { Session.OnChange += StateHasChanged; }
public void Dispose() { Session.OnChange -= StateHasChanged; }
```

---

## Member 5 — Auth Flows

**Goal**: Enhance login/logout, protect admin-only UI, handle edge cases.

**Files**:
- `computer_project.Web/Components/Pages/Login.razor` — login form, `HandleLogin()`
- `computer_project.Web/Components/Pages/Register.razor` — registration form
- `computer_project.Web/Components/Pages/Settings.razor` — admin-only guard (`Session.IsAdmin`)
- `computer_project.Web/Components/ConsumerOnly.razor` — role gate wrapper

**What to do**:
- Add redirect to `/login` when unauthenticated users hit protected pages.
- Add login expiry/session timeout handling.
- Ensure Settings page blocks consumers (`ConsumerOnly` or direct `Session.IsAdmin` check).
- Validate admin badge rendering in `MainLayout.razor` line 48.

**Key code** (`Login.razor` lines 58–80):
```csharp
private async Task HandleLogin()
{
    var user = await ApiClient.LoginAsync(username, password);
    if (user != null) { Session.Login(user); Navigation.NavigateTo("/dashboard"); }
    else { errorMessage = "Invalid username or password."; }
}
```

**Key code** (`ConsumerOnly.razor`):
```razor
@if (Session.IsAdmin)
{
    <div>Consumer Access Only ... <a href="settings">User & System Management</a></div>
}
else { @ChildContent }
```

---

## Member 6 — Header Actions

**Goal**: Wire the quick-action buttons (log meal, log water, notifications) to real functionality.

**Files**:
- `computer_project.Web/Components/Layout/MainLayout.razor` — header action buttons (lines 40–51)
- `computer_project.Web/SmartBiteApiClient.cs` — `AddMealAsync()`, `AddWaterIntakeAsync()`

**What to do**:
- Wire "Quick Log Meal" button (`bi-plus-circle`) to open a modal or navigate to `/meals`.
- Wire "Log Water" button (`bi-droplet`) to call `AddWaterIntakeAsync()` with a default amount or open a prompt.
- Wire "Notifications" button (`bi-bell`) to show a dropdown or popover with recent activity.
- All three are currently no-op `<button>` elements with no `@onclick`.

**Key code** (`MainLayout.razor` lines 40–51):
```razor
@if (Session.IsEndUser)
{
    <button class="btn btn-sm btn-link text-muted" title="Quick Log Meal">
        <i class="bi bi-plus-circle"></i>
    </button>
    <button class="btn btn-sm btn-link text-muted" title="Log Water">
        <i class="bi bi-droplet"></i>
    </button>
}
else if (Session.IsAdmin)
{
    <span class="badge ...">ADMIN CONSOLE</span>
}
<button class="btn btn-sm btn-link text-muted" title="Notifications">
    <i class="bi bi-bell"></i>
</button>
```

---

## Member 7 — Profile Chip

**Goal**: Handle missing initials, long usernames, and avatar fallback.

**Files**:
- `computer_project.Web/Components/Layout/MainLayout.razor` — profile section (lines 53–64)
- `computer_project.Web/Services/UserSession.cs` — `CurrentUser` properties

**What to do**:
- Guard `Username[0]` against null/empty (currently can throw `NullReferenceException`).
- Truncate long usernames in the display (`text-truncate` or max-width).
- Add a fallback avatar (e.g. `?` or generic icon) when `CurrentUser` is null.
- Ensure role label displays correctly for all roles.

**Key code** (`MainLayout.razor` lines 53–64):
```razor
@if (Session.IsLoggedIn)
{
    <div class="d-flex align-items-center ...">
        <div class="text-end d-none d-md-block">
            <div class="small fw-bold lh-1">@Session.CurrentUser?.Username</div>
            <div class="text-muted small lh-1">@Session.CurrentUser?.Role</div>
        </div>
        <div class="bg-primary text-white rounded-circle ..." style="width: 32px; height: 32px;">
            @Session.CurrentUser?.Username[0].ToString().ToUpper()  <!-- can throw -->
        </div>
    </div>
}
```

---

## Member 8 — Feedback / Contact Feature

**Goal**: Build out the feedback page with form validation, API submission, and success/error states.

**Files**:
- `computer_project.Web/Components/Pages/Feedback.razor` — UI form, rating stars, contact cards
- `computer_project.Web/SmartBiteApiClient.cs` — add a `SubmitFeedbackAsync()` method
- `computer_project.ApiService/Program.cs` — add a `POST /feedback` endpoint
- `computer_project.Web/Models.cs` — add a `Feedback` model if needed

**What to do**:
- Add form validation (require rating + message before submit).
- Wire submit button to call API and show success/error toast.
- Add a `Feedback` model class and a `POST /feedback` endpoint on the API.
- Consider adding a confirmation message or thank-you state after submit.

**Key code** (`Feedback.razor` lines 59–80):
```razor
<div class="col-md-7">
    <div class="card ...">
        <h5>Send Us Feedback</h5>
        <label>How would you rate your experience?</label>
        <div class="d-flex gap-3">
            @for (int i = 1; i <= 5; i++)
            {
                int rating = i;
                <button class="btn @(currentRating >= rating ? "btn-primary" : "btn-outline-secondary")"
                        @onclick="() => currentRating = rating">★ @rating</button>
            }
        </div>
        <!-- TODO: add textarea + submit button + API call -->
    </div>
</div>
```

---

## Member 9 — API Client Hardening

**Goal**: Add error handling, typed DTOs, retry logic, and cancellation across all API calls.

**Files**:
- `computer_project.Web/SmartBiteApiClient.cs` — all HTTP methods
- `computer_project.Web/Models.cs` — model classes used as DTOs
- `computer_project.ApiService/Program.cs` — API endpoints (response shapes)

**What to do**:
- Wrap every call in try/catch to handle `HttpRequestException` (see `GetGoalAsync` for the pattern).
- Return typed result objects instead of raw nulls (e.g. `ApiResult<T>` with success/error).
- Add retry/backoff for transient failures (consider `Polly` or manual retry loop).
- Ensure all methods accept and forward `CancellationToken`.

**Key code** (`SmartBiteApiClient.cs` — existing pattern to follow):
```csharp
public async Task<UserGoal?> GetGoalAsync(int userId, CancellationToken ct = default)
{
    try
    {
        return await httpClient.GetFromJsonAsync<UserGoal>($"/goals/{userId}", ct);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return null;  // ← this pattern should be applied to all methods
    }
}
```

**Methods to harden**:
- `GetMealsAsync`, `AddMealAsync`
- `GetShoppingListAsync`, `AddShoppingListItemAsync`, `UpdateShoppingListItemAsync`, `DeleteShoppingListItemAsync`
- `GetReportAsync`
- `GetWaterIntakesAsync`, `AddWaterIntakeAsync`
- `LoginAsync`, `RegisterUserAsync`
- `GetUsersAsync`, `DeleteUserAsync`, `UpdateUserAsync`

---

## Quick Reference

| # | Area | Primary File(s) | Owner |
|---|------|-----------------|-------|
| 1 | Layout & responsiveness | `MainLayout.razor`, `MainLayout.razor.css`, `app.css` | — |
| 2 | Theming pipeline | `MainLayout.razor`, `app.css`, `UserSession.cs` | — |
| 3 | Navigation & routing | `NavMenu.razor`, `NavMenu.razor.css`, `app.css` | — |
| 4 | Session state | `UserSession.cs`, `MainLayout.razor` | — |
| 5 | Auth flows | `Login.razor`, `Register.razor`, `Settings.razor`, `ConsumerOnly.razor` | — |
| 6 | Header actions | `MainLayout.razor` (lines 40–51), `SmartBiteApiClient.cs` | — |
| 7 | Profile chip | `MainLayout.razor` (lines 53–64), `UserSession.cs` | — |
| 8 | Feedback/Contact | `Feedback.razor`, `SmartBiteApiClient.cs`, `Program.cs`, `Models.cs` | — |
| 9 | API client hardening | `SmartBiteApiClient.cs`, `Models.cs`, `Program.cs` | — |

