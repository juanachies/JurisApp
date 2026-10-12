using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class FolderAndLawyerTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public FolderAndLawyerTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Lawyer_upgrade_requires_pro_plan_and_license_file()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        using var noFile = new MultipartFormDataContent
        {
            { new StringContent("T-1"), "licenseNumber" },
            { new StringContent("Colegio"), "barAssociation" },
            { new StringContent("CABA"), "province" },
            { new StringContent("Penal"), "specialty" }
        };
        var withoutPro = await client.PostAsync("/api/lawyer-profiles", noFile);
        Assert.Equal(HttpStatusCode.BadRequest, withoutPro.StatusCode);

        var plans = await client.GetPlansAsync();
        await client.SimulatePurchaseAsync(plans.First(p => p.Type == "Pro").Id);

        var stillNoFile = await client.PostAsync("/api/lawyer-profiles", noFile);
        Assert.Equal(HttpStatusCode.BadRequest, stillNoFile.StatusCode);

        var created = await client.RequestLawyerVerificationAsync();
        Assert.Equal("Pending", created.GetProperty("verificationStatus").GetString());
    }

    [Fact]
    public async Task Admin_can_approve_and_reject_verification()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);
        var plans = await client.GetPlansAsync();
        await client.SimulatePurchaseAsync(plans.First(p => p.Type == "Pro").Id);
        var profile = await client.RequestLawyerVerificationAsync("T-999");
        var profileId = profile.GetProperty("id").GetGuid();

        using var adminClient = _factory.CreateClient();
        adminClient.WithToken((await adminClient.LoginAdminAsync()).Token);

        var listed = await adminClient.GetAsync("/api/lawyer-profiles/requests?status=Pending");
        Assert.Equal(HttpStatusCode.OK, listed.StatusCode);

        var rejected = await adminClient.PostAsJsonAsync($"/api/lawyer-profiles/requests/{profileId}/reject", new
        {
            reason = "Documentación ilegible"
        });
        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);

        var resubmitted = await client.RequestLawyerVerificationAsync("T-999");
        var newId = resubmitted.GetProperty("id").GetGuid();
        var approved = await adminClient.PostAsync($"/api/lawyer-profiles/requests/{newId}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approved.StatusCode);
    }

    [Fact]
    public async Task Only_verified_lawyer_can_manage_folders()
    {
        using var regular = _factory.CreateClient();
        regular.WithToken((await regular.RegisterAsync()).Token);
        var forbidden = await regular.PostAsJsonAsync("/api/folders", new { name = "Caso" });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var lawyerClient = _factory.CreateClient();
        await _factory.BecomeVerifiedLawyerAsync(lawyerClient);

        var created = await lawyerClient.PostAsJsonAsync("/api/folders", new
        {
            name = "Expediente A",
            legalContext = "Civil"
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var folderId = (await created.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        var deleted = await lawyerClient.DeleteAsync($"/api/folders/{folderId}");
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
    }
}
