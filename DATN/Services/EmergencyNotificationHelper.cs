using DATN.Models;
using System;

namespace DATN.Services
{
    public static class EmergencyNotificationHelper
    {
        public static string CreateDoctorEmergencyEmail(
            StrokeUser patient,
            string locationLink,
            string mapLink,
            string additionalInfo = null)
        {
            string additionalContent = !string.IsNullOrEmpty(additionalInfo)
                ? $"<p style='margin: 15px 0; font-weight: bold;'>Nội dung: {additionalInfo}</p>"
                : "";

            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>THÔNG BÁO KHẨN CẤP</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                .emergency-header {{ background-color: #ff0000; color: white; padding: 15px; text-align: center; font-size: 20px; font-weight: bold; margin-bottom: 20px; border-radius: 5px; animation: blink 1s infinite; }}
                .patient-info {{ background-color: #fff8f8; border-left: 5px solid #ff0000; padding: 15px; margin-bottom: 20px; }}
                .location-info {{ background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-radius: 5px; }}
                .action-button {{ display: inline-block; background-color: #ff0000; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 10px; }}
                .contact-info {{ background-color: #fffaeb; border-left: 5px solid #ffc107; padding: 15px; margin-bottom: 20px; }}
                .timestamp {{ font-size: 12px; color: #666; text-align: right; margin-top: 20px; }}
                @keyframes blink {{ 0% {{ opacity: 1; }} 50% {{ opacity: 0.8; }} 100% {{ opacity: 1; }} }}
            </style>
        </head>
        <body>
            <div class='emergency-header'>
                🚨 THÔNG BÁO KHẨN CẤP: BỆNH NHÂN YÊU CẦU TRỢ GIÚP 🚨
            </div>
    
            <p><strong>Kính gửi Bác sĩ,</strong></p>
    
            <p>Bệnh nhân của bạn vừa kích hoạt nút khẩn cấp và cần được hỗ trợ ngay lập tức.</p>
    
            <div class='patient-info'>
                <h3 style='margin-top: 0; color: #ff0000;'>Thông tin bệnh nhân:</h3>
                <p><strong>Họ tên:</strong> {patient.PatientName}</p>
                <p><strong>ID:</strong> {patient.UserId}</p>
                <p><strong>Thời gian kích hoạt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                {additionalContent}
            </div>
    
            <div class='contact-info'>
                <h3 style='margin-top: 0; color: #d32f2f;'>Thông tin liên hệ khẩn cấp:</h3>
                <p><strong>Điện thoại:</strong> {patient.Phone}</p>
                {(!string.IsNullOrEmpty(patient.Email) ? $"<p><strong>Email:</strong> {patient.Email}</p>" : "")}
                <p><strong>Khuyến nghị:</strong> Vui lòng liên hệ ngay với bệnh nhân và/hoặc các dịch vụ cấp cứu y tế nếu cần.</p>
            </div>
    
            <div class='location-info'>
                <h3 style='margin-top: 0;'>Vị trí hiện tại của bệnh nhân:</h3>
                <p><a href='{locationLink}' style='color: #0066cc;'>Xem chi tiết trên hệ thống</a></p>
                <p><a href='{mapLink}' style='color: #0066cc;'>Xem trên bản đồ</a></p>
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

        /// <summary>
        /// Tạo email khẩn cấp định dạng đẹp cho người thân
        /// </summary>
        public static string CreateFamilyEmergencyEmail(
            StrokeUser patient,
            StrokeUser familyMember,
            string locationLink,
            string mapLink,
            string additionalInfo = null)
        {
            string additionalContent = !string.IsNullOrEmpty(additionalInfo)
                ? $"<p style='margin: 15px 0; font-weight: bold;'>Nội dung: {additionalInfo}</p>"
                : "";

            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>THÔNG BÁO KHẨN CẤP</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                .emergency-header {{ background-color: #ff0000; color: white; padding: 15px; text-align: center; font-size: 20px; font-weight: bold; margin-bottom: 20px; border-radius: 5px; animation: blink 1s infinite; }}
                .patient-info {{ background-color: #fff8f8; border-left: 5px solid #ff0000; padding: 15px; margin-bottom: 20px; }}
                .location-info {{ background-color: #f5f5f5; padding: 15px; margin-bottom: 20px; border-radius: 5px; }}
                .action-button {{ display: inline-block; background-color: #ff0000; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 10px; margin-right: 10px; }}
                .secondary-button {{ display: inline-block; background-color: #0066cc; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 10px; }}
                .contact-info {{ background-color: #fffaeb; border-left: 5px solid #ffc107; padding: 15px; margin-bottom: 20px; }}
                .timestamp {{ font-size: 12px; color: #666; text-align: right; margin-top: 20px; }}
                .action-area {{ margin: 20px 0; }}
                @keyframes blink {{ 0% {{ opacity: 1; }} 50% {{ opacity: 0.8; }} 100% {{ opacity: 1; }} }}
            </style>
        </head>
        <body>
            <div class='emergency-header'>
                🚨 THÔNG BÁO KHẨN CẤP: NGƯỜI THÂN CẦN TRỢ GIÚP 🚨
            </div>
    
            <p><strong>Kính gửi {familyMember.PatientName},</strong></p>
    
            <p>Người thân của bạn vừa kích hoạt nút khẩn cấp và có thể cần sự hỗ trợ ngay lập tức!</p>
    
            <div class='patient-info'>
                <h3 style='margin-top: 0; color: #ff0000;'>Thông tin người thân:</h3>
                <p><strong>Họ tên:</strong> {patient.PatientName}</p>
                <p><strong>Thời gian kích hoạt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                {additionalContent}
            </div>
    
            <div class='contact-info'>
                <h3 style='margin-top: 0; color: #d32f2f;'>Thông tin liên hệ:</h3>
                <p><strong>Điện thoại:</strong> {patient.Phone}</p>
                {(!string.IsNullOrEmpty(patient.Email) ? $"<p><strong>Email:</strong> {patient.Email}</p>" : "")}
            </div>
    
            <div class='location-info'>
                <h3 style='margin-top: 0;'>Vị trí hiện tại:</h3>
                <p>Người thân của bạn đã chia sẻ vị trí. Bạn có thể xem vị trí này để đến hỗ trợ họ.</p>
                <p><a href='{mapLink}' style='color: #0066cc;'>Xem vị trí trên bản đồ</a></p>
            </div>
    
            <div class='action-area'>
                <h3 style='color: #d32f2f;'>Hành động khẩn cấp:</h3>
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
    }
}