namespace JurisApp.Infrastructure.AI;

internal static class ClaudeDebugResponseWriter
{
    public static async Task SaveSegmentedAnalysisResponseAsync(
        string rawResponse,
        string categoryKey,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "debug", "claude-responses");
        Directory.CreateDirectory(directory);

        var safeCategory = string.Join(
            "_",
            categoryKey.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}_{safeCategory}.txt";
        var filePath = Path.Combine(directory, fileName);

        await File.WriteAllTextAsync(filePath, rawResponse, cancellationToken);
    }
}
