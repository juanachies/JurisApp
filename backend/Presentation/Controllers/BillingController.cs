using JurisApp.Application.DTOs.Billing;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Payments;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JurisApp.Presentation.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IPlanService _planService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWebHostEnvironment _environment;

    public BillingController(
        IPaymentService paymentService,
        IPlanService planService,
        ICurrentUserService currentUserService,
        IWebHostEnvironment environment)
    {
        _paymentService = paymentService;
        _planService = planService;
        _currentUserService = currentUserService;
        _environment = environment;
    }

    [HttpPost("create-checkout-session")]
    [Authorize]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _paymentService.CreateCheckoutSessionAsync(userId, request.PlanId, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new CreateCheckoutSessionResponse { Url = result.Value! });
    }

    [HttpPost("simulate-purchase")]
    [Authorize]
    public async Task<IActionResult> SimulatePurchase(
        [FromBody] CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"))
        {
            return BadRequest(new
            {
                Code = "Validation",
                Message = "Simulate purchase is only available in Development."
            });
        }

        var userId = _currentUserService.UserId!.Value;
        var result = await _planService.ActivatePaidSubscriptionAsync(
            userId,
            request.PlanId,
            $"mock_cus_{userId:N}",
            $"mock_sub_{Guid.NewGuid():N}",
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var parsed = await _paymentService.ParseCheckoutCompletedWebhookAsync(json, signature, cancellationToken);
        if (!parsed.IsSuccess)
            return parsed.ToActionResult();

        if (parsed.Value is null)
            return Ok();

        var notification = parsed.Value;
        var result = await _planService.ActivatePaidSubscriptionAsync(
            notification.UserId,
            notification.PlanId,
            notification.StripeCustomerId,
            notification.StripeSubscriptionId,
            cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok();
    }
}
