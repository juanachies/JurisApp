using System.Text.RegularExpressions;
using JurisApp.Domain.Entities;

namespace JurisApp.Infrastructure.AI;

internal static class SkillFormat
{
    public static string AppendReminder(string userMessage, IReadOnlyList<CustomSkill> skills)
    {
        var formats = skills
            .Select(skill => (Name: skill.Name.Trim(), Format: skill.OutputFormat.Trim()))
            .Where(skill => skill.Format.Length > 0)
            .ToList();

        if (formats.Count == 0)
            return userMessage;

        var blocks = string.Join(
            "\n\n",
            formats.Select(skill => $"Skill «{skill.Name}»:\n{skill.Format}"));

        return userMessage.TrimEnd() +
               "\n\n---\n" +
               "Instrucción de JurisApp para este turno. Es obligatoria y pesa más que el estilo de las respuestas anteriores y que la estructura habitual de una consulta:\n" +
               "La respuesta completa tiene que seguir el formato de salida, con sus títulos y en ese orden. " +
               "No la reemplaces por prosa ni por secciones como Razones, Recomendación o Conclusión si el formato no las pide.\n" +
               "Si el formato pide una tabla, incluí una tabla Markdown: encabezado, una línea que sea solo `|---|` y cada fila en su propia línea.\n" +
               "Respetá los topes de palabras, de filas y de preguntas que indique el formato.\n" +
               "No inventes hechos del legajo. Si un dato no está en el texto disponible, escribí \"No consta en el texto disponible\".\n" +
               "Si el formato dice que una parte va sólo cuando fue solicitada y este mensaje no la pidió, no la incluyas.\n\n" +
               blocks;
    }

    public static bool MissesRequiredFormat(IReadOnlyList<CustomSkill> skills, string? reply)
    {
        foreach (var skill in skills)
        {
            var format = skill.OutputFormat.Trim();
            if (format.Length == 0)
                continue;

            if (string.IsNullOrWhiteSpace(reply))
                return true;

            if (AsksForTable(format) && !HasMarkdownTable(reply))
                return true;

            foreach (var marker in RequiredMarkers(format))
            {
                if (!reply.Contains(marker, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private static bool AsksForTable(string format) =>
        format.Contains("tabla", StringComparison.OrdinalIgnoreCase)
        || Regex.IsMatch(format, @"\|[^|\r\n]+\|");

    private static bool HasMarkdownTable(string reply) =>
        Regex.IsMatch(reply, @"\|[^\r\n]*-{3,}[^\r\n]*\|");

    private static IEnumerable<string> RequiredMarkers(string format)
    {
        var parts = Regex.Split(format.Trim(), @"\r?\n+|(?<=\.)\s+(?=\p{Lu})");
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in parts)
        {
            var trimmed = part.Trim().TrimStart('-', '*', '#').Trim();
            var colon = trimmed.IndexOf(':');
            if (colon < 3 || colon > 48)
                continue;

            var label = trimmed[..colon].Trim();
            var firstSentence = trimmed[(colon + 1)..].Split('.', 2)[0];
            if (label.Contains('|') || IsConditional(firstSentence) || !seen.Add(label))
                continue;

            yield return label;
        }
    }

    private static bool IsConditional(string text) =>
        Regex.IsMatch(
            text,
            @"s[oó]lo si|únicamente si|unicamente si|si fue solicitad|si se solicit",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
