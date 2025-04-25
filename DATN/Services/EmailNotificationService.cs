using DATN.Configuration;

namespace DATN.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IBackgroundEmailQueue _queue;
        private readonly EmailService _emailService;
        public readonly IEmailTemplateService _emailTemplateService;

        public EmailNotificationService(IBackgroundEmailQueue queue, EmailService emailService, IEmailTemplateService emailTemplateService)
        {
            _queue = queue;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        //public Task SendNotificationAsync(string toEmail, string subject, string message)
        //{
        //    string emailBody = _emailTemplateService.BuildEmailContent(subject, message);
        //    Console.WriteLine($"[EmailNotificationService] Enqueue task for email: {toEmail}, subject: {subject}");
        //    _queue.EnqueueEmail(() => _emailService.SendEmailAsync(toEmail, subject, message));
        //    return Task.CompletedTask;
        //}   
        public Task SendNotificationAsync(string toEmail, string subject, string message)
        {
            string emailBody = _emailTemplateService.BuildEmailContent(subject, message);
            Console.WriteLine($"[EmailNotificationService] Built email body: {emailBody}");
            _queue.EnqueueEmail(() => _emailService.SendEmailAsync(toEmail, subject, emailBody));
            return Task.CompletedTask;
        }

    }
}
