using DATN.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemHealthsController : ControllerBase
    {
        private readonly StrokeDbContext _context;
        public SystemHealthsController(StrokeDbContext context)
        {
            _context = context;
        }

       // [Authorize(Roles ="admin")]  test tis thoii cos chi merge laij nha Huy 
        [HttpGet("CountUser")]
        public async Task<IActionResult> GetCountUser()
        {
            int countUser = await _context.StrokeUsers.CountAsync();
            if (countUser > 0)
            {
                return Ok(new { countUser });
            }
            else
            {
                return BadRequest();
            }

        }
    }

}
