using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Domain.Entities;

public class AIJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AIJobType JobType { get; set; }
    public AIJobStatus Status { get; set; } = AIJobStatus.Pending;
    public AIProviderName Provider { get; set; }
    public string? InputData { get; set; }
    public string? OutputData { get; set; }
    public string? ErrorMessage { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public decimal? CostUsd { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
