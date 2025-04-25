    using DATN.Data;
    using DATN.Dto;
    using DATN.Models;
    using DATN.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace DATN.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class WarningController : ControllerBase
        {
            private readonly StrokeDbContext _context;
            private readonly INotificationService _notificationService ;
       

            public WarningController(StrokeDbContext context, INotificationService notificationService)
            {
                _context = context;
                _notificationService = notificationService;
            }

       
            [HttpPost("device-reading")]
            public async Task<IActionResult> ProcessDeviceReading([FromBody] DeviceDataDto deviceData)
            {
                if (deviceData == null)
                    return BadRequest("Invalid device data.");
                if (deviceData.Measurements == null)
                    return BadRequest("Measurement data is required.");

            
                var strokeUser = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == deviceData.UserId);
                if (strokeUser == null)
                    return NotFound("User not found.");

                int overallLevel = 0;  // 0: Normal, 1: Risk, 2: Alarm
                List<string> detailsList = new List<string>();

                // 1. Temperature (Standard: 37°C)
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
                        detailsList.Add($"Temperature: {temp}°C (normal: 37 ±0.5°C, {(level == 1 ? "Risk" : "Alarm")})");
                }

                // 2. Systolic Pressure (Standard: 120 mmHg)
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
                        detailsList.Add($"Systolic Pressure: {systolic} mmHg (normal: ≤140, {(level == 1 ? "Risk" : "Alarm")})");
                }

                // 3. Diastolic Pressure (Standard: 80 mmHg)
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
                        detailsList.Add($"Diastolic Pressure: {diastolic} mmHg (normal: ≤90, {(level == 1 ? "Risk" : "Alarm")})");
                }

                // 4. Heart Rate (Standard: 75 bpm)
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
                        detailsList.Add($"Heart Rate: {hr} bpm (normal: 60–90, {(level == 1 ? "Risk" : "Alarm")})");
                }

                // 5. SPO2 (Standard: 95%)
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
                        detailsList.Add($"SPO2: {spo2}% (normal: ≥95%, {(level == 1 ? "Risk" : "Alarm")})");
                }

                // 6. Blood pH (Standard: 7.4)
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
                        detailsList.Add($"Blood pH: {ph} (normal: 7.4 ±0.05, {(level == 1 ? "Risk" : "Alarm")})");
                }

            
                // 0 => NORMAL; 1 => WARNING; 2 => RISK.
                string classification;
                if (overallLevel == 0)
                    classification = "NORMAL";
                else if (overallLevel == 1)
                    classification = "WARNING";
                else
                    classification = "RISK";

          
                bool hasGps = deviceData.GPS != null &&
                              (Math.Abs(deviceData.GPS.Lat) > 0.0001f || Math.Abs(deviceData.GPS.Long) > 0.0001f);
                if (classification == "WARNING" && !hasGps)
                    classification = "RISK";

                string details = (detailsList.Count > 0)
                    ? string.Join("; ", detailsList)
                    : "All measurements are normal.";
                string description = (classification == "NORMAL")
                    ? details
                    : $"{classification}: {details}.";

            
                if (classification == "NORMAL")
                {
                    return Ok(description);
                }

          
                if (classification == "WARNING" || classification == "RISK")
                {
                    await _notificationService.SendNotificationAsync(strokeUser.Email, "Warning Alert", description);
                }

                // save record into DB
                Warning warningRecord = new Warning
                {
                    UserId = deviceData.UserId,
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Warnings.Add(warningRecord);
                await _context.SaveChangesAsync();

                return Ok($"Warning processed and stored successfully. Details: {description}");
            }
        }
    }
