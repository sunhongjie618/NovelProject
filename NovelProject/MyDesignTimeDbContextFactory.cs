using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovelProject
{
    //设计时上下文，用于迁移使用，不必要从配置文件中读
    public class MyDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<MyDbContext> builder1 = new DbContextOptionsBuilder<MyDbContext>();
            string connstr = "Server=.;Database=Novelset;Trusted_Connection=True;TrustServerCertificate=True";
            builder1.UseSqlServer(connstr);
            MyDbContext ctx = new MyDbContext(builder1.Options);
            return ctx;
        }
    }
}
