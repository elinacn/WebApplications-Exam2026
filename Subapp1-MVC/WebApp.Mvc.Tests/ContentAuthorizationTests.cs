using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Models;

namespace WebApp.Tests;

public class ContentAuthorizationTests
{
    [Fact]
    public async Task Anonymous_visitors_cannot_open_or_submit_any_content_management_action()
    {
        using var factory = new TestWebAppFactory();
        using var browser = factory.CreateBrowser();
        var token = await TestWebAppFactory.GetAntiForgeryTokenAsync(browser, "/Account/Login");
        var pages = new[] { "/Quests/Quests?section=create", "/Quests/Edit/1", "/Quests/Delete/1",
            "/Courses/Create", "/Courses/Edit/1", "/Courses/Delete/1" };
        foreach (var url in pages)
        {
            var response = await browser.GetAsync(url);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString ?? "");
        }
        foreach (var url in new[] { "/Quests/Create", "/Quests/Edit/1", "/Quests/Delete/1",
            "/Courses/Create", "/Courses/Edit/1", "/Courses/Delete/1" })
        {
            var response = await browser.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>
            { ["__RequestVerificationToken"] = token }));
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString ?? "");
        }
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SheetsDbContext>();
        Assert.NotEmpty(await db.Quests.ToListAsync());
        var quests = await browser.GetStringAsync("/Quests/Quests");
        Assert.Contains("Sign in to create", quests);
        Assert.DoesNotContain("/Quests/Edit/", quests);
        Assert.DoesNotContain("/Quests/Delete/", quests);
        var courses = await browser.GetStringAsync("/Courses");
        Assert.Contains("Admin sign-in", courses);
    }

    [Fact]
    public async Task Admin_can_create_edit_and_delete_a_course()
    {
        using var factory = new TestWebAppFactory();
        using var browser = factory.CreateBrowser();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SheetsDbContext>();
            (await db.Users.SingleAsync(u => u.Username == "anna")).IsAdmin = true;
            await db.SaveChangesAsync();
        }
        await TestWebAppFactory.SignInAsync(browser, "anna", UserSeeder.DemoPassword);
        var form = new Dictionary<string, string> { ["Code"] = "AUTH-101", ["Title"] = "Permission test course" };
        var invalid = await PostAsync(browser, "/Courses/Create", "/Courses/Create", new() { ["Code"] = "AUTH-101" });
        Assert.Equal(HttpStatusCode.OK, invalid.StatusCode);
        Assert.Contains("Course title is required.", await invalid.Content.ReadAsStringAsync());

        var created = await PostAsync(browser, "/Courses/Create", "/Courses/Create", form);
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        var duplicate = await PostAsync(browser, "/Courses/Create", "/Courses/Create", form);
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        Assert.Contains("A course with this code already exists.", await duplicate.Content.ReadAsStringAsync());
        int id;
        using (var scope = factory.Services.CreateScope())
            id = (await scope.ServiceProvider.GetRequiredService<SheetsDbContext>().Courses.SingleAsync(c => c.Code == "AUTH-101")).Id;
        Assert.Contains("AUTH-101", await browser.GetStringAsync("/Courses"));
        Assert.Contains("Permission test course", await browser.GetStringAsync($"/Courses/Details/{id}"));
        Assert.Contains("AUTH-101", await browser.GetStringAsync($"/Courses/Edit/{id}"));
        Assert.Contains("AUTH-101", await browser.GetStringAsync($"/Courses/Delete/{id}"));
        form["Id"] = id.ToString();
        form["Title"] = "Updated permission course";
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, $"/Courses/Edit/{id}", $"/Courses/Edit/{id}", form)).StatusCode);
        Assert.Contains("Updated permission course", await browser.GetStringAsync($"/Courses/Details/{id}"));
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, $"/Courses/Delete/{id}", $"/Courses/Delete/{id}", [])).StatusCode);
        using var verify = factory.Services.CreateScope();
        Assert.False(await verify.ServiceProvider.GetRequiredService<SheetsDbContext>().Courses.AnyAsync(c => c.Id == id));
    }

    [Fact]
    public async Task Admin_can_create_edit_and_delete_a_quest()
    {
        using var factory = new TestWebAppFactory();
        using var browser = factory.CreateBrowser();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SheetsDbContext>();
            (await db.Users.SingleAsync(u => u.Username == "anna")).IsAdmin = true;
            await db.SaveChangesAsync();
        }
        await TestWebAppFactory.SignInAsync(browser, "anna", UserSeeder.DemoPassword);
        var form = new Dictionary<string, string>
        {
            ["NewQuest.Title"] = "Permission test quest", ["NewQuest.Difficulty"] = "Beginner",
            ["NewQuest.Questions[0].Type"] = "freeText", ["NewQuest.Questions[0].Question"] = "Say yes",
            ["NewQuest.Questions[0].Points"] = "1", ["NewQuest.Questions[0].AcceptedAnswers[0]"] = "yes"
        };
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, "/Quests/Quests?section=create", "/Quests/Create", form)).StatusCode);
        int id;
        using (var scope = factory.Services.CreateScope())
            id = (await scope.ServiceProvider.GetRequiredService<SheetsDbContext>().Quests.SingleAsync(q => q.Title == "Permission test quest")).Id;
        var signedInLibrary = await browser.GetStringAsync("/Quests/Quests");
        Assert.Contains($"/Quests/Edit/{id}", signedInLibrary);
        form["NewQuest.Id"] = id.ToString();
        form["NewQuest.Title"] = "Updated permission quest";
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, $"/Quests/Edit/{id}", $"/Quests/Edit/{id}", form)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, $"/Quests/Delete/{id}", $"/Quests/Delete/{id}", [])).StatusCode);
        using var verify = factory.Services.CreateScope();
        Assert.False(await verify.ServiceProvider.GetRequiredService<SheetsDbContext>().Quests.AnyAsync(q => q.Id == id));
    }

    [Fact]
    public async Task Admin_can_manage_users_without_exposing_hashes_or_binding_progress()
    {
        using var factory = new TestWebAppFactory();
        using var browser = factory.CreateBrowser();
        await TestWebAppFactory.SignInAsync(browser, "admin", UserSeeder.DemoPassword);
        var form = new Dictionary<string, string>
        {
            ["Username"] = "viewmodeluser", ["Name"] = "View Model User", ["StudyProgram"] = "Web",
            ["password"] = "testpassword123", ["Id"] = "99999", ["Points"] = "999",
            ["QuizzesCompleted"] = "999", ["PasswordHash"] = "forged hash"
        };
        var invalid = await PostAsync(browser, "/Users/Create", "/Users/Create", new() { ["Username"] = "viewmodeluser" });
        Assert.Equal(HttpStatusCode.OK, invalid.StatusCode);
        Assert.Contains("Name is required.", await invalid.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, "/Users/Create", "/Users/Create", form)).StatusCode);
        var duplicate = await PostAsync(browser, "/Users/Create", "/Users/Create", form);
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        Assert.Contains("This username is already taken.", await duplicate.Content.ReadAsStringAsync());

        int id;
        string hash;
        using (var scope = factory.Services.CreateScope())
        {
            var user = await scope.ServiceProvider.GetRequiredService<SheetsDbContext>().Users.SingleAsync(u => u.Username == "viewmodeluser");
            id = user.Id;
            Assert.NotEqual(99999, id);
            Assert.Equal(0, user.Points);
            Assert.Equal(0, user.QuizzesCompleted);
            hash = Assert.IsType<string>(user.PasswordHash);
            Assert.NotEqual("forged hash", hash);
            Assert.NotEqual(form["password"], hash);
        }
        Assert.Contains("View Model User", await browser.GetStringAsync("/Users"));
        var details = await browser.GetStringAsync($"/Users/Details/{id}");
        Assert.Contains("Set", details);
        Assert.DoesNotContain(hash, details);
        Assert.Contains("viewmodeluser", await browser.GetStringAsync($"/Users/Edit/{id}"));
        Assert.Contains("viewmodeluser", await browser.GetStringAsync($"/Users/Delete/{id}"));

        form["Id"] = id.ToString();
        form["Name"] = "Updated View Model User";
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, $"/Users/Edit/{id}", $"/Users/Edit/{id}", form)).StatusCode);
        Assert.Contains("Updated View Model User", await browser.GetStringAsync($"/Users/Details/{id}"));
        using (var scope = factory.Services.CreateScope())
        {
            var user = await scope.ServiceProvider.GetRequiredService<SheetsDbContext>().Users.FindAsync(id);
            Assert.NotNull(user);
            Assert.Equal(hash, user.PasswordHash);
            Assert.Equal(0, user.Points);
            Assert.Equal(0, user.QuizzesCompleted);
        }
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(browser, $"/Users/Delete/{id}", $"/Users/Delete/{id}", [])).StatusCode);
        using var verify = factory.Services.CreateScope();
        Assert.False(await verify.ServiceProvider.GetRequiredService<SheetsDbContext>().Users.AnyAsync(u => u.Id == id));
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient browser, string formUrl, string actionUrl, Dictionary<string, string> data)
    {
        data["__RequestVerificationToken"] = await TestWebAppFactory.GetAntiForgeryTokenAsync(browser, formUrl);
        return await browser.PostAsync(actionUrl, new FormUrlEncodedContent(data));
    }
}
