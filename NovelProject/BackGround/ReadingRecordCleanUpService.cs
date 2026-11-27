
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NovelProject.BackGround
{
    public class ReadingRecordCleanUpService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReadingRecordCleanUpService> _logger;


        public ReadingRecordCleanUpService(IServiceScopeFactory scopeFactory, ILogger<ReadingRecordCleanUpService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            //应用启动后先等十秒

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //计算下一次执行的时间
                    var nextTime = CalculateNextRunTime();

                    var delayTime = nextTime - DateTime.UtcNow;

                    if (delayTime > TimeSpan.Zero)
                    {
                        _logger.LogInformation("下一次执行将在{NextTime}后",nextTime);
                        await Task.Delay(delayTime, stoppingToken);
                    }
                    //加上参数解决大表删除锁表，造成用户体验不佳
                    await CleanUpExpiredRecordsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("阅读记录清理正在结束");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清理阅读记录历史时发生未知错误");
                    //延迟一小时后，再重试
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task CleanUpExpiredRecordsAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            var Time = DateTime.UtcNow.AddDays(-30);

            var deleteCount=await ctx.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM T_UserReadingRecords WHERE LastReadTime < {Time}",stoppingToken);

            _logger.LogInformation("清除了{DeleteCount}条过期阅读记录",deleteCount);

        }

        private DateTime CalculateNextRunTime()
        {
            var now = DateTime.UtcNow;
            //每天凌晨两点，客流量少的时候操作
            var nextrun = now.Date.AddHours(2);
            //如果今天已经超过了两点，则明天两点再执行
            if (now > nextrun)
            {
                nextrun = nextrun.AddDays(1);
            }
            return nextrun;
        }
    }
}
