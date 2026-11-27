# WebApplications-Exam2026

The repository contains two independent sub-applications. The sub-applications do not share code, 
projects or databases, and each sub-app can be run without the other. 

| Folder | Subapp | Technology |
| --- | --- | --- |
| `Subapp1-MVC/WebApp.Mvc` | Subapp 1 | ASP.NET 10 MVC (frontend and backend) |
| `Subapp1-MVC/WebApp.Mvc.Tests` | Subapp 1 | xUnit tests for 'WebApp.Mvc' |
| `Subapp2-API-React/WebbApp.Api` | Subapp 2 | ASP.NET 10 Web API (backend) |
| `Subapp2-API-React/webapp-react` | Subapp 2 | React + Vite (frontend) |

`WebApplication1.sln` in the repository root contains the three .NET projects, so
both subapps can be built together with `dotnet build WebApplication1.sln`
All commands below are run from the repository root, and the folder with the `.sln`.

## Running subapp 1 (ASP.NET 10 MVC)

Subapp 1 has its own SQLite database (`Subapp1-MVC/WebApp.Mvc/SheetsDB.db`). It is migrated
and seeded on start with demo users and the quests from 'WebApp.Mvc/Tasks'.

1. Start the app (http://localhost:5222):
```bash
dotnet run --project Subapp1-MVC/WebApp.Mvc --launch-profile http
```
2. Open http://localhost:5222

Demo accounts (password 'quest1234'): 'anna', 'beatrice', 'calvin' …
There is no default admin account for subapp 1.

### Running the tests

```bash
dotnet test Subapp1-MVC/WebApp.Mvc.Tests                               # all 36 tests
dotnet test Subapp1-MVC/WebApp.Mvc.Tests --filter Category=Positive    # the positive tests
dotnet test Subapp1-MVC/WebApp.Mvc.Tests --filter Category=Negative    # the negative tests
```

The tests use a temporary database, so the real `SheetsDB.db` is never touched.
See `Subapp1-MVC/WebApp.Mvc.Tests/README.md` for what each test file covers.

## Running subapp 2 (ASP.NET 10 Web API + React)

Subapp 2 is independent of subapp 1: it has its own API project and its own
SQLite database (`Subapp2-API-React/WebApp.Api/WebAppApi.db`, created and seeded on first start).
Node.js and npm are needed for the React app.

1. Start the API (http://localhost:5114):
```bash
dotnet run --project Subapp2-API-React/WebApp.Api --launch-profile http
```
2. In a second terminal, start the React app (http://localhost:5173):
```bash
cd Subapp2-API-React/webapp-react
npm install
npm run dev
```
3. Open http://localhost:5173. Vite forwards every `/api` request to the API, so the API
   must be running or the page shows "Request failed (502)".

Demo accounts (password `quest1234`): `admin` (Admin: can manage quests, courses
and users) and `anna`, `beatrice`, … (students). Delete `WebAppApi.db` to reset the data.

### Where things are

| Requirement | Subapp 1 |
|---|---|
| Does not crash | `Subapp1-MVC/WebApp.Mvc/Program.cs` (`UseExceptionHandler`, `UseStatusCodePagesWithReExecute`), `Views/Shared/Error.cshtml`, `Views/Home/StatusCode.cshtml` |
| Web UI with interactive forms | Razor views in `Subapp1-MVC/WebApp.Mvc/Views`; forms in `Views/Quests/_CreateQuestForm.cshtml`, `Views/Quests/Play.cshtml`, `Views/Users/Create.cshtml`, `Views/Courses/Create.cshtml`, `Views/Account/Login.cshtml` |
| CRUD / 3+ entities with CRUD | Quest (`Controllers/QuestsController.cs`, `Views/Quests`), Course (`Controllers/CoursesController.cs`, `Views/Courses`), User (`Controllers/UsersController.cs`, `Views/Users`) |
| Many-to-many | User ↔ Quest through `QuestCompletion` (best score, attempts) and `SavedQuest` (bookmarks, composite key), both in `Models`; relationships configured in `Models/SheetsDbContext.cs` |
| Complex domain rule | `Services/QuestScoringService.cs` with `Models/DifficultyRules.cs` (XP only on first completion), `Services/DailyStreakService.cs` (daily streak), quest ownership (`CanEditQuest` in `QuestsController`: only the creator or an admin can edit or delete a quest), question rules (`ValidateQuest` in `QuestsController`, `Models/CodeAnswerChecker.cs`), last admin cannot be demoted or deleted (`UsersController`) |
| Conditional rendering (2+ types) | `if` in `Views/Shared/_Layout.cshtml` (admin and signed-in links) and `Views/Quests/Quests.cshtml` (section and empty-list checks, edit/delete links per quest); ternary `? :` in `Views/Quests/Quests.cshtml` (library heading) and `Views/Quests/Play.cshtml` (result outcome); logical AND `&&` in `Views/Quests/Play.cshtml` (`Model.Submitted && !q.IsCorrect`) and `Views/Quests/Quests.cshtml` (`quest.CanEdit \|\| quest.CanDelete`) |
| Multi-parameter search and filtering | `QuestsController.Quests(section, searchText, difficulty)` → `DAL/QuestRepository.SearchAsync(search, difficulty, creatorId, savedByUserId)`; filter form in `Views/Quests/Quests.cshtml`; student search on `Views/Leaderboard/Leaderboard.cshtml` |
| Repository pattern and DAL | `Subapp1-MVC/WebApp.Mvc/DAL` (`IQuestRepository`, `ICourseRepository`, `IUserRepository`, `IProgressRepository` and implementations), registered in `Program.cs` |
| Async/await database access | All repository methods in `DAL` return `Task`, awaited in controller actions and services |
| ViewModels | `Subapp1-MVC/WebApp.Mvc/ViewModels` (e.g. `QuestsPageViewModel`, `CreateQuestViewModel`, `PlayQuestViewModel`, `LeaderboardViewModel`) |
| Validation | Data annotations in `Models` and `ViewModels`, `Models/PasswordRules.cs`, `ValidateQuest` and duplicate checks in controllers on the server; client side with jQuery validation (`Views/Shared/_ValidationScriptsPartial.cshtml`, loaded from `Views/Quests/Quests.cshtml` on the create/edit form and on the Course and User forms), `required` on the login form, and custom checks in the inline script in `Views/Quests/_CreateQuestForm.cshtml` (difficulty and correct option required) |
| Error handling and logging | `try/catch` around saves in the controllers with `TempData["Error"]` / `ModelState` messages, global handling in `Program.cs`, `ILogger` in `QuestsController`, `CoursesController`, `UsersController`, `AccountController`, `QuestScoringService` and the seeders |
| Authentication and roles | Cookie auth in `Program.cs`, `Controllers/AccountController.cs`, `Models/AppRoles.cs`; `[Authorize]` on quest create, edit and delete (owner or admin, checked with `CanEditQuest`), `[Authorize(Roles = AppRoles.Admin)]` on Course and User management; role-based navigation in `Views/Shared/_Layout.cshtml`; first admin created through `AccountController.SetupAdmin` (Development, localhost only) |
| Unit tests (12+, 6 positive and 6 negative) | `Subapp1-MVC/WebApp.Mvc.Tests`: 36 test cases, 8 tagged Positive and 8 tagged Negative (`[Trait("Category", ...)]`), described in `WebApp.Mvc.Tests/README.md` |
| User experience | Shared layout and navigation in `Views/Shared/_Layout.cshtml`, styling in `wwwroot/css` (`quests.css`, `site.css`), success/error banners via `TempData`, sound feedback on `Views/Quests/Play.cshtml` |


| Requirement | Subapp 2 |
|---|---|
| Does not crash | `Subapp2-API-React/Webapp.Api/Middleware/GlobalExceptionHandler.cs`, `app.UseExceptionHandler()` in `Program.cs`, `NotFoundPage.jsx` and `ErrorMessage.jsx` in React |
| Web UI with interactive forms | React pages in `Subapp2-API-React/webapp-react/src/pages` (e.g. `QuestFormPage.jsx`, `UserFormPage.jsx`, `CourseFormPage.jsx`, `PlayQuestPage.jsx`, `LoginPage.jsx`) |
| 3+ entities with CRUD | Quest, Course, User (`Subapp2-API-React/WebApp.Api/Controllers`, React pages in `Subapp2-API-React/webapp-react/src/pages`) |
| Many-to-many | User ↔ Quest through `QuestCompletion` (`Subapp2-API-React/WebApp.Api/Models`) |
| Complex domain rule | `QuestValidator` (question rules) and `QuestScoringService` (XP only on first completion) |
| Conditional rendering (2+ types) | `ProtectedRoute.jsx` (`if` and early returns), `QuestsPage.jsx` (`isAdmin && (...)` and ternary for loading / empty list) |
| Multi-parameter search and filtering | `GET /api/quests?search=&difficulty=&courseId=&status=&sort=`, filters on the Quests page |
| Async/await database access | All repository methods in `Webapp.Api/DAL` return `Task`, awaited in the controllers and services |
| DTOs | `Subapp2-API-React/WebApp.Api/Dtos` |
| Client-side API service layer | `Subapp2-API-React/webapp-react/src/services` (only `httpClient.js` calls `fetch`) |
| Validation | Data annotations + `QuestValidator` on the server, matching checks in each React form |
| Error handling and logging | `GlobalExceptionHandler`, `ILogger` in controllers and services, `ErrorMessage` in React |
| Authentication and roles | Cookie auth in `Program.cs`, `[Authorize(Roles = "Admin")]`, `ProtectedRoute` in React |
| User experience | `webapp-react/src/components` (`Layout`, `Navbar`, `Spinner`, `EmptyState`, `ConfirmDialog`), `ToastProvider.jsx`, `index.css` |
| Coding style | Controllers / DAL / Services / Dtos on the API; pages / components / hooks / services / context in React |
