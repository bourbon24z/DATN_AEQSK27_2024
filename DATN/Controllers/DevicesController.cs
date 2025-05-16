using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        public DevicesController(StrokeDbContext context)
        {
            _context = context;
        }
        [HttpPost("add-device")]
        public async Task<IActionResult> AddDevice([FromBody] deviceDTO device)
        {
            try {
                var userId = device.UserId;
                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                var existingDevice = await _context.Device
                        .FirstOrDefaultAsync(d => d.Series == device.Series && d.UserId == userId);
                if (existingDevice != null)
                {
                    existingDevice.DeviceName = device.DeviceName;
                    existingDevice.DeviceType = device.DeviceType;
                    existingDevice.IsLocked = false;
                    existingDevice.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    var responseDTO = new deviceDTO
                    {
                        DeviceId = existingDevice.DeviceId,
                        UserId = existingDevice.UserId,
                        DeviceName = existingDevice.DeviceName,
                        DeviceType = existingDevice.DeviceType,
                        Series = existingDevice.Series,
                        IsLocked = existingDevice.IsLocked,
                        CreatedAt = existingDevice.CreatedAt,
                        UpdatedAt = existingDevice.UpdatedAt
                    };

                    return Ok(new
                    {
                        message = "Device updated successfully",
                        device = responseDTO,
                        isNew = false
                    });
                }
                else
                {
                    var newDevice = new Device
                    {
                        UserId = userId,
                        DeviceName = device.DeviceName,
                        DeviceType = device.DeviceType,
                        Series = device.Series,
                        IsLocked = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Device.Add(newDevice);
                    await _context.SaveChangesAsync();
                    var responseDTO = new deviceDTO
                    {
                        DeviceId = newDevice.DeviceId,
                        UserId = newDevice.UserId,
                        DeviceName = newDevice.DeviceName,
                        DeviceType = newDevice.DeviceType,
                        Series = newDevice.Series,
                        IsLocked = newDevice.IsLocked,
                        CreatedAt = newDevice.CreatedAt,
                        UpdatedAt = newDevice.UpdatedAt
                    };

                    return Ok(new
                    {
                        message = "Device added successfully",
                        device = responseDTO,
                        isNew = true
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("get-devices/{userId}")]
        public async Task<IActionResult> GetDevices(int userId)
        {
            try {
                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                var devices = await _context.Device
                .Where(d => d.UserId == userId && !d.IsLocked)
                .ToListAsync();

                if (devices == null || !devices.Any())
                {
                    return Ok(new { message = "No active devices found for this user", devices = new Device[0] });
                }

                return Ok(new { message = "Devices retrieved successfully", devices });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("lock-device/{deviceId}")]
        public async Task<IActionResult> LockDevice(int deviceId)
        {
            try
            {
                var device = await _context.Device.FindAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found");
                }

                device.IsLocked = true;
                device.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Device locked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("unlock-device/{deviceId}")]
        public async Task<IActionResult> UnlockDevice(int deviceId)
        {
            try
            {
                var device = await _context.Device.FindAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found");
                }


                device.IsLocked = false;
                device.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Device unlocked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpGet("get-all-devices/{userId}")]
        public async Task<IActionResult> GetAllDevices(int userId)
        {
            try
            {
                var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                
                var devices = await _context.Device
                    .Where(d => d.UserId == userId)
                    .Select(d => new {
                        d.DeviceId,
                        d.DeviceName,
                        d.DeviceType,
                        d.Series,
                        d.UserId,
                        d.IsLocked,
                        d.CreatedAt,
                        d.UpdatedAt,
                        DataCount = d.UserMedicalDatas.Count
                    })
                    .ToListAsync();

                if (devices == null || !devices.Any())
                {
                    return Ok(new { message = "No devices found for this user", devices = new object[0] });
                }

                return Ok(new { message = "All devices retrieved successfully", devices });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
            [HttpDelete("delete-device/{deviceId}")]
            public async Task<IActionResult> DeleteDevice(int deviceId)
            {
                var device = await _context.Device.FindAsync(deviceId);
                if (device == null)
                {
                    return NotFound("Device not found");
                }
                var userMedicalDatas = await _context.UserMedicalDatas
                    .Where(umd => umd.DeviceId == deviceId)
                    .ToListAsync();
                if (userMedicalDatas != null && userMedicalDatas.Count > 0)
                {
                    foreach (var umd in userMedicalDatas)
                    {
                        _context.UserMedicalDatas.Remove(umd);
                    }
                }
                _context.Device.Remove(device);
                await _context.SaveChangesAsync();
                return Ok("Device deleted successfully");
            }

        }
    }
