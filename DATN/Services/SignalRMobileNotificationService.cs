using DATN.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class SignalRMobileNotificationService : IMobileNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRMobileNotificationService> _logger;

        public SignalRMobileNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRMobileNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<bool> SendNotificationToTopicAsync(
            string topic,
            string title,
            string body,
            string notificationType,
            Dictionary<string, string> additionalData = null)
        {
            try
            {
               
                var notification = CreateNotificationObject(title, body, notificationType, additionalData);

                
                await _hubContext.Clients.Group(topic)
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation($"Đã gửi thông báo đến topic {topic}: {title}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo đến topic {topic}");
                return false;
            }
        }

        public async Task<bool> SendNotificationToUserAsync(
            int userId,
            string title,
            string body,
            string notificationType,
            Dictionary<string, string> additionalData = null)
        {
            try
            {
                
                var notification = CreateNotificationObject(title, body, notificationType, additionalData);

                await _hubContext.Clients.Group(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation($"Đã gửi thông báo đến userId {userId}: {title}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo đến userId {userId}: {ex.Message}");
                return false;
            }
        }

        private object CreateNotificationObject(
            string title,
            string message,
            string type,
            Dictionary<string, string> additionalData = null)
        {
            var notification = new
            {
                id = Guid.NewGuid().ToString(),
                title = title,
                message = message,
                type = type.ToLower(),
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
            };

            
            return notification;
        }
    }
}