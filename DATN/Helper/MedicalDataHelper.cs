using Microsoft.EntityFrameworkCore.Query.Internal;

namespace DATN.Helper
{
	public class MedicalDataHelper
	{
		public class AverageMedicalData
		{
			public float? Temperature { get; set; }
			public float? SpO2 { get; set; }
			public float? HeartRate { get; set; }
			public float? BloodPh { get; set; }
			public float? SystolicPressure { get; set; }
			public float? DiastolicPressure { get; set; }
		}

		public class PercentAverageMedicalData
		{
			public float Temperature { get; set; }
			public float SpO2 { get; set; }
			public float HeartRate { get; set; }
			public float BloodPh { get; set; }
			public float SystolicPressure { get; set; }
			public float DiastolicPressure { get; set; }
		}

		public static PercentAverageMedicalData GetPercentAverageMedicalData(AverageMedicalData averageMedicalData)
		{
			var percentAverageMedicalData = new PercentAverageMedicalData
			{
				Temperature = Math.Abs((float)((averageMedicalData.Temperature - 36) / 36.0 * 100)),
				SpO2 = Math.Abs((float)((averageMedicalData.SpO2 - 95) / 95.0 * 100)),
				HeartRate = Math.Abs((float)((averageMedicalData.HeartRate - 75) / 75.0 * 100)),
				BloodPh = Math.Abs((float)((averageMedicalData.BloodPh - 7.4) / 7.4 * 100)),
				SystolicPressure = Math.Abs((float)((averageMedicalData.SystolicPressure - 120) / 120.0 * 100)),
				DiastolicPressure = Math.Abs((float)((averageMedicalData.DiastolicPressure - 80) / 80.0 * 100)),
			};
			return percentAverageMedicalData;
		}

		public static string getWarningTemperature(double temperaturePercent)
		{
			if (temperaturePercent <= 1.35)
			{
				return "Normal";
			}
			else if (temperaturePercent <= 2.7)
			{
				return "Risk";
			}
			else
			{
				return "Warning";
			}
		}
		public static string getWarningSpO2(double spO2Percent)
		{
			if (spO2Percent <= 1.05)
			{
				return "Normal";
			}
			else if (spO2Percent <= 5.25)
			{
				return "Risk";
			}
			else
			{
				return "Warning";
			}
		}
		public static string getWarningHeartRate(double heartRatePercent)
		{
			if (heartRatePercent <= 20)
			{
				return "Normal";
			}
			else if (heartRatePercent <= 33.33)
			{
				return "Risk";
			}
			else
			{
				return "Warning";
			}
		}
		public static string getWarningBloodPh(double bloodPhPercent)
		{
			if (bloodPhPercent < 0.68)
			{
				return "Normal";
			}
			else if (bloodPhPercent < 2.7)
			{
				return "Risk";
			}
			else
			{
				return "Warning";
			}
		}
		public static string getWarningSystolicPressure(double systolicPressurePercent)
		{
			if (systolicPressurePercent <= 16.17)
			{
				return "Normal";
			}
			else if (systolicPressurePercent <= 33.33)
			{
				return "Risk";
			}
			else
			{
				return "Warning";
			}
		}
		public static string getWarningDiastolicPressure(double diastolicPressurePercent)
		{
			if (diastolicPressurePercent <= 12.5)
			{
				return "Normal";
			}
			else if (diastolicPressurePercent <= 25)
			{
				return "Risk";
			}
			else
			{
				return "Warning";
			}
		}

	}
}
