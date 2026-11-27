using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class CommentLikeConfig : IEntityTypeConfiguration<CommentLike>
    {
        public void Configure(EntityTypeBuilder<CommentLike> builder)
        {
            builder.ToTable("T_CommentLikes");
            builder.HasKey(l => l.LikeId);
            builder.Property(l => l.CreateTime)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);

            builder.HasIndex(l => new { l.UserId, l.CommentId }).IsUnique();//一个用户只能给一条评论点赞一次
            builder.HasIndex(l => l.NovelId);
            //外键配置
            builder.HasOne<User>().WithMany().HasForeignKey(l => l.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<NovelComments>().WithMany().HasForeignKey(l => l.CommentId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Novels>().WithMany().HasForeignKey(l => l.NovelId).IsRequired().OnDelete(DeleteBehavior.NoAction);
        }
    }
}
