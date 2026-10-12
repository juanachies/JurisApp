using System.Globalization;
using JurisApp.Domain.Entities;
using JurisApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JurisApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<LawyerProfile> LawyerProfiles => Set<LawyerProfile>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentAnalysis> DocumentAnalyses => Set<DocumentAnalysis>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<CustomSkill> CustomSkills => Set<CustomSkill>();
    public DbSet<ChatCustomSkill> ChatCustomSkills => Set<ChatCustomSkill>();
    public DbSet<AITask> AITasks => Set<AITask>();
    public DbSet<AITaskStep> AITaskSteps => Set<AITaskStep>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<ChatAudit> ChatAudits => Set<ChatAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        if (string.Equals(Database.ProviderName, "Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.Ordinal))
            UseSqliteCompatiblePostgresTypes(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Las migraciones existentes declaran Guid, DateTime y bool con tipos de SQLite.
    /// En PostgreSQL el modelo de runtime tiene que escribir esos mismos tipos.
    /// </summary>
    private static void UseSqliteCompatiblePostgresTypes(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.GetValueConverter() is not null)
                continue;

            var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            if (clrType == typeof(Guid))
            {
                property.SetValueConverter(new GuidToStringConverter());
                property.SetColumnType("text");
            }
            else if (clrType == typeof(DateTime))
            {
                property.SetValueConverter(new ValueConverter<DateTime, string>(
                    value => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                    value => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)));
                property.SetColumnType("text");
            }
            else if (clrType == typeof(bool))
            {
                property.SetValueConverter(new ValueConverter<bool, int>(
                    value => value ? 1 : 0,
                    value => value != 0));
                property.SetColumnType("integer");
            }
        }
    }
}