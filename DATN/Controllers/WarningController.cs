using DATN.Data;
using DATN.Dto;
using DATN.Models;
using DATN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public WarningController(StrokeDbContext context, INotificationService notificationService, INotificationFormatterService notificationFormatter, IMobileNotificationService mobileNotificationService)
        {
            _context = context;
            _notificationService = notificationService;
            _notificationFormatter = notificationFormatter;
            _mobileNotificationService = mobileNotificationService;
        }

        [HttpPost("device-reading")]
        public async Task<IActionResult> ProcessDeviceReading([FromBody] DeviceDataDto deviceData)
        {
            if (deviceData == null)
                return BadRequest("Dữ liệu không hợp lệ.");
            if (deviceData.Measurements == null)
                return BadRequest("Dữ liệu đo lường là bắt buộc.");
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

                // sned email
                await _notificationService.SendNotificationAsync(strokeUser.Email, "Cảnh báo", formattedDescription);
                Console.WriteLine($"[WarningController] Đã gửi thông báo email cho {strokeUser.Email}");

                // send web notification
                await _notificationService.SendWebNotificationAsync(
                    deviceData.UserId,
                    classification == "WARNING" ? "Cảnh Báo Nghiêm Trọng" : "Cảnh Báo",
                    formattedDescription,
                    classification.ToLower()
                );
                Console.WriteLine($"[WarningController] Đã gửi thông báo web cho người dùng ID {deviceData.UserId}");

                // send mobile notification
                if (_mobileNotificationService != null)
                {
                    try
                    {
                       
                        string briefNotification = CreateBriefMobileNotification(classificationVietnamese, detailsList);

                        
                        var additionalData = new Dictionary<string, string>
                        {
                            { "fullDescription", formattedDescription },
                            { "timestamp", DateTime.UtcNow.ToString("o") }
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

               
                Warning warningRecord = new Warning
                {
                    UserId = deviceData.UserId,
                    Description = formattedDescription,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Warnings.Add(warningRecord);

                if (hasGps)
                {
                    var gpsRecord = new Gps
                    {
                        UserId = deviceData.UserId,
                        Lat = deviceData.GPS.Lat,
                        Lon = deviceData.GPS.Long,
                        CreatedAt = DateTime.UtcNow
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

            
            if (details.Count <= 2)
            {
                content = string.Join("; ", details);
            }
            
            else
            {
               
                var shortenedDetails = details.Take(2)
                    .Select(d => {
                        
                        int bracketPos = d.IndexOf(" (");
                        if (bracketPos > 0)
                            return d.Substring(0, bracketPos);
                        return d;
                    })
                    .ToList();

                content = string.Join("; ", shortenedDetails);
                content += $" và {details.Count - 2} chỉ số khác";
            }

            return $"{classificationVietnamese}: {content}";
        }
    }
}