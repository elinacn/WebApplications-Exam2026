# WebApp.Tests

xUnit tests for the WebApp project: 41 test cases, of which 12 are unit tests (6 positive, 6 negative). They are run by hand during development and are not part of any build or deployment step.

## Running

From the repository root:

```bash
dotnet test WebApp.Tests                                              # all 41 tests
dotnet test WebApp.Tests --filter Type=Unit                           # the 12 unit tests
dotnet test WebApp.Tests --filter "Type=Unit&Category=Positive"       # the 6 positive unit tests
dotnet test WebApp.Tests --filter "Type=Unit&Category=Negative"       # the 6 negative unit tests
dotnet test WebApp.Tests --filter Category=Positive                   # all 10 positive tests (unit and integration)
dotnet test WebApp.Tests --filter Category=Negative                   # all 11 negative tests (unit and integration)
```

Unit tests check one class's logic on its own, with no database or web server. They are tagged `Type=Unit`. The `Category` tags (`Positive`, `Negative`) are also used on some integration tests, so combine the two filters to select only unit tests.

In VS Code, the tests also appear in the Testing panel (C# Dev Kit).

## What is tested

| File | Tests | Type |
|---|---|---|
| `CodeAnswerCheckerTests.cs` | Grading of fill-in-the-blank code questions | Unit |
| `PasswordRulesTests.cs` | Password length rules | Unit |
| `QuestFormMapperTests.cs` | Turning the quest form into database rows | Unit |
| `ClaimsPrincipalExtensionsTests.cs` | Reading the signed-in user's ID from the login claims | Unit |
| `UserTests.cs` | The "online" indicator (last seen within 5 minutes) | Unit |
| `DifficultyRulesTests.cs` | Rejecting unknown quest difficulties | Unit |
| `DifficultyXpTests.cs` | Difficulty XP rewards, retakes and legacy names | Service with SQLite |
| `SubmissionSaveTests.cs` | Successful submissions and failed score/streak saves | Controller with SQLite |
| `PasswordChangeTests.cs` | Current password checks and ordinary profile edits | Integration |
| `RoleAuthorizationTests.cs` | Admin/student access, ownership, role updates and self-promotion protection | Integration |
| `AdminSetupTests.cs` | One-time local admin setup and password validation | Integration |
| `ContentAuthorizationTests.cs` | Signed-out restrictions and signed-in quest/course CRUD | Integration |
| `SignInTests.cs` | Signing in, wrong passwords, and `[Authorize]` on saving quests | Integration |

## How the integration tests work

The integration tests start the real app in memory through `TestWebAppFactory`, which points the app at a temporary SQLite file. On startup the app migrates and seeds it like any other database: 8 demo users with the demo password, and the quests from `WebApp/Tasks`. The file is deleted when the tests finish, so the real `WebApp/SheetsDB.db` is never used.

The tests fill in forms the way a browser does: they load the page first and post back its anti-forgery token.
