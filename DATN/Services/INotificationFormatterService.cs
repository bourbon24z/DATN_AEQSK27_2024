using DATN.Dto;
using System;
using System.Text;

namespace DATN.Services
{
    public interface INotificationFormatterService
    {
        string FormatWarningMessage(string classification, string details, GPSDataDto location);
    }

    public class NotificationFormatterService : INotificationFormatterService
    {
        public string FormatWarningMessage(string classification, string details, GPSDataDto location)
        {
            var sb = new StringBuilder();

            
            sb.Append("<div style='font-weight: bold; font-size: 1.2em; margin-bottom: 10px;'>");
            sb.Append($"❗ {classification} ❗");
            sb.Append("</div>");

            
            sb.Append("<div style='margin-bottom: 10px;'>");
            sb.Append($"⏰ Thời gian phát hiện: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}");
            sb.Append("</div>");

            
            sb.Append("<div style='margin-bottom: 10px;'>");
            sb.Append("<div><strong>📊 CHI TIẾT:</strong></div>");

            
            string[] detailsArray = details.Split(';');
            foreach (var detail in detailsArray)
            {
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    sb.Append("<div style='margin-left: 20px;'>• ");
                    sb.Append(detail.Trim());
                    sb.Append("</div>");
                }
            }
            sb.Append("</div>");

            
            if (location != null &&
                (Math.Abs(location.Lat) > 0.0001f || Math.Abs(location.Long) > 0.0001f))
            {
                sb.Append("<div style='margin-bottom: 10px;'>");
                sb.Append("<div><strong>📍 VỊ TRÍ:</strong></div>");
                sb.Append($"<a href='https://www.openstreetmap.org/?mlat={location.Lat}&mlon={location.Long}&zoom=15' target='_blank'>");
                sb.Append("Xem bản đồ");
                sb.Append("</a>");
                sb.Append("</div>");
            }

            
            sb.Append("<div style='margin-top: 10px; font-style: italic;'>");
            sb.Append("⚠️ Vui lòng kiểm tra sức khỏe hoặc liên hệ với bác sĩ nếu tình trạng kéo dài.");
            sb.Append("</div>");

            return sb.ToString();
        }
    }
}