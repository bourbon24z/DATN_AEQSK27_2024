using DATN.Dto;
using DATN.Models;
using System;
using System.Text;

namespace DATN.Services
{
    public interface INotificationFormatterService
    {
        string FormatWarningMessage(string classification, string details, GPSDataDto location);
        string FormatEmergencyMessage(StrokeUser user, string locationLink, string additionalInfo = null);
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
        public string FormatEmergencyMessage(StrokeUser user, string locationLink, string additionalInfo = null)
        {
            string message = $"🚨 THÔNG BÁO KHẨN CẤP! 🚨\n\n" +
                           $"Bệnh nhân {user.PatientName} vừa bấm nút khẩn cấp!\n\n" +
                           $"Vui lòng liên hệ ngay qua số điện thoại: {user.Phone}\n";

            if (!string.IsNullOrEmpty(user.Email))
            {
                message += $"Hoặc email: {user.Email}\n\n";
            }

            message += $"Xem vị trí hiện tại của bệnh nhân: {locationLink}";

            if (!string.IsNullOrWhiteSpace(additionalInfo))
            {
                message += $"\n\nThông tin bổ sung: {additionalInfo}";
            }

            message += $"\n\nThời gian thông báo: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";

            return message;
        }
    }
}