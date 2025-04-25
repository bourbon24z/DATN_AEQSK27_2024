namespace DATN.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string BuildEmailContent(string subject, string message)
        {
            
            return $@"
                <html>
                    <head>
                        <style>
                            body {{ font-family: Times New Roman, serif; font-size: 18px; }}
                            h2 {{ color: #2E86C1; }}
                            p {{ line-height: 1.5; }}
                            .footer {{ font-size: 10px; color: gray; margin-top: 20px; }}
                        </style>
                    </head>
                    <body>
                        <h2>{subject}</h2>
                        <p>{message}</p>
                        <hr/>
                        <p class='footer'>Email tự động từ hệ thống. Vui lòng không trả lời email này.</p>
                    </body>
                </html>";

           
        }
    }
}
