//using SendGrid.Helpers.Mail;
//using SendGrid;

//namespace DATN.Services
//{
//    public class EmailService
//    {
//        private readonly IConfiguration _configuration;

//        public EmailService(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public async Task SendEmailAsync(string email, string subject, string message)
//        {
//            var apiKey = _configuration["SendGrid:ApiKey"];
//            var client = new SendGridClient(apiKey);
//            var from = new EmailAddress("noreply@yourdomain.com", "Your App Name");
//            var to = new EmailAddress(email);
//            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);
//            var response = await client.SendEmailAsync(msg);
//        }
//    }
//}
