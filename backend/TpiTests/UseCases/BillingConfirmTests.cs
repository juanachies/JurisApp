using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.Infrastructure.Payments;
using JurisApp.TpiTests.Fixtures;
using JurisApp.TpiTests.Helpers;

namespace JurisApp.TpiTests.UseCases;

public class BillingConfirmTests : IClassFixture<JurisAppApiFactory>
{
    private readonly JurisAppApiFactory _factory;

    public BillingConfirmTests(JurisAppApiFactory factory) => _factory = factory;

    [Fact]
    public void Checkout_success_url_includes_the_session_placeholder()
    {
        Assert.Equal(
            "https://jurisapp-nine.vercel.app/billing/success?session_id={CHECKOUT_SESSION_ID}",
            StripePaymentService.AppendCheckoutSessionId("https://jurisapp-nine.vercel.app/billing/success"));

        Assert.Equal(
            "https://example.com/ok?foo=1&session_id={CHECKOUT_SESSION_ID}",
            StripePaymentService.AppendCheckoutSessionId("https://example.com/ok?foo=1"));

        Assert.Equal(
            "https://example.com/ok?session_id={CHECKOUT_SESSION_ID}",
            StripePaymentService.AppendCheckoutSessionId(
                "https://example.com/ok?session_id={CHECKOUT_SESSION_ID}"));
    }

    [Fact]
    public async Task Confirm_without_a_paid_checkout_leaves_the_free_plan()
    {
        using var client = _factory.CreateClient();
        var auth = await client.RegisterAsync();
        client.WithToken(auth.Token);

        var response = await client.PostAsJsonAsync("/api/billing/confirm", new { sessionId = (string?)null });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var current = await client.GetFromJsonAsync<JsonElement>("/api/plans/current", ApiClient.Json);
        Assert.Equal("Free", current.GetProperty("planType").GetString());
    }
}
