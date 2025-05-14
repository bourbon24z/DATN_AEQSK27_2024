using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class IndicatorsController : ControllerBase
	{
		private readonly StrokeDbContext _context;
		public IndicatorsController(StrokeDbContext context)
		{
			_context = context;
		}
		[HttpPost("add-clinical-indicator")]
		[Authorize(Roles = "user")]
		public async Task<IActionResult> AddClinicalIndicator([FromBody] ClinicalIndicatorDTO clinicalIndicatorDTO)
		{
			if (clinicalIndicatorDTO == null)
			{
				return BadRequest("Invalid data.");
			}
			var clinicalIndicatorExists = await _context.ClinicalIndicators
				.Where(x => x.UserID == clinicalIndicatorDTO.UserID && x.IsActived == true)
				.ToListAsync();
			if (clinicalIndicatorExists.Any())
			{
				_context.ClinicalIndicators.RemoveRange(clinicalIndicatorExists);
			}
			var clinicalIndicator = new ClinicalIndicator()
			{
				UserID = clinicalIndicatorDTO.UserID,
				IsActived = true,
				RecordedAt = clinicalIndicatorDTO.RecordedAt,
				DauDau = clinicalIndicatorDTO.DauDau,
				TeMatChi = clinicalIndicatorDTO.TeMatChi,
				ChongMat = clinicalIndicatorDTO.ChongMat,
				KhoNoi = clinicalIndicatorDTO.KhoNoi,
				MatTriNhoTamThoi = clinicalIndicatorDTO.MatTriNhoTamThoi,
				LuLan = clinicalIndicatorDTO.LuLan,
				GiamThiLuc = clinicalIndicatorDTO.GiamThiLuc,
				MatThangCan = clinicalIndicatorDTO.MatThangCan,
				BuonNon = clinicalIndicatorDTO.BuonNon,
				KhoNuot = clinicalIndicatorDTO.KhoNuot
			};
			await _context.ClinicalIndicators.AddAsync(clinicalIndicator);
			await _context.SaveChangesAsync();
			return Ok(clinicalIndicator);
		}

		[HttpPost("add-molecular-indicator")]
        [Authorize(Roles = "admin,doctor")]
        public async Task<IActionResult> AddMolecularIndicator([FromBody] MolecularIndicatorDTO molecularIndicatorDTO)
		{
			if (molecularIndicatorDTO == null)
			{
				return BadRequest("Invalid data.");
			}
            var molecularIndicatorExists = await _context.MolecularIndicators
					.Where(x => x.UserID == molecularIndicatorDTO.UserID && x.IsActived == true)
					.ToListAsync();
            if (molecularIndicatorExists.Any())
            {
                _context.MolecularIndicators.RemoveRange(molecularIndicatorExists);
            }
            var molecularIndicator = new MolecularIndicator()
			{
				UserID = molecularIndicatorDTO.UserID,
				IsActived = true,
				RecordedAt = molecularIndicatorDTO.RecordedAt,
				MiR_30e_5p = molecularIndicatorDTO.MiR_30e_5p,
				MiR_16_5p = molecularIndicatorDTO.MiR_16_5p,
				MiR_140_3p = molecularIndicatorDTO.MiR_140_3p,
				MiR_320d = molecularIndicatorDTO.MiR_320d,
				MiR_320p = molecularIndicatorDTO.MiR_320p,
				MiR_20a_5p = molecularIndicatorDTO.MiR_20a_5p,
				MiR_26b_5p = molecularIndicatorDTO.MiR_26b_5p,
				MiR_19b_5p = molecularIndicatorDTO.MiR_19b_5p,
				MiR_874_5p = molecularIndicatorDTO.MiR_874_5p,
				MiR_451a = molecularIndicatorDTO.MiR_451a
			};
			await _context.MolecularIndicators.AddAsync(molecularIndicator);
			await _context.SaveChangesAsync();
			return Ok(molecularIndicator);
		}

		[HttpPost("add-subclinical-Indicator")]
        [Authorize(Roles = "admin,doctor")]


        public async Task<IActionResult> AddSubclinicalIndicator([FromBody] SubclinicalIndicatorDTO subclinicalIndicatorDTO)
		{
			if (subclinicalIndicatorDTO == null)
			{
				return BadRequest("Invalid data.");
			}
            var subclinicalIndicatorExists = await _context.SubclinicalIndicators
					.Where(x => x.UserID == subclinicalIndicatorDTO.UserID && x.IsActived == true)
					.ToListAsync();
            if (subclinicalIndicatorExists.Any())
            {
                _context.SubclinicalIndicators.RemoveRange(subclinicalIndicatorExists);
            }
            var subclinicalIndicator = new SubclinicalIndicator()
			{
				UserID = subclinicalIndicatorDTO.UserID,
				IsActived = true,
				RecordedAt = subclinicalIndicatorDTO.RecordedAt,
				D_dimer = subclinicalIndicatorDTO.D_dimer,
				GFAP = subclinicalIndicatorDTO.GFAP,
				Lipids = subclinicalIndicatorDTO.Lipids,
				MMP9 = subclinicalIndicatorDTO.MMP9,
				NT_proBNP = subclinicalIndicatorDTO.NT_proBNP,
				Protein = subclinicalIndicatorDTO.Protein,
				RBP4 = subclinicalIndicatorDTO.RBP4,
				S100B = subclinicalIndicatorDTO.S100B,
				sRAGE = subclinicalIndicatorDTO.sRAGE,
				VonWillebrand = subclinicalIndicatorDTO.VonWillebrand

			};
			await _context.SubclinicalIndicators.AddAsync(subclinicalIndicator);
			await _context.SaveChangesAsync();
			return Ok(subclinicalIndicator);
		}

		[HttpGet("get-indicator/{userId}")]
        [Authorize(Roles = "admin,doctor")]
        public async Task<IActionResult> GetIndicator(int userId)
		{
			var clinicalIndicator = await _context.ClinicalIndicators.FirstOrDefaultAsync(x => x.UserID == userId&& x.IsActived==true);
			var molecularIndicator = await _context.MolecularIndicators.FirstOrDefaultAsync(x => x.UserID == userId&& x.IsActived==true);
			var subclinicalIndicator = await _context.SubclinicalIndicators.FirstOrDefaultAsync(x => x.UserID == userId && x.IsActived == true);
			if (clinicalIndicator == null && molecularIndicator == null && subclinicalIndicator == null)
			{
				return NotFound("No indicators found for this user.");
			}
			return Ok(new
			{
				ClinicalIndicator = clinicalIndicator,
				MolecularIndicator = molecularIndicator,
				SubclinicalIndicator = subclinicalIndicator
			});
		}
		[HttpGet("get-percent-indicator-is-true")]
        [Authorize(Roles = "admin,doctor")]
        public async Task<IActionResult> GetPercentIndicatorIsTrue(int userId)
		{
			var clinicalIndicator = await _context.ClinicalIndicators.FirstOrDefaultAsync(x => x.UserID == userId && x.IsActived == true);
			var molecularIndicator = await _context.MolecularIndicators.FirstOrDefaultAsync(x => x.UserID == userId && x.IsActived == true);
			var subclinicalIndicator = await _context.SubclinicalIndicators.FirstOrDefaultAsync(x => x.UserID == userId && x.IsActived == true);
			int totalCount1 = 0;
			int trueCount1 = 0;
			int totalCount2 = 0;
			int trueCount2 = 0;
			int totalCount3 = 0;
			int trueCount3 = 0;
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
			var percent1 = 0;
			var percent2 = 0;
			var percent3 = 0;
			if(totalCount1 > 0 && molecularIndicator==null && subclinicalIndicator==null)
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
			var percent = percent1 + percent2 + percent3;
			return Ok(new
			{
				Percent = percent,
				ClinicalIndicator = new
				{
					Percent = percent1,
					TotalCount = totalCount1,
					TrueCount = trueCount1
				},
				MolecularIndicator = new
				{
					Percent = percent2,
					TotalCount = totalCount2,
					TrueCount = trueCount2
				},
				SubclinicalIndicator = new
				{
					Percent = percent3,
					TotalCount = totalCount3,
					TrueCount = trueCount3
				}
			});

		}
	}
}

