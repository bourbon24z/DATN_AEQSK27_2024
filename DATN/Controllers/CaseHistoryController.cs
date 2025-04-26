using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaseHistoryController : ControllerBase
    {
        private readonly StrokeDbContext _context;

        public CaseHistoryController(StrokeDbContext context)
        {
            _context = context;
        }

        [HttpPost("caseHistory")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "doctor")]
        public async Task<IActionResult> CreateCaseHistory([FromBody] CaseHistoryDto caseHistoryDto)
        {
            var user = await _context.StrokeUsers.FirstOrDefaultAsync(c => c.UserId == caseHistoryDto.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var caseHistory = new CaseHistory
            {
                ProgressNotes = caseHistoryDto.ProgressNotes,
                Time = caseHistoryDto.Time,
                StatusOfMr = caseHistoryDto.StatusOfMr,
                UserId = caseHistoryDto.UserId,
                StrokeUser = user
            };

            _context.CaseHistories.Add(caseHistory);
            await _context.SaveChangesAsync();
            return Ok("Create case history successful.");
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "doctor")]
        public async Task<IActionResult> GetCaseHistoryByUserId([FromRoute] int id)
        {
            var caseHistory = await _context.CaseHistories.Where(c => c.UserId == id).ToArrayAsync();
            if (caseHistory == null || caseHistory.Length == 0)
            {
                return NotFound("Case history not found.");
            }
            return Ok(caseHistory);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "doctor")]
        public async Task<IActionResult> GetCaseHistory()
        {
            var caseHistory = await _context.CaseHistories.ToArrayAsync();
            if (caseHistory == null || caseHistory.Length == 0)
            {
                return NotFound("No case histories found.");
            }
            return Ok(caseHistory);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "doctor")]
        public async Task<IActionResult> UpdateCaseHistory([FromRoute] int id, [FromBody] CaseHistoryDto caseHistoryDto)
        {
            var caseHistory = await _context.CaseHistories.FirstOrDefaultAsync(c => c.CaseHistoryId == id);
            if (caseHistory == null)
            {
                return NotFound("Case history not found.");
            }

            caseHistory.ProgressNotes = caseHistoryDto.ProgressNotes;
            caseHistory.Time = caseHistoryDto.Time;
            caseHistory.StatusOfMr = caseHistoryDto.StatusOfMr;
            _context.CaseHistories.Update(caseHistory);
            await _context.SaveChangesAsync();
            return Ok("Update case history successful.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "doctor")]
        public async Task<IActionResult> DeleteCaseHistory([FromRoute] int id)
        {
            var caseHistory = await _context.CaseHistories.FirstOrDefaultAsync(c => c.CaseHistoryId == id);
            if (caseHistory == null)
            {
                return NotFound("Case history not found.");
            }

            _context.CaseHistories.Remove(caseHistory);
            await _context.SaveChangesAsync();
            return Ok("Delete case history successful.");
        }
    }
}
