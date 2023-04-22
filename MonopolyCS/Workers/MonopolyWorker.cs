using Microsoft.Extensions.Hosting;
using NLog;

namespace MonopolyCS;

public sealed class MonopolyWorker : BackgroundService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public MonopolyWorker()
    {
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Trace("Worker Running...");
            await Task.Delay(1000, stoppingToken);
        }
    }
}