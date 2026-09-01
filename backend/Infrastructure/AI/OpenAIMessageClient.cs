using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JurisApp.Infrastructure.AI;

public sealed class OpenAIMessageClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIMessageClient> _logger;

    public OpenAIMessageClient(
        HttpClient httpClient,
        IOptions<OpenAIOptions> options,
        ILogger<OpenAIMessageClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsLiveMode() =>
        _options.Enabled && !string.IsNullOrWhiteSpace(_options.ApiKey);

    public string Model =>
        string.IsNullOrWhiteSpace(_options.Model) ? "gpt-4o" : _options.Model;

    public async Task<string> SendAsync(
        string systemPrompt,
        object messages,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/chat/completions";

        var body = new
        {
            model = Model,
            max_tokens = maxTokens ?? _options.MaxTokens,
            messages = BuildMessages(systemPrompt, messages)
        };

        _logger.LogInformation(
            "OpenAI request → URL: {Url}, Model: {Model}",
            requestUrl,
            Model);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("chat/completions", body, cancellationToken);
        }
        catch (Exception ex) when (ex is not AIServiceException)
        {
            _logger.LogError(ex, "Error de red al llamar a OpenAI en {Url}", requestUrl);

            if (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                throw new AIServiceException(
                    "OpenAI tardó demasiado en responder. Intentá de nuevo o reducí el tamaño del documento.",
                    ex);
            }

            throw new AIServiceException("No se pudo conectar con el servicio de IA.", ex);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI error → Status: {StatusCode}, URL: {Url}, Model: {Model}, Body: {Body}",
                (int)response.StatusCode,
                requestUrl,
                Model,
                responseBody);

            throw new AIServiceException(
                $"El servicio de IA respondió con error {(int)response.StatusCode}.");
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Respuesta inesperada de OpenAI: {Body}", responseBody);
            throw new AIServiceException("La respuesta del servicio de IA no tiene el formato esperado.", ex);
        }
    }

    private static List<object> BuildMessages(string systemPrompt, object messages)
    {
        var payload = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (messages is IEnumerable<object> existing)
            payload.AddRange(existing);
        else
            payload.Add(messages);

        return payload;
    }
}
