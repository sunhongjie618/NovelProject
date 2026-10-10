using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class UserReadingRecordsConfig : IEntityTypeConfiguration<UserReadingRecords>
    {
        public void Configure(EntityTypeBuilder<UserReadingRecords> builder)
        {
            builder.ToTable("T_UserReadingRecords");
            builder.HasKey(r => r.RecordId);
            builder.Property(r => r.UserId).IsRequired();
            builder.Property(r => r.NovelId).IsRequired();
            builder.Property(r => r.LastReadChapterId);
            builder.Property(r=>r.LastReadTime).IsRequired().HasDefaultValue(DateTime.UtcNow);
            builder.Property(r => r.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(r => new { r.UserId, r.NovelId }).IsUnique();
            builder.HasIndex(r => r.UserId);
            builder.HasIndex(r => r.NovelId);

            builder.HasOne(r => r.User).WithMany(u=>u.ReadingRecords).HasForeignKey(r => r.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(r => r.Novel).WithMany(u=>u.ReadingRecords).HasForeignKey(r => r.NovelId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(r => r.Chapter).WithMany().HasForeignKey(r => r.LastReadChapterId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);



            builder.HasQueryFilter(r => r.IsDeleted == false);
        }
    }
}
