using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using JurisApp.Application;
using JurisApp.Infrastructure;
using JurisApp.Infrastructure.AI;
using JurisApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (origins is { Length: > 0 })
        {
            policy.WithOrigins(origins);
        }
        else
        {
            policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173",
                "http://localhost:5248",
                "https://localhost:7212");
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

var deepSeekOpts = builder.Configuration.GetSection(DeepSeekOptions.SectionName).Get<DeepSeekOptions>()
    ?? new DeepSeekOptions();
if (builder.Configuration.GetValue<bool>("AI:UseMock"))
    deepSeekOpts.Enabled = false;

if (deepSeekOpts.Enabled && string.IsNullOrWhiteSpace(deepSeekOpts.ApiKey))
{
    app.Logger.LogWarning(
        "AI:DeepSeek:Enabled=true pero falta AI:DeepSeek:ApiKey. Se usará respuesta simulada.");
}
else if (!deepSeekOpts.Enabled)
{
    app.Logger.LogInformation("DeepSeek deshabilitado. Modo desarrollo activo en AIService.");
}
else
{
    app.Logger.LogInformation(
        "DeepSeek configurado → BaseUrl: {BaseUrl}, Model: {Model}",
        string.IsNullOrWhiteSpace(deepSeekOpts.BaseUrl) ? "https://api.deepseek.com" : deepSeekOpts.BaseUrl,
        string.IsNullOrWhiteSpace(deepSeekOpts.Model) ? "deepseek-v4-pro" : deepSeekOpts.Model);
}

if (builder.Configuration.GetValue<bool>("Stripe:UseMock"))
    app.Logger.LogInformation("Stripe mock mode enabled. Use POST /api/billing/simulate-purchase to test subscriptions.");

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DevDataSeeder.SeedAsync(db, scope.ServiceProvider);
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

public partial class Program { }
