using System.Text;
using Microsoft.EntityFrameworkCore;
using SoruHavuzu.API.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using SoruHavuzu.API.Middleware;
using SoruHavuzu.API.Services;
using SoruHavuzu.Application.Common;
using SoruHavuzu.Application.Interfaces;
using SoruHavuzu.Infrastructure.Persistence;
using SoruHavuzu.Infrastructure.Persistence.Seed;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;

namespace SoruHavuzu.API.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection ValidateJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT anahtarı kaynak kodda tutulmaz; user-secrets veya ortam değişkeni (Jwt__Key) ile beslenir.
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT anahtarı yapılandırılmamış veya çok kısa (en az 32 karakter). " +
                "Geliştirme için: dotnet user-secrets set \"Jwt:Key\" \"<gizli-anahtar>\" " +
                "veya Jwt__Key ortam değişkenini ayarlayın.");
        }

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")))
                };
                // JWT doğrulama hatalarını detaylı logla ve WWW-Authenticate header'ına hata bilgisi ekle.
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        logger.LogWarning(
                            context.Exception,
                            "JWT doğrulama başarısız: {Message}. Path: {Path}, Header: {AuthHeader}",
                            context.Exception.Message,
                            context.Request.Path,
                            context.Request.Headers.Authorization.ToString());
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Hata detayını WWW-Authenticate header'ına ekle.
                        if (!context.Response.HasStarted)
                        {
                            context.ErrorDescription = context.AuthenticateFailure?.Message
                                ?? "Kimlik doğrulama başarısız.";
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOrTeacher", policy =>
                policy.RequireRole("Admin", "Teacher"));
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));
        });

        // IP bazlı katmanlı rate limiting (Global + Login + Contact + Auth).
        services.AddRateLimiting(configuration);

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAdminApp", policy =>
            {
                policy.WithOrigins(
                        "http://soruasistanim.localhost", "http://admin.soruasistanim.localhost",
                        "https://soruasistanim.com", "https://admin.soruasistanim.com",
                        "https://localhost:7223","http://localhost:5173","http://localhost:5174"
                    ) // Admin projenizin tam adresi (Sonda / olmasın)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        var tickerQUsername = configuration["TickerQ:Dashboard:Username"]
            ?? throw new InvalidOperationException("TickerQ:Dashboard:Username yapılandırılmalı.");
        var tickerQPassword = configuration["TickerQ:Dashboard:Password"]
            ?? throw new InvalidOperationException("TickerQ:Dashboard:Password yapılandırılmalı.");

        // TickerQ dashboard sadece yapılandırmadan gelen kimlik bilgileriyle açılır.
        services.AddTickerQ(options =>
        {
            options.AddDashboard(opt =>
            {
                opt.SetBasePath("tickerq");
                opt.WithBasicAuth(tickerQUsername, tickerQPassword);
            });
        });

        return services;
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
            await DataSeeder.SeedAsync(context);

            // db/curriculum altındaki faz bazlı müfredat dosyalarını (Sınıf → Ders → Ünite/Tema → Konu → Kazanım) aktar.
            var curriculumSeeder = services.GetRequiredService<ICurriculumSeederService>();
            var seedLog = await curriculumSeeder.SeedAsync();
            foreach (var line in seedLog)
                Console.WriteLine(line);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("SoruHavuzu.API.Startup");
            logger.LogError(ex, "Veritabanı tohumlanırken bir hata oluştu.");
        }
    }

    public static WebApplication UseApiMiddlewares(this WebApplication app)
    {
        // Reverse proxy (Nginx vb.) arkasında gerçek client IP'sini güvenli şekilde çözer.
        app.UseForwardedHeaders(app.Configuration);

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options.WithTitle("SoruHavuzu API"));
        }
        // Profil fotoğrafları gibi wwwroot altındaki statik dosyaları sunar.
        app.UseStaticFiles();
        app.UseCors("AllowAdminApp");
        app.UseAuthentication();
        // Rate limiter endpoint metadata'sını okuyabilmek için routing sonrası,
        // UserId loglayabilmek için authentication sonrası çalışır.
        app.UseRateLimiter();
        // Docker/Coolify proxy TLS'i dış kenarda sonlandırır; redirect'i proxy yapar.
        if (!app.Environment.IsDevelopment() && !app.Configuration.GetValue<bool>("ReverseProxy:TerminatesTls"))
            app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.UseTickerQ();
        return app;
    }
}

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (!authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            return;

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            In = ParameterLocation.Header,
            BearerFormat = "JWT",
            Description = "JWT Authorization header. Örnek: Bearer {token}"
        };
    }
}
