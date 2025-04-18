using DATN.Data;
using DATN.Dto;
using DATN.Helper;
using DATN.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DATN.Helper.MedicalDataHelper;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMedicalDatasController : ControllerBase
    {
		private readonly StrokeDbContext _context;
		public UserMedicalDatasController(StrokeDbContext context)
		{
			_context = context;
		}

		[HttpPost("medicaldata")]
		public async Task<IActionResult> AddMedicalData([FromBody]MedicalDataDTO medicalData)
		{
			var device = await _context.Device.FirstOrDefaultAsync(d => d.Series == medicalData.Series);
			if (device == null)
			{
				return NotFound("Device not found");
			}
			var userMedicalData = new UserMedicalData
			{
				DeviceId = device.DeviceId,
				SystolicPressure = medicalData.SystolicPressure,
				DiastolicPressure = medicalData.DiastolicPressure,
				Temperature = medicalData.Temperature,
				BloodPh = medicalData.BloodPh,
				RecordedAt = medicalData.RecordedAt,
				Spo2Information = medicalData.Spo2Information,
				HeartRate = medicalData.HeartRate,
				CreatedAt = DateTime.UtcNow
			};
			_context.UserMedicalDatas.Add(userMedicalData);
			await _context.SaveChangesAsync();
			return Ok(new { message = "Data added successfully" });

		}


		[HttpGet("daily/{date}/{deviceId}")]
		public async Task<IActionResult> GetDataByDate(DateTime date,int deviceId)
		{
			if (date == null || deviceId == null)
			{
				return BadRequest("Data is emty");
			}
			var databyDates = await _context.UserMedicalDatas.Where(u => u.DeviceId==deviceId && u.RecordedAt.Date == date.Date).ToListAsync();

			return Ok(databyDates);

		}

		[HttpGet("average-last-14-days/{deviceId}")]
		public async Task<IActionResult> GetAverageLast14Days(int deviceId)
		{
			var currentDate = DateTime.UtcNow.Date;
			var startDate = currentDate.AddDays(-13);

			var data = await _context.UserMedicalDatas
				.Where(d => d.RecordedAt.Date >= startDate && d.RecordedAt.Date <= currentDate && d.DeviceId == deviceId)
				.ToListAsync();

			if (data == null || !data.Any())
			{
				return NotFound("Có cái nịt nề chứ tìm");
			}

			var averageData = data
				.GroupBy(d => d.RecordedAt.Date)
				.Select(g => new
				{
					Date = g.Key,
					AverageSpO2 = g.Average(d => d.Temperature),
					AverageSpO3 = g.Average(d => d.Spo2Information),
					AverageSpO4 = g.Average(d => d.HeartRate),
					AverageSpO5 = g.Average(d => d.BloodPh),
					AverageSpO6 = g.Average(d => d.SystolicPressure)
				})
				.OrderBy(d => d.Date)
				.ToList();

			return Ok(averageData);
		}

		[HttpGet("daily-nightly-all/{date}/{deviceId}")]
		public async Task<IActionResult> GetDailyNightlyAllData(DateTime date, int deviceId)
		{
			// Lấy dữ liệu cả ngày (từ 0h đến 23h59)
			var allDayData = await _context.UserMedicalDatas
				.Where(d => d.RecordedAt.Date == date.Date && d.DeviceId == deviceId)
				.ToListAsync();

			// Lấy dữ liệu ban ngày (từ 6h sáng đến 18h)
			var dailyData = allDayData
				.Where(d => d.RecordedAt.Hour >= 6 && d.RecordedAt.Hour < 18)
				.ToList();

			// Lấy dữ liệu ban đêm (từ 18h đến 6h sáng ngày hôm sau)
			var nightlyData = await _context.UserMedicalDatas
				.Where(d => (d.RecordedAt.Date == date.Date && d.RecordedAt.Hour >= 18) ||
							 (d.RecordedAt.Date == date.Date.AddDays(1) && d.RecordedAt.Hour < 6) &&
							 d.DeviceId == deviceId)
				.ToListAsync();

			if ((allDayData == null || !allDayData.Any()) && (dailyData == null || !dailyData.Any()) && (nightlyData == null || !nightlyData.Any()))
			{
				return NotFound("No data found for the selected date and user.");
			}

			return Ok(new
			{
				Date = date.ToString("yyyy-MM-dd"),
				AllDayData = allDayData,
				DailyData = dailyData,
				NightlyData = nightlyData
			});
		}


		[HttpGet("average-daily-night-last-14-days/{deviceId}")]
		public async Task<IActionResult> GetDailyNightAverageLast14Days(int deviceId)
		{
			var currentDate = DateTime.UtcNow.Date;
			var startDate = currentDate.AddDays(-13);

			// Lấy dữ liệu trong 14 ngày
			var data = await _context.UserMedicalDatas
				.Where(d => d.RecordedAt.Date >= startDate && d.RecordedAt.Date <= currentDate && d.DeviceId == deviceId)
				.ToListAsync();

			if (data == null || !data.Any())
			{
				return NotFound("Không có dữ liệu trong 14 ngày gần nhất.");
			}

			var averageAll14Day = new AverageMedicalData
			{
				Temperature = data.Average(d => d.Temperature),
				SpO2 = data.Average(d => d.Spo2Information),
				HeartRate = data.Average(d => d.HeartRate),
				BloodPh = data.Average(d => d.BloodPh),
				SystolicPressure = data.Average(d => d.SystolicPressure),
				DiastolicPressure = data.Average(d => d.DiastolicPressure),

			};
			var dataPercent = MedicalDataHelper.GetPercentAverageMedicalData(averageAll14Day);
			var warning = new List<object>();
			warning.Add(new
			{
				Temperature = MedicalDataHelper.getWarningTemperature(dataPercent.Temperature),
				SpO2 = MedicalDataHelper.getWarningSpO2(dataPercent.SpO2),
				HeartRate = MedicalDataHelper.getWarningHeartRate(dataPercent.HeartRate),
				BloodPh = MedicalDataHelper.getWarningBloodPh(dataPercent.BloodPh),
				SystolicPressure = MedicalDataHelper.getWarningSystolicPressure(dataPercent.SystolicPressure),
				DiastolicPressure = MedicalDataHelper.getWarningDiastolicPressure(dataPercent.DiastolicPressure)
			});
			// Tạo danh sách kết quả theo ngày
			var result = new List<object>();

			// Lặp qua từng ngày trong khoảng 14 ngày
			for (var date = startDate; date <= currentDate; date = date.AddDays(1))
			{
				// Lấy dữ liệu của ngày hiện tại
				var dailyData = data.Where(d => d.RecordedAt.Date == date).ToList();

				// Tính trung bình cho cả ngày (AllDay)
				var averageAllDay = dailyData.Any() ? new
				{
					AverageTemperature = dailyData.Average(d => d.Temperature),
					AverageSpO2 = dailyData.Average(d => d.Spo2Information),
					AverageHeartRate = dailyData.Average(d => d.HeartRate),
					AverageBloodPh = dailyData.Average(d => d.BloodPh),
					AverageSystolicPressure = dailyData.Average(d => d.SystolicPressure),
					AverageDiastolicPressure = dailyData.Average(d => d.DiastolicPressure),
					
				} : null;
				

				// Tính trung bình cho ban ngày (Daily: 6h - 18h)
				var dailyDayData = dailyData.Where(d => d.RecordedAt.Hour >= 6 && d.RecordedAt.Hour < 18).ToList();
				var averageDaily = dailyDayData.Any() ? new
				{
					AverageTemperature = dailyDayData.Average(d => d.Temperature),
					AverageSpO2 = dailyDayData.Average(d => d.Spo2Information),
					AverageHeartRate = dailyDayData.Average(d => d.HeartRate),
					AverageBloodPh = dailyDayData.Average(d => d.BloodPh),
					AverageSystolicPressure = dailyDayData.Average(d => d.SystolicPressure),
					AverageDiastolicPressure = dailyDayData.Average(d => d.DiastolicPressure)
				} : null;

				// Tính trung bình cho ban đêm (Nightly: 18h - 6h sáng ngày hôm sau)
				var nightlyData = data
					.Where(d => (d.RecordedAt.Date == date && d.RecordedAt.Hour >= 18) ||
								(d.RecordedAt.Date == date.AddDays(1) && d.RecordedAt.Hour < 6))
					.ToList();
				var averageNightly = nightlyData.Any() ? new
				{
					AverageTemperature = nightlyData.Average(d => d.Temperature),
					AverageSpO2 = nightlyData.Average(d => d.Spo2Information),
					AverageHeartRate = nightlyData.Average(d => d.HeartRate),
					AverageBloodPh = nightlyData.Average(d => d.BloodPh),
					AverageSystolicPressure = nightlyData.Average(d => d.SystolicPressure),
					AverageDiastolicPressure = nightlyData.Average(d => d.DiastolicPressure)
				} : null;

				// Thêm kết quả của ngày hiện tại vào danh sách
				result.Add(new
				{
					Date = date.ToString("yyyy-MM-dd"),
					AllDayAverage = averageAllDay,
					DailyAverage = averageDaily,
					NightlyAverage = averageNightly
				});
			}

			return Ok(new {
				averageAll14Day,
				dataPercent,
				warning,
				result
			});
		}
	}



}
