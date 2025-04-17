using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Dto
{
	public class SubclinicalIndicatorDTO
	{
		public int UserID { get; set; }
		public DateTime RecordedAt { get; set; }
		public bool S100B { get; set; }
		public bool MMP9 { get; set; }
		public bool GFAP { get; set; }
		public bool RBP4 { get; set; }
		public bool NT_proBNP { get; set; }
		public bool sRAGE { get; set; }
		public bool D_dimer { get; set; }
		public bool Lipids { get; set; }
		public bool Protein { get; set; }
		public bool VonWillebrand { get; set; }
	}
}
