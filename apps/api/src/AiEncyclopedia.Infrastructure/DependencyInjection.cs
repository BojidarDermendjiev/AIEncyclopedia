using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Infrastructure.AI;
using AiEncyclopedia.Infrastructure.Auth;
using AiEncyclopedia.Infrastructure.Cache;
using AiEncyclopedia.Infrastructure.Pdf;
using AiEncyclopedia.Infrastructure.Payment;
using AiEncyclopedia.Infrastructure.Persistence;
using AiEncyclopedia.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AiEncyclopedia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // ── Strongly-typed options (validated at startup) ─────────────────────
        services.AddOptions<JwtOptions>()
            .Bind(config.GetSection(JwtOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OpenAIOptions>()
            .Bind(config.GetSection(OpenAIOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AnthropicOptions>()
            .Bind(config.GetSection(AnthropicOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<GoogleOptions>()
            .Bind(config.GetSection(GoogleOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AIOptions>()
            .Bind(config.GetSection(AIOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<StripeOptions>()
            .Bind(config.GetSection(StripeOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(
                config.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Redis ─────────────────────────────────────────────────────────────
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(config.GetConnectionString("Redis") ?? "localhost:6379,abortConnect=false"));
        services.AddScoped<ICacheService, RedisCacheService>();

        // ── Auth ──────────────────────────────────────────────────────────────
        services.AddScoped<ITokenService, TokenService>();

        // ── AI Providers ──────────────────────────────────────────────────────
        services.AddHttpClient<OpenAIProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/v1/");
            client.Timeout = TimeSpan.FromSeconds(
                config.GetValue<int>("AI:TimeoutSeconds", 120));
        });
        services.AddHttpClient<ClaudeProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.anthropic.com/v1/");
            client.Timeout = TimeSpan.FromSeconds(
                config.GetValue<int>("AI:TimeoutSeconds", 120));
        });
        services.AddHttpClient<GeminiProvider>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
            client.Timeout = TimeSpan.FromSeconds(
                config.GetValue<int>("AI:TimeoutSeconds", 120));
        });
        services.AddScoped<IAIProviderFactory, AIProviderFactory>();

        // ── PDF Generation ────────────────────────────────────────────────────
        services.AddScoped<IPdfGenerationService, PdfGenerationService>();

        // ── Stripe ────────────────────────────────────────────────────────────
        var stripeKey = config[$"{StripeOptions.Section}:SecretKey"] ?? "";
        if (!string.IsNullOrEmpty(stripeKey))
            Stripe.StripeConfiguration.ApiKey = stripeKey;
        services.AddScoped<IStripeService, StripeService>();

        return services;
    }
}
