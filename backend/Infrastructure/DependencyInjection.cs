using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Infrastructure.AI;
using JurisApp.Infrastructure.Auth;
using JurisApp.Infrastructure.Files;
using JurisApp.Infrastructure.Persistence;
using JurisApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JurisApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddDatabase(configuration, environment)
            .AddRepositories()
            .AddAuthServices()
            .AddAIService(configuration)
            .AddFileStorage();

        return services;
    }

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        var provider = configuration["Database:Provider"];

        services.AddDbContext<AppDbContext>(options =>
        {
            if (environment.IsDevelopment() &&
                string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILawyerProfileRepository, LawyerProfileRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IDocumentAnalysisRepository, DocumentAnalysisRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<ICustomSkillRepository, CustomSkillRepository>();
        services.AddScoped<IAITaskRepository, AITaskRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }

    private static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    private static IServiceCollection AddAIService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["AI:Claude:ApiKey"];
        var useMock = configuration.GetValue<bool>("AI:UseMock");

        if (useMock || string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddScoped<IAIService, MockAIService>();
            return services;
        }

        services.AddHttpClient<IAIService, AIService>(client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com/v1/");
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        });

        return services;
    }

    private static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        return services;
    }
}
