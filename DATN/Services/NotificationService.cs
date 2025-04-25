namespace DATN.Services
{
    public class NotificationService : INotificationService
    {
        public Task SendNotificationAsync(string toEmail, string subject, string message)
        {
           
            return Task.CompletedTask;
        }
    }
}
