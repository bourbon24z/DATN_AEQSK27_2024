using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Services
{
    public interface IPatientNotificationService
    {
        Task SendNotificationToPatientDoctorsAsync(
            int patientId,
            string title,
            string message,
            string type = "warning",
            bool saveWarning = false,
            List<string> detailsList = null,
            double? latitude = null,
            double? longitude = null);

        Task SendNotificationToPatientFamilyAsync(
            int patientId,
            string title,
            string message,
            string type = "warning",
            bool saveWarning = true,
            List<string> detailsList = null,
            double? latitude = null,
            double? longitude = null);

        Task SendNotificationToPatientCircleAsync(
            int patientId,
            string title,
            string message,
            string type = "warning",
            List<string> detailsList = null,
            double? latitude = null,
            double? longitude = null);
    }
}