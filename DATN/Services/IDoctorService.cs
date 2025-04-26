using DATN.Dto;
using DATN.Models;

public interface IDoctorService
{
    Task<CaseHistory> CreateCaseHistoryAsync(CaseHistoryDto dto);
    Task<List<CaseHistory>> GetAllCaseHistoriesAsync();
    Task<CaseHistory> GetCaseHistoryByIdAsync(int id);
    Task<List<DoctorEvaluation>> GetDoctorEvaluationsAsync(int caseHistoryId);
    Task AddDoctorEvaluationAsync(int caseHistoryId, DoctorEvaluationDto dto, int doctorId);
    Task<List<MedicalImage>> GetMedicalImagesAsync(int caseHistoryId);
    Task AddMedicalImageAsync(int caseHistoryId, MedicalImageDto dto);
}