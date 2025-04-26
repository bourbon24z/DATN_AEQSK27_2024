using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly StrokeDbContext _context;

        public DoctorService(StrokeDbContext context)
        {
            _context = context;
        }

        public async Task<CaseHistory> CreateCaseHistoryAsync(CaseHistoryDto dto)
        {
            var user = await _context.StrokeUsers.FirstOrDefaultAsync(c => c.UserId == dto.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var caseHistory = new CaseHistory
            {
                ProgressNotes = dto.ProgressNotes,
                Time = dto.Time,
                StatusOfMr = dto.StatusOfMr,
                UserId = dto.UserId
            };

            _context.CaseHistories.Add(caseHistory);
            await _context.SaveChangesAsync();
            return caseHistory;
        }

        public async Task<List<CaseHistory>> GetAllCaseHistoriesAsync()
        {
            return await _context.CaseHistories
                .Include(c => c.StrokeUser)
                // Loại bỏ Include không hợp lệ
                .ToListAsync();
        }

        public async Task<CaseHistory> GetCaseHistoryByIdAsync(int id)
        {
            return await _context.CaseHistories
                .Include(c => c.StrokeUser)
                // Loại bỏ Include không hợp lệ
                .FirstOrDefaultAsync(c => c.CaseHistoryId == id);
        }

        public async Task<List<DoctorEvaluation>> GetDoctorEvaluationsAsync(int caseHistoryId)
        {
            return await _context.DoctorEvaluations
                .Include(e => e.Doctor)
                .Where(e => e.CaseHistoryId == caseHistoryId)
                .ToListAsync();
        }

        public async Task AddDoctorEvaluationAsync(int caseHistoryId, DoctorEvaluationDto dto, int doctorId)
        {
            var caseHistory = await _context.CaseHistories.FindAsync(caseHistoryId);
            if (caseHistory == null)
                throw new KeyNotFoundException("Case history not found");

            var evaluation = new DoctorEvaluation
            {
                CaseHistoryId = caseHistoryId,
                DoctorId = doctorId,
                EvaluationDate = dto.EvaluationDate,
                EvaluationNotes = dto.EvaluationNotes
            };

            _context.DoctorEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MedicalImage>> GetMedicalImagesAsync(int caseHistoryId)
        {
            return await _context.MedicalImages
                .Where(m => m.CaseHistoryId == caseHistoryId)
                .ToListAsync();
        }

        public async Task AddMedicalImageAsync(int caseHistoryId, MedicalImageDto dto)
        {
            var caseHistory = await _context.CaseHistories.FindAsync(caseHistoryId);
            if (caseHistory == null)
                throw new KeyNotFoundException("Case history not found");

            var image = new MedicalImage
            {
                CaseHistoryId = caseHistoryId,
                ImageUrl = dto.ImageUrl,
                CapturedAt = dto.CapturedAt,
                Metadata = dto.Metadata
            };

            _context.MedicalImages.Add(image);
            await _context.SaveChangesAsync();
        }
    }
}