# WebApp.Tests

xUnit tests for the WebApp project: 40 test cases. They are run by hand during development and are not part of any build or deployment step.

## Running

From the repository root:

```bash
dotnet test Subapp1-MVC/WebApp.Mvc.Tests                               # all 40 tests
dotnet test Subapp1-MVC/WebApp.Mvc.Tests --filter Category=Positive    # the 12 positive cases (unit and integration)
dotnet test Subapp1-MVC/WebApp.Mvc.Tests --filter Category=Negative    # the 11 negative cases (unit and integration)
```

In VS Code, the tests also appear in the Testing panel (C# Dev Kit).

## What is tested

| File | Tests | Type |
|---|---|---|
| `CodeAnswerCheckerTests.cs` | Grading of fill-in-the-blank code questions | Unit |
| `PasswordRulesTests.cs` | Password length rules | Unit |
| `QuestFormMapperTests.cs` | Turning the quest form into database rows | Unit |
| `ClaimsPrincipalExtensionTests.cs` | Numeric user ID, anonymous users, missing and invalid claims | Unit |
| `UserTests.cs` | Recent activity means online; missing or old activity means offline | Unit |
| `DifficultyRulesTests.cs` | Missing, blank and unknown difficulties rejected by normalization and XP calculation | Unit |
| `DifficultyXpTests.cs` | Legacy difficulty names (2 additional unit cases) and XP/retakes (4 SQLite cases) | Unit and service with SQLite |
| `SubmissionSaveTests.cs` | Successful submissions and failed score/streak saves | Controller with SQLite |
| `PasswordChangeTests.cs` | Current password checks and ordinary profile edits | Integration |
| `RoleAuthorizationTests.cs` | Admin/student access, ownership, role updates and self-promotion protection | Integration |
| `ContentAuthorizationTests.cs` | Signed-out restrictions, quest/course/user CRUD, ViewModel rendering and protected account fields | Integration |
| `SignInTests.cs` | Signing in, wrong passwords, and `[Authorize]` on saving quests | Integration |

## Unit-test count

The report's 12 distinct unit tests are: `CodeAnswerCheckerTests` (1 positive, 2 negative), `PasswordRulesTests` (1 positive, 1 negative), `QuestFormMapperTests` (2 positive), `ClaimsPrincipalExtensionTests` (1 positive, 1 negative), `UserTests` (1 positive, 1 negative), and `DifficultyRulesTests` (1 negative). Total: 6 positive and 6 negative, without counting different theory inputs as separate methods. Two additional legacy-difficulty theory cases bring the isolated total to 14 cases. SQLite and HTTP integration tests provide additional coverage.

## How the integration tests work

The integration tests start the real app in memory through `TestWebAppFactory`, which points the app at a temporary SQLite file. On startup the app migrates and seeds it like any other database: 8 demo students and an initial admin with the demo password, and the quests from `WebApp/Tasks`. The file is deleted when the tests finish, so the real `WebApp/SheetsDB.db` is never used.

The tests fill in forms the way a browser does: they load the page first and post back its anti-forgery token.
