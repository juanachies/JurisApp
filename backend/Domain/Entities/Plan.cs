using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class Plan : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public PlanType Type { get; private set; }
    public decimal Price { get; private set; }
    public string LimitsJson { get; private set; } = string.Empty;
    public string? StripeProductId { get; private set; }
    public string? StripePriceId { get; private set; }

    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

    protected Plan() { }

    public Plan(Guid id, string name, PlanType type, decimal price, string limitsJson)
        : base(id)
    {
        Name = name;
        Type = type;
        Price = price;
        LimitsJson = limitsJson;
    }

    public void SetStripeIds(string? productId, string? priceId)
    {
        StripeProductId = productId;
        StripePriceId = priceId;
        Touch();
    }

    public void Update(string name, PlanType type, decimal price, string limitsJson)
    {
        Name = name;
        Type = type;
        Price = price;
        LimitsJson = limitsJson;
        Touch();
    }
}
