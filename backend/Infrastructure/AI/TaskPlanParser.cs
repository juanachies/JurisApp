using System.Text.Json;
using System.Text.RegularExpressions;
using JurisApp.Application.Interfaces.AI;

namespace JurisApp.Infrastructure.AI;

internal static class TaskPlanParser
{
    public static StructuredTaskPlan Parse(string raw, string description)
    {
        var json = JsonResponseHelper.StripMarkdownJson(raw);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var objective = JsonResponseHelper.ReadString(root, "objective");
            var summary = JsonResponseHelper.ReadString(root, "summary");
            var steps = new List<StructuredTaskStep>();

            if (root.TryGetProperty("steps", out var stepsElement) &&
                stepsElement.ValueKind == JsonValueKind.Array)
            {
                var order = 1;
                foreach (var step in stepsElement.EnumerateArray())
                {
                    steps.Add(new StructuredTaskStep
                    {
                        Order = step.TryGetProperty("order", out var o) && o.TryGetInt32(out var n) ? n : order,
                        Title = JsonResponseHelper.ReadString(step, "title"),
                        Description = JsonResponseHelper.ReadString(step, "description")
                    });
                    order++;
                }
            }

            if (steps.Count > 0)
            {
                return new StructuredTaskPlan
                {
                    Objective = string.IsNullOrWhiteSpace(objective) ? description : objective,
                    Summary = summary,
                    Steps = steps.OrderBy(s => s.Order).ToList()
                };
            }
        }
        catch
        {
            // fallback below
        }

        return ParseNumberedListFallback(raw, description);
    }

    private static StructuredTaskPlan ParseNumberedListFallback(string raw, string description)
    {
        var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var steps = new List<StructuredTaskStep>();
        var order = 1;

        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"^\d+[\.\)]\s*(.+)$");
            if (!match.Success)
                continue;

            var text = match.Groups[1].Value.Trim();
            var title = text.Length > 120 ? text[..120] + "..." : text;
            steps.Add(new StructuredTaskStep
            {
                Order = order++,
                Title = title,
                Description = text
            });
        }

        if (steps.Count == 0)
        {
            steps = BuildDefaultSteps(description);
        }

        return new StructuredTaskPlan
        {
            Objective = description,
            Summary = raw.Trim(),
            Steps = steps
        };
    }

    public static StructuredTaskPlan BuildMockPlan(string description)
    {
        return new StructuredTaskPlan
        {
            Objective = description,
            Summary = "Plan de trabajo legal simulado para el caso.",
            Steps = BuildDefaultSteps(description)
        };
    }

    private static List<StructuredTaskStep> BuildDefaultSteps(string description)
    {
        return
        [
            new() { Order = 1, Title = "Identificar hechos jurídicamente relevantes", Description = "Extraer y ordenar cronológicamente los hechos del caso con relevancia probatoria y jurídica." },
            new() { Order = 2, Title = "Listar documentación necesaria", Description = "Inventariar contratos, comprobantes, comunicaciones y prueba documental requerida." },
            new() { Order = 3, Title = "Detectar riesgos y puntos débiles", Description = "Analizar defensas posibles del demandado, prescripción, prueba insuficiente y riesgos procesales." },
            new() { Order = 4, Title = "Preparar teoría del caso", Description = "Definir pretensión, fundamentos fácticos y jurídicos, y relación causa-pretensión." },
            new() { Order = 5, Title = "Armar esquema de demanda", Description = "Estructurar carátula, hechos, derecho, prueba y petitorio." },
            new() { Order = 6, Title = "Redactar intimación previa", Description = "Preparar carta documento o intimación extrajudicial previa si corresponde." },
            new() { Order = 7, Title = "Sugerir próximos pasos procesales", Description = "Indicar vía judicial, medidas cautelares, conciliación y cronograma recomendado." }
        ];
    }
}
