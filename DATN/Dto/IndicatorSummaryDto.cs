namespace DATN.Dto
{
    public class IndicatorSummaryDto
    {
        public int UserId { get; set; }
        public string PatientName { get; set; }
        public int ClinicalPercent { get; set; }
        public int MolecularPercent { get; set; }
        public int SubclinicalPercent { get; set; }
        public int OverallRiskPercent { get; set; }
    }
}
