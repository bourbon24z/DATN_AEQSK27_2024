using DATN.Data;

namespace DATN.Services
{
    public class CleanUpTempUsersService : BackgroundService
    {
        private readonly StrokeDbContext _context;
        private readonly ILogger<CleanUpTempUsersService> _logger;

        public CleanUpTempUsersService(StrokeDbContext context, ILogger<CleanUpTempUsersService> logger)
        {
            _context = context;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cleaning up expired temporary registrations");
                var expiredUsers = _context.UserRegistrationTemps
                    .Where(u => u.OtpExpiry < DateTime.UtcNow).ToList();
                if (expiredUsers.Any())
                {
                    _context.UserRegistrationTemps.RemoveRange(expiredUsers);
                    await _context.SaveChangesAsync();
                }
                
                await Task.Delay(TimeSpan.FromHours(0.1), stoppingToken);
            }
        }
    }
}
