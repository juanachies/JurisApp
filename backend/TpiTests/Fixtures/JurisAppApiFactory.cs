using JurisApp.Application.Interfaces.Auth;
using JurisApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace JurisApp.TpiTests.Fixtures;

public class JurisAppApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"jurisapp-tpi-{Guid.NewGuid():N}.db");
    private readonly string _uploadsPath = Path.Combine(Path.GetTempPath(), $"jurisapp-tpi-uploads-{Guid.NewGuid():N}");

    public CapturingEmailSender Emails { get; } = new();

    protected virtual int TaskStepDelayMs => 0;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(BuildSettings());
        });
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(BuildSettings());
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    private Dictionary<string, string?> BuildSettings() => new()
    {
        ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
        ["Database:Provider"] = "Sqlite",
        ["AI:UseMock"] = "true",
        ["AI:TaskStepDelayMilliseconds"] = TaskStepDelayMs.ToString(),
        ["Stripe:UseMock"] = "true",
        ["FileStorage:BasePath"] = _uploadsPath,
        ["Jwt:Secret"] = "JurisApp-Development-JWT-Secret-Key-Must-Be-At-Least-32-Characters-Long",
        ["Jwt:Issuer"] = "JurisApp",
        ["Jwt:Audience"] = "JurisApp",
        ["Jwt:ExpiresInMinutes"] = "60"
    };

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
            if (File.Exists(_dbPath + "-wal"))
                File.Delete(_dbPath + "-wal");
            if (File.Exists(_dbPath + "-shm"))
                File.Delete(_dbPath + "-shm");
            if (Directory.Exists(_uploadsPath))
                Directory.Delete(_uploadsPath, true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}

public sealed class SlowTaskFactory : JurisAppApiFactory
{
    protected override int TaskStepDelayMs => 400;
}
