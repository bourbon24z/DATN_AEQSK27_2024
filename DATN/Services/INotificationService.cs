namespace DATN.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string toEmail, string subject, string message);
    }
}
