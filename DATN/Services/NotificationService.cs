using DATN.Configuration;
using DATN.Data;
using DATN.Hubs;
using DATN.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly StrokeDbContext _dbContext;
        private readonly IBackgroundEmailQueue _queue;
        private readonly EmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            StrokeDbContext dbContext,
            IBackgroundEmailQueue queue,
            EmailService emailService,
            IEmailTemplateService emailTemplateService)
        {
            _hubContext = hubContext;
            _dbContext = dbContext;
            _queue = queue;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        public Task SendNotificationAsync(string toEmail, string subject, string message)
        {
            string emailBody = _emailTemplateService.BuildEmailContent(subject, message);
            _queue.EnqueueEmail(() => _emailService.SendEmailAsync(toEmail, subject, emailBody));
            Console.WriteLine($"[NotificationService] Email enqueued to {toEmail}: {subject}");
            return Task.CompletedTask;
        }

        public async Task SendWebNotificationAsync(int userId, string title, string message, string type = "warning")
        {
            try
            {
              
                var notification = new
                {
                   
                    id = Guid.NewGuid().ToString(),
                    title = title,
                    message = message,
                    type = type.ToLower(),
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                };

                Console.WriteLine($"[NotificationService] Sending notification to userId {userId}");

               
                await _hubContext.Clients.Group(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);

                Console.WriteLine($"[NotificationService] Notification sent successfully to group {userId}");

                
                Warning warningRecord = new Warning
                {
                    UserId = userId,
                    Description = $"{title}\n{message}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _dbContext.Warnings.Add(warningRecord);
                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"[NotificationService] Notification saved to database with id {warningRecord.WarningId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] ERROR sending notification to userId {userId}: {ex.Message}");
                Console.WriteLine($"[NotificationService] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<Warning>> GetUserWarningsAsync(int userId, int count = 10)
        {
            return await _dbContext.Warnings
                .Where(w => w.UserId == userId && w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public Task SendMobileNotificationAsync(string title, string message, IList<string> deviceTokens)
        {
            
            return Task.CompletedTask;
        }

        public Task SendWebNotificationAsync(string title, string message, IList<WebPushSubscription> subscriptions)
        {
           
            return Task.CompletedTask;
        }
    }
}