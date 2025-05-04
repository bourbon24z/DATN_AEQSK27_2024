using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services
{

    public interface IMobileNotificationService
    {
     
        /// <param name="topic">Topic để gửi thông báo</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="body">Nội dung thông báo</param>
        /// <param name="notificationType">Loại thông báo (warning, risk, info)</param>
        /// <param name="additionalData">Dữ liệu bổ sung gửi kèm thông báo</param>
        /// <returns>True nếu gửi thành công</returns>
        Task<bool> SendNotificationToTopicAsync(
            string topic,
            string title,
            string body,
            string notificationType,
            Dictionary<string, string> additionalData = null);

      
        /// <param name="userId">ID của người dùng</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="body">Nội dung thông báo</param>
        /// <param name="notificationType">Loại thông báo (warning, risk, info)</param>
        /// <param name="additionalData">Dữ liệu bổ sung gửi kèm thông báo</param>
        /// <returns>True nếu gửi thành công</returns>
        Task<bool> SendNotificationToUserAsync(
            int userId,
            string title,
            string body,
            string notificationType,
            Dictionary<string, string> additionalData = null);
    }
}