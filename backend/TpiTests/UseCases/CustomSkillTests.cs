using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class CustomSkillTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public CustomSkillTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Verified_lawyer_can_manage_and_apply_skills()
    {
        using var client = _factory.CreateClient();
        var (_, profileId, _) = await _factory.BecomeVerifiedLawyerAsync(client);

        var created = await client.PostAsJsonAsync("/api/custom-skills", new
        {
            lawyerProfileId = profileId,
            name = "Red flags laborales",
            whenToUse = "Contratos de trabajo",
            instructions = "Detectá cláusulas abusivas.",
            examples = "Período de prueba excesivo",
            redFlags = "Renuncia a indemnización",
            outputFormat = "Lista numerada"
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var skill = await created.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json);
        var skillId = skill.GetProperty("id").GetGuid();
        Assert.True(skill.GetProperty("isActive").GetBoolean());

        var chat = await client.PostAsJsonAsync("/api/chats", new { title = "Con skill" });
        var chatId = (await chat.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        var applied = await client.PostAsJsonAsync("/api/custom-skills/apply", new
        {
            chatId,
            customSkillId = skillId
        });
        Assert.Equal(HttpStatusCode.OK, applied.StatusCode);

        var deactivated = await client.PostAsync($"/api/custom-skills/{skillId}/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);

        var applyInactive = await client.PostAsJsonAsync("/api/custom-skills/apply", new
        {
            chatId,
            customSkillId = skillId
        });
        Assert.Equal(HttpStatusCode.BadRequest, applyInactive.StatusCode);

        var activated = await client.PostAsync($"/api/custom-skills/{skillId}/activate", null);
        Assert.Equal(HttpStatusCode.OK, activated.StatusCode);

        var deleted = await client.DeleteAsync($"/api/custom-skills/{skillId}");
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
    }

    [Fact]
    public async Task Regular_user_cannot_manage_skills()
    {
        using var client = _factory.CreateClient();
        client.WithToken((await client.RegisterAsync()).Token);
        var created = await client.PostAsJsonAsync("/api/custom-skills", new
        {
            lawyerProfileId = Guid.NewGuid(),
            name = "Skill",
            whenToUse = "x",
            instructions = "x",
            examples = "x",
            redFlags = "x",
            outputFormat = "x"
        });
        Assert.Equal(HttpStatusCode.Forbidden, created.StatusCode);
    }
}
