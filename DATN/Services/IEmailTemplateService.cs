namespace DATN.Services
{
    public interface IEmailTemplateService
    {
        string BuildEmailContent(string subject, string message);
    }
}
