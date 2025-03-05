using DATN.Data;
using DATN.Dto;
using DATN.Mapper;
using DATN.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
		public async Task<IActionResult> CreateCaseHistory([FromBody] CaseHistoryDto caseHistoryDto)
		{
			var patient = await _context.Patients.FirstOrDefaultAsync(c => c.PatientId == caseHistoryDto.ChPatientId);
			if (patient == null)
			{
				return BadRequest("Patient not found.");
			}
			var caseHistory = new CaseHistory
			{
				ProgressNotes = caseHistoryDto.ProgressNotes,
				Time = caseHistoryDto.Time,
				StatusOfMr = caseHistoryDto.StatusOfMr,
				ChPatientId = caseHistoryDto.ChPatientId,
				Patient = patient
			};
			_context.CaseHistories.Add(caseHistory);
			await _context.SaveChangesAsync();
			return Ok("Create case history successful.");
		}


		[HttpGet("{id}")]
		public async Task<IActionResult> GetCaseHistoryByIdPatient([FromRoute] int id)
		{
			var caseHistory = await _context.CaseHistories.Where(c => c.ChPatientId == id).ToArrayAsync();
			if (caseHistory == null)
			{
				return NotFound();
			}
			return Ok(caseHistory);
		}



		[HttpGet]
		public async Task<IActionResult> GetCaseHistory()
		{
			var caseHistory = await _context.CaseHistories.ToArrayAsync();
			if (caseHistory == null)
			{
				return NotFound();
			}
			return Ok(caseHistory);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateCaseHistory([FromRoute] int id, [FromBody] CaseHistoryDto caseHistoryDto)
		{
			var caseHistory = await _context.CaseHistories.FirstOrDefaultAsync(c => c.CaseHistoryId == id);
			if (caseHistory == null)
			{
				return NotFound();
			}
			caseHistory.ProgressNotes = caseHistoryDto.ProgressNotes;
			caseHistory.Time = caseHistoryDto.Time;
			caseHistory.StatusOfMr = caseHistoryDto.StatusOfMr;
			_context.CaseHistories.Update(caseHistory);
			await _context.SaveChangesAsync();
			return Ok("Update case history successful.");
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCaseHistory([FromRoute] int id)
		{
			var dbCase = await _context.CaseHistories.FirstOrDefaultAsync(c => c.CaseHistoryId == id);
			if (dbCase == null)
			{
				return NotFound();
			}
			_context.CaseHistories.Remove(dbCase);
			return Ok("Delete case history successful.");
		}

	}
}
