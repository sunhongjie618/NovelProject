using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class UserFavoritesConfig : IEntityTypeConfiguration<UserFavorites>
    {
        public void Configure(EntityTypeBuilder<UserFavorites> builder)
        {
            builder.ToTable("T_UserFavorites");
            builder.HasKey(f => f.FavoriteId);
            builder.Property(f => f.UserId).IsRequired();
            builder.Property(f => f.NovelId).IsRequired();
            builder.Property(f=>f.CreateTime).IsRequired().HasDefaultValue(DateTime.UtcNow);
            builder.Property(f => f.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(f => new { f.UserId, f.NovelId }).IsUnique();
            builder.HasIndex(f => f.UserId);
            builder.HasIndex(f => f.NovelId);

            builder.HasOne(f => f.User).WithMany(u=>u.Favorites).HasForeignKey(f => f.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Novel).WithMany(n=>n.Favorites).HasForeignKey(f => f.NovelId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(f => f.IsDeleted==false);
        }
    }
}
