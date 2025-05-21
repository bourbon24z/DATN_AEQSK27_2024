using DATN.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class PatientNotificationService : IPatientNotificationService
    {
        private readonly StrokeDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly IMobileNotificationService _mobileNotificationService;
        private readonly ILogger<PatientNotificationService> _logger;

        public PatientNotificationService(
            StrokeDbContext dbContext,
            INotificationService notificationService,
            IMobileNotificationService mobileNotificationService,
            ILogger<PatientNotificationService> logger)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            _mobileNotificationService = mobileNotificationService;
            _logger = logger;
        }

        public async Task SendNotificationToPatientDoctorsAsync(
             int patientId,
             string title,
             string message,
             string type = "warning",
             bool saveWarning = false,
             List<string> detailsList = null)
        {
            try
            {
                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bắt đầu gửi thông báo cho bác sĩ của bệnh nhân ID {patientId}");

                if (patientId <= 0)
                {
                    _logger.LogWarning($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] patientId không hợp lệ: {patientId}");
                    return;
                }

               
                var patient = await _dbContext.StrokeUsers.FindAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không tìm thấy thông tin bệnh nhân ID {patientId}");
                    return;
                }

                
                var doctorIds = await _dbContext.Relationships
                    .Where(r => r.UserId == patientId && r.RelationshipType == "doctor-patient")
                    .Select(r => r.InviterId)
                    .Distinct()
                    .ToListAsync();

                if (!doctorIds.Any())
                {
                    _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không tìm thấy bác sĩ nào liên kết với bệnh nhân {patient.PatientName} (ID: {patientId})");
                    return;
                }

                
                var doctors = await _dbContext.StrokeUsers
                    .Where(u => doctorIds.Contains(u.UserId))
                    .ToListAsync();

                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Gửi thông báo đến {doctors.Count} bác sĩ của bệnh nhân {patient.PatientName} (ID: {patientId})");

                foreach (var doctor in doctors)
                {
                    try
                    {
                       
                        await _notificationService.SendWebNotificationAsync(
                            doctor.UserId, title, message, type, saveWarning);

                        
                        if (_mobileNotificationService != null)
                        {
                            await _mobileNotificationService.SendNotificationToUserAsync(
                                doctor.UserId, title, message, type, null);
                        }

                       
                        if (!string.IsNullOrEmpty(doctor.Email))
                        {
                            string emailSubject = $"{(type == "warning" ? "⚠️ NGUY HIỂM" : "⚠️ CẢNH BÁO")}: Bệnh nhân {patient.PatientName}";

                            string emailBody;
                            if (type == "emergency" || type == "test")
                            {
                                
                                emailBody = $@"
                                <!DOCTYPE html>
                                <html>
                                <head>
                                    <meta charset='UTF-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                    <title>THÔNG BÁO KHẨN CẤP</title>
                                    <style>
                                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                                        .emergency-header {{ background-color: #ff0000; color: white; padding: 15px; text-align: center; font-size: 20px; font-weight: bold; margin-bottom: 20px; border-radius: 5px; }}
                                        .patient-info {{ background-color: #fff8f8; border-left: 5px solid #ff0000; padding: 15px; margin-bottom: 20px; }}
                                        .content {{ padding: 15px; margin: 15px 0; border-left: 5px solid #ff0000; background-color: #fff1f0; }}
                                    </style>
                                </head>
                                <body>
                                    <div class='emergency-header'>
                                        {title}
                                    </div>
                                    
                                    <p>Kính gửi Bác sĩ {doctor.PatientName},</p>
                                    
                                    <p><strong>Bệnh nhân {patient.PatientName} đang trong tình trạng khẩn cấp!</strong></p>
                                    
                                    <div class='patient-info'>
                                        <h3 style='margin-top: 0; color: #ff0000;'>Thông tin bệnh nhân:</h3>
                                        <p><strong>Họ tên:</strong> {patient.PatientName}</p>
                                        <p><strong>ID:</strong> {patient.UserId}</p>
                                        <p><strong>Thời gian kích hoạt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                                    </div>
                                    
                                    <div class='content'>
                                        {message}
                                    </div>
                                    
                                    <p>Vui lòng kiểm tra và liên hệ ngay với bệnh nhân.</p>
                                    <p>Trân trọng,<br>Hệ thống Giám sát Sức khỏe</p>
                                </body>
                                </html>
                                ";
                            }
                            else if (detailsList != null && detailsList.Count > 0)
                            {
                                
                                string borderColor = type == "warning" ? "#ff0000" : "#ff9800";
                                string bgColor = type == "warning" ? "#fff1f0" : "#fff8e1";
                                string headerBgColor = type == "warning" ? "#ff0000" : "#ff9800";

                               
                                string readingsHtml = "";
                                foreach (var reading in detailsList)
                                {
                                    readingsHtml += $"<li style='margin-bottom:8px;'>{reading}</li>";
                                }

                                emailBody = $@"
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
                                    
                                    <p><strong>Kính gửi Bác sĩ {doctor.PatientName},</strong></p>
                                    
                                    <p>Hệ thống giám sát sức khỏe đã phát hiện chỉ số bất thường đối với bệnh nhân {patient.PatientName}.</p>
                                    
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
                            else
                            {
                                
                                emailBody = $@"
                                <h2>{title}</h2>
                                <p>Kính gửi Bác sĩ {doctor.PatientName},</p>
                                <p>Hệ thống giám sát sức khỏe phát hiện bất thường ở bệnh nhân {patient.PatientName}:</p>
                                <div style='padding: 10px; margin: 15px 0; border-left: 4px solid {(type == "warning" ? "#ff0000" : "#ff9800")}; background-color: {(type == "warning" ? "#fff1f0" : "#fff8e1")}'>
                                    {message}
                                </div>
                                <p>Vui lòng kiểm tra ứng dụng để biết thêm chi tiết.</p>
                                <p>Trân trọng,<br>Hệ thống Giám sát Sức khỏe</p>
                                ";
                            }

                            await _notificationService.SendNotificationAsync(
                                doctor.Email, emailSubject, emailBody);

                            _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Đã gửi email thông báo đến bác sĩ {doctor.PatientName} ({doctor.Email})");
                        }
                        else
                        {
                            _logger.LogWarning($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bác sĩ {doctor.PatientName} (ID: {doctor.UserId}) không có email");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lỗi khi gửi thông báo cho bác sĩ {doctor.PatientName} (ID: {doctor.UserId})");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lỗi khi gửi thông báo đến bác sĩ của bệnh nhân ID {patientId}");
            }
        }

        public async Task SendNotificationToPatientFamilyAsync(
            int patientId,
            string title,
            string message,
            string type = "warning",
            bool saveWarning = true,
            List<string> detailsList = null)
        {
            try
            {
                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bắt đầu gửi thông báo cho gia đình của bệnh nhân ID {patientId}");

                var patient = await _dbContext.StrokeUsers
                    .FirstOrDefaultAsync(u => u.UserId == patientId);

                if (patient == null)
                {
                    _logger.LogWarning($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không tìm thấy thông tin bệnh nhân ID {patientId}");
                    return;
                }

                var familyRelationships = await _dbContext.Relationships
                    .Where(r => (r.UserId == patientId || r.InviterId == patientId) &&
                              r.RelationshipType == "family")
                    .ToListAsync();

                if (!familyRelationships.Any())
                {
                    _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không tìm thấy gia đình nào liên kết với bệnh nhân ID {patientId}");
                    return;
                }

                var familyIds = new List<int>();
                foreach (var relationship in familyRelationships)
                {
                    if (relationship.UserId == patientId)
                        familyIds.Add(relationship.InviterId);
                    else
                        familyIds.Add(relationship.UserId);
                }

                familyIds = familyIds.Distinct().ToList();

                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Gửi thông báo đến {familyIds.Count} thành viên gia đình của bệnh nhân ID {patientId}");

                var familyMembers = await _dbContext.StrokeUsers
                    .Where(u => familyIds.Contains(u.UserId))
                    .ToListAsync();

                var allTasks = new List<Task>();

                foreach (var familyMember in familyMembers)
                {
                    try
                    {
                        _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Chuẩn bị gửi thông báo cho thành viên gia đình {familyMember.PatientName} (ID {familyMember.UserId})");

                       
                        allTasks.Add(_notificationService.SendWebNotificationAsync(
                            familyMember.UserId, title, message, type, saveWarning));

                       
                        allTasks.Add(_mobileNotificationService.SendNotificationToUserAsync(
                            familyMember.UserId, title, message, type, null));

                      
                        if (!string.IsNullOrEmpty(familyMember.Email))
                        {
                            string emailSubject = $"{(type == "warning" ? "⚠️ NGUY HIỂM" : "⚠️ CẢNH BÁO")}: Người thân {patient.PatientName}";

                            string emailBody;
                            if (type == "emergency" || type == "test")
                            {
                                
                                emailBody = $@"
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
                                        .content {{ padding: 15px; margin: 15px 0; border-left: 5px solid #ff0000; background-color: #fff1f0; }}
                                        .action-button {{ display: inline-block; background-color: #ff0000; color: white; padding: 12px 20px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 10px; margin-right: 10px; }}
                                        @keyframes blink {{ 0% {{ opacity: 1; }} 50% {{ opacity: 0.8; }} 100% {{ opacity: 1; }} }}
                                    </style>
                                </head>
                                <body>
                                    <div class='emergency-header'>
                                        {title}
                                    </div>
                                    
                                    <p>Kính gửi {familyMember.PatientName},</p>
                                    
                                    <p><strong>Người thân {patient.PatientName} của bạn đang trong tình trạng khẩn cấp!</strong></p>
                                    
                                    <div class='patient-info'>
                                        <h3 style='margin-top: 0; color: #ff0000;'>Thông tin người thân:</h3>
                                        <p><strong>Họ tên:</strong> {patient.PatientName}</p>
                                        <p><strong>Thời gian kích hoạt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                                    </div>
                                    
                                    <div class='content'>
                                        {message}
                                    </div>
                                    
                                    <p>Vui lòng kiểm tra và liên hệ ngay với người thân của bạn.</p>
                                    
                                    <div>
                                        <a href='tel:{patient.Phone}' class='action-button'>📞 Gọi cho người thân</a>
                                        <a href='tel:115' class='action-button' style='background-color: #0066cc;'>📞 Gọi cấp cứu (115)</a>
                                    </div>
                                    
                                    <p>Trân trọng,<br>Hệ thống Giám sát Sức khỏe</p>
                                </body>
                                </html>
                                ";
                            }
                            else if (detailsList != null && detailsList.Count > 0)
                            {
                                
                                string borderColor = type == "warning" ? "#ff0000" : "#ff9800";
                                string bgColor = type == "warning" ? "#fff1f0" : "#fff8e1";
                                string headerBgColor = type == "warning" ? "#ff0000" : "#ff9800";

                               
                                string readingsHtml = "";
                                foreach (var reading in detailsList)
                                {
                                    readingsHtml += $"<li style='margin-bottom:8px;'>{reading}</li>";
                                }

                                emailBody = $@"
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
                                    
                                    <p><strong>Kính gửi {familyMember.PatientName},</strong></p>
                                    
                                    <p>Hệ thống giám sát sức khỏe đã phát hiện chỉ số bất thường đối với người thân {patient.PatientName} của bạn.</p>
                                    
                                    <div class='patient-info'>
                                        <h3 style='margin-top: 0; color: {borderColor};'>Thông tin người thân:</h3>
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
                            else
                            {
                               
                                emailBody = $@"
                                <h2>{title}</h2>
                                <p>Kính gửi {familyMember.PatientName},</p>
                                <p>Hệ thống giám sát sức khỏe phát hiện bất thường ở bệnh nhân {patient.PatientName}:</p>
                                <div style='padding: 10px; margin: 15px 0; border-left: 4px solid {(type == "warning" ? "#ff0000" : "#ff9800")}; background-color: {(type == "warning" ? "#fff1f0" : "#fff8e1")}'>
                                    {message}
                                </div>
                                <p>Vui lòng kiểm tra ứng dụng để biết thêm chi tiết.</p>
                                <p>Trân trọng,<br>Hệ thống Giám sát Sức khỏe</p>
                                ";
                            }

                            allTasks.Add(_notificationService.SendNotificationAsync(
                                familyMember.Email, emailSubject, emailBody));

                            _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Đã đưa email vào hàng đợi để gửi đến {familyMember.Email} (thành viên gia đình ID {familyMember.UserId})");
                        }
                        else
                        {
                            _logger.LogWarning($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Thành viên gia đình {familyMember.PatientName} (ID {familyMember.UserId}) không có email");
                        }
                    }
                    catch (Exception memberEx)
                    {
                        _logger.LogError(memberEx, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lỗi khi gửi thông báo cho thành viên gia đình ID {familyMember.UserId}");
                    }
                }

                await Task.WhenAll(allTasks);

                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Đã gửi thông báo thành công đến {familyMembers.Count} thành viên gia đình của bệnh nhân {patient.PatientName} (ID {patientId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lỗi khi gửi thông báo đến gia đình của bệnh nhân ID {patientId}");
            }
        }

        public async Task SendNotificationToPatientCircleAsync(
            int patientId,
            string title,
            string message,
            string type = "warning",
            List<string> detailsList = null)
        {
            try
            {
                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bắt đầu gửi thông báo đến vòng tròn của bệnh nhân ID {patientId}");

                var patient = await _dbContext.StrokeUsers.FindAsync(patientId);
                string patientName = patient?.PatientName ?? "Unknown";

                var doctorIds = await _dbContext.Relationships
                    .Where(r => r.UserId == patientId && r.RelationshipType == "doctor-patient")
                    .Select(r => r.InviterId)
                    .ToListAsync();

                var familyRelationships = await _dbContext.Relationships
                    .Where(r => (r.UserId == patientId || r.InviterId == patientId) &&
                               r.RelationshipType == "family")
                    .ToListAsync();

                var familyIds = new List<int>();
                foreach (var relationship in familyRelationships)
                {
                    if (relationship.UserId == patientId)
                        familyIds.Add(relationship.InviterId);
                    else
                        familyIds.Add(relationship.UserId);
                }

                _logger.LogInformation($"====== NOTIFICATION RECIPIENTS FOR PATIENT {patientId} ======");
                _logger.LogInformation($"DOCTORS ({doctorIds.Count}): {string.Join(", ", doctorIds)}");
                _logger.LogInformation($"FAMILY MEMBERS ({familyIds.Count}): {string.Join(", ", familyIds)}");
                _logger.LogInformation($"======================================================");

                await Task.WhenAll(
                    SendNotificationToPatientDoctorsAsync(patientId, title, message, type, saveWarning: false, detailsList),
                    SendNotificationToPatientFamilyAsync(patientId, title, message, type, saveWarning: false, detailsList)
                );

                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Đã gửi thông báo thành công đến bác sĩ và gia đình của bệnh nhân {patientName} (ID {patientId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Lỗi khi gửi thông báo đến bác sĩ và gia đình của bệnh nhân ID {patientId}");
                throw; 
            }
        }
    }
}