using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JurisApp.TpiTests.Fixtures;

namespace JurisApp.TpiTests.Helpers;

public sealed class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class AuthUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class AuthPayload
{
    public string Token { get; set; } = string.Empty;
    public AuthUser User { get; set; } = null!;
}

public sealed class PlanPayload
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string LimitsJson { get; set; } = string.Empty;
}

public static class ApiClient
{
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static HttpClient WithToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<AuthPayload> RegisterAsync(
        this HttpClient client,
        string? email = null,
        string password = "Password1!",
        string firstName = "Ana",
        string lastName = "Test")
    {
        email ??= $"user.{Guid.NewGuid():N}@jurisapp.test";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName,
            lastName,
            email,
            password
        });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthPayload>(Json);
        return payload!;
    }

    public static async Task<AuthPayload> LoginAsync(this HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthPayload>(Json);
        return payload!;
    }

    public static async Task<AuthPayload> LoginAdminAsync(this HttpClient client)
        => await client.LoginAsync("admin@jurisapp.local", "Admin123!");

    public static async Task<List<PlanPayload>> GetPlansAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/api/plans");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<PlanPayload>>(Json))!;
    }

    public static async Task SimulatePurchaseAsync(this HttpClient client, Guid planId)
    {
        var response = await client.PostAsJsonAsync("/api/billing/simulate-purchase", new { planId });
        response.EnsureSuccessStatusCode();
    }

    public static async Task<JsonElement> RequestLawyerVerificationAsync(
        this HttpClient client,
        string licenseNumber = "T-12345")
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(licenseNumber), "licenseNumber");
        content.Add(new StringContent("Colegio de Abogados"), "barAssociation");
        content.Add(new StringContent("Buenos Aires"), "province");
        content.Add(new StringContent("Civil"), "specialty");
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(file, "licenseDocument", "matricula.png");

        var response = await client.PostAsync("/api/lawyer-profiles", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }

    public static async Task<(HttpClient LawyerClient, Guid LawyerProfileId, AuthPayload Auth)> BecomeVerifiedLawyerAsync(
        this JurisAppApiFactory factory,
        HttpClient client)
    {
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var plans = await client.GetPlansAsync();
        var pro = plans.First(p => p.Type == "Pro");
        await client.SimulatePurchaseAsync(pro.Id);

        var profile = await client.RequestLawyerVerificationAsync();
        var profileId = profile.GetProperty("id").GetGuid();

        using var adminClient = factory.CreateClient();
        var admin = await adminClient.LoginAdminAsync();
        adminClient.WithToken(admin.Token);
        var approve = await adminClient.PostAsync($"/api/lawyer-profiles/requests/{profileId}/approve", null);
        approve.EnsureSuccessStatusCode();

        var relogin = await client.LoginAsync(auth.User.Email, "Password1!");
        client.WithToken(relogin.Token);
        return (client, profileId, relogin);
    }

    public static async Task<ApiError?> ReadErrorAsync(this HttpResponseMessage response)
        => await response.Content.ReadFromJsonAsync<ApiError>(Json);
}
