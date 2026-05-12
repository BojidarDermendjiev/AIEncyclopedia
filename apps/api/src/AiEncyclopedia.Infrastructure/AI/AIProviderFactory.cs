using AiEncyclopedia.Application.Common.Interfaces;
using AiEncyclopedia.Domain.Enums;
using AiEncyclopedia.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AiEncyclopedia.Infrastructure.AI;

public class AIProviderFactory : IAIProviderFactory
{
    private readonly IServiceProvider _services;
    private readonly AIOptions _opts;

    public AIProviderFactory(IServiceProvider services, IOptions<AIOptions> opts)
    {
        _services = services;
        _opts = opts.Value;
    }

    public IAIProvider GetProvider(AIProviderName? provider = null)
    {
        var target = provider ?? GetDefaultProviderName();
        return target switch
        {
            AIProviderName.OpenAI => _services.GetRequiredService<OpenAIProvider>(),
            AIProviderName.Claude => _services.GetRequiredService<ClaudeProvider>(),
            AIProviderName.Gemini => _services.GetRequiredService<GeminiProvider>(),
            _ => _services.GetRequiredService<OpenAIProvider>()
        };
    }

    public IAIProvider GetDefaultProvider() => GetProvider(GetDefaultProviderName());

    private AIProviderName GetDefaultProviderName() =>
        _opts.DefaultProvider.ToLowerInvariant() switch
        {
            "claude" => AIProviderName.Claude,
            "gemini" => AIProviderName.Gemini,
            _ => AIProviderName.OpenAI
        };
}
