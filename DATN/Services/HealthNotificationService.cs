using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class HealthNotificationService : IHealthNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<HealthNotificationService> _logger;

        public HealthNotificationService(
            INotificationService notificationService,
            ILogger<HealthNotificationService> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task SendHealthWarningToAllDoctors()
        {
            try
            {
                await _notificationService.SendNotificationToRolesAsync(
                    new[] { "doctor", "admin" },
                    "Cảnh Báo Sức Khỏe Bệnh Nhân",
                    "Có bệnh nhân cần được chăm sóc khẩn cấp",
                    "alert"
                );

                _logger.LogInformation("Đã gửi cảnh báo sức khỏe cho tất cả bác sĩ và admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi cảnh báo sức khỏe cho bác sĩ");
                throw;
            }
        }

        public async Task SendPatientStatusToFamilyAndDoctors(int patientId, string status)
        {
            try
            {
                await _notificationService.SendNotificationToRolesAsync(
                    new[] { "doctor", "family" },
                    "Cập Nhật Tình Trạng Bệnh Nhân",
                    $"Bệnh nhân ID {patientId} đang ở trạng thái: {status}",
                    "info"
                );

                _logger.LogInformation($"Đã gửi cập nhật trạng thái bệnh nhân {patientId} cho gia đình và bác sĩ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo trạng thái bệnh nhân {patientId}");
                throw;
            }
        }

        public async Task NotifyFamilyAboutAbnormalReadings(int patientId, string readingType, string value)
        {
            try
            {
                await _notificationService.SendNotificationToRolesAsync(
                    new[] { "family" },
                    "Chỉ Số Bất Thường",
                    $"Bệnh nhân {patientId} có chỉ số {readingType}: {value} bất thường",
                    "warning"
                );

                _logger.LogInformation($"Đã thông báo chỉ số bất thường của bệnh nhân {patientId} cho gia đình");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo chỉ số bất thường cho bệnh nhân {patientId}");
                throw;
            }
        }

        public async Task NotifyEmergencyContacts(int patientId, string urgentMessage, bool includeDoctors = true)
        {
            try
            {
                var roles = includeDoctors
                    ? new[] { "family", "doctor" }
                    : new[] { "family" };

                await _notificationService.SendNotificationToRolesAsync(
                    roles,
                    "THÔNG BÁO KHẨN CẤP",
                    urgentMessage,
                    "critical"
                );

                _logger.LogInformation($"Đã gửi thông báo khẩn cấp về bệnh nhân {patientId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi gửi thông báo khẩn cấp cho bệnh nhân {patientId}");
                throw;
            }
        }
    }
}