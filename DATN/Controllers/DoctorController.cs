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