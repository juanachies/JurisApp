using System.Threading.Channels;
using JurisApp.Application.Interfaces.Services;

namespace JurisApp.Infrastructure.AI;

public sealed class AITaskExecutionQueue : IAITaskExecutionQueue
{
    private readonly Channel<(Guid UserId, Guid TaskId)> _channel =
        Channel.CreateUnbounded<(Guid UserId, Guid TaskId)>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync((userId, taskId), cancellationToken);

    public IAsyncEnumerable<(Guid UserId, Guid TaskId)> ReadAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}
