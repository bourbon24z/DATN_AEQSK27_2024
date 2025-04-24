using DATN.Data;
using DATN.Dto;
using DATN.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarningsController : ControllerBase
    {
		private readonly StrokeDbContext _context;
		public WarningsController( StrokeDbContext context)
		{
			_context = context;			
		}
		[HttpGet]
		public async Task<IActionResult> AddWarning(WarningDTO warning)
		{
			if (warning.Description == null && warning.UserId == null)
			{
				return BadRequest("Value is emty!!");
			}
			var user = await _context.StrokeUsers.FindAsync(warning.UserId);
			if (user == null)
			{
				return BadRequest("User not found");
			}
			var newWarning = new Warning
			{
				Description = warning.Description,
				CreatedAt = DateTime.UtcNow,
				IsActive = true,
				StrokeUser = user,
				UserId = user.UserId,
			};
			_context.Warnings.Add(newWarning);

			return Ok(newWarning);
			
		}

	}
}
