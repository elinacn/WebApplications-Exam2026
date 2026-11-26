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

| Requirement | Subapp 2 |
|---|---|
| 3+ entities with CRUD | Quest, Course, User (`Subapp2-API-React/WebApp.Api/Controllers`, React pages in `Subapp2-API-React/webapp-react/src/pages`) |
| Many-to-many | User ↔ Quest through `QuestCompletion` (`Subapp2-API-React/WebApp.Api/Models`) |
| Complex domain rule | `QuestValidator` (question rules) and `QuestScoringService` (XP only on first completion) |
| Multi-parameter search | `GET /api/quests?search=&difficulty=&courseId=&status=&sort=`, filters on the Quests page |
| DTOs | `Subapp2-API-React/WebApp.Api/Dtos` |
| Client-side API service layer | `Subapp2-API-React/webapp-react/src/services` (only `httpClient.js` calls `fetch`) |
| Validation | Data annotations + `QuestValidator` on the server, matching checks in each React form |
| Error handling and logging | `GlobalExceptionHandler`, `ILogger` in controllers and services, `ErrorMessage` in React |
| Authentication and roles | Cookie auth in `Program.cs`, `[Authorize(Roles = "Admin")]`, `ProtectedRoute` in React |
