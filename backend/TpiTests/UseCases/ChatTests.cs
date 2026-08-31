using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class ChatTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public ChatTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task User_can_create_continue_and_delete_chat()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var created = await client.PostAsJsonAsync("/api/chats", new { title = "Consulta laboral" });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var chat = await created.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json);
        var chatId = chat.GetProperty("id").GetGuid();

        var sent = await client.PostAsJsonAsync($"/api/chats/{chatId}/messages", new
        {
            content = "Hola, necesito un resumen."
        });
        Assert.Equal(HttpStatusCode.OK, sent.StatusCode);

        var loaded = await client.GetFromJsonAsync<JsonElement>($"/api/chats/{chatId}", ApiClient.Json);
        var messages = loaded.GetProperty("messages").EnumerateArray().ToList();
        Assert.Equal(2, messages.Count);
        Assert.Equal("User", messages[0].GetProperty("role").GetString());
        Assert.Equal("Hola, necesito un resumen.", messages[0].GetProperty("content").GetString());
        Assert.Equal("Assistant", messages[1].GetProperty("role").GetString());

        var deleted = await client.DeleteAsync($"/api/chats/{chatId}");
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);

        var missing = await client.GetAsync($"/api/chats/{chatId}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Create_chat_requires_title_and_foreign_chat_is_unauthorized()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var invalid = await client.PostAsJsonAsync("/api/chats", new { title = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var created = await client.PostAsJsonAsync("/api/chats", new { title = "Mio" });
        var chatId = (await created.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        using var other = _factory.CreateClient();
        other.WithToken((await other.RegisterAsync()).Token);
        var access = await other.GetAsync($"/api/chats/{chatId}");
        Assert.Equal(HttpStatusCode.Unauthorized, access.StatusCode);
    }
}
