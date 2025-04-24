using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Dto
{
	public class WarningDTO
	{
		public int UserId { get; set; }
		public string Description { get; set; }

	}
}
