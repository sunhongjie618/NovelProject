using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NovelProject
{
    public class MyDbContext:IdentityDbContext<User,Role,long>
    {
        public DbSet<Novels> Novels { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Chapters> Chapters { get; set; }
        public DbSet<UserFavorites> UserFavorites { get; set; }
        public DbSet<UserReadingRecords> UserReadingRecords { get; set; }
        public DbSet<NovelComments> NovelComments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
