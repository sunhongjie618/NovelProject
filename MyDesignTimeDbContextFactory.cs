using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovelProject
{
    public class MyDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<MyDbContext> builder1 = new DbContextOptionsBuilder<MyDbContext>();
            string connstr = "Server=101.37.145.38,1433;Database=Novelset;User Id=sa;Password=NovelDB@2026;TrustServerCertificate=True;Connect Timeout=30";
            builder1.UseSqlServer(connstr);
            MyDbContext ctx = new MyDbContext(builder1.Options);
            return ctx;
        }
    }
}
