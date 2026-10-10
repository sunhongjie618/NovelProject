
using Microsoft.EntityFrameworkCore;

namespace NovelProject.BackGround
{
    public class DatabaseWarmupService : BackgroundService
    {
        private readonly ILogger<DatabaseWarmupService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseWarmupService(ILogger<DatabaseWarmupService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        //background的所有异步方法都需要传入stoppingToken参数，防止程序结束后异步方法继续执行
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //启动后等待延迟3秒
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            try
            {
                await WarmUp(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("预热程序正在结束!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库预热失败!");
            }
        }

        //预热连接数据库，减少程序启动时的等待时间，优化性能,初始化EF Core的模型缓存
        private async Task WarmUp(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            await ctx.Database.CanConnectAsync();
            await ctx.Categories.AsNoTracking().FirstOrDefaultAsync(stoppingToken);

        }
    }
}
