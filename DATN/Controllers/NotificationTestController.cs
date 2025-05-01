using DATN.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationTestController : ControllerBase
{
    private readonly IMobileNotificationService _mobileNotificationService;

    public NotificationTestController(IMobileNotificationService mobileNotificationService)
    {
        _mobileNotificationService = mobileNotificationService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestNotification([FromBody] NotificationTestDto model)
    {
        if (model == null)
            return BadRequest("Dữ liệu không hợp lệ");

        var additionalData = new Dictionary<string, string>
        {
            { "testId", Guid.NewGuid().ToString() },
            { "sentAt", DateTime.UtcNow.ToString("o") }
        };

        bool result = await _mobileNotificationService.SendNotificationToUserAsync(
            model.UserId,
            model.Title,
            model.Body,
            model.NotificationType,
            additionalData
        );

        if (result)
            return Ok(new { success = true, message = "Thông báo đã được gửi thành công!" });
        else
            return BadRequest(new { success = false, message = "Không thể gửi thông báo" });
    }
}

public class NotificationTestDto
{
    public int UserId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string NotificationType { get; set; } = "info";  // "info", "risk", "warning"
}