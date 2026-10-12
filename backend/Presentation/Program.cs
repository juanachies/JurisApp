using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using DotNetEnv;
using JurisApp.Application;
using JurisApp.Infrastructure;
using JurisApp.Infrastructure.AI;
using JurisApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

string? FindEnvFile()
{
    var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var directory in new[]
             {
                 Directory.GetCurrentDirectory(),
                 AppContext.BaseDirectory
             })
    {
        directories.Add(directory);

        var current = new DirectoryInfo(directory);
        for (var i = 0; i < 10 && current is not null; i++)
        {
            directories.Add(current.FullName);
            current = current.Parent;
        }
    }

    foreach (var directory in directories)
    {
        var envPath = Path.Combine(directory, ".env");
        if (File.Exists(envPath))
            return envPath;
    }

    return null;
}

var envPath = FindEnvFile();
if (!string.IsNullOrWhiteSpace(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JurisApp API",
        Version = "v1"
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá el JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // .NET 8+ no mapea claims JWT → URI largos por defecto; hay que alinear RoleClaimType.
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        NameClaimType = JwtRegisteredClaimNames.Sub,
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization();

var allowedOrigins = GetAllowedOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

var openAiOpts = builder.Configuration.GetSection(OpenAIOptions.SectionName).Get<OpenAIOptions>()
    ?? new OpenAIOptions();
if (builder.Configuration.GetValue<bool>("AI:UseMock"))
    openAiOpts.Enabled = false;

if (openAiOpts.Enabled && string.IsNullOrWhiteSpace(openAiOpts.ApiKey))
{
    app.Logger.LogWarning(
        "AI:OpenAI:Enabled=true pero falta AI:OpenAI:ApiKey. Se usará respuesta simulada.");
}
else if (!openAiOpts.Enabled)
{
    app.Logger.LogInformation("OpenAI deshabilitado. Modo desarrollo activo en AIService.");
}
else
{
    app.Logger.LogInformation(
        "OpenAI configurado → BaseUrl: {BaseUrl}, Model: {Model}",
        string.IsNullOrWhiteSpace(openAiOpts.BaseUrl) ? "https://api.openai.com/v1" : openAiOpts.BaseUrl,
        string.IsNullOrWhiteSpace(openAiOpts.Model) ? "gpt-4o" : openAiOpts.Model);
}

if (builder.Configuration.GetValue<bool>("Stripe:UseMock"))
    app.Logger.LogInformation("Stripe mock mode enabled. Use POST /api/billing/simulate-purchase to test subscriptions.");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    app.Logger.LogInformation("Applying database migrations.");
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("Database migrations applied.");

    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        await DevDataSeeder.SeedAsync(db, scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception exception)
        {
            app.Logger.LogError(exception, "Unhandled exception");
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var origin = context.Request.Headers.Origin.ToString();
            if (allowedOrigins.Contains(origin, StringComparer.Ordinal))
            {
                context.Response.Headers.AccessControlAllowOrigin = origin;
                context.Response.Headers.Append("Vary", "Origin");
            }
        }
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/billing/webhook"))
        context.Request.EnableBuffering();

    await next();
});

app.MapControllers();

app.Run();

static string[] GetAllowedOrigins(ConfigurationManager configuration)
{
    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    if (origins is { Length: > 0 })
    {
        return origins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    return
    [
        configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173",
        "http://localhost:5248",
        "https://localhost:7212"
    ];
}

public partial class Program { }
