using DATN.Data;
using DATN.Hubs;
using DATN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileNotificationsController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public MobileNotificationsController(StrokeDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Lấy danh sách thông báo của người dùng
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId, [FromQuery] bool unreadOnly = false, [FromQuery] int limit = 20)
        {
            // 1. Tạo query cơ bản
            var query = _context.Warnings
                .Where(w => w.UserId == userId);

            // 2. Thêm điều kiện nếu chỉ lấy thông báo chưa đọc
            if (unreadOnly)
            {
                query = query.Where(w => w.IsActive);
            }

            // 3. Sắp xếp và giới hạn kết quả
            var warnings = await query
                .OrderByDescending(w => w.CreatedAt)
                .Take(limit)
                
                .ToListAsync(); // Lấy dữ liệu từ database

            // 4. Xử lý dữ liệu sau khi đã truy vấn từ database
            var notifications = warnings.Select(w => new
            {
                id = w.WarningId,
                // Xử lý chuỗi tại đây là an toàn vì đã có dữ liệu trong bộ nhớ
                title = w.Description.Split('\n').FirstOrDefault() ?? "Cảnh báo",
                message = string.Join("\n", w.Description.Split('\n').Skip(1)),
                // Phân loại type dựa trên nội dung
                type = w.Description.Contains("NGUY HIỂM", StringComparison.OrdinalIgnoreCase) ? "warning" :
                       w.Description.Contains("CẢNH BÁO", StringComparison.OrdinalIgnoreCase) ? "risk" : "info",
                isRead = !w.IsActive,
                timestamp = w.CreatedAt
            }).ToList();

            return Ok(notifications);
        }

        // Đánh dấu thông báo đã đọc
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var warning = await _context.Warnings.FindAsync(id);

            if (warning == null)
                return NotFound("Không tìm thấy thông báo");

            warning.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { id = warning.WarningId, message = "Đã đánh dấu đã đọc" });
        }

        // Đánh dấu tất cả thông báo của người dùng là đã đọc
        [HttpPut("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var warnings = await _context.Warnings
                .Where(w => w.UserId == userId && w.IsActive)
                .ToListAsync();

            if (warnings.Count == 0)
                return Ok(new { count = 0, message = "Không có thông báo cần đánh dấu" });

            foreach (var warning in warnings)
            {
                warning.IsActive = false;
            }

            await _context.SaveChangesAsync();

            return Ok(new { count = warnings.Count, message = $"Đã đánh dấu {warnings.Count} thông báo đã đọc" });
        }

       
        [HttpPost("test")]
        public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Message))
                return BadRequest("Thiếu thông tin thông báo");

            try
            {
                var notification = new
                {
                    id = Guid.NewGuid().ToString(),
                    title = model.Title ?? "Thông báo test",
                    message = model.Message,
                    type = model.Type?.ToLower() ?? "info",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
                };

                
                await _hubContext.Clients.Group(model.UserId.ToString())
                    .SendAsync("ReceiveNotification", notification);

              
                var warning = new Warning
                {
                    UserId = model.UserId,
                    Description = $"{model.Title ?? "Thông báo test"}\n{model.Message}",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Warnings.Add(warning);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Thông báo đã được gửi",
                    notification = notification,
                    warningId = warning.WarningId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    public class TestNotificationModel
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // info, risk, warning
    }
}