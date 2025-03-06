using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Dto
{
	public class CaseHistoryDto
	{
		public required string ProgressNotes { get; set; }
		public DateTimeOffset Time { get; set; }
		public required string StatusOfMr { get; set; }
		public int ChPatientId { get; set; }
	}
}
