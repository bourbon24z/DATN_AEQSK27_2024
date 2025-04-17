using System.Text.Json.Serialization;

namespace DATN.Models
{
    public class InvitationCode
    {
        public int InvitationId { get; set; }
        public string Code { get; set; }
        public int InviterUserId { get; set; }
        public string Status { get; set; } //active, used, expired
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        [JsonIgnore]
		public StrokeUser InviterUser { get; set; }
    }

}
