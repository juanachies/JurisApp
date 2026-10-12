using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class ProfileAndAdminUsersTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public ProfileAndAdminUsersTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task User_can_view_and_edit_profile()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var me = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        var updated = await client.PutAsJsonAsync("/api/users/me", new
        {
            firstName = "Carla",
            lastName = "Perez",
            theme = "Dark"
        });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var body = await updated.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json);
        Assert.Equal("Carla", body.GetProperty("firstName").GetString());
        Assert.Equal("Dark", body.GetProperty("theme").GetString());
    }

    [Fact]
    public async Task Edit_profile_rejects_invalid_data()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var invalid = await client.PutAsJsonAsync("/api/users/me", new
        {
            firstName = "",
            lastName = "",
            theme = "Dark"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Admin_can_list_and_deactivate_users_but_cannot_assign_unverified_lawyer()
    {
        using var client = _factory.CreateClient();
        var user = await client.RegisterAsync();

        using var adminClient = _factory.CreateClient();
        var admin = await adminClient.LoginAdminAsync();
        adminClient.WithToken(admin.Token);

        var list = await adminClient.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var forbiddenLawyer = await adminClient.PutAsJsonAsync($"/api/users/{user.User.Id}", new
        {
            role = "Lawyer"
        });
        Assert.Equal(HttpStatusCode.BadRequest, forbiddenLawyer.StatusCode);

        var deactivated = await adminClient.PutAsJsonAsync($"/api/users/{user.User.Id}", new
        {
            isActive = false
        });
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = user.User.Email,
            password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Non_admin_cannot_list_users()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);
        var list = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
    }
}
