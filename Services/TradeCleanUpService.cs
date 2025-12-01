using Microsoft.EntityFrameworkCore;
using TraceWebApi.EntityFrameworkCore;

public class TradeCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TradeCleanupService> _logger;

    public TradeCleanupService(IServiceProvider serviceProvider, ILogger<TradeCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldTrades();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during trade cleanup.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task CleanupOldTrades()
{
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TraceAppDbContext>();

    var cutoffTime = DateTime.UtcNow.AddDays(-14); //every 2 weeks

    var oldTrades = dbContext.CryptoTrades.Where(t => t.Time < cutoffTime);
    int deletedCount = await oldTrades.CountAsync(); // Make the count async for performance

    if (deletedCount > 0)
    {
        dbContext.CryptoTrades.RemoveRange(oldTrades);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation($"Deleted {deletedCount} old trades.");
    }
}
}
