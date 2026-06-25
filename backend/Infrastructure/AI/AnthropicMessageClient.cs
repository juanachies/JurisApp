using System.Net.Http.Json;
using System.Text.Json;
using JurisApp.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JurisApp.Infrastructure.AI;

public sealed class AnthropicMessageClient
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ILogger<AnthropicMessageClient> _logger;

    public AnthropicMessageClient(
        HttpClient httpClient,
        IOptions<ClaudeOptions> options,
        ILogger<AnthropicMessageClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsLiveMode() =>
        _options.Enabled && !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<string> SendAsync(
        string systemPrompt,
        object messages,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/v1/messages";

        var body = new
        {
            model = _options.Model,
            max_tokens = maxTokens ?? _options.MaxTokens,
            system = systemPrompt,
            messages
        };

        _logger.LogInformation(
            "Claude request → URL: {Url}, Model: {Model}",
            requestUrl,
            _options.Model);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/v1/messages", body, cancellationToken);
        }
        catch (Exception ex) when (ex is not AIServiceException)
        {
            _logger.LogError(ex, "Error de red al llamar a Claude en {Url}", requestUrl);

            if (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                throw new AIServiceException(
                    "Claude tardó demasiado en responder. El análisis segmentado puede tardar varios minutos; intentá de nuevo o reducí el tamaño del documento.",
                    ex);
            }

            throw new AIServiceException("No se pudo conectar con el servicio de IA.", ex);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Claude error → Status: {StatusCode}, URL: {Url}, Model: {Model}, Body: {Body}",
                (int)response.StatusCode,
                requestUrl,
                _options.Model,
                responseBody);

            throw new AIServiceException(
                $"El servicio de IA respondió con error {(int)response.StatusCode}.");
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Respuesta inesperada de Claude: {Body}", responseBody);
            throw new AIServiceException("La respuesta del servicio de IA no tiene el formato esperado.", ex);
        }
    }
}
