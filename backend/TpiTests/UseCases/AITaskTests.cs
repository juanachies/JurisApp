using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class AITaskTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public AITaskTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task User_can_create_validate_execute_and_cancel_task()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var chat = await client.PostAsJsonAsync("/api/chats", new { title = "Tarea" });
        var chatId = (await chat.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        var created = await client.PostAsJsonAsync("/api/ai-tasks", new
        {
            chatId,
            description = "Armar un plan para una demanda laboral."
        });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var task = await created.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json);
        var taskId = task.GetProperty("id").GetGuid();
        Assert.Equal("AwaitingApproval", task.GetProperty("status").GetString());
        Assert.True(task.GetProperty("steps").GetArrayLength() > 0);

        var cancelledPending = await client.PostAsync($"/api/ai-tasks/{taskId}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelledPending.StatusCode);

        var another = await client.PostAsJsonAsync("/api/ai-tasks", new
        {
            chatId,
            description = "Preparar intimación por despido."
        });
        var task2Id = (await another.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        var approved = await client.PostAsync($"/api/ai-tasks/{task2Id}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approved.StatusCode);

        JsonElement latest = default;
        for (var i = 0; i < 40; i++)
        {
            var loaded = await client.GetFromJsonAsync<JsonElement>($"/api/ai-tasks/{task2Id}", ApiClient.Json);
            var status = loaded.GetProperty("status").GetString();
            latest = loaded;
            if (status is "Completed" or "Cancelled" or "Failed")
                break;
            await Task.Delay(100);
        }

        Assert.Equal("Completed", latest.GetProperty("status").GetString());
    }
}

public class AITaskCancelInProgressTests : IClassFixture<SlowTaskFactory>
{
    private readonly SlowTaskFactory _factory;

    public AITaskCancelInProgressTests(SlowTaskFactory factory) => _factory = factory;

    [Fact]
    public async Task User_can_cancel_task_while_running()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var chat = await client.PostAsJsonAsync("/api/chats", new { title = "Cancelable" });
        var chatId = (await chat.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        var created = await client.PostAsJsonAsync("/api/ai-tasks", new
        {
            chatId,
            description = "Plan largo para cancelar."
        });
        var taskId = (await created.Content.ReadFromJsonAsync<JsonElement>(ApiClient.Json)).GetProperty("id").GetGuid();

        var approve = client.PostAsync($"/api/ai-tasks/{taskId}/approve", null);
        await Task.Delay(150);
        var cancel = await client.PostAsync($"/api/ai-tasks/{taskId}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);
        await approve;

        JsonElement latest = default;
        for (var i = 0; i < 40; i++)
        {
            latest = await client.GetFromJsonAsync<JsonElement>($"/api/ai-tasks/{taskId}", ApiClient.Json);
            var status = latest.GetProperty("status").GetString();
            if (status is "Cancelled" or "Completed")
                break;
            await Task.Delay(100);
        }

        Assert.Equal("Cancelled", latest.GetProperty("status").GetString());
    }
}
