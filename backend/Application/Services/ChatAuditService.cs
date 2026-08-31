using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace JurisApp.Application.Services;

public class ChatAuditService : IChatAuditService
{
    public const string PromptVersion = "jurisapp-v1";

    private readonly IChatAuditRepository _chatAuditRepository;
    private readonly IConfiguration _configuration;

    public ChatAuditService(
        IChatAuditRepository chatAuditRepository,
        IConfiguration configuration)
    {
        _chatAuditRepository = chatAuditRepository;
        _configuration = configuration;
    }

    public async Task RecordAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        var model = UseMock() ? "mock" : ResolveModel();
        var existing = await _chatAuditRepository.GetByChatIdAsync(chatId, cancellationToken);
        if (existing is null)
        {
            await _chatAuditRepository.AddAsync(
                new ChatAudit(Guid.NewGuid(), chatId, model, PromptVersion),
                cancellationToken);
            return;
        }

        existing.Update(model, PromptVersion);
        _chatAuditRepository.Update(existing);
    }

    private string ResolveModel()
    {
        var configured = _configuration["AI:DeepSeek:Model"];
        return string.IsNullOrWhiteSpace(configured) ? "deepseek-v4-pro" : configured;
    }

    private bool UseMock()
        => string.Equals(_configuration["AI:UseMock"], "true", StringComparison.OrdinalIgnoreCase);
}
