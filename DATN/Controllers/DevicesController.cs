using DATN.Data;
using DATN.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
		private readonly StrokeDbContext _context;
		public DevicesController( StrokeDbContext context)
		{
			_context = context;
		}
		[HttpPost("add-device")]
		public async Task<IActionResult> AddDevice([FromBody] deviceDTO device)
		{
            var userId = device.UserId;
            var user = await _context.StrokeUsers.FirstOrDefaultAsync(u => u.UserId == userId);
			if (user == null)
			{
				return NotFound("User not found");
			}
			var newDevice = new Models.Device
			{
				UserId = userId,
				DeviceName = device.DeviceName,
				DeviceType = device.DeviceType,
				Series = device.Series
			};
			_context.Device.Add(newDevice);
			await _context.SaveChangesAsync();
			return Ok(newDevice);
		}
		[HttpGet("get-devices/{userId}")]
		public async Task<IActionResult> GetDevices(int userId)
		{
			var devices = await _context.Device
				.Where(d => d.UserId == userId)
				.ToListAsync();
			if (devices == null || devices.Count == 0)
			{
				return NotFound("No devices found for this user");
			}
			return Ok(devices);
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
