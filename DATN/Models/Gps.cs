using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    [Table("gps")]
    public class Gps
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("gps_id")]
        public int GpsId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        
        [Column("long")]
        public float Lon { get; set; }

        [Column("lat")]
        public float Lat { get; set; }

        [Column("created_at", TypeName = "datetime(6)")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual StrokeUser StrokeUser { get; set; }
    }
}
