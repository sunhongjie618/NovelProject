using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovelProject
{
    public class CategoriesConfig : IEntityTypeConfiguration<Categories>
    {
        public void Configure(EntityTypeBuilder<Categories> builder)
        {
            builder.ToTable("T_Categories");
            builder.HasKey(c => c.CategoryId);
            builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(10).IsUnicode();
            builder.Property(c => c.SortOrder).IsRequired();
            builder.Property(c => c.Description).IsUnicode().HasMaxLength(200);
            builder.Property(c => c.IsEnable).IsRequired().HasDefaultValue(true);
            builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);

            builder.HasIndex(c => c.CategoryName).IsUnique();
            builder.HasIndex(c => c.IsEnable);
            builder.HasQueryFilter(c => c.IsDeleted==false);
        }
    }
}
