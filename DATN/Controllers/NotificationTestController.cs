using DATN.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NotificationTestController : ControllerBase
{
    private readonly IMobileNotificationService _mobileNotificationService;
    private readonly IPatientNotificationService _patientNotificationService;

    public NotificationTestController(IMobileNotificationService mobileNotificationService, IPatientNotificationService patientNotificationService)
    {
        _mobileNotificationService = mobileNotificationService;
        _patientNotificationService = patientNotificationService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestNotification([FromBody] NotificationTestDto model)
    {
        if (model == null)
            return BadRequest("Dữ liệu không hợp lệ");

        var additionalData = new Dictionary<string, string>
        {
            { "testId", Guid.NewGuid().ToString() },
            { "sentAt", DateTime.Now.ToString("o") }
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
    [HttpPost("test-patient-circle")]
    public async Task<IActionResult> TestPatientCircleNotification([FromBody] PatientNotificationTestDto model)
    {
        try
        {
            await _patientNotificationService.SendNotificationToPatientCircleAsync(
                model.PatientId,
                model.Title,
                model.Message,
                model.Type ?? "info"
            );

            return Ok(new
            {
                success = true,
                message = $"Đã gửi thông báo đến bác sĩ và gia đình của bệnh nhân ID {model.PatientId}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPost("test-patient-doctors")]
    public async Task<IActionResult> TestPatientDoctorsNotification([FromBody] PatientNotificationTestDto model)
    {
        try
        {
            await _patientNotificationService.SendNotificationToPatientDoctorsAsync(
                model.PatientId,
                model.Title,
                model.Message,
                model.Type ?? "info"
            );

            return Ok(new
            {
                success = true,
                message = $"Đã gửi thông báo đến bác sĩ của bệnh nhân ID {model.PatientId}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPost("test-patient-family")]
    public async Task<IActionResult> TestPatientFamilyNotification([FromBody] PatientNotificationTestDto model)
    {
        try
        {
            await _patientNotificationService.SendNotificationToPatientFamilyAsync(
                model.PatientId,
                model.Title,
                model.Message,
                model.Type ?? "info"
            );

            return Ok(new
            {
                success = true,
                message = $"Đã gửi thông báo đến gia đình của bệnh nhân ID {model.PatientId}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}

public class PatientNotificationTestDto
{
    public int PatientId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
}

public class NotificationTestDto
{
    public int UserId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string NotificationType { get; set; } = "info";  // "info", "risk", "warning"
}