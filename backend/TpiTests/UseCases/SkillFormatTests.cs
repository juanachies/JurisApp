using JurisApp.Domain.Entities;
using JurisApp.Infrastructure.AI;

namespace JurisApp.TpiTests.UseCases;

public class SkillFormatTests
{
    private const string OutputFormat =
        """
        Diagnóstico: hasta 80 palabras.
        Tabla: Prioridad | Hallazgo | Soporte documental | Qué falta verificar | Siguiente paso. Máximo cinco filas.
        Preguntas al cliente: hasta tres, concretas y necesarias.
        Próximo paso: acción sugerida y condiciones para realizarla.
        Borrador: sólo si fue solicitado, con [POR CONFIRMAR] en los datos pendientes.
        """;

    private const string ProseReply =
        """
        No es posible afirmar que Lucía cumplió el contrato.

        Razones
        Entrega cuestionada.

        Recomendación para la Intimación
        Recolectar pruebas.

        Próximo Paso
        Colectar evidencias.
        """;

    private const string FormattedReply =
        """
        Diagnóstico
        No consta una entrega completa ni una aceptación del plan de cuotas.

        Tabla
        | Prioridad | Hallazgo | Soporte documental | Qué falta verificar | Siguiente paso |
        | --- | --- | --- | --- | --- |
        | 1 | Envío incompleto | Folio 4 | Comprobante del 3 de octubre | Pedir el correo |

        Preguntas al cliente
        1. ¿Aceptás el plan de cuatro cuotas?

        Próximo paso
        No enviar la intimación hasta tener el comprobante.
        """;

    [Fact]
    public void Reminder_puts_the_output_format_after_the_user_message()
    {
        var reminder = SkillFormat.AppendReminder("¿Puedo intimar hoy?", [Skill()]);

        Assert.StartsWith("¿Puedo intimar hoy?", reminder);
        Assert.Contains("Diagnóstico: hasta 80 palabras.", reminder);
        Assert.Contains("No la reemplaces por prosa", reminder);
        Assert.Contains("Borrador: sólo si fue solicitado", reminder);
    }

    [Fact]
    public void Prose_reply_misses_the_skill_format_and_a_real_table_does_not()
    {
        var skills = new[] { Skill() };

        Assert.True(SkillFormat.MissesRequiredFormat(skills, ProseReply));
        Assert.False(SkillFormat.MissesRequiredFormat(skills, FormattedReply));
    }

    [Fact]
    public void Optional_draft_is_not_required_when_the_format_says_only_if_requested()
    {
        Assert.DoesNotContain("Borrador", FormattedReply);
        Assert.False(SkillFormat.MissesRequiredFormat([Skill()], FormattedReply));
    }

    private static CustomSkill Skill() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Revisión crítica antes de reclamar",
        "Antes de intimar",
        "No afirmes cumplimiento sin prueba.",
        "",
        "",
        OutputFormat);
}
