using DATN.Configuration;

namespace DATN.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _queue;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IBackgroundEmailQueue queue, ILogger<EmailBackgroundService> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service is running");
            while (!stoppingToken.IsCancellationRequested)
            {
                var emailTask = await _queue.DequeueAsync(stoppingToken);
                try
                {
                    await emailTask();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing email task");
                }
            }
        }
    }

}
