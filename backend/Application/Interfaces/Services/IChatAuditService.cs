namespace JurisApp.Application.Interfaces.Services;

public interface IChatAuditService
{
    Task RecordAsync(Guid chatId, CancellationToken cancellationToken = default);
}
