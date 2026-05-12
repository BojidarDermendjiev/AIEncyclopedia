using AiEncyclopedia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiEncyclopedia.Infrastructure.Persistence.Configurations;

public class KnowledgeRelationConfiguration : IEntityTypeConfiguration<KnowledgeRelation>
{
    public void Configure(EntityTypeBuilder<KnowledgeRelation> builder)
    {
        builder.HasKey(kr => kr.Id);
        builder.Property(kr => kr.RelationType).HasConversion<string>().HasMaxLength(50);
        builder.Property(kr => kr.Strength).HasPrecision(3, 2);

        builder.HasOne(kr => kr.SourceTopic)
            .WithMany(t => t.OutgoingRelations)
            .HasForeignKey(kr => kr.SourceTopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(kr => kr.TargetTopic)
            .WithMany(t => t.IncomingRelations)
            .HasForeignKey(kr => kr.TargetTopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
