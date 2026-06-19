using JurisApp.Domain.Entities;
using JurisApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<LawyerProfile> LawyerProfiles => Set<LawyerProfile>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentAnalysis> DocumentAnalyses => Set<DocumentAnalysis>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<CustomSkill> CustomSkills => Set<CustomSkill>();
    public DbSet<ChatCustomSkill> ChatCustomSkills => Set<ChatCustomSkill>();
    public DbSet<AITask> AITasks => Set<AITask>();
    public DbSet<AITaskStep> AITaskSteps => Set<AITaskStep>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}