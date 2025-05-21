using DATN.Models;
using System;
using System.Collections.Generic;

namespace DATN.Services
{
    public static class HealthNotificationHelper
    {
        public static string CreateHealthWarningEmail(
            StrokeUser patient,
            StrokeUser recipient,
            string title,
            List<string> abnormalReadings,
            string type = "warning")
        {
            
            string borderColor = type == "warning" ? "#ff0000" : "#ff9800";
            string bgColor = type == "warning" ? "#fff1f0" : "#fff8e1";
            string headerBgColor = type == "warning" ? "#ff0000" : "#ff9800";

            
            string readingsHtml = "";
            foreach (var reading in abnormalReadings)
            {
                readingsHtml += $"<li style='margin-bottom:8px;'>{reading}</li>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .warning-header {{ background-color: {headerBgColor}; color: white; padding: 15px; text-align: center; font-size: 18px; font-weight: bold; margin-bottom: 20px; border-radius: 5px; }}
        .patient-info {{ background-color: {bgColor}; border-left: 5px solid {borderColor}; padding: 15px; margin-bottom: 20px; }}
        .readings-list {{ background-color: #f8f8f8; padding: 15px; margin-bottom: 20px; border-radius: 5px; }}
        .action-button {{ display: inline-block; background-color: #0066cc; color: white; padding: 10px 15px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 10px; }}
        .timestamp {{ font-size: 12px; color: #666; text-align: right; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='warning-header'>
        {title}
    </div>
    
    <p><strong>Kính gửi {recipient.PatientName},</strong></p>
    
    <p>Hệ thống giám sát sức khỏe đã phát hiện chỉ số bất thường đối với {(recipient.UserId == patient.UserId ? "bạn" : $"bệnh nhân {patient.PatientName}")}.</p>
    
    <div class='patient-info'>
        <h3 style='margin-top: 0; color: {borderColor};'>Thông tin bệnh nhân:</h3>
        <p><strong>Họ tên:</strong> {patient.PatientName}</p>
        <p><strong>Thời gian phát hiện:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
    </div>
    
    <div class='readings-list'>
        <h3 style='margin-top: 0;'>Các chỉ số bất thường:</h3>
        <ul>
            {readingsHtml}
        </ul>
    </div>
    
    <p>Vui lòng kiểm tra ứng dụng để biết thêm chi tiết và đề xuất xử lý.</p>
    
    <a href='tel:115' class='action-button'>📞 Gọi cấp cứu nếu cần thiết</a>
    
    <div class='timestamp'>
        Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
    </div>
</body>
</html>
            ";
        }
    }
}