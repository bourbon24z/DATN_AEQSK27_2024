using DATN.Data;
using DATN.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly StrokeDbContext _context;

        public DoctorController(StrokeDbContext context)
        {
            _context = context;
        }

        
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDoctorDashboard()
        {
            try
            {
                
                var totalPatients = await _context.StrokeUsers
                    .Where(u => _context.UserRoles
                        .Any(ur => ur.UserId == u.UserId && ur.Role.RoleName == "user" && ur.IsActive))
                    .CountAsync();

                
                var totalCaseHistories = await _context.CaseHistories.CountAsync();

                
                var recentCaseHistories = await _context.CaseHistories
                    .Include(c => c.StrokeUser)
                    .OrderByDescending(c => c.Time)
                    .Take(5)
                    .Select(c => new
                    {
                        CaseHistoryId = c.CaseHistoryId,
                        PatientName = c.StrokeUser.PatientName,
                        Time = c.Time,
                        FormattedTime = c.Time.ToString("dd/MM/yyyy HH:mm"),
                        StatusOfMr = c.StatusOfMr
                    })
                    .ToListAsync();

              
                var highRiskPatients = await _context.ClinicalIndicators
                    .Where(ci =>
                        (ci.DauDau ? 1 : 0) +
                        (ci.TeMatChi ? 1 : 0) +
                        (ci.ChongMat ? 1 : 0) +
                        (ci.KhoNoi ? 1 : 0) +
                        (ci.MatTriNhoTamThoi ? 1 : 0) +
                        (ci.LuLan ? 1 : 0) +
                        (ci.GiamThiLuc ? 1 : 0) +
                        (ci.MatThangCan ? 1 : 0) +
                        (ci.BuonNon ? 1 : 0) +
                        (ci.KhoNuot ? 1 : 0) >= 7)
                    .CountAsync();

                
                var newEvaluations = await _context.DoctorEvaluations
                    .Where(e => e.EvaluationDate >= DateTime.Now.AddDays(-7))
                    .CountAsync();

                return Ok(new
                {
                    Success = true,
                    CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Data = new
                    {
                        TotalPatients = totalPatients,
                        TotalCaseHistories = totalCaseHistories,
                        HighRiskPatients = highRiskPatients,
                        NewEvaluations = newEvaluations,
                        RecentCaseHistories = recentCaseHistories
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        
        [HttpGet("patient/{userId}/summary")]
        public async Task<IActionResult> GetPatientSummary(int userId)
        {
            try
            {
                
                var patient = await _context.StrokeUsers.FindAsync(userId);
                if (patient == null)
                    return NotFound(new { Success = false, Message = "Không tìm thấy bệnh nhân" });

               
                var clinicalIndicator = await _context.ClinicalIndicators
                    .FirstOrDefaultAsync(ci => ci.UserID == userId && ci.IsActived);

                var molecularIndicator = await _context.MolecularIndicators
                    .FirstOrDefaultAsync(mi => mi.UserID == userId && mi.IsActived);

                var subclinicalIndicator = await _context.SubclinicalIndicators
                    .FirstOrDefaultAsync(si => si.UserID == userId && si.IsActived);

                var recentMedicalData = await _context.UserMedicalDatas
                    .Include(umd => umd.Device)
                    .Where(umd => umd.Device.UserId == userId)
                    .OrderByDescending(umd => umd.RecordedAt)
                    .Take(10)
                    .ToListAsync();

                
                var caseHistories = await _context.CaseHistories
                    .Where(ch => ch.UserId == userId)
                    .OrderByDescending(ch => ch.Time)
                    .Take(5)
                    .ToListAsync();

               
                int percent1 = 0, percent2 = 0, percent3 = 0, totalPercent = 0;
                int totalCount1 = 0, trueCount1 = 0, totalCount2 = 0, trueCount2 = 0, totalCount3 = 0, trueCount3 = 0;

                if (clinicalIndicator != null)
                {
                    totalCount1 += 10;
                    if (clinicalIndicator.DauDau) trueCount1++;
                    if (clinicalIndicator.TeMatChi) trueCount1++;
                    if (clinicalIndicator.ChongMat) trueCount1++;
                    if (clinicalIndicator.KhoNoi) trueCount1++;
                    if (clinicalIndicator.MatTriNhoTamThoi) trueCount1++;
                    if (clinicalIndicator.LuLan) trueCount1++;
                    if (clinicalIndicator.GiamThiLuc) trueCount1++;
                    if (clinicalIndicator.MatThangCan) trueCount1++;
                    if (clinicalIndicator.BuonNon) trueCount1++;
                    if (clinicalIndicator.KhoNuot) trueCount1++;
                }

                if (molecularIndicator != null)
                {
                    totalCount2 += 10;
                    if (molecularIndicator.MiR_30e_5p) trueCount2++;
                    if (molecularIndicator.MiR_16_5p) trueCount2++;
                    if (molecularIndicator.MiR_140_3p) trueCount2++;
                    if (molecularIndicator.MiR_320d) trueCount2++;
                    if (molecularIndicator.MiR_320p) trueCount2++;
                    if (molecularIndicator.MiR_20a_5p) trueCount2++;
                    if (molecularIndicator.MiR_26b_5p) trueCount2++;
                    if (molecularIndicator.MiR_19b_5p) trueCount2++;
                    if (molecularIndicator.MiR_874_5p) trueCount2++;
                    if (molecularIndicator.MiR_451a) trueCount2++;
                }

                if (subclinicalIndicator != null)
                {
                    totalCount3 += 10;
                    if (subclinicalIndicator.S100B) trueCount3++;
                    if (subclinicalIndicator.MMP9) trueCount3++;
                    if (subclinicalIndicator.GFAP) trueCount3++;
                    if (subclinicalIndicator.RBP4) trueCount3++;
                    if (subclinicalIndicator.NT_proBNP) trueCount3++;
                    if (subclinicalIndicator.sRAGE) trueCount3++;
                    if (subclinicalIndicator.D_dimer) trueCount3++;
                    if (subclinicalIndicator.Lipids) trueCount3++;
                    if (subclinicalIndicator.Protein) trueCount3++;
                    if (subclinicalIndicator.VonWillebrand) trueCount3++;
                }

                
                if (totalCount1 > 0 && molecularIndicator == null && subclinicalIndicator == null)
                {
                    percent1 = (trueCount1 * 70) / totalCount1;
                }
                else
                {
                    if (totalCount1 != 0)
                    {
                        percent1 = (trueCount1 * 30) / totalCount1;
                    }
                    if (totalCount2 != 0)
                    {
                        percent2 = (trueCount2 * 30) / totalCount2;
                    }
                    if (totalCount3 != 0)
                    {
                        percent3 = (trueCount3 * 30) / totalCount3;
                    }
                }

                totalPercent = percent1 + percent2 + percent3;

                var patientGps = await _context.Gps
                    .FirstOrDefaultAsync(g => g.UserId == userId);

               
                var relationships = await _context.Relationships
                    .Include(r => r.Inviter)
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                
                var devices = await _context.Device
                    .Where(d => d.UserId == userId)
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Data = new
                    {
                        PatientInfo = new
                        {
                            UserId = patient.UserId,
                            PatientName = patient.PatientName,
                            DateOfBirth = patient.DateOfBirth,
                            Age = DateTime.Today.Year - patient.DateOfBirth.Year,
                            Gender = patient.Gender ? "Nam" : "Nữ",
                            Phone = patient.Phone,
                            Email = patient.Email
                        },
                        RiskAssessment = new
                        {
                            TotalRiskPercentage = totalPercent,
                            ClinicalRiskPercentage = percent1,
                            MolecularRiskPercentage = percent2,
                            SubclinicalRiskPercentage = percent3,
                            RiskLevel = totalPercent >= 70 ? "Cao" :
                                       totalPercent >= 30 ? "Trung bình" : "Thấp"
                        },
                        ClinicalIndicators = clinicalIndicator,
                        MolecularIndicators = molecularIndicator,
                        SubclinicalIndicators = subclinicalIndicator,
                        Location = patientGps != null ? new
                        {
                            Latitude = patientGps.Lat,
                            Longitude = patientGps.Lon,
                            LastUpdated = patientGps.CreatedAt
                        } : null,
                        Devices = devices.Select(d => new
                        {
                            d.DeviceId,
                            d.DeviceName,
                            d.DeviceType,
                            d.Series
                        }).ToList(),
                        MedicalDataSummary = recentMedicalData.Select(md => new
                        {
                            DeviceName = md.Device.DeviceName,
                            RecordedAt = md.RecordedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            HeartRate = md.HeartRate,
                            SystolicPressure = md.SystolicPressure,
                            DiastolicPressure = md.DiastolicPressure,
                            Temperature = md.Temperature,
                            SpO2 = md.Spo2Information,
                            BloodPh = md.BloodPh
                        }).ToList(),
                        RecentCaseHistories = caseHistories.Select(ch => new
                        {
                            CaseHistoryId = ch.CaseHistoryId,
                            Time = ch.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                            StatusOfMr = ch.StatusOfMr,
                            ProgressNotes = ch.ProgressNotes?.Length > 100
                                ? ch.ProgressNotes.Substring(0, 100) + "..."
                                : ch.ProgressNotes
                        }).ToList(),
                        FamilyRelationships = relationships.Select(r => new
                        {
                            RelationshipId = r.RelationshipId,
                            RelationshipType = r.RelationshipType,
                            FamilyMember = new
                            {
                                UserId = r.Inviter.UserId,
                                PatientName = r.Inviter.PatientName,
                                Phone = r.Inviter.Phone,
                                Email = r.Inviter.Email
                            }
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

     
        [HttpGet("patient/{userId}/anomalies")]
        public async Task<IActionResult> GetPatientAnomalies(int userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var patient = await _context.StrokeUsers.FindAsync(userId);
                if (patient == null)
                    return NotFound(new { Success = false, Message = "Không tìm thấy bệnh nhân" });

                
                if (!startDate.HasValue)
                    startDate = DateTime.Now.AddDays(-7);
                if (!endDate.HasValue)
                    endDate = DateTime.Now;

              
                var medicalData = await _context.UserMedicalDatas
                    .Include(md => md.Device)
                    .Where(md => md.Device.UserId == userId &&
                           md.RecordedAt >= startDate &&
                           md.RecordedAt <= endDate)
                    .OrderBy(md => md.RecordedAt)
                    .ToListAsync();

                if (!medicalData.Any())
                    return NotFound(new { Success = false, Message = "Không có dữ liệu y tế trong khoảng thời gian đã chọn" });

                
                var anomalies = new List<object>();

                foreach (var data in medicalData)
                {
                    var issues = new List<string>();

                  
                    if (data.Temperature > 37.5f || data.Temperature < 36.0f)
                        issues.Add($"Nhiệt độ: {data.Temperature}°C ({(data.Temperature > 37.5f ? "cao" : "thấp")})");

                   
                    if (data.HeartRate > 100 || data.HeartRate < 60)
                        issues.Add($"Nhịp tim: {data.HeartRate} bpm ({(data.HeartRate > 100 ? "cao" : "thấp")})");

                    
                    if (data.SystolicPressure > 140 || data.DiastolicPressure > 90)
                        issues.Add($"Huyết áp: {data.SystolicPressure}/{data.DiastolicPressure} mmHg (cao)");

                   
                    if (data.Spo2Information < 95)
                        issues.Add($"SpO2: {data.Spo2Information}% (thấp)");

                    
                    if (data.BloodPh > 7.45f || data.BloodPh < 7.35f)
                        issues.Add($"pH máu: {data.BloodPh} ({(data.BloodPh > 7.45f ? "kiềm" : "axit")})");

                   
                    if (issues.Any())
                    {
                        anomalies.Add(new
                        {
                            RecordedAt = data.RecordedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            DeviceName = data.Device.DeviceName,
                            Issues = issues
                        });
                    }
                }

                
                var stats = new
                {
                    TotalReadings = medicalData.Count,
                    AbnormalReadings = anomalies.Count,
                    AbnormalPercentage = medicalData.Count > 0
                        ? (double)anomalies.Count / medicalData.Count * 100
                        : 0,
                    DateRange = new
                    {
                        From = startDate.Value.ToString("yyyy-MM-dd"),
                        To = endDate.Value.ToString("yyyy-MM-dd")
                    }
                };

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        PatientInfo = new
                        {
                            UserId = patient.UserId,
                            PatientName = patient.PatientName
                        },
                        Statistics = stats,
                        Anomalies = anomalies
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}