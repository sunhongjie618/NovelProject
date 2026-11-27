using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class NovelCommentsConfig : IEntityTypeConfiguration<NovelComments>
    {
        public void Configure(EntityTypeBuilder<NovelComments> builder)
        {
            builder.ToTable("T_NovelComments");
            builder.HasKey(c => c.CommentId);
            builder.Property(c => c.UserId).IsRequired(false);
            builder.Property(c => c.NovelId).IsRequired();
            builder.Property(c => c.ChapterId).IsRequired(false);
            builder.Property(c => c.Content).IsRequired().IsUnicode().HasMaxLength(500);
            builder.Property(c => c.CreateTime).IsRequired().HasDefaultValue(DateTime.UtcNow);
            builder.Property(c => c.LikeCount).IsRequired().HasDefaultValue(0);
            builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);

            // 索引配置
            builder.HasIndex(c => c.NovelId);
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.CreateTime);

            // 显式外键配置
            builder.HasOne(c => c.User)
                   .WithMany(u=>u.Comments)
                   .HasForeignKey(c => c.UserId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(c => c.Novel)
                   .WithMany(u=>u.Comments)
                   .HasForeignKey(c => c.NovelId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Chapter)
                   .WithMany(u=>u.Comments)
                   .HasForeignKey(c => c.ChapterId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Cascade);


            builder.HasQueryFilter(c => !c.IsDeleted);

        }
    }
}
