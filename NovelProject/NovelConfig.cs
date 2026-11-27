using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class NovelConfig : IEntityTypeConfiguration<Novels>
    {
        public void Configure(EntityTypeBuilder<Novels> builder)
        {

            builder.ToTable("T_Novels");
            builder.HasKey(n => n.NovelId);
            builder.Property(n => n.Title).IsRequired().IsUnicode().HasMaxLength(50);
            builder.Property(n => n.AuthorName).IsRequired().IsUnicode().HasMaxLength(20);
            builder.Property(n => n.CoverUrl).HasMaxLength(500);
            builder.Property(n => n.Description).IsUnicode().HasMaxLength(500);
            builder.Property(n => n.Status).IsRequired().HasDefaultValue(1);
            builder.Property(n => n.WordCount).IsRequired().HasDefaultValue(0);
            builder.Property(n => n.ChapterCount).IsRequired().HasDefaultValue(0);
            builder.Property(n => n.PublishTime).IsRequired().HasDefaultValue(DateTime.UtcNow);
            builder.Property(n => n.LastUpdateTime).IsRequired().HasDefaultValue(DateTime.UtcNow);
            builder.Property(n => n.ClickCount).IsRequired().HasDefaultValue(0);
            builder.Property(n => n.FavoriteCount).IsRequired().HasDefaultValue(0);
            builder.Property(n => n.IsDeleted).IsRequired().HasDefaultValue(false);

            // 索引配置
            builder.HasIndex(n => n.CategoryId);
            builder.HasIndex(n => n.AuthorId);
            builder.HasIndex(n => n.Status);
            builder.HasIndex(n => n.LastUpdateTime);
            builder.HasIndex(n => n.Title);

            // 显式外键配置
            // 1. Novel 和 Category 的关系
            builder.HasOne(n => n.Category)
                   .WithMany(c=>c.Novel)
                   .HasForeignKey(n => n.CategoryId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            // 2. Novel 和 User（作者）的关系
            builder.HasOne(n => n.Author)
                   .WithMany(u=>u.AuthoredNovels)
                   .HasForeignKey(n => n.AuthorId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(n => n.IsDeleted==false);
        }
    }
}
