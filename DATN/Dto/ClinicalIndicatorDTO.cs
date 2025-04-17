using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Dto
{
	public class ClinicalIndicatorDTO
	{
		public int UserID { get; set; }
		public DateTime RecordedAt { get; set; }
		public bool DauDau { get; set; }
		public bool TeMatChi { get; set; }
		public bool ChongMat { get; set; }
		public bool KhoNoi { get; set; }
		public bool MatTriNhoTamThoi { get; set; }
		public bool LuLan { get; set; }
		public bool GiamThiLuc { get; set; }
		public bool MatThangCan { get; set; }
		public bool BuonNon { get; set; }
		public bool KhoNuot { get; set; }
	}
}
