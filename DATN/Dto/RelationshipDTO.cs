namespace DATN.Dto
{
	public class RelationshipDTO
	{
		public int RelationshipId { get; set; }
		public int UserId { get; set; } // enter
		public int InviterId { get; set; } // sender
		public string NameInviter { get; set; }
		public string EmailInviter { get; set; }
		public string RelationshipType { get; set; } // family, friend, etc.
		public DateTime CreatedAt { get; set; }
	}
}
