using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class DocumentTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public DocumentTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Upload_requires_exactly_one_destination()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        using var none = BuildFileContent(null, null);
        var noneResponse = await client.PostAsync("/api/documents/upload", none);
        Assert.Equal(HttpStatusCode.BadRequest, noneResponse.StatusCode);
    }

    [Fact]
    public async Task Upload_to_chat_and_analyze_by_type()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var chat = await client.PostAsJsonAsync("/api/chats", new { title = "Docs" });
        var chatId = (await chat.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        using var file = BuildFileContent(chatId, null);
        var uploaded = await client.PostAsync("/api/documents/upload", file);
        Assert.Equal(HttpStatusCode.OK, uploaded.StatusCode);
        var document = await uploaded.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json);
        var documentId = document.GetProperty("id").GetGuid();
        Assert.Equal(chatId, document.GetProperty("chatId").GetGuid());

        var analyze = await client.PostAsJsonAsync("/api/documents/analyze", new
        {
            documentId,
            types = new[] { "Summary", "Recommendations" }
        });
        Assert.Equal(HttpStatusCode.OK, analyze.StatusCode);
        var analysis = await analyze.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json);
        Assert.False(string.IsNullOrWhiteSpace(analysis.GetProperty("summary").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(analysis.GetProperty("recommendations").GetString()));

        var again = await client.PostAsJsonAsync("/api/documents/analyze", new
        {
            documentId,
            types = new[] { "RiskAnalysis" }
        });
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        var updated = await again.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json);
        Assert.False(string.IsNullOrWhiteSpace(updated.GetProperty("risks").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(updated.GetProperty("summary").GetString()));
    }

    [Fact]
    public async Task Upload_to_folder_only_for_verified_lawyer()
    {
        using var client = _factory.CreateClient();
        var (_, profileId, _) = await _factory.BecomeVerifiedLawyerAsync(client);

        var folder = await client.PostAsJsonAsync("/api/folders", new
        {
            name = "Caso 1",
            legalContext = "Laboral"
        });
        Assert.Equal(HttpStatusCode.OK, folder.StatusCode);
        var folderId = (await folder.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        using var file = BuildFileContent(null, folderId);
        var uploaded = await client.PostAsync("/api/documents/upload", file);
        Assert.Equal(HttpStatusCode.OK, uploaded.StatusCode);

        var listed = await client.GetAsync($"/api/documents/folder/{folderId}");
        Assert.Equal(HttpStatusCode.OK, listed.StatusCode);
        Assert.NotEqual(profileId, Guid.Empty);
    }

    private static MultipartFormDataContent BuildFileContent(Guid? chatId, Guid? folderId)
    {
        var content = new MultipartFormDataContent();
        if (chatId.HasValue)
            content.Add(new StringContent(chatId.Value.ToString()), "chatId");
        if (folderId.HasValue)
            content.Add(new StringContent(folderId.Value.ToString()), "folderId");

        var file = new ByteArrayContent(Encoding.UTF8.GetBytes("Contrato de prueba para analisis."));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(file, "file", "contrato.txt");
        return content;
    }
}
