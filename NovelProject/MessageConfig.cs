using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class MessageConfig : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("T_Messages");
            builder.HasKey(m => m.MessageId);

            builder.Property(m => m.FromUserName).IsRequired().HasMaxLength(50);
            builder.Property(m => m.Content).IsRequired().HasMaxLength(1000);
            builder.Property(m => m.ConversationId).IsRequired().HasMaxLength(40);
            builder.Property(m => m.CreateTime).IsRequired();
            builder.Property(m => m.IsRead).HasDefaultValue(false);
            builder.Property(m => m.IsDeleted).HasDefaultValue(false);

            builder.HasIndex(m => new { m.ConversationId, m.MessageId });
            builder.HasIndex(m => new { m.ToUserId, m.IsRead });
        }
    }
}
