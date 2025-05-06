using DATN.Data;
using DATN.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class SignalRMobileNotificationService : IMobileNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRMobileNotificationService> _logger;
        private readonly StrokeDbContext _dbContext; 

        public SignalRMobileNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRMobileNotificationService> logger,
            StrokeDbContext dbContext) 
        {
            _hubContext = hubContext;
            _logger = logger;
            _dbContext = dbContext; 
        }

        public async Task<bool> SendNotificationToRolesAsync(
            IEnumerable<string> roles,
            string title,
            string body,
            string notificationType,
            Dictionary<string, string> additionalData = null)
        {
            try
            {
                
                var userIds = await _dbContext.UserRoles
                    .Where(ur => roles.Contains(ur.Role.RoleName) && ur.IsActive)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation($"Gửi thông báo đến {userIds.Count} người dùng với roles: {string.Join(", ", roles)}");

                
                var notification = CreateNotificationObject(title, body, notificationType, additionalData);

               
                var tasks = userIds.Select(userId =>
                    _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", notification));

                await Task.WhenAll(tasks);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo đến roles: {string.Join(", ", roles)}");
                return false;
            }
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
           
            var baseNotification = new
            {
                id = Guid.NewGuid().ToString(),
                title = title,
                message = message,
                type = type.ToLower(),
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
            };

            
            if (additionalData != null && additionalData.Count > 0)
            {
                
                var props = baseNotification.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(baseNotification));

                foreach (var item in additionalData)
                {
                    props[item.Key] = item.Value;
                }

                return props;
            }

            return baseNotification;
        }
    }
}