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
             bool saveWarning = false)
        {
            try
            {
                if (patientId <= 0)
                {
                    _logger.LogWarning("patientId không hợp lệ: {patientId}", patientId);
                    return;
                }

                
                var doctorIds = await _dbContext.Relationships
                    .Where(r => r.UserId == patientId && r.RelationshipType == "doctor-patient")
                    .Select(r => r.InviterId)
                    .Distinct()
                    .ToListAsync();

                var patient = await _dbContext.StrokeUsers.FindAsync(patientId);
                if (patient == null)
                {
                    _logger.LogWarning("Không tìm thấy thông tin bệnh nhân ID {patientId}", patientId);
                    return;
                }

                if (!doctorIds.Any())
                {
                    _logger.LogInformation($"Không tìm thấy bác sĩ nào liên kết với bệnh nhân ID {patientId}");
                    return;
                }

                _logger.LogInformation($"Gửi thông báo đến {doctorIds.Count} bác sĩ của bệnh nhân ID {patientId}");


                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                        await _notificationService.SendWebNotificationAsync(
                            doctorId, title, message, type, saveWarning);

                        _logger.LogInformation($"Đã gửi thông báo web thành công đến bác sĩ ID {doctorId}");
                        var doctor = await _dbContext.StrokeUsers.FindAsync(doctorId);
                        if (doctor != null && !string.IsNullOrEmpty(doctor.Email))
                        {
                            string emailSubject = $"Cảnh báo sức khỏe bệnh nhân {patient.PatientName}";
                            await _notificationService.SendNotificationAsync(doctor.Email, emailSubject, message);
                            _logger.LogInformation($"Đã gửi email thông báo đến bác sĩ {doctor.PatientName} ({doctor.Email})");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi khi gửi thông báo web đến bác sĩ ID {doctorId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo đến bác sĩ của bệnh nhân ID {patientId}");
            }

        }

        public async Task SendNotificationToPatientFamilyAsync(
            int patientId,
            string title,
            string message,
            string type = "warning",
            bool saveWarning = true)
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
                        _logger.LogInformation($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Chuẩn bị gửi thông báo cho thành viên gia đình {familyMember.PatientName} (ID {familyMember.UserId})");

                       
                        allTasks.Add(_notificationService.SendWebNotificationAsync(
                            familyMember.UserId, title, message, type, saveWarning));

                       
                        allTasks.Add(_mobileNotificationService.SendNotificationToUserAsync(
                            familyMember.UserId, title, message, type, null));

                        
                        if (!string.IsNullOrEmpty(familyMember.Email))
                        {
                            string emailSubject = $"Cảnh báo sức khỏe bệnh nhân {patient.PatientName}";
                            string emailBody = $@"
                        <h2>{title}</h2>
                        <p>Kính gửi {familyMember.PatientName},</p>
                        <p>Hệ thống giám sát sức khỏe phát hiện bất thường ở bệnh nhân {patient.PatientName}:</p>
                        <div style='padding: 10px; margin: 15px 0; border-left: 4px solid {(type == "warning" ? "#ff0000" : "#ff9800")}; background-color: {(type == "warning" ? "#fff1f0" : "#fff8e1")}'>
                            {message}
                        </div>
                        <p>Vui lòng kiểm tra ứng dụng để biết thêm chi tiết.</p>
                        <p>Trân trọng,<br>Hệ thống Giám sát Sức khỏe</p>
                    ";

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
                _logger.LogError(ex, $"Lỗi khi gửi thông báo đến gia đình của bệnh nhân ID {patientId}");
            }
        }

        public async Task SendNotificationToPatientCircleAsync(
            int patientId,
            string title,
            string message,
            string type = "warning")
        {
            try
            {
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
                    SendNotificationToPatientDoctorsAsync(patientId, title, message, type, saveWarning: false),
                    SendNotificationToPatientFamilyAsync(patientId, title, message, type, saveWarning: false)
                );

                _logger.LogInformation($"Đã gửi thông báo thành công đến bác sĩ và gia đình của bệnh nhân ID {patientId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo đến bác sĩ và gia đình của bệnh nhân ID {patientId}");
            }
        }
    }
}