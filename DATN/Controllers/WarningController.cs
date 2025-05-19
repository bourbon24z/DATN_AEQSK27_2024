using DATN.Data;
using DATN.Dto;
using DATN.Hubs;
using DATN.Models;
using DATN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarningController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly INotificationFormatterService _notificationFormatter;
        private readonly IMobileNotificationService _mobileNotificationService;
        private readonly IHealthNotificationService _healthNotificationService;
        private readonly IPatientNotificationService _patientNotificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public WarningController(StrokeDbContext context, INotificationService notificationService, 
                                                          INotificationFormatterService notificationFormatter, 
                                                          IMobileNotificationService mobileNotificationService, 
                                                          IPatientNotificationService patientNotificationService,
                                                          IHubContext<NotificationHub> hubContext)
        {
                                _context = context;
                                _notificationService = notificationService;
                                _notificationFormatter = notificationFormatter;
                                _mobileNotificationService = mobileNotificationService;
                                _patientNotificationService = patientNotificationService;
                                _hubContext = hubContext;
        }

        [HttpPost("device-reading")]
        [Authorize]
        public async Task<IActionResult> ProcessDeviceReading([FromBody] DeviceDataDto deviceData)
        {
            if (deviceData == null)
                return BadRequest("Dữ liệu không hợp lệ.");
            if (deviceData.Measurements == null)
                return BadRequest("Dữ liệu đo lường là bắt buộc.");

            var tokenUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(tokenUserIdStr, out int tokenUserId))
            {
                return BadRequest("Invalid user token");
            }
            deviceData.UserId = tokenUserId;
            
            if (deviceData.UserId != tokenUserId)
            {
                
                return StatusCode(403, new { message = "You can only submit data for your own account" });

            }
            var strokeUser = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == deviceData.UserId);
            if (strokeUser == null)
                return NotFound("Không tìm thấy người dùng.");

            int overallLevel = 0;  // 0: Bình thường, 1: Cảnh báo, 2: Nguy hiểm
            List<string> detailsList = new List<string>();

            // 1. Nhiệt độ (Chuẩn: 37°C)
            if (deviceData.Measurements.Temperature.HasValue)
            {
                float temp = deviceData.Measurements.Temperature.Value;
                float diff = Math.Abs(temp - 37f);
                int level = 0;
                if (diff <= 0.5f)
                    level = 0;
                else if (diff > 0.5f && diff < 1f)
                    level = 1;
                else // diff >= 1f
                    level = 2;
                overallLevel = Math.Max(overallLevel, level);
                if (level > 0)
                    detailsList.Add($"Nhiệt độ: {temp}°C (bình thường: 37 ±0.5°C, {(level == 1 ? "Cảnh báo" : "Nguy hiểm")})");
            }

            // 2. Huyết áp tâm thu (Chuẩn: 120 mmHg)
            if (deviceData.Measurements.SystolicPressure.HasValue)
            {
                float systolic = deviceData.Measurements.SystolicPressure.Value;
                int level = 0;
                if (systolic <= 140f)
                    level = 0;
                else if (systolic > 140f && systolic <= 160f)
                    level = 1;
                else // systolic > 160
                    level = 2;
                overallLevel = Math.Max(overallLevel, level);
                if (level > 0)
                    detailsList.Add($"Huyết áp tâm thu: {systolic} mmHg (bình thường: ≤140, {(level == 1 ? "Cảnh báo" : "Nguy hiểm")})");
            }

            // 3. Huyết áp tâm trương (Chuẩn: 80 mmHg)
            if (deviceData.Measurements.DiastolicPressure.HasValue)
            {
                float diastolic = deviceData.Measurements.DiastolicPressure.Value;
                int level = 0;
                if (diastolic <= 90f)
                    level = 0;
                else if (diastolic > 90f && diastolic <= 100f)
                    level = 1;
                else // diastolic > 100
                    level = 2;
                overallLevel = Math.Max(overallLevel, level);
                if (level > 0)
                    detailsList.Add($"Huyết áp tâm trương: {diastolic} mmHg (bình thường: ≤90, {(level == 1 ? "Cảnh báo" : "Nguy hiểm")})");
            }

            // 4. Nhịp tim (Chuẩn: 75 bpm)
            if (deviceData.Measurements.HeartRate.HasValue)
            {
                float hr = deviceData.Measurements.HeartRate.Value;
                int level = 0;
                if (hr >= 60 && hr <= 90)
                    level = 0;
                else if ((hr >= 50 && hr < 60) || (hr > 90 && hr <= 100))
                    level = 1;
                else if (hr < 50 || hr > 100)
                    level = 2;
                overallLevel = Math.Max(overallLevel, level);
                if (level > 0)
                    detailsList.Add($"Nhịp tim: {hr} bpm (bình thường: 60–90, {(level == 1 ? "Cảnh báo" : "Nguy hiểm")})");
            }

            // 5. SPO2 (Chuẩn: 95%)
            if (deviceData.Measurements.SPO2.HasValue)
            {
                float spo2 = deviceData.Measurements.SPO2.Value;
                int level = 0;
                if (spo2 >= 95f)
                    level = 0;
                else if (spo2 >= 90f && spo2 < 95f)
                    level = 1;
                else // spo2 < 90
                    level = 2;
                overallLevel = Math.Max(overallLevel, level);
                if (level > 0)
                    detailsList.Add($"SPO2: {spo2}% (bình thường: ≥95%, {(level == 1 ? "Cảnh báo" : "Nguy hiểm")})");
            }

            // 6. Độ pH máu (Chuẩn: 7.4)
            if (deviceData.Measurements.BloodPH.HasValue)
            {
                float ph = deviceData.Measurements.BloodPH.Value;
                float diff = Math.Abs(ph - 7.4f);
                int level = 0;
                if (diff <= 0.05f)
                    level = 0;
                else if (diff > 0.05f && diff < 0.2f)
                    level = 1;
                else // diff >= 0.2
                    level = 2;
                overallLevel = Math.Max(overallLevel, level);
                if (level > 0)
                    detailsList.Add($"Độ pH máu: {ph} (bình thường: 7.4 ±0.05, {(level == 1 ? "Cảnh báo" : "Nguy hiểm")})");
            }

            string classification;
            if (overallLevel == 0)
                classification = "NORMAL";
            else if (overallLevel == 1)
                classification = "RISK";
            else
                classification = "WARNING";

            bool hasGps = deviceData.GPS != null &&
                          (Math.Abs(deviceData.GPS.Lat) > 0.0001f || Math.Abs(deviceData.GPS.Long) > 0.0001f);

            if (classification == "WARNING" && !hasGps)
                classification = "RISK";

            string details = (detailsList.Count > 0)
                ? string.Join("; ", detailsList)
                : "Tất cả các chỉ số đều bình thường.";

            string classificationVietnamese;
            if (classification == "NORMAL")
                classificationVietnamese = "BÌNH THƯỜNG";
            else if (classification == "RISK")
                classificationVietnamese = "CẢNH BÁO";
            else
                classificationVietnamese = "NGUY HIỂM";

            string formattedDescription = _notificationFormatter.FormatWarningMessage(
                classificationVietnamese,
                details,
                hasGps ? deviceData.GPS : null
            );

            if (classification == "NORMAL")
            {
                return Ok("Tất cả các chỉ số đều bình thường.");
            }

            if (classification == "WARNING" || classification == "RISK")
            {
                Console.WriteLine($"[WarningController] Đã phát hiện tình trạng {classification} cho người dùng ID {deviceData.UserId}");

                
                await _notificationService.SendNotificationAsync(strokeUser.Email, "Cảnh báo", formattedDescription);
                Console.WriteLine($"[WarningController] Đã gửi thông báo email cho {strokeUser.Email}");

              
                await _notificationService.SendWebNotificationAsync(
                    deviceData.UserId,
                    classification == "WARNING" ? "Cảnh Báo Nghiêm Trọng" : "Cảnh Báo",
                    formattedDescription,
                    classification.ToLower(),
                    true 
                );
                Console.WriteLine($"[WarningController] Đã gửi thông báo web cho người dùng ID {deviceData.UserId}");

                
                await _patientNotificationService.SendNotificationToPatientCircleAsync(
                    deviceData.UserId,
                    classification == "WARNING" ? "Cảnh Báo Sức Khỏe Bệnh Nhân" : "Cảnh Báo Sức Khỏe",
                    $"Bệnh nhân {strokeUser.PatientName} (ID: {deviceData.UserId}) có chỉ số bất thường: {details}",
                    classification.ToLower()
                );

                // send mobile notification
                if (_mobileNotificationService != null)
                {
                    try
                    {
                       
                        string briefNotification = CreateBriefMobileNotification(classificationVietnamese, detailsList);

                        
                        var additionalData = new Dictionary<string, string>
                        {
                            { "fullDescription", formattedDescription },
                            { "timestamp", DateTime.Now.ToString("o") }
                        };

                       
                        bool mobileSent = await _mobileNotificationService.SendNotificationToUserAsync(
                            deviceData.UserId,
                            GetMobileNotificationTitle(classification),
                            briefNotification,
                            classification.ToLower(),
                            additionalData);

                        if (mobileSent)
                        {
                            Console.WriteLine($"[WarningController] Đã gửi thông báo mobile cho người dùng ID {deviceData.UserId}");
                        }
                        else
                        {
                            Console.WriteLine($"[WarningController] Không thể gửi thông báo mobile cho người dùng ID {deviceData.UserId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WarningController] Lỗi khi gửi thông báo mobile: {ex.Message}");
                    }
                }

                if (hasGps)
                {
                    var gpsRecord = new Gps
                    {
                        UserId = deviceData.UserId,
                        Lat = deviceData.GPS.Lat,
                        Lon = deviceData.GPS.Long,
                        CreatedAt = DateTime.Now
                    };
                    _context.Gps.Add(gpsRecord);
                    Console.WriteLine($"[WarningController] Đã lưu dữ liệu GPS. Vĩ độ: {deviceData.GPS.Lat}, Kinh độ: {deviceData.GPS.Long}");
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Đã xử lý và lưu cảnh báo thành công",
                details = formattedDescription
            });
        }

        // 1. get all warning
        [HttpGet]
        [Authorize(Roles = "admin,doctor")]
        //http://localhost:5062/api/Warning
        public async Task<IActionResult> GetWarnings(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.Warnings.AsQueryable();

                if (User.IsInRole("doctor") && !User.IsInRole("admin"))
                {
                    var doctorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!int.TryParse(doctorIdStr, out int doctorId))
                    {
                        return BadRequest("Invalid doctor identifier");
                    }

                    var patientIds = await _context.Relationships
                        .Where(r => r.InviterId == doctorId &&
                                   r.RelationshipType == "doctor-patient")
                        .Select(r => r.UserId)
                        .ToListAsync();


                    query = query.Where(w => patientIds.Contains(w.UserId));
                }

                if (startDate.HasValue)
                    query = query.Where(w => w.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(w => w.CreatedAt <= endDate.Value);

                if (isActive.HasValue)
                    query = query.Where(w => w.IsActive == isActive.Value);


                var warnings = await query
                   .OrderByDescending(w => w.CreatedAt)
                   .Include(w => w.StrokeUser)
                   .Select(w => new
           {
                   w.WarningId,
                   w.UserId,
                   PatientName = w.StrokeUser.PatientName,
                   w.Description,
                   w.CreatedAt,
                   FormattedTimestamp = w.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                   w.IsActive
           })
                .ToListAsync();

                return Ok(new
                {
                    TotalCount = warnings.Count,
                    Warnings = warnings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        // get by id
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin,doctor")]
        //http://localhost:5062/api/Warning/user/{userId}
        public async Task<IActionResult> GetUserWarnings(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                
                if (User.IsInRole("doctor"))
                {
                    var doctorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!int.TryParse(doctorIdStr, out int doctorId))
                    {
                        return BadRequest("Invalid doctor identifier");
                    }

                    var hasRelationship = await _context.Relationships
                        .AnyAsync(r => r.InviterId == doctorId &&
                                      r.UserId == userId &&
                                      r.RelationshipType == "doctor-patient");

                    if (!hasRelationship)
                    {
                        return Forbid("You do not have access to this patient's warnings");
                    }
                }

                var query = _context.Warnings
                    .Where(w => w.UserId == userId);

               
                if (startDate.HasValue)
                    query = query.Where(w => w.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(w => w.CreatedAt <= endDate.Value);

                if (isActive.HasValue)
                    query = query.Where(w => w.IsActive == isActive.Value);

                
                var warnings = await query
                    .OrderByDescending(w => w.CreatedAt)
                    .Select(w => new
                    {
                        w.WarningId,
                        w.UserId,
                        w.Description,
                        w.CreatedAt,
                        w.IsActive,
                        FormattedTimestamp = w.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")
                    })
                    .ToListAsync();

                var user = await _context.StrokeUsers.FindAsync(userId);

                return Ok(new
                {
                    Patient = new
                    {
                        UserId = userId,
                        PatientName = user != null ? user.PatientName : "Unknown"
                    },
                    TotalCount = warnings.Count,
                    Warnings = warnings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        //get warning for user
        [HttpGet("my-warnings")]
        [Authorize(Roles = "user")]
        //http://localhost:5062/api/Warning/my-warnings
        public async Task<IActionResult> GetMyWarnings(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }

                var query = _context.Warnings
                    .Where(w => w.UserId == userId && w.IsActive);

                
                if (startDate.HasValue)
                    query = query.Where(w => w.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(w => w.CreatedAt <= endDate.Value);

               
                var warnings = await query
                    .OrderByDescending(w => w.CreatedAt)
                    .Select(w => new
                    {
                        w.WarningId,
                        w.Description,
                        w.CreatedAt,
                        FormattedTimestamp = w.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalCount = warnings.Count,
                    Warnings = warnings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // get detail warning
        [HttpGet("{id}")]
        [Authorize(Roles = "admin,doctor,user")]
        //http://localhost:5062/api/Warning/{id}
        public async Task<IActionResult> GetWarningById(int id)
        {
            try
            {
                var warning = await _context.Warnings
                    .Include(w => w.StrokeUser) 
                    .FirstOrDefaultAsync(w => w.WarningId == id);

                if (warning == null)
                    return NotFound("Warning not found");

                
                if (User.IsInRole("user"))
                {
                    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!int.TryParse(userIdStr, out int userId) || warning.UserId != userId)
                    {
                        return StatusCode(403, new { message = "You do not have access to this warning" });
                    }
                }
                else if (User.IsInRole("doctor"))
                {
                    var doctorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!int.TryParse(doctorIdStr, out int doctorId))
                    {
                        return BadRequest("Invalid doctor identifier");
                    }

                    var hasRelationship = await _context.Relationships
                        .AnyAsync(r => r.InviterId == doctorId &&
                                      r.UserId == warning.UserId &&
                                      r.RelationshipType == "doctor-patient");

                    if (!hasRelationship)
                    {
                        return StatusCode(403, new { message = "You do not have access to this patient's warnings" });
                    }
                }

                return Ok(new
                {
                    warning.WarningId, 
                    warning.UserId,
                    PatientName = warning.StrokeUser?.PatientName,
                    warning.Description,
                    warning.CreatedAt,
                    FormattedTimestamp = warning.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    warning.IsActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

       // del warning
        [HttpDelete("{id}")]
        [Authorize (Roles= "admin")]
        //http://localhost:5062/api/Warning/{id}
        public async Task<IActionResult> DeleteWarning(int id)
        {
            try
            {
                var warning = await _context.Warnings.FindAsync(id);
                if (warning == null)
                    return NotFound("Warning not found");

                _context.Warnings.Remove(warning);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Warning has been permanently deleted",
                    WarningId = id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // del hàng loạt
        [HttpDelete("batch")]
        [Authorize(Roles = "admin")]
        //http://localhost:5062/api/Warning/batch
        public async Task<IActionResult> BatchDeleteWarnings([FromBody] List<int> warningIds)
        {
            try
            {
                if (warningIds == null || !warningIds.Any())
                    return BadRequest("Warning IDs are required");

                var warnings = await _context.Warnings
                    .Where(w => warningIds.Contains(w.WarningId))
                    .ToListAsync();

                if (!warnings.Any())
                    return NotFound("No warnings found with the provided IDs");

                _context.Warnings.RemoveRange(warnings);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = $"{warnings.Count} warnings have been permanently deleted",
                    DeletedWarningIds = warnings.Select(w => w.WarningId)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        
        [HttpGet("test-recipients/{patientId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TestNotificationRecipients(int patientId)
        {
            try
            {
                
                var patient = await _context.StrokeUsers.FindAsync(patientId);
                if (patient == null)
                    return NotFound($"Không tìm thấy bệnh nhân ID {patientId}");

                
                var doctors = await _context.Relationships
                    .Where(r => r.UserId == patientId && r.RelationshipType == "doctor-patient")
                    .Join(_context.StrokeUsers,
                        rel => rel.InviterId,
                        user => user.UserId,
                        (rel, user) => new { UserId = user.UserId, Name = user.PatientName })
                    .ToListAsync();

                
                var familyRelationships = await _context.Relationships
                    .Where(r => (r.UserId == patientId || r.InviterId == patientId) &&
                               r.RelationshipType == "family")
                    .ToListAsync();

                var familyIds = new List<int>();
                foreach (var rel in familyRelationships)
                {
                    if (rel.UserId == patientId)
                        familyIds.Add(rel.InviterId);
                    else
                        familyIds.Add(rel.UserId);
                }

                var familyMembers = await _context.StrokeUsers
                    .Where(u => familyIds.Contains(u.UserId))
                    .Select(u => new { UserId = u.UserId, Name = u.PatientName })
                    .ToListAsync();

                
                var connections = _hubContext.Clients.Group(patientId.ToString()).ToString();

                return Ok(new
                {
                    Patient = new { patientId, Name = patient.PatientName },
                    Doctors = doctors,
                    FamilyMembers = familyMembers,
                    TotalRecipients = doctors.Count + familyMembers.Count + 1, 
                    OnlineStatus = !string.IsNullOrEmpty(connections) ? "Some recipients are online" : "No recipients currently online"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi kiểm tra người nhận thông báo: {ex.Message}");
            }
        }
        [HttpGet("mobile-user-notifications")]
        [Authorize]
        public async Task<IActionResult> GetUserNotifications([FromQuery] DateTime? since = null)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return BadRequest("Invalid user identifier");
                }

                
                var query = _context.Warnings
                    .Where(w => w.UserId == userId && w.IsActive);

                if (since.HasValue)
                {
                    query = query.Where(w => w.CreatedAt >= since.Value);
                }
                else
                {
                    query = query.Where(w => w.CreatedAt >= DateTime.Now.AddDays(-7));
                }

                var warnings = await query
                    .OrderByDescending(w => w.CreatedAt)
                    .Select(w => new
                    {
                        id = w.WarningId.ToString(),
                        title = GetTitleFromDescription(w.Description),
                        message = ExtractPlainTextFromHtml(w.Description),
                        fullHtmlContent = w.Description,
                        type = w.Description.Contains("NGUY HIỂM") ? "warning" :
                               w.Description.Contains("CẢNH BÁO") ? "risk" : "normal",
                        timestamp = w.CreatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                        persistence = true,
                        requiresAction = true,
                        isRead = false 
                    })
                    .ToListAsync();

                return Ok(warnings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpPost("test-send/{patientId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TestSendNotification(int patientId)
        {
            try
            {
                var patient = await _context.StrokeUsers.FindAsync(patientId);
                if (patient == null)
                    return NotFound($"Không tìm thấy bệnh nhân ID {patientId}");

                
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var title = $"Test Notification ({timestamp})";
                var message = $"This is a test notification sent at {timestamp}. If you see this, you are configured to receive alerts for patient {patient.PatientName} (ID: {patientId}).";

                
                Console.WriteLine($"==== SENDING TEST NOTIFICATION FOR PATIENT {patientId} ====");

               
                await _notificationService.SendWebNotificationAsync(
                    patientId,
                    title,
                    message,
                    "test",
                    true 
                );

                
                await _patientNotificationService.SendNotificationToPatientCircleAsync(
                    patientId,
                    title,
                    message,
                    "test"
                );

               
                await Task.Delay(500);

                return Ok(new
                {
                    Status = "Test notification sent",
                    Timestamp = timestamp,
                    Patient = new { patientId, Name = patient.PatientName },
                    Message = "Check logs and notifications to see who received it"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending test notification: {ex.Message}");
            }
        }
        private string GetMobileNotificationTitle(string classification)
        {
            return classification switch
            {
                "WARNING" => "🚨 Cảnh Báo Nghiêm Trọng",
                "RISK" => "⚠️ Cảnh Báo",
                _ => "ℹ️ Thông Báo"
            };
        }

       
        private string CreateBriefMobileNotification(string classificationVietnamese, List<string> details)
        {
            
            if (details.Count == 0)
            {
                return $"{classificationVietnamese}: Kiểm tra sức khỏe của bạn";
            }

            string content;

            var shortenedDetails = details.Select(d => {
                int bracketPos = d.IndexOf(" (");
                if (bracketPos > 0)
                    return d.Substring(0, bracketPos);
                return d;
            }).ToList();

           
            if (shortenedDetails.Count <= 2)
            {
                content = string.Join("; ", shortenedDetails);
            }
            else
            {
                content = string.Join("; ", shortenedDetails.Take(2));
                content += $" và {details.Count - 2} chỉ số khác";
            }

            
            return $"{classificationVietnamese}: {content}. Nhấn để xem chi tiết.";
        }

        private static string GetTitleFromDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "Cảnh Báo";

            if (description.Contains("NGUY HIỂM"))
                return "🚨 Cảnh Báo Nghiêm Trọng";
            else if (description.Contains("CẢNH BÁO"))
                return "⚠️ Cảnh Báo";

            return "ℹ️ Thông Báo";
        }
        public static string ExtractPlainTextFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            try
            {
                
                string classification = "Thông báo";
                if (html.Contains("NGUY HIỂM"))
                    classification = "NGUY HIỂM";
                else if (html.Contains("CẢNH BÁO"))
                    classification = "CẢNH BÁO";
                else if (html.Contains("BÌNH THƯỜNG"))
                    classification = "BÌNH THƯỜNG";

                
                string detectionTime = string.Empty;
                var timeMatch = Regex.Match(html, @"Thời gian phát hiện: ([0-9]{2}/[0-9]{2}/[0-9]{4} [0-9]{2}:[0-9]{2})");
                if (timeMatch.Success)
                    detectionTime = timeMatch.Groups[1].Value;

                
                var measurements = new List<string>();

               
                var tempMatch = Regex.Match(html, @"Nhiệt độ: ([\d\.]+)°C");
                if (tempMatch.Success)
                    measurements.Add($"Nhiệt độ: {tempMatch.Groups[1].Value}°C");

               
                var sysMatch = Regex.Match(html, @"Huyết áp tâm thu: ([\d\.]+) mmHg");
                if (sysMatch.Success)
                    measurements.Add($"Huyết áp tâm thu: {sysMatch.Groups[1].Value} mmHg");

                
                var diaMatch = Regex.Match(html, @"Huyết áp tâm trương: ([\d\.]+) mmHg");
                if (diaMatch.Success)
                    measurements.Add($"Huyết áp tâm trương: {diaMatch.Groups[1].Value} mmHg");

                
                var hrMatch = Regex.Match(html, @"Nhịp tim: ([\d\.]+) bpm");
                if (hrMatch.Success)
                    measurements.Add($"Nhịp tim: {hrMatch.Groups[1].Value} bpm");

                
                var spo2Match = Regex.Match(html, @"SPO2: ([\d\.]+)%");
                if (spo2Match.Success)
                    measurements.Add($"SPO2: {spo2Match.Groups[1].Value}%");

                
                var phMatch = Regex.Match(html, @"Độ pH máu: ([\d\.]+)");
                if (phMatch.Success)
                    measurements.Add($"pH máu: {phMatch.Groups[1].Value}");

                
                string locationInfo = string.Empty;
                var locationMatch = Regex.Match(html, @"https://www\.openstreetmap\.org/\?mlat=([\d\.]+)&mlon=([\d\.]+)");
                if (locationMatch.Success)
                    locationInfo = $"Vị trí: {locationMatch.Groups[1].Value}, {locationMatch.Groups[2].Value}";

              
                var result = new System.Text.StringBuilder();
                result.Append($"{classification}: ");

              
                if (measurements.Count == 0)
                {
                    result.Append("Kiểm tra sức khỏe của bạn");
                }
                else if (measurements.Count <= 2)
                {
                    result.Append(string.Join("; ", measurements));
                }
                else
                {
                    
                    var shortMeasurements = measurements.Select(m =>
                    {
                        
                        var parts = m.Split(':');
                        if (parts.Length >= 2)
                        {
                            
                            var valueMatch = Regex.Match(parts[1], @"([\d\.]+)");
                            if (valueMatch.Success)
                                return $"{parts[0].Trim()}: {valueMatch.Groups[1].Value}";
                        }
                        return m;
                    }).ToList();

                    result.Append(string.Join("; ", shortMeasurements.Take(2)));
                    result.Append($" và {measurements.Count - 2} chỉ số khác");
                }

                
                result.Append(". Nhấn để xem chi tiết.");

                return result.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting plain text: {ex.Message}");
                return "Có cảnh báo sức khỏe mới. Nhấn để xem chi tiết.";
            }
        }
    }
}