using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Dto
{
	public class CaseHistoryDto
	{
        public string ProgressNotes { get; set; }
        public DateTime Time { get; set; }
        public string StatusOfMr { get; set; }
        public int UserId { get; set; }
    }
}
