using System.Net;
using System.Net.Http.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;
using Microsoft.AspNetCore.WebUtilities;

namespace JurisApp.TpiTests.UseCases;

public class AuthTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public AuthTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_creates_account_and_returns_jwt()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();

        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        Assert.Equal("User", auth.User.Role);

        client.WithToken(auth.Token);
        var me = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Register_rejects_invalid_data_and_duplicate_email()
    {
        using var client = _factory.CreateClient();
        var invalid = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "",
            lastName = "",
            email = "no-es-email",
            password = "123"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var first = await client.RegisterAsync("dup@jurisapp.test");
        Assert.False(string.IsNullOrWhiteSpace(first.Token));

        var dup = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Otra",
            lastName = "Persona",
            email = "dup@jurisapp.test",
            password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Conflict, dup.StatusCode);
    }

    [Fact]
    public async Task Login_succeeds_without_email_verification_and_rejects_bad_credentials()
    {
        using var client = _factory.CreateClient();
        var email = $"login.{Guid.NewGuid():N}@jurisapp.test";
        await client.RegisterAsync(email);

        var auth = await client.LoginAsync(email, "Password1!");
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));

        var bad = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "wrong-password"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
    }

    [Fact]
    public async Task Recover_access_allows_password_reset()
    {
        using var client = _factory.CreateClient();
        var email = $"reset.{Guid.NewGuid():N}@jurisapp.test";
        await client.RegisterAsync(email);

        var forgot = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email });
        Assert.Equal(HttpStatusCode.OK, forgot.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(_factory.Emails.LastResetLink));

        var uri = new Uri(_factory.Emails.LastResetLink!);
        var query = QueryHelpers.ParseQuery(uri.Query);
        var token = query["token"].ToString();

        var reset = await client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            token,
            newPassword = "NewPass123!"
        });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var login = await client.LoginAsync(email, "NewPass123!");
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
    }

    [Fact]
    public async Task Recover_access_unknown_email_still_succeeds()
    {
        using var client = _factory.CreateClient();
        var forgot = await client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email = "noexiste@jurisapp.test"
        });
        Assert.Equal(HttpStatusCode.OK, forgot.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoints_require_authentication()
    {
        using var client = _factory.CreateClient();
        var me = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }
}
