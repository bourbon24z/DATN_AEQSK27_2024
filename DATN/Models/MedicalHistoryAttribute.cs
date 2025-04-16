using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{

    [Table("medicalhistoryattributes")]
    public class MedicalHistoryAttribute
    {
        [Key]
        [Column("ValueId")]
        public int ValueId { get; set; }

        [Required]
        [Column("AttributeName")]
        public string AttributeName { get; set; }

        [Required]
        [Column("DataType")]
        public string DataType { get; set; }  // Ex: "BOOLEAN"

        [Column("Unit")]
        public string Unit { get; set; }

        [Column("GroupName")]
        public string GroupName { get; set; }  // Ex: "Clinical", "Molecular", "Subclinical"
    }
}
