using DATN.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services
{
    public interface INotificationService
    {

        Task SendNotificationAsync(string toEmail, string subject, string message);
        Task SendWebNotificationAsync(int userId, string title, string message, string type = "warning", bool saveWarning = true);
        Task<List<Warning>> GetUserWarningsAsync(int userId, int count = 10);
        Task SendMobileNotificationAsync(string title, string message, IList<string> deviceTokens);
        Task SendWebNotificationAsync(string title, string message, IList<WebPushSubscription> subscriptions);
        Task SendNotificationToRolesAsync(IEnumerable<string> roles, string title, string message, string type = "warning");
    }

    
    public class WebPushSubscription
    {
        public string Endpoint { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }
    }
}