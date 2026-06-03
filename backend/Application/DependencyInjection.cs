using JurisApp.Application.Services;
using JurisApp.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JurisApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILawyerProfileService, LawyerProfileService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ICustomSkillService, CustomSkillService>();
        services.AddScoped<IFolderService, FolderService>();
        services.AddScoped<IAITaskService, AITaskService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPlanService, PlanService>();

        return services;
    }
}
