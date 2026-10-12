using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class PlansTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public PlansTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public async Task User_can_list_subscribe_change_and_cancel_plans()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var plans = await client.GetPlansAsync();
        Assert.Contains(plans, p => p.Type == "Free");
        Assert.Contains(plans, p => p.Type == "Pro");

        var free = plans.First(p => p.Type == "Free");
        var pro = plans.First(p => p.Type == "Pro");
        var max = plans.First(p => p.Type == "Max");

        var current = await client.GetFromJsonAsync<JsonElement>("/api/plans/current", ApiClient.Json);
        Assert.True(current.GetProperty("hasActiveSubscription").GetBoolean());
        Assert.Equal(free.Id, current.GetProperty("planId").GetGuid());

        var change = await client.PostAsync($"/api/plans/{pro.Id}/change", null);
        Assert.Equal(HttpStatusCode.OK, change.StatusCode);

        var changeMax = await client.PostAsync($"/api/plans/{max.Id}/change", null);
        Assert.Equal(HttpStatusCode.OK, changeMax.StatusCode);

        var cancel = await client.PostAsync("/api/plans/current/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);

        var afterCancel = await client.GetFromJsonAsync<JsonElement>("/api/plans/current", ApiClient.Json);
        Assert.False(afterCancel.GetProperty("hasActiveSubscription").GetBoolean());

        var subscribe = await client.PostAsync($"/api/plans/{free.Id}/subscribe", null);
        Assert.Equal(HttpStatusCode.OK, subscribe.StatusCode);
    }

    [Fact]
    public async Task Subscribe_conflict_and_cancel_without_active_are_handled()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);
        var free = (await client.GetPlansAsync()).First(p => p.Type == "Free");

        var alreadySubscribed = await client.PostAsync($"/api/plans/{free.Id}/subscribe", null);
        Assert.Equal(HttpStatusCode.Conflict, alreadySubscribed.StatusCode);

        var cancel = await client.PostAsync("/api/plans/current/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);

        var cancelAgain = await client.PostAsync("/api/plans/current/cancel", null);
        Assert.Equal(HttpStatusCode.NotFound, cancelAgain.StatusCode);

        var resubscribe = await client.PostAsync($"/api/plans/{free.Id}/subscribe", null);
        Assert.Equal(HttpStatusCode.OK, resubscribe.StatusCode);

        var second = await client.PostAsync($"/api/plans/{free.Id}/subscribe", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Admin_can_crud_plans_and_cannot_delete_plan_with_subscriptions()
    {
        using var client = _factory.CreateClient();
        await client.RegisterAsync();
        var free = (await client.GetPlansAsync()).First(p => p.Type == "Free");

        using var adminClient = _factory.CreateClient();
        adminClient.WithToken((await adminClient.LoginAdminAsync()).Token);

        var created = await adminClient.PostAsJsonAsync("/api/plans", new
        {
            name = "Extra",
            type = "Pro",
            price = 10,
            limitsJson = """{"chats":1,"documents":1,"aiTasks":1}"""
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var createdPlan = await created.Content.ReadFromJsonAsync<PlanPayload>(ApiClient.Json);

        var updated = await adminClient.PutAsJsonAsync($"/api/plans/{createdPlan!.Id}", new
        {
            name = "Extra Plus",
            type = "Pro",
            price = 15,
            limitsJson = """{"chats":2,"documents":2,"aiTasks":2}"""
        });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var deletedExtra = await adminClient.DeleteAsync($"/api/plans/{createdPlan.Id}");
        Assert.Equal(HttpStatusCode.OK, deletedExtra.StatusCode);

        var deleteFree = await adminClient.DeleteAsync($"/api/plans/{free.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteFree.StatusCode);
    }
}
