using DATN.Data;
using DATN.Dto;
using DATN.Hubs;
using DATN.Models;
using DATN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmergencyButtonController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly INotificationFormatterService _notificationFormatter;
        private readonly IMobileNotificationService _mobileNotificationService;
        private readonly IPatientNotificationService _patientNotificationService;
        private readonly ILogger<EmergencyButtonController> _logger;

        public EmergencyButtonController(
            StrokeDbContext context,
            INotificationService notificationService,
            INotificationFormatterService notificationFormatter,
            IMobileNotificationService mobileNotificationService,
            IPatientNotificationService patientNotificationService,
            ILogger<EmergencyButtonController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _notificationFormatter = notificationFormatter;
            _mobileNotificationService = mobileNotificationService;
            _patientNotificationService = patientNotificationService;
            _logger = logger;
        }

      
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateEmergency([FromBody] EmergencyRequestDto request)
        {
            try
            {
                // Lấy userId từ token
                var tokenUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(tokenUserIdStr, out int tokenUserId))
                {
                    return BadRequest("Invalid user token");
                }

               
                if (request.UserId.HasValue && request.UserId.Value != tokenUserId)
                {
                    return StatusCode(403, new { message = "You can only activate emergency for your own account" });
                }

                
                int userId = request.UserId ?? tokenUserId;

                
                var strokeUser = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (strokeUser == null)
                {
                    return NotFound("User not found");
                }

                _logger.LogInformation($"Emergency button activated by user {userId} ({strokeUser.PatientName}) at {DateTime.UtcNow}");

                
                var gps = new Gps
                {
                    UserId = userId,
                    Lat = request.Latitude,
                    Lon = request.Longitude,
                    CreatedAt = DateTime.Now
                };
                _context.Gps.Add(gps);
                await _context.SaveChangesAsync();

               
                string locationLink = $"{Request.Scheme}://{Request.Host}/emergency-location/{gps.GpsId}";
                string openStreetMapLink = $"https://www.openstreetmap.org/#map=16/{request.Latitude}/{request.Longitude}";

                string formattedDescription = $"🚨 THÔNG BÁO KHẨN CẤP! 🚨\n\n" +
                    $"Bệnh nhân {strokeUser.PatientName} vừa bấm nút khẩn cấp!\n\n" +
                    $"Vui lòng liên hệ ngay qua số điện thoại: {strokeUser.Phone}\n";

                if (!string.IsNullOrEmpty(strokeUser.Email))
                {
                    formattedDescription += $"Hoặc email: {strokeUser.Email}\n\n";
                }

                formattedDescription += $"Xem vị trí hiện tại của bệnh nhân:\n" +
                    $"- Trang chi tiết: {locationLink}\n" +
                    $"- OpenStreetMap: {openStreetMapLink}\n";

                if (!string.IsNullOrWhiteSpace(request.AdditionalInfo))
                {
                    formattedDescription += $"\nThông tin bổ sung: {request.AdditionalInfo}";
                }

                formattedDescription += $"\n\nThời gian thông báo: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")}";

               
                var warning = new Warning
                {
                    UserId = userId,
                    Description = formattedDescription,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Warnings.Add(warning);
                await _context.SaveChangesAsync();

                
                if (!string.IsNullOrEmpty(strokeUser.Email))
                {
                    await _notificationService.SendNotificationAsync(
                        strokeUser.Email,
                        "🚨 CẢNH BÁO KHẨN CẤP!",
                        formattedDescription
                    );
                }

               
                await _notificationService.SendWebNotificationAsync(
                    userId,
                    "🚨 CẢNH BÁO KHẨN CẤP!",
                    formattedDescription,
                    "emergency",
                    false 
                );

               
                await _patientNotificationService.SendNotificationToPatientCircleAsync(
                    userId,
                    "🚨 CẢNH BÁO KHẨN CẤP!",
                    $"Bệnh nhân {strokeUser.PatientName} (ID: {userId}) vừa kích hoạt nút khẩn cấp! Vui lòng kiểm tra ngay.",
                    "emergency"
                );

                
                var additionalData = new Dictionary<string, string>
                {
                    { "warningId", warning.WarningId.ToString() },
                    { "gpsId", gps.GpsId.ToString() },
                    { "latitude", request.Latitude.ToString() },
                    { "longitude", request.Longitude.ToString() },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "locationLink", locationLink },
                    { "openStreetMapLink", openStreetMapLink }
                };

                await _mobileNotificationService.SendNotificationToUserAsync(
                    userId,
                    "🚨 CẢNH BÁO KHẨN CẤP!",
                    $"Bệnh nhân {strokeUser.PatientName} cần trợ giúp khẩn cấp!",
                    "emergency",
                    additionalData
                );

                return Ok(new
                {
                    message = "Emergency alert activated successfully",
                    warningId = warning.WarningId,
                    gpsId = gps.GpsId,
                    locationLink = locationLink,
                    openStreetMapLink = openStreetMapLink
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating emergency button");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

       
        [HttpGet("location/{gpsId}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetEmergencyLocation(int gpsId)
        {
            try
            {
                var gpsData = await _context.Gps.FindAsync(gpsId);
                if (gpsData == null)
                {
                    return NotFound("GPS data not found");
                }

                var user = await _context.StrokeUsers.FindAsync(gpsData.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                
                var relatedWarning = await _context.Warnings
                    .Where(w => w.UserId == user.UserId &&
                               w.CreatedAt >= gpsData.CreatedAt.AddMinutes(-5) &&
                               w.CreatedAt <= gpsData.CreatedAt.AddMinutes(5) &&
                               w.Description.Contains("KHẨN CẤP"))
                    .OrderByDescending(w => w.CreatedAt)
                    .FirstOrDefaultAsync();

               
                var relationships = await _context.Relationships
                    .Where(r => r.UserId == user.UserId || r.InviterId == user.UserId)
                    .ToListAsync();

               
                var relatedUserIds = relationships
                    .Select(r => r.UserId == user.UserId ? r.InviterId : r.UserId)
                    .Distinct()
                    .ToList();

                
                var relatedUsers = await _context.StrokeUsers
                    .Where(u => relatedUserIds.Contains(u.UserId))
                    .Select(u => new { u.UserId, u.PatientName })
                    .ToDictionaryAsync(u => u.UserId, u => u.PatientName);

                
                var relationshipDtos = relationships.Select(r => new RelationshipDto
                {
                    Type = r.RelationshipType,
                    WithUserId = r.UserId == user.UserId ? r.InviterId : r.UserId,
                    WithUserName = relatedUsers.ContainsKey(r.UserId == user.UserId ? r.InviterId : r.UserId)
                        ? relatedUsers[r.UserId == user.UserId ? r.InviterId : r.UserId]
                        : "Unknown"
                }).ToList();

                var result = new EmergencyLocationResponseDto
                {
                    UserId = user.UserId,
                    PatientName = user.PatientName,
                    PhoneNumber = user.Phone,
                    Email = user.Email,
                    Latitude = gpsData.Lat,
                    Longitude = gpsData.Lon,
                    RecordedAt = gpsData.CreatedAt,
                    FormattedTime = gpsData.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    Relationships = relationshipDtos,
                    WarningId = relatedWarning?.WarningId ?? 0,
                    OpenStreetMapLink = $"https://www.openstreetmap.org/#map=16/{gpsData.Lat}/{gpsData.Lon}"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving emergency location for GPS ID {gpsId}");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

       
        [HttpPut("resolve/{warningId}")]
        public async Task<IActionResult> ResolveEmergency(int warningId)
        {
            try
            {
                var warning = await _context.Warnings.FindAsync(warningId);
                if (warning == null)
                {
                    return NotFound("Warning not found");
                }

                
                var tokenUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(tokenUserIdStr, out int tokenUserId))
                {
                    return BadRequest("Invalid user token");
                }

                
                bool canUpdate = false;

               
                if (warning.UserId == tokenUserId)
                {
                    canUpdate = true;
                }
                
                else if (User.IsInRole("admin"))
                {
                    canUpdate = true;
                }
              
                else
                {
                    canUpdate = await _context.Relationships
                        .AnyAsync(r => (
                            
                            (r.InviterId == tokenUserId && r.UserId == warning.UserId && r.RelationshipType == "doctor-patient") ||
                            
                            ((r.InviterId == tokenUserId && r.UserId == warning.UserId) ||
                             (r.UserId == tokenUserId && r.InviterId == warning.UserId)) && r.RelationshipType == "family"
                        ));
                }

                if (!canUpdate)
                {
                    return StatusCode(403, new { message = "You do not have permission to resolve this emergency" });
                }

                warning.IsActive = false;
                await _context.SaveChangesAsync();

              
                var user = await _context.StrokeUsers.FindAsync(warning.UserId);

                if (user != null)
                {
                    
                    var resolver = await _context.StrokeUsers.FindAsync(tokenUserId);
                    string resolverName = resolver?.PatientName ?? "Someone";

                    
                    string resolvedMessage = $"Tình huống khẩn cấp của {user.PatientName} đã được giải quyết bởi {resolverName}.";

                    await _patientNotificationService.SendNotificationToPatientCircleAsync(
                        warning.UserId,
                        "Tình Huống Khẩn Cấp Đã Giải Quyết",
                        resolvedMessage,
                        "info"
                    );

                    
                    await _notificationService.SendWebNotificationAsync(
                        warning.UserId,
                        "Tình Huống Khẩn Cấp Đã Giải Quyết",
                        resolvedMessage,
                        "info",
                        false
                    );
                }

                _logger.LogInformation($"Emergency warning ID {warningId} resolved by user ID {tokenUserId}");

                return Ok(new
                {
                    message = "Emergency has been marked as resolved",
                    warningId = warningId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resolving emergency warning ID {warningId}");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        
        [HttpPost("test/{patientId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TestEmergencyNotification(int patientId, [FromBody] EmergencyRequestDto request)
        {
            try
            {
                var user = await _context.StrokeUsers.FindAsync(patientId);
                if (user == null)
                {
                    return NotFound($"Không tìm thấy bệnh nhân ID {patientId}");
                }

                _logger.LogInformation($"===== TEST EMERGENCY NOTIFICATION FOR PATIENT {patientId} =====");

               
                var gps = new Gps
                {
                    UserId = patientId,
                    Lat = request.Latitude,
                    Lon = request.Longitude,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Gps.Add(gps);
                await _context.SaveChangesAsync();

                
                string locationLink = $"{Request.Scheme}://{Request.Host}/emergency-location/{gps.GpsId}";
                string openStreetMapLink = $"https://www.openstreetmap.org/#map=16/{request.Latitude}/{request.Longitude}";

                string testDescription = $"[TEST] 🚨 THÔNG BÁO KHẨN CẤP! 🚨\n\n" +
                    $"Bệnh nhân {user.PatientName} vừa bấm nút khẩn cấp!\n\n" +
                    $"Vui lòng liên hệ ngay qua số điện thoại: {user.Phone}\n";

                if (!string.IsNullOrEmpty(user.Email))
                {
                    testDescription += $"Hoặc email: {user.Email}\n\n";
                }

                testDescription += $"Xem vị trí hiện tại của bệnh nhân:\n" +
                    $"- Trang chi tiết: {locationLink}\n" +
                    $"- OpenStreetMap: {openStreetMapLink}\n";

                testDescription += $"\nThông tin bổ sung: Đây là thông báo TEST";
                testDescription += $"\n\nThời gian thông báo: {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")}";

               
                var warning = new Warning
                {
                    UserId = patientId,
                    Description = testDescription,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Warnings.Add(warning);
                await _context.SaveChangesAsync();

               
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await _notificationService.SendNotificationAsync(
                        user.Email,
                        "[TEST] 🚨 CẢNH BÁO KHẨN CẤP!",
                        testDescription
                    );
                }

                
                await _notificationService.SendWebNotificationAsync(
                    patientId,
                    "[TEST] 🚨 CẢNH BÁO KHẨN CẤP!",
                    testDescription,
                    "test",
                    false
                );

               
                await _patientNotificationService.SendNotificationToPatientCircleAsync(
                    patientId,
                    "[TEST] 🚨 CẢNH BÁO KHẨN CẤP!",
                    $"[TEST] Bệnh nhân {user.PatientName} (ID: {patientId}) vừa kích hoạt nút khẩn cấp! Vui lòng kiểm tra ngay.",
                    "test"
                );

                
                var additionalData = new Dictionary<string, string>
                {
                    { "warningId", warning.WarningId.ToString() },
                    { "gpsId", gps.GpsId.ToString() },
                    { "latitude", request.Latitude.ToString() },
                    { "longitude", request.Longitude.ToString() },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "locationLink", locationLink },
                    { "openStreetMapLink", openStreetMapLink },
                    { "type", "test" }
                };

                await _mobileNotificationService.SendNotificationToUserAsync(
                    patientId,
                    "[TEST] 🚨 CẢNH BÁO KHẨN CẤP!",
                    $"[TEST] Bệnh nhân {user.PatientName} cần trợ giúp khẩn cấp!",
                    "test",
                    additionalData
                );

                return Ok(new
                {
                    message = "Test emergency notification sent successfully",
                    warningId = warning.WarningId,
                    gpsId = gps.GpsId,
                    locationLink = locationLink,
                    openStreetMapLink = openStreetMapLink
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test emergency notification");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

       
        [HttpGet("my-emergencies")]
        public async Task<IActionResult> GetMyEmergencies(
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
                    .Where(w => w.UserId == userId && w.Description.Contains("KHẨN CẤP"));

                if (startDate.HasValue)
                    query = query.Where(w => w.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(w => w.CreatedAt <= endDate.Value);

                var emergencies = await query
                    .OrderByDescending(w => w.CreatedAt)
                    .Select(w => new EmergencyListItemDto
                    {
                        WarningId = w.WarningId,
                        Description = w.Description,
                        CreatedAt = w.CreatedAt,
                        FormattedTimestamp = w.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        IsActive = w.IsActive,
                        
                        GpsId = _context.Gps
                            .Where(g => g.UserId == w.UserId &&
                                  g.CreatedAt >= w.CreatedAt.AddMinutes(-5) &&
                                  g.CreatedAt <= w.CreatedAt.AddMinutes(5))
                            .OrderByDescending(g => g.CreatedAt)
                            .Select(g => (int?)g.GpsId)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalCount = emergencies.Count,
                    Emergencies = emergencies
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's emergency history");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        
        [HttpGet("patient-emergencies/{patientId}")]
        [Authorize(Roles = "admin,doctor")]
        public async Task<IActionResult> GetPatientEmergencies(
            int patientId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
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
                                      r.UserId == patientId &&
                                      r.RelationshipType == "doctor-patient");

                    if (!hasRelationship)
                    {
                        return StatusCode(403, new { message = "You do not have access to this patient's emergencies" });
                    }
                }

                var query = _context.Warnings
                    .Where(w => w.UserId == patientId && w.Description.Contains("KHẨN CẤP"));

                if (startDate.HasValue)
                    query = query.Where(w => w.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(w => w.CreatedAt <= endDate.Value);

                var emergencies = await query
                    .OrderByDescending(w => w.CreatedAt)
                    .Select(w => new EmergencyListItemDto
                    {
                        WarningId = w.WarningId,
                        Description = w.Description,
                        CreatedAt = w.CreatedAt,
                        FormattedTimestamp = w.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        IsActive = w.IsActive,
                        GpsId = _context.Gps
                            .Where(g => g.UserId == w.UserId &&
                                  g.CreatedAt >= w.CreatedAt.AddMinutes(-5) &&
                                  g.CreatedAt <= w.CreatedAt.AddMinutes(5))
                            .OrderByDescending(g => g.CreatedAt)
                            .Select(g => (int?)g.GpsId)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                var patient = await _context.StrokeUsers.FindAsync(patientId);

                return Ok(new
                {
                    Patient = new
                    {
                        UserId = patientId,
                        PatientName = patient != null ? patient.PatientName : "Unknown"
                    },
                    TotalCount = emergencies.Count,
                    Emergencies = emergencies
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting emergency history for patient ID {patientId}");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}