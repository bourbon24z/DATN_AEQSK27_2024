using DATN.Models;
using System;

namespace DATN.Services
{
    public static class EmergencyNotificationHelper
    {
        public static string CreateDoctorEmergencyEmail(
            StrokeUser patient,
            string locationLink,
            double latitude,
            double longitude,
            string additionalInfo = null)
        {
            string additionalContent = !string.IsNullOrEmpty(additionalInfo)
                ? $"<p style='margin: 15px 0; font-weight: bold;'>Nội dung: {additionalInfo}</p>"
                : "";

            string openMapLink = $"https://www.openstreetmap.org/?mlat={latitude}&mlon={longitude}#map=16/{latitude}/{longitude}";
            string googleMapsLink = $"https://www.google.com/maps?q={latitude},{longitude}";

            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>THÔNG BÁO KHẨN CẤP</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                .emergency-header {{ background-color: #ff0000; color: white; padding: 15px; text-align: center; font-size: 20px; font-weight: bold; margin-bottom: 20px; border-radius: 5px; animation: blink 1s infinite; box-shadow: 0 4px 8px rgba(255, 0, 0, 0.2); }}
                .patient-info {{ background-color: #fff8f8; border-left: 5px solid #ff0000; padding: 15px; margin-bottom: 20px; border-radius: 0 5px 5px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .location-info {{ background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .action-button {{ display: inline-block; background-color: #ff0000; color: white !important; padding: 12px 20px; text-decoration: none !important; border-radius: 5px; font-weight: bold; margin-top: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.2); transition: all 0.3s ease; }}
                .action-button:hover {{ background-color: #d32f2f; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }}
                .map-button {{ display: inline-block; background-color: #0066cc; color: white !important; padding: 10px 15px; text-decoration: none !important; border-radius: 5px; font-weight: bold; margin-top: 10px; margin-right: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); transition: all 0.3s ease; }}
                .map-button:hover {{ background-color: #0055aa; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }}
                .contact-info {{ background-color: #fffaeb; border-left: 5px solid #ffc107; padding: 15px; margin-bottom: 20px; border-radius: 0 5px 5px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .timestamp {{ font-size: 12px; color: #666; text-align: right; margin-top: 20px; padding-top: 10px; border-top: 1px solid #eee; }}
                .coords {{ margin-top: 8px; font-size: 12px; color: #666; background-color: #f0f0f0; padding: 5px; border-radius: 3px; display: inline-block; }}
                @keyframes blink {{ 0% {{ opacity: 1; }} 50% {{ opacity: 0.8; }} 100% {{ opacity: 1; }} }}
                h3 {{ margin-top: 0; color: #d32f2f; }}
            </style>
        </head>
        <body>
            <div class='emergency-header'>
                🚨 THÔNG BÁO KHẨN CẤP: BỆNH NHÂN YÊU CẦU TRỢ GIÚP 🚨
            </div>
    
            <p><strong>Kính gửi Bác sĩ,</strong></p>
    
            <p>Bệnh nhân của bạn vừa kích hoạt nút khẩn cấp và cần được hỗ trợ ngay lập tức.</p>
    
            <div class='patient-info'>
                <h3>Thông tin bệnh nhân:</h3>
                <p><strong>Họ tên:</strong> {patient.PatientName}</p>
                <p><strong>ID:</strong> {patient.UserId}</p>
                <p><strong>Thời gian kích hoạt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                {additionalContent}
            </div>
    
            <div class='contact-info'>
                <h3>Thông tin liên hệ khẩn cấp:</h3>
                <p><strong>Điện thoại:</strong> <a href='tel:{patient.Phone}' style='color: #d32f2f;'>{patient.Phone}</a></p>
                {(!string.IsNullOrEmpty(patient.Email) ? $"<p><strong>Email:</strong> <a href='mailto:{patient.Email}' style='color: #0066cc;'>{patient.Email}</a></p>" : "")}
                <p><strong>Khuyến nghị:</strong> Vui lòng liên hệ ngay với bệnh nhân và/hoặc các dịch vụ cấp cứu y tế nếu cần.</p>
            </div>
    
            <div class='location-info'>
                <h3>Vị trí hiện tại của bệnh nhân:</h3>
                <p><a href='{locationLink}' style='color: #0066cc;'>Xem chi tiết trên hệ thống</a></p>
                <div>
                    <a href='{openMapLink}' target='_blank' class='map-button'>🗺️ OpenStreetMap</a>
                    <a href='{googleMapsLink}' target='_blank' class='map-button'>🗺️ Google Maps</a>
                </div>
                <div class='coords'>Tọa độ GPS: {latitude}, {longitude}</div>
                <a href='tel:115' class='action-button'>📞 GỌI CẤP CỨU (115)</a>
            </div>
    
            <p>Đây là thông báo tự động từ hệ thống giám sát sức khỏe. Vui lòng phản hồi kịp thời.</p>
    
            <div class='timestamp'>
                Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
            </div>
        </body>
        </html>
                    ";
        }

        public static string CreateFamilyEmergencyEmail(
            StrokeUser patient,
            StrokeUser familyMember,
            string locationLink,
            double latitude,
            double longitude,
            string additionalInfo = null)
        {
            string additionalContent = !string.IsNullOrEmpty(additionalInfo)
                ? $"<p style='margin: 15px 0; font-weight: bold;'>Nội dung: {additionalInfo}</p>"
                : "";

            string openMapLink = $"https://www.openstreetmap.org/?mlat={latitude}&mlon={longitude}#map=16/{latitude}/{longitude}";
            string googleMapsLink = $"https://www.google.com/maps?q={latitude},{longitude}";

            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>THÔNG BÁO KHẨN CẤP</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                .emergency-header {{ background-color: #ff0000; color: white; padding: 15px; text-align: center; font-size: 20px; font-weight: bold; margin-bottom: 20px; border-radius: 5px; animation: blink 1s infinite; box-shadow: 0 4px 8px rgba(255, 0, 0, 0.2); }}
                .patient-info {{ background-color: #fff8f8; border-left: 5px solid #ff0000; padding: 15px; margin-bottom: 20px; border-radius: 0 5px 5px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .location-info {{ background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .action-button {{ display: inline-block; background-color: #ff0000; color: white !important; padding: 12px 20px; text-decoration: none !important; border-radius: 5px; font-weight: bold; margin-top: 10px; margin-right: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.2); transition: all 0.3s ease; }}
                .action-button:hover {{ background-color: #d32f2f; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }}
                .secondary-button {{ display: inline-block; background-color: #0066cc; color: white !important; padding: 12px 20px; text-decoration: none !important; border-radius: 5px; font-weight: bold; margin-top: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); transition: all 0.3s ease; }}
                .secondary-button:hover {{ background-color: #0055aa; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }}
                .map-button {{ display: inline-block; background-color: #0066cc; color: white !important; padding: 10px 15px; text-decoration: none !important; border-radius: 5px; font-weight: bold; margin-top: 10px; margin-right: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); transition: all 0.3s ease; }}
                .map-button:hover {{ background-color: #0055aa; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }}
                .contact-info {{ background-color: #fffaeb; border-left: 5px solid #ffc107; padding: 15px; margin-bottom: 20px; border-radius: 0 5px 5px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .timestamp {{ font-size: 12px; color: #666; text-align: right; margin-top: 20px; padding-top: 10px; border-top: 1px solid #eee; }}
                .action-area {{ margin: 20px 0; background-color: #fff4f4; padding: 15px; border-radius: 5px; border-left: 5px solid #ff6b6b; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .coords {{ margin-top: 8px; font-size: 12px; color: #666; background-color: #f0f0f0; padding: 5px; border-radius: 3px; display: inline-block; }}
                @keyframes blink {{ 0% {{ opacity: 1; }} 50% {{ opacity: 0.8; }} 100% {{ opacity: 1; }} }}
                h3 {{ margin-top: 0; color: #d32f2f; }}
            </style>
        </head>
        <body>
            <div class='emergency-header'>
                🚨 THÔNG BÁO KHẨN CẤP: NGƯỜI THÂN CẦN TRỢ GIÚP 🚨
            </div>
    
            <p><strong>Kính gửi {familyMember.PatientName},</strong></p>
    
            <p>Người thân của bạn vừa kích hoạt nút khẩn cấp và có thể cần sự hỗ trợ ngay lập tức!</p>
    
            <div class='patient-info'>
                <h3>Thông tin người thân:</h3>
                <p><strong>Họ tên:</strong> {patient.PatientName}</p>
                <p><strong>Thời gian kích hoạt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                {additionalContent}
            </div>
    
            <div class='contact-info'>
                <h3>Thông tin liên hệ:</h3>
                <p><strong>Điện thoại:</strong> <a href='tel:{patient.Phone}' style='color: #d32f2f;'>{patient.Phone}</a></p>
                {(!string.IsNullOrEmpty(patient.Email) ? $"<p><strong>Email:</strong> <a href='mailto:{patient.Email}' style='color: #0066cc;'>{patient.Email}</a></p>" : "")}
            </div>
    
            <div class='location-info'>
                <h3>Vị trí hiện tại:</h3>
                <p>Người thân của bạn đã chia sẻ vị trí. Bạn có thể xem vị trí này để đến hỗ trợ họ.</p>
                <div>
                    <a href='{openMapLink}' target='_blank' class='map-button'>🗺️ OpenStreetMap</a>
                    <a href='{googleMapsLink}' target='_blank' class='map-button'>🗺️ Google Maps</a>
                </div>
                <div class='coords'>Tọa độ GPS: {latitude}, {longitude}</div>
            </div>
    
            <div class='action-area'>
                <h3>Hành động khẩn cấp:</h3>
                <a href='tel:{patient.Phone}' class='action-button'>📞 GỌI CHO NGƯỜI THÂN</a>
                <a href='tel:115' class='secondary-button'>📞 GỌI CẤP CỨU (115)</a>
            </div>
    
            <p>Đây là thông báo tự động từ hệ thống giám sát sức khỏe. Vui lòng phản hồi kịp thời.</p>
    
            <div class='timestamp'>
                Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
            </div>
        </body>
        </html>
                    ";
        }

        public static string CreateUserEmergencyEmail(
            StrokeUser patient,
            string locationLink,
            double latitude,
            double longitude,
            string additionalInfo = null)
        {
            string additionalContent = !string.IsNullOrEmpty(additionalInfo)
                ? $"<p style='margin: 15px 0; font-weight: bold;'>Nội dung: {additionalInfo}</p>"
                : "";

            string openMapLink = $"https://www.openstreetmap.org/?mlat={latitude}&mlon={longitude}#map=16/{latitude}/{longitude}";
            string googleMapsLink = $"https://www.google.com/maps?q={latitude},{longitude}";

            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>THÔNG BÁO KHẨN CẤP</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                .emergency-header {{ background-color: #ff0000; color: white; padding: 15px; text-align: center; font-size: 20px; font-weight: bold; margin-bottom: 20px; border-radius: 5px; animation: blink 1s infinite; box-shadow: 0 4px 8px rgba(255, 0, 0, 0.2); }}
                .user-info {{ background-color: #fff8f8; border-left: 5px solid #ff0000; padding: 15px; margin-bottom: 20px; border-radius: 0 5px 5px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .location-info {{ background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .action-button {{ display: inline-block; background-color: #ff0000; color: white !important; padding: 12px 20px; text-decoration: none !important; border-radius: 5px; font-weight: bold; margin-top: 15px; box-shadow: 0 2px 4px rgba(0,0,0,0.2); transition: all 0.3s ease; }}
                .action-button:hover {{ background-color: #d32f2f; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }}
                .map-button {{ display: inline-block; background-color: #0066cc; color: white !important; padding: 10px 15px; text-decoration: none !important; border-radius: 5px; font-weight: bold; margin-top: 10px; margin-right: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); transition: all 0.3s ease; }}
                .map-button:hover {{ background-color: #0055aa; transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); }}
                .contact-info {{ background-color: #fffaeb; border-left: 5px solid #ffc107; padding: 15px; margin-bottom: 20px; border-radius: 0 5px 5px 0; }}
                .timestamp {{ font-size: 12px; color: #666; text-align: right; margin-top: 20px; padding-top: 10px; border-top: 1px solid #eee; }}
                .notification-area {{ background-color: #e8f5e9; border-left: 5px solid #4caf50; padding: 15px; margin-bottom: 20px; border-radius: 0 5px 5px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                .coords {{ margin-top: 8px; font-size: 12px; color: #666; background-color: #f0f0f0; padding: 5px; border-radius: 3px; display: inline-block; }}
                .resolved {{ background-color: #e3f2fd; border-left: 5px solid #2196f3; padding: 15px; margin: 20px 0; border-radius: 0 5px 5px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }}
                @keyframes blink {{ 0% {{ opacity: 1; }} 50% {{ opacity: 0.8; }} 100% {{ opacity: 1; }} }}
                h3 {{ margin-top: 0; color: #2e7d32; }}
                .user-info h3 {{ color: #d32f2f; }}
                .location-info h3 {{ color: #0066cc; }}
            </style>
        </head>
        <body>
            <div class='emergency-header'>
                🚨 THÔNG BÁO KHẨN CẤP: NÚT KHẨN CẤP ĐÃ ĐƯỢC KÍCH HOẠT 🚨
            </div>
    
            <p><strong>Kính gửi {patient.PatientName},</strong></p>
    
            <p>Bạn vừa kích hoạt nút khẩn cấp. Thông tin về tình trạng khẩn cấp của bạn đã được gửi đến bác sĩ và người thân.</p>
    
            <div class='notification-area'>
                <h3>Thông báo đã được gửi đến:</h3>
                <p><strong>✓ Bác sĩ của bạn</strong></p>
                <p><strong>✓ Người thân của bạn</strong></p>
                <p>Họ sẽ liên hệ với bạn trong thời gian sớm nhất.</p>
            </div>
    
            <div class='user-info'>
                <h3>Thông tin của bạn:</h3>
                <p><strong>Họ tên:</strong> {patient.PatientName}</p>
                <p><strong>Điện thoại:</strong> <a href='tel:{patient.Phone}' style='color: #d32f2f;'>{patient.Phone}</a></p>
                <p><strong>Thời gian kích hoạt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                {additionalContent}
            </div>
    
            <div class='location-info'>
                <h3>Vị trí hiện tại của bạn:</h3>
                <p><a href='{locationLink}' style='color: #0066cc;'>Xem chi tiết trên hệ thống</a></p>
                <div>
                    <a href='{openMapLink}' target='_blank' class='map-button'>🗺️ OpenStreetMap</a>
                    <a href='{googleMapsLink}' target='_blank' class='map-button'>🗺️ Google Maps</a>
                </div>
                <div class='coords'>Tọa độ GPS: {latitude}, {longitude}</div>
                <a href='tel:115' class='action-button'>📞 GỌI CẤP CỨU (115)</a>
            </div>
    
            <div class='resolved'>
                <h3 style='color: #1565c0;'>Đánh dấu tình trạng đã giải quyết:</h3>
                <p>Nếu đây là kích hoạt do nhầm lẫn hoặc tình huống đã được giải quyết, bạn có thể đánh dấu bằng cách đăng nhập vào ứng dụng và chọn <strong>""Đã Giải Quyết""</strong> trong chi tiết cảnh báo.</p>
            </div>

            <p>Nếu đây là yêu cầu khẩn cấp thực sự, vui lòng gọi ngay cho dịch vụ cấp cứu (115) hoặc liên hệ với cơ sở y tế gần nhất.</p>

            <div class='timestamp'>
                Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
            </div>
        </body>
        </html>
                    ";
        }
    }
}