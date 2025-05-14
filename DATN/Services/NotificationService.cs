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

        public async Task SendWebNotificationAsync(int userId, string title, string message, string type = "warning", bool saveWarning = true)
        {
            try
            {
                if (_hubContext == null)
                {
                    Console.WriteLine($"[NotificationService] ERROR: _hubContext is null");
                    return;
                }

                var notification = new
                {
                    id = Guid.NewGuid().ToString(),
                    title = title ?? "Notification",
                    message = message ?? "No content",
                    type = (type ?? "warning").ToLower(),
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                };

                Console.WriteLine($"[NotificationService] Sending notification to userId {userId}");

                try
                {
                    await _hubContext.Clients.Group(userId.ToString())
                        .SendAsync("ReceiveNotification", notification);
                    Console.WriteLine($"[NotificationService] Notification sent successfully to group {userId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NotificationService] ERROR sending SignalR notification: {ex.Message}");
                }

               
                if (saveWarning)
                {
                    try
                    {
                        Console.WriteLine($"[NotificationService] Verifying user exists with ID {userId}");
                        var userExists = await _dbContext.StrokeUsers.AnyAsync(u => u.UserId == userId);

                        if (!userExists)
                        {
                            Console.WriteLine($"[NotificationService] WARNING: User with ID {userId} does not exist");
                            return;
                        }

                        Console.WriteLine($"[NotificationService] Creating new Warning entity for user {userId}");

                        var warningRecord = new Warning
                        {
                            UserId = userId,
                            Description = $"{title}\n{message}",
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        Console.WriteLine($"[NotificationService] Adding Warning to context");
                        _dbContext.Warnings.Add(warningRecord);

                        Console.WriteLine($"[NotificationService] Entity state: {_dbContext.Entry(warningRecord).State}");

                        Console.WriteLine($"[NotificationService] Calling SaveChangesAsync()");
                        var result = await _dbContext.SaveChangesAsync();

                        Console.WriteLine($"[NotificationService] SaveChanges result: {result} row(s) affected");
                        Console.WriteLine($"[NotificationService] Warning ID after save: {warningRecord.WarningId}");
                    }
                    catch (DbUpdateException dbEx)
                    {
                        Console.WriteLine($"[NotificationService] DATABASE ERROR: {dbEx.Message}");
                        Console.WriteLine($"[NotificationService] Exception type: {dbEx.GetType().FullName}");

                        if (dbEx.InnerException != null)
                        {
                            Console.WriteLine($"[NotificationService] Inner exception: {dbEx.InnerException.Message}");
                        }

                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NotificationService] DATABASE ERROR: {ex.Message}");
                        Console.WriteLine($"[NotificationService] Exception type: {ex.GetType().FullName}");
                        Console.WriteLine($"[NotificationService] Stack trace: {ex.StackTrace}");

                        throw;
                    }
                }
                else
                {
                    Console.WriteLine($"[NotificationService] Skipping warning creation (saveWarning=false)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] GENERAL ERROR: {ex.Message}");
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
        public async Task SendNotificationToRolesAsync(IEnumerable<string> roles, string title, string message, string type = "warning")
        {
            try
            {
                
                var userIds = await _dbContext.UserRoles
                    .Where(ur => roles.Contains(ur.Role.RoleName) && ur.IsActive)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                Console.WriteLine($"[NotificationService] Sending notification to {userIds.Count} users with roles: {string.Join(", ", roles)}");

                
                var tasks = userIds.Select(userId =>
                    SendWebNotificationAsync(userId, title, message, type));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] ERROR sending notification to roles {string.Join(", ", roles)}: {ex.Message}");
                throw;
            }
        }      
    }
}
