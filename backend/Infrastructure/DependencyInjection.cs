using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Payments;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Infrastructure.AI;
using JurisApp.Infrastructure.Auth;
using JurisApp.Infrastructure.Files;
using JurisApp.Infrastructure.Payments;
using JurisApp.Infrastructure.Persistence;
using JurisApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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
            .AddFileStorage()
            .AddStripePayments(configuration, environment);

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
            if ((environment.IsDevelopment() || environment.IsEnvironment("Testing")) &&
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
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
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
        services.AddScoped<IChatAuditRepository, ChatAuditRepository>();

        return services;
    }

    private static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailSender, LoggingEmailSender>();

        return services;
    }

    private static IServiceCollection AddAIService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<OpenAIOptions>()
            .Bind(configuration.GetSection(OpenAIOptions.SectionName))
            .PostConfigure(options =>
            {
                if (configuration.GetValue<bool>("AI:UseMock"))
                    options.Enabled = false;

                if (string.IsNullOrWhiteSpace(options.BaseUrl))
                    options.BaseUrl = "https://api.openai.com/v1";

                if (string.IsNullOrWhiteSpace(options.Model))
                    options.Model = configuration["AI:Model"] ?? "gpt-4o";

                options.ApiKey ??= configuration["AI:OpenAI:ApiKey"];
            });

        services.AddHttpClient<OpenAIMessageClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(30, options.HttpTimeoutSeconds));

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        });

        services.AddScoped<IAIService, AIService>();
        services.AddSingleton<IAITaskExecutionQueue, AITaskExecutionQueue>();
        services.AddHostedService<AITaskExecutionHostedService>();

        return services;
    }

    private static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDocumentTextExtractor, DocumentTextExtractor>();
        return services;
    }

    private static IServiceCollection AddStripePayments(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection(StripeOptions.SectionName))
            .PostConfigure(options =>
            {
                if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
                    return;

                var useMockSetting = configuration[$"{StripeOptions.SectionName}:UseMock"];
                if (string.IsNullOrWhiteSpace(useMockSetting))
                    options.UseMock = true;
            });

        if (configuration.GetValue<bool>("Stripe:UseMock"))
            services.AddScoped<IPaymentService, MockPaymentService>();
        else
            services.AddScoped<IPaymentService, StripePaymentService>();

        return services;
    }
}
