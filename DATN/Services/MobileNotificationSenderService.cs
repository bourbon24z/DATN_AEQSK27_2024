using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services
{
    public interface IMobileNotificationSenderService
    {
        Task<bool> SendHealthAlertAsync(int userId, string classification, List<string> abnormalDetails, string fullDescription);
    }

    public class MobileNotificationSenderService : IMobileNotificationSenderService
    {
        private readonly IMobileNotificationService _mobileNotificationService;
        private readonly ILogger<MobileNotificationSenderService> _logger;

        public MobileNotificationSenderService(
            IMobileNotificationService mobileNotificationService,
            ILogger<MobileNotificationSenderService> logger)
        {
            _mobileNotificationService = mobileNotificationService;
            _logger = logger;
        }

        public async Task<bool> SendHealthAlertAsync(
            int userId,
            string classification,
            List<string> abnormalDetails,
            string fullDescription)
        {
            try
            {
                
                string briefNotification = CreateBriefNotification(classification, abnormalDetails);

                
                var additionalData = new Dictionary<string, string>
                {
                    { "fullDescription", fullDescription },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                };

                
                bool result = await _mobileNotificationService.SendNotificationToUserAsync(
                    userId,
                    GetNotificationTitle(classification),
                    briefNotification,
                    classification.ToLower(),
                    additionalData);

                if (result)
                {
                    _logger.LogInformation($"Đã gửi thông báo mobile cho người dùng ID {userId}");
                }
                else
                {
                    _logger.LogWarning($"Không thể gửi thông báo mobile cho người dùng ID {userId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi gửi thông báo mobile: {ex.Message}");
                return false;
            }
        }

        private string GetNotificationTitle(string classification)
        {
            return classification switch
            {
                "WARNING" => "🚨 Cảnh Báo Nghiêm Trọng",
                "RISK" => "⚠️ Cảnh Báo",
                _ => "ℹ️ Thông Báo"
            };
        }

        private string CreateBriefNotification(string classification, List<string> abnormalDetails)
        {
            string classificationVietnamese = classification switch
            {
                "NORMAL" => "BÌNH THƯỜNG",
                "RISK" => "CẢNH BÁO",
                "WARNING" => "NGUY HIỂM",
                _ => classification
            };

            
            if (abnormalDetails == null || abnormalDetails.Count == 0)
            {
                return $"{classificationVietnamese}: Kiểm tra chỉ số sức khỏe của bạn";
            }

            
            string content;

            if (abnormalDetails.Count <= 2)
            {
                
                content = string.Join("; ", abnormalDetails);
            }
            else
            {
                
                content = string.Join("; ", abnormalDetails.Take(2)) +
                          $" và {abnormalDetails.Count - 2} chỉ số khác";
            }

            return $"{classificationVietnamese}: {content}";
        }
    }
}