namespace DATN.Models
{
    public class Relationship
    {
        public int RelationshipId { get; set; }
        public int UserId { get; set; } // enter
        public int InviterId { get; set; } // sender
        public string RelationshipType { get; set; } // family, friend, etc.
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public StrokeUser User { get; set; }
        public StrokeUser Inviter { get; set; }
    }

}
