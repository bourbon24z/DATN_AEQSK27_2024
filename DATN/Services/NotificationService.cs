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
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                Console.WriteLine($"[NotificationService] Sending web notification to userId {userId}");
                Console.WriteLine($"[NotificationService] Notification details: {System.Text.Json.JsonSerializer.Serialize(notification)}");

               
                await _hubContext.Clients.Group(userId.ToString())
                    .SendAsync("ReceiveNotification", notification);

                Console.WriteLine($"[NotificationService] Notification sent successfully to group {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] ERROR sending notification to userId {userId}: {ex.Message}");
                Console.WriteLine($"[NotificationService] Stack trace: {ex.StackTrace}");
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