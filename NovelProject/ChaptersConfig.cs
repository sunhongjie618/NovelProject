using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class ChaptersConfig : IEntityTypeConfiguration<Chapters>
    {
        public void Configure(EntityTypeBuilder<Chapters> builder)
        {
            builder.ToTable("T_Chapters");
            builder.HasKey(c => c.ChapterId);
            builder.Property(c => c.NovelId).IsRequired();
            builder.Property(c => c.ChapterNumber).IsRequired();
            builder.Property(c => c.Title).IsRequired().IsUnicode().HasMaxLength(100);
            builder.Property(c => c.Content).IsRequired().IsUnicode().HasMaxLength(int.MaxValue);
            builder.Property(c => c.WordCount).IsRequired();
            builder.Property(c => c.PublishTime).IsRequired().HasDefaultValue(DateTime.UtcNow);
            builder.Property(c => c.UpdateTime).IsRequired(false);
            builder.Property(c => c.IsLocked).IsRequired().HasDefaultValue(false);
            builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(c => new { c.NovelId, c.ChapterNumber }).IsUnique();
            builder.HasIndex(c => c.NovelId);

            builder.HasOne(c => c.Novel).WithMany(n => n.Chapters).HasForeignKey(c => c.NovelId).IsRequired().OnDelete(DeleteBehavior.Cascade);//级联删除，删除小说时，章节一起被删掉
            builder.HasQueryFilter(c => c.IsDeleted==false);
        }
    }
}
