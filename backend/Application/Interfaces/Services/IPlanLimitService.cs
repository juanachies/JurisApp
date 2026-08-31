using JurisApp.Application.Common;

namespace JurisApp.Application.Interfaces.Services;

public interface IPlanLimitService
{
    Task<Result> EnsureCanCreateChatAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> EnsureCanUploadDocumentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> EnsureCanCreateAiTaskAsync(Guid userId, CancellationToken cancellationToken = default);
}
