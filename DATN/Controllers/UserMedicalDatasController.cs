using DATN.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserMedicalDatasController : ControllerBase
    {
		private readonly StrokeDbContext _context;
		public UserMedicalDatasController(StrokeDbContext context)
		{
			_context = context;
		}



	}
}
