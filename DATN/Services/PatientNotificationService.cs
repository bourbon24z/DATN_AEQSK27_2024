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
                
                var familyRelationships = await _dbContext.Relationships
                    .Where(r => (r.UserId == patientId || r.InviterId == patientId) &&
                               r.RelationshipType == "family")
                    .ToListAsync();

                if (!familyRelationships.Any())
                {
                    _logger.LogInformation($"Không tìm thấy gia đình nào liên kết với bệnh nhân ID {patientId}");
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

                _logger.LogInformation($"Gửi thông báo đến {familyIds.Count} thành viên gia đình của bệnh nhân ID {patientId}");

               
                var webTasks = familyIds.Select(familyId =>
                    _notificationService.SendWebNotificationAsync(familyId, title, message, type, saveWarning));

                
                var mobileTasks = familyIds.Select(familyId =>
                    _mobileNotificationService.SendNotificationToUserAsync(familyId, title, message, type, null));

                
                await Task.WhenAll(webTasks.Concat(mobileTasks));

                _logger.LogInformation($"Đã gửi thông báo thành công đến gia đình của bệnh nhân ID {patientId}");
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