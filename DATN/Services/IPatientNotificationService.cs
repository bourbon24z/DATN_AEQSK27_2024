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
            bool saveWarning =false);

     
        Task SendNotificationToPatientFamilyAsync(
            int patientId,
            string title,
            string message,
            string type = "warning",
            bool saveWarning = false);

        
        Task SendNotificationToPatientCircleAsync(
            int patientId,
            string title,
            string message,
            string type = "warning");
    }
}