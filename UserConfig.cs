using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("T_Users");
            builder.Property(u => u.NickName).IsUnicode().HasMaxLength(20);
            builder.Property(u => u.AvatarUrl).HasMaxLength(500);
            builder.Property(u=>u.RegisterTime).IsRequired().HasDefaultValue(DateTime.UtcNow);
            builder.Property(u => u.LastLoginTime).IsRequired(false);
            builder.Property(u => u.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.HasIndex(u => u.NickName); // 按昵称搜索用户
            builder.HasIndex(u => u.RegisterTime); // 按注册时间筛选/统计
            builder.HasIndex(u => u.IsDeleted); // 软删除过滤加速
            builder.HasQueryFilter(u => u.IsDeleted==false);
        }
    }
}
