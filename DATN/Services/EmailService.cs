using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    //send mail infor
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        if (string.IsNullOrEmpty(email))
        {
            Console.WriteLine("Email address is null or empty");
            throw new ArgumentNullException(nameof(email), "Email address cannot be null or empty.");
        }

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Huy Nguyen Cute Pho Mai Que Nhat The Gioi", _configuration["Smtp:Username"]));
        emailMessage.To.Add(new MailboxAddress(email, email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };

        try
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            Console.WriteLine($"Email sent to {email}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
            throw;
        }
    }
}
