using System.Text.Json;
using JurisApp.Application.Interfaces.Segmentation;
using JurisApp.Application.Models.Segmentation;
using Microsoft.Extensions.Logging;

namespace JurisApp.Infrastructure.Segmentation;

public sealed class JsonDocumentSegmentationCatalog : IDocumentSegmentationCatalog
{
  private const string ResourceName = "JurisApp.Application.Resources.document-segmentations.json";

  private static readonly string[] ExpectedCategoryKeys =
  [
    "contrato_servicios",
    "contrato_laboral",
    "contrato_alquiler",
    "carta_documento",
    "demanda",
    "contestacion_demanda",
    "convenio_pago",
    "reclamo_deuda",
    "defensa_consumidor",
    "despido",
    "accidente_laboral",
    "sucesion",
    "sociedad_poder_acta",
    "documento_administrativo",
    "consulta_juridica_general",
    "pregunta_libre"
  ];

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  private readonly IReadOnlyDictionary<string, DocumentCategoryDefinition> _categories;
  private readonly ILogger<JsonDocumentSegmentationCatalog> _logger;

  public JsonDocumentSegmentationCatalog(ILogger<JsonDocumentSegmentationCatalog> logger)
  {
    _logger = logger;
    _categories = LoadCatalog();
    ValidateCatalog();
  }

  public Task<DocumentCategoryDefinition?> GetByCategoryKeyAsync(
    string categoryKey,
    CancellationToken cancellationToken = default)
  {
    _categories.TryGetValue(categoryKey, out var definition);
    return Task.FromResult(definition);
  }

  public Task<IReadOnlyList<DocumentCategoryDefinition>> GetAllAsync(
    CancellationToken cancellationToken = default)
  {
    IReadOnlyList<DocumentCategoryDefinition> all = _categories.Values.ToList();
    return Task.FromResult(all);
  }

  public Task<IReadOnlyCollection<string>> GetCategoryKeysAsync(
    CancellationToken cancellationToken = default)
  {
    IReadOnlyCollection<string> keys = _categories.Keys.ToList();
    return Task.FromResult(keys);
  }

  public bool IsValidCategoryKey(string categoryKey) =>
    _categories.ContainsKey(categoryKey);

  private IReadOnlyDictionary<string, DocumentCategoryDefinition> LoadCatalog()
  {
    var assembly = typeof(DocumentSegmentationCatalogRoot).Assembly;
    using var stream = assembly.GetManifestResourceStream(ResourceName)
      ?? throw new InvalidOperationException($"No se encontró el recurso embebido {ResourceName}.");

    var root = JsonSerializer.Deserialize<DocumentSegmentationCatalogRoot>(stream, JsonOptions)
      ?? throw new InvalidOperationException("El catálogo de segmentaciones está vacío o es inválido.");

    return root.DocumentSegmentations;
  }

  private void ValidateCatalog()
  {
    var missing = ExpectedCategoryKeys.Where(key => !_categories.ContainsKey(key)).ToList();
    if (missing.Count > 0)
    {
      _logger.LogWarning(
        "El catálogo de segmentaciones no incluye categorías esperadas: {MissingKeys}",
        string.Join(", ", missing));
    }
  }
}
