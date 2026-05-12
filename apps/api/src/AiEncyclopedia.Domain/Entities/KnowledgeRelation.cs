using AiEncyclopedia.Domain.Enums;

namespace AiEncyclopedia.Domain.Entities;

public class KnowledgeRelation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourceTopicId { get; set; }
    public Guid TargetTopicId { get; set; }
    public RelationType RelationType { get; set; }
    public decimal Strength { get; set; }

    public Topic SourceTopic { get; set; } = null!;
    public Topic TargetTopic { get; set; } = null!;
}
