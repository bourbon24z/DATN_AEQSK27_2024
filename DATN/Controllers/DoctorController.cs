using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        private readonly IndicatorsController _indicatorsController;

        public DoctorController(StrokeDbContext context)
        {
            _context = context;
            _indicatorsController = new IndicatorsController(context);
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
                    .AsNoTracking()
                    .Where(ci => ci.UserID == userId && ci.IsActived)
                    .Select(ci => new {
                        ci.ClinicalIndicatorID,
                        ci.UserID,
                        ci.IsActived,
                        ci.RecordedAt,
                        ci.DauDau,
                        ci.TeMatChi,
                        ci.ChongMat,
                        ci.KhoNoi,
                        ci.MatTriNhoTamThoi,
                        ci.LuLan,
                        ci.GiamThiLuc,
                        ci.MatThangCan,
                        ci.BuonNon,
                        ci.KhoNuot
                    })
                    .FirstOrDefaultAsync();

                var molecularIndicator = await _context.MolecularIndicators
                    .AsNoTracking()
                    .Where(mi => mi.UserID == userId && mi.IsActived)
                    .Select(mi => new {
                        mi.MolecularIndicatorID,
                        mi.UserID,
                        mi.IsActived,
                        mi.RecordedAt,
                        mi.MiR_30e_5p,
                        mi.MiR_16_5p,
                        mi.MiR_140_3p,
                        mi.MiR_320d,
                        mi.MiR_320p,
                        mi.MiR_20a_5p,
                        mi.MiR_26b_5p,
                        mi.MiR_19b_5p,
                        mi.MiR_874_5p,
                        mi.MiR_451a
                    })
                    .FirstOrDefaultAsync();

                var subclinicalIndicator = await _context.SubclinicalIndicators
                    .AsNoTracking()
                    .Where(si => si.UserID == userId && si.IsActived)
                    .Select(si => new {
                        si.SubclinicalIndicatorID,
                        si.UserID,
                        si.IsActived,
                        si.RecordedAt,
                        si.S100B,
                        si.MMP9,
                        si.GFAP,
                        si.RBP4,
                        si.NT_proBNP,
                        si.sRAGE,
                        si.D_dimer,
                        si.Lipids,
                        si.Protein,
                        si.VonWillebrand
                    })
                    .FirstOrDefaultAsync();

               
                var indicatorsController = new IndicatorsController(_context);
                var riskResult = await indicatorsController.GetPercentIndicatorIsTrue(userId) as OkObjectResult;

                if (riskResult == null || riskResult.Value == null)
                {
                    return BadRequest(new { Success = false, Message = "Không thể tính toán chỉ số rủi ro" });
                }

                dynamic riskData = riskResult.Value;

                
                var patientGps = await _context.Gps
                    .Where(g => g.UserId == userId)
                    .OrderByDescending(g => g.CreatedAt)
                    .FirstOrDefaultAsync();

                
                var deviceIds = await _context.Device
                    .Where(d => d.UserId == userId)
                    .Select(d => d.DeviceId)
                    .ToListAsync();

                
                var devices = _context.Device
                    .Where(d => d.UserId == userId)
                    .Select(d => new
                    {
                        d.DeviceId,
                        d.DeviceName,
                        d.DeviceType,
                        d.Series
                    })
                    .ToList();

              
                var recentMedicalData = new List<object>();
                if (deviceIds.Any())
                {
                    var medicalDataQuery = await _context.UserMedicalDatas
                        .Include(umd => umd.Device)
                        .Where(umd => umd.DeviceId.HasValue && deviceIds.Contains(umd.DeviceId.Value))
                        .OrderByDescending(umd => umd.RecordedAt)
                        .Take(10)
                        .ToListAsync();

                    recentMedicalData = medicalDataQuery.Select(md => new
                    {
                        DeviceName = md.Device.DeviceName,
                        RecordedAt = md.RecordedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        HeartRate = md.HeartRate,
                        SystolicPressure = md.SystolicPressure,
                        DiastolicPressure = md.DiastolicPressure,
                        Temperature = md.Temperature,
                        SpO2 = md.Spo2Information,
                        BloodPh = md.BloodPh
                    }).Cast<object>().ToList();
                }

               
                var caseHistories = _context.CaseHistories
                    .Where(ch => ch.UserId == userId)
                    .OrderByDescending(ch => ch.Time)
                    .Take(5)
                    .Select(ch => new
                    {
                        CaseHistoryId = ch.CaseHistoryId,
                        Time = ch.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        StatusOfMr = ch.StatusOfMr,
                        ProgressNotes = ch.ProgressNotes != null && ch.ProgressNotes.Length > 100
                            ? ch.ProgressNotes.Substring(0, 100) + "..."
                            : ch.ProgressNotes
                    })
                    .ToList();

               
                var relationships = _context.Relationships
                    .Include(r => r.Inviter)
                    .Where(r => r.UserId == userId)
                    .Select(r => new
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
                    })
                    .ToList();

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
                            TotalRiskPercentage = riskData.Percent,
                            ClinicalRiskPercentage = riskData.ClinicalIndicator.Percent,
                            MolecularRiskPercentage = riskData.MolecularIndicator.Percent,
                            SubclinicalRiskPercentage = riskData.SubclinicalIndicator.Percent,
                            RiskLevel = Convert.ToInt32(riskData.Percent) >= 70 ? "Cao" :
                                      Convert.ToInt32(riskData.Percent) >= 30 ? "Trung bình" : "Thấp"
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
                        Devices = devices,
                        MedicalDataSummary = recentMedicalData,
                        RecentCaseHistories = caseHistories,
                        FamilyRelationships = relationships
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
        [HttpGet("patients")]
        //http://localhost:5062/api/doctor/patients
        public async Task<IActionResult> GetPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                
                var patientUserIds = await _context.UserRoles
                    .Where(ur => ur.Role.RoleName == "user" && ur.IsActive)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

              
                var patientsQuery = _context.StrokeUsers
                    .AsNoTracking()
                    .Where(u => patientUserIds.Contains(u.UserId));

               
                var totalCount = await patientsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

               
                var patients = await patientsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var patientDtos = patients.Select(patient => new
                {
                    UserId = patient.UserId,
                    Username = patient.Username,
                    PatientName = patient.PatientName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = DateTime.Today.Year - patient.DateOfBirth.Year,
                    Gender = patient.Gender,
                    Phone = patient.Phone,
                    Email = patient.Email
                }).ToList();

                return Ok(new
                {
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Patients = patientDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("patients/search")]
        //http://localhost:5062/api/doctor/patients/search?query=nguyễn
        public async Task<IActionResult> SearchPatients([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Search query cannot be empty");
                }

              
                var patientUserIds = await _context.UserRoles
                    .Where(ur => ur.Role.RoleName == "user" && ur.IsActive)
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                
                var searchQuery = query.ToLower();
                var patientsQuery = _context.StrokeUsers
                    .AsNoTracking()
                    .Where(u => patientUserIds.Contains(u.UserId) &&
                           (u.PatientName.ToLower().Contains(searchQuery) ||
                            u.Email.ToLower().Contains(searchQuery) ||
                            u.Phone.Contains(searchQuery) ||
                            u.Username.ToLower().Contains(searchQuery)));

               
                var totalCount = await patientsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

               
                var patients = await patientsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var patientDtos = patients.Select(patient => new
                {
                    UserId = patient.UserId,
                    Username = patient.Username,
                    PatientName = patient.PatientName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = DateTime.Today.Year - patient.DateOfBirth.Year,
                    Gender = patient.Gender,
                    Phone = patient.Phone,
                    Email = patient.Email
                }).ToList();

                return Ok(new
                {
                    Query = query,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Patients = patientDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("my-patients")]
        //http://localhost:5062/api/doctor/my-patients
        public async Task<IActionResult> GetMyPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
              
                var doctorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(doctorIdStr, out int doctorId))
                {
                    return BadRequest("Invalid doctor identifier");
                }

           
                var patientIds = await _context.Relationships
                    .Where(r => r.InviterId == doctorId && r.RelationshipType == "doctor-patient")
                    .Select(r => r.UserId)
                    .ToListAsync();

                if (!patientIds.Any())
                {
                    return Ok(new
                    {
                        TotalCount = 0,
                        TotalPages = 0,
                        CurrentPage = page,
                        PageSize = pageSize,
                        Patients = new List<object>()
                    });
                }

               
                var patientsQuery = _context.StrokeUsers
                    .AsNoTracking()
                    .Where(u => patientIds.Contains(u.UserId));

             
                var totalCount = await patientsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var patients = await patientsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var patientDtos = patients.Select(patient => new
                {
                    UserId = patient.UserId,
                    Username = patient.Username,
                    PatientName = patient.PatientName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = DateTime.Today.Year - patient.DateOfBirth.Year,
                    Gender = patient.Gender,
                    Phone = patient.Phone,
                    Email = patient.Email
                }).ToList();

                return Ok(new
                {
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Patients = patientDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("generate-invitation-code")]
        //http://localhost:5062/api/doctor/generate-invitation-code
        public async Task<IActionResult> GenerateInvitationCode()
        {
            try
            {
               
                var doctorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(doctorIdStr, out int doctorId))
                {
                    return BadRequest("Invalid doctor identifier");
                }

                
                string code = GenerateRandomCode(6);

              
                var existingCode = await _context.InvitationCodes
                    .FirstOrDefaultAsync(ic => ic.InviterUserId == doctorId && ic.Status == "active");

                if (existingCode != null)
                {
                   
                    existingCode.Code = code;
                    existingCode.CreatedAt = DateTime.UtcNow;
                    existingCode.ExpiresAt = DateTime.UtcNow.AddDays(7);
                    _context.InvitationCodes.Update(existingCode);
                }
                else
                {
                    
                    var invitationCode = new InvitationCode
                    {
                        Code = code,
                        InviterUserId = doctorId,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(7) 
                    };

                    _context.InvitationCodes.Add(invitationCode);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Invitation code generated successfully",
                    Code = code,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("my-patients/search")]
        //http://localhost:5062/api/doctor/my-patients/search?query=nguyễn
        public async Task<IActionResult> SearchMyPatients([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("Search query cannot be empty");
                }

                var doctorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(doctorIdStr, out int doctorId))
                {
                    return BadRequest("Invalid doctor identifier");
                }

                
                var patientIds = await _context.Relationships
                    .Where(r => r.InviterId == doctorId && r.RelationshipType == "doctor-patient")
                    .Select(r => r.UserId)
                    .ToListAsync();

                if (!patientIds.Any())
                {
                    return Ok(new
                    {
                        Query = query,
                        TotalCount = 0,
                        TotalPages = 0,
                        CurrentPage = page,
                        PageSize = pageSize,
                        Patients = new List<object>()
                    });
                }

                
                var searchQuery = query.ToLower();
                var patientsQuery = _context.StrokeUsers
                    .AsNoTracking()
                    .Where(u => patientIds.Contains(u.UserId) &&
                           (u.PatientName.ToLower().Contains(searchQuery) ||
                            u.Email.ToLower().Contains(searchQuery) ||
                            u.Phone.Contains(searchQuery) ||
                            u.Username.ToLower().Contains(searchQuery)));

                
                var totalCount = await patientsQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var patients = await patientsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var patientDtos = patients.Select(patient => new
                {
                    UserId = patient.UserId,
                    Username = patient.Username,
                    PatientName = patient.PatientName,
                    DateOfBirth = patient.DateOfBirth,
                    Age = DateTime.Today.Year - patient.DateOfBirth.Year,
                    Gender = patient.Gender,
                    Phone = patient.Phone,
                    Email = patient.Email
                }).ToList();

                return Ok(new
                {
                    Query = query,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Patients = patientDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("check-patient-access/{patientId}")]
        //http://localhost:5062/api/doctor/check-patient-access/123
        public async Task<IActionResult> CheckPatientAccess(int patientId)
        {
            try
            {
              
                var doctorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(doctorIdStr, out int doctorId))
                {
                    return BadRequest("Invalid doctor identifier");
                }

                var patientExists = await _context.StrokeUsers.AnyAsync(u => u.UserId == patientId);
                if (!patientExists)
                {
                    return NotFound("Patient not found");
                }

                
                var hasRelationship = await _context.Relationships
                    .AnyAsync(r => r.InviterId == doctorId &&
                                  r.UserId == patientId &&
                                  r.RelationshipType == "doctor-patient");

                if (!hasRelationship)
                {
                    return Ok(new
                    {
                        HasAccess = false,
                        Message = "You do not have a doctor-patient relationship with this patient."
                    });
                }

                return Ok(new
                {
                    HasAccess = true,
                    Message = "You have access to this patient's data."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Helper method để tạo mã ngẫu nhiên
        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}