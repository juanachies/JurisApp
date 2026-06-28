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
        if (await db.Plans.AnyAsync())
            return;

        var plans = new[]
        {
            new Plan(Guid.NewGuid(), "Free", PlanType.Free, 0m,
                """{"chats":5,"documents":10,"aiTasks":3}"""),
            new Plan(Guid.NewGuid(), "Pro", PlanType.Pro, 29.99m,
                """{"chats":50,"documents":100,"aiTasks":30}"""),
            new Plan(Guid.NewGuid(), "Max", PlanType.Max, 79.99m,
                """{"chats":-1,"documents":-1,"aiTasks":-1}""")
        };

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
