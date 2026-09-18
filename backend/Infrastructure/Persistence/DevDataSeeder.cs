using JurisApp.Application.Interfaces.Auth;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JurisApp.Infrastructure.Persistence;

public static class DevDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, IServiceProvider serviceProvider)
    {
        await SeedPlansAsync(db);
        await SeedAdminUserAsync(db, serviceProvider);
        await EnsureAdminVerifiedAsync(db);
    }

    private static async Task SeedPlansAsync(AppDbContext db)
    {
        const string proProductId = "prod_VGKYsmDxaBryLy";
        const string proPriceId = "price_1UFnt475qtdPaMEQ4rzPOm8W";
        const string maxProductId = "prod_VGKYYbRQud8U2h";
        const string maxPriceId = "price_1UFntk75qtdPaMEQsenOZqfT";

        var existingPlans = await db.Plans.ToListAsync();
        var existingPro = existingPlans.FirstOrDefault(plan => plan.Type == PlanType.Pro);
        var existingMax = existingPlans.FirstOrDefault(plan => plan.Type == PlanType.Max);

        if (existingPro is not null || existingMax is not null)
        {
            if (existingPro is not null &&
                (existingPro.StripeProductId != proProductId || existingPro.StripePriceId != proPriceId))
                existingPro.SetStripeIds(proProductId, proPriceId);

            if (existingMax is not null &&
                (existingMax.StripeProductId != maxProductId || existingMax.StripePriceId != maxPriceId))
                existingMax.SetStripeIds(maxProductId, maxPriceId);

            await db.SaveChangesAsync();
            return;
        }

        var freePlan = new Plan(Guid.NewGuid(), "Free", PlanType.Free, 0m,
            """{"chats":5,"documents":10,"aiTasks":3}""");
        var proPlan = new Plan(Guid.NewGuid(), "Pro", PlanType.Pro, 29.99m,
            """{"chats":50,"documents":100,"aiTasks":30}""");
        proPlan.SetStripeIds(proProductId, proPriceId);

        var maxPlan = new Plan(Guid.NewGuid(), "Max", PlanType.Max, 79.99m,
            """{"chats":-1,"documents":-1,"aiTasks":-1}""");
        maxPlan.SetStripeIds(maxProductId, maxPriceId);

        var plans = new[] { freePlan, proPlan, maxPlan };

        await db.Plans.AddRangeAsync(plans);
        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(AppDbContext db, IServiceProvider serviceProvider)
    {
        const string adminEmail = "admin@jurisapp.local";

        if (await db.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
        var admin = new User(
            Guid.NewGuid(),
            "Admin",
            "JurisApp",
            adminEmail,
            passwordHasher.HashPassword("Admin123!"),
            UserRole.Admin);
        admin.VerifyEmail();

        await db.Users.AddAsync(admin);
        await db.SaveChangesAsync();
    }

    public static async Task EnsureAdminVerifiedAsync(AppDbContext db)
    {
        const string adminEmail = "admin@jurisapp.local";
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (admin is not null && !admin.IsEmailVerified)
        {
            admin.VerifyEmail();
            await db.SaveChangesAsync();
        }
    }
}
