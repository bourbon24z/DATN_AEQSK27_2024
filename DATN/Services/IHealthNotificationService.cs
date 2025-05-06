namespace DATN.Services
{
    public interface IHealthNotificationService
    {
        Task SendHealthWarningToAllDoctors();
        Task SendPatientStatusToFamilyAndDoctors(int patientId, string status);
        Task NotifyFamilyAboutAbnormalReadings(int patientId, string readingType, string value);
        Task NotifyEmergencyContacts(int patientId, string urgentMessage, bool includeDoctors = true);
    }
}
