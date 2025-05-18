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


            sb.Append("<div style='border: 3px solid #ff0000; border-radius: 8px; padding: 15px; background-color: #fff1f0; margin-bottom: 15px; animation: pulse 1.5s infinite; box-shadow: 0 0 10px rgba(255,0,0,0.5);'>");

            
            sb.Append("<div style='font-weight: bold; font-size: 1.5em; color: #ff0000; margin-bottom: 12px; text-align: center; text-transform: uppercase; text-shadow: 1px 1px 3px rgba(0,0,0,0.2);'>");
            sb.Append($"⚠️ CẢNH BÁO: {classification} ⚠️");
            sb.Append("</div>");

           
            sb.Append("<div style='margin-bottom: 12px; background-color: #ffdddd; padding: 8px; border-radius: 5px; font-weight: bold;'>");
            sb.Append($"⏰ Thời gian phát hiện: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}");
            sb.Append("</div>");

            
            sb.Append("<div style='margin-bottom: 12px; background-color: #fff; padding: 10px; border-left: 5px solid #ff0000; border-radius: 0 5px 5px 0;'>");
            sb.Append("<div style='font-weight: bold; color: #ff0000; font-size: 1.1em; margin-bottom: 8px;'>🔴 CHI TIẾT QUAN TRỌNG:</div>");

            
            string[] detailsArray = details.Split(';');
            foreach (var detail in detailsArray)
            {
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    sb.Append("<div style='margin-left: 15px; margin-bottom: 5px; font-weight: bold;'>🔹 ");
                    sb.Append(detail.Trim());
                    sb.Append("</div>");
                }
            }
            sb.Append("</div>");

            
            if (location != null &&
                (Math.Abs(location.Lat) > 0.0001f || Math.Abs(location.Long) > 0.0001f))
            {
                sb.Append("<div style='margin-bottom: 12px;'>");
                sb.Append("<div style='font-weight: bold; color: #ff0000;'>📍 VỊ TRÍ:</div>");
                sb.Append($"<a href='https://www.openstreetmap.org/?mlat={location.Lat}&mlon={location.Long}&zoom=15' target='_blank' style='display: inline-block; background-color: #ff3b30; color: white; padding: 8px 15px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 5px;'>");
                sb.Append("👉 XEM BẢN ĐỒ NGAY");
                sb.Append("</a>");
                sb.Append("</div>");
            }

            
            sb.Append("<div style='margin-top: 15px; font-weight: bold; background-color: #ffdddd; padding: 10px; border-radius: 5px; border-left: 5px solid #ff0000;'>");
            sb.Append("⚠️ CẢNH BÁO: Cần kiểm tra sức khỏe NGAY LẬP TỨC hoặc liên hệ các cơ quan y tế gần nhất để được xử lý kịp thời! Tình trạng này có thể gây nguy hiểm nếu không được xử lý ngay lập tức.");
            sb.Append("</div>");

           
            sb.Append("<div style='text-align: center; margin-top: 15px;'>");
            sb.Append("<a href='tel:115' style='display: inline-block; background-color: #ff3b30; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-right: 10px;'>");
            sb.Append("📞 GỌI CẤP CỨU");
            sb.Append("</a>");
            sb.Append("</div>");

            
            sb.Append("<style>");
            sb.Append("@keyframes pulse {");
            sb.Append("0% { box-shadow: 0 0 0 0 rgba(255, 0, 0, 0.7); }");
            sb.Append("70% { box-shadow: 0 0 0 10px rgba(255, 0, 0, 0); }");
            sb.Append("100% { box-shadow: 0 0 0 0 rgba(255, 0, 0, 0); }");
            sb.Append("}");
            sb.Append("</style>");

            
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