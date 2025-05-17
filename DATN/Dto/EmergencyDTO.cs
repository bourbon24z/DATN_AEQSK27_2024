using System;
using System.ComponentModel.DataAnnotations;

    namespace DATN.Dto
    {
        public class EmergencyRequestDto
        {
            public int? UserId { get; set; }

            [Required]
            public float Latitude { get; set; }

            [Required]
            public float Longitude { get; set; }

            public string AdditionalInfo { get; set; }
        }

        public class EmergencyLocationResponseDto
        {
            public int UserId { get; set; }
            public string PatientName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public DateTime RecordedAt { get; set; }
            public string FormattedTime { get; set; }
            public List<RelationshipDto> Relationships { get; set; }
            public int WarningId { get; set; }
            public string OpenStreetMapLink { get; set; }
        }

    public class RelationshipDto
        {
            public string Type { get; set; }
            public int WithUserId { get; set; }
            public string WithUserName { get; set; }
        }

        public class EmergencyListItemDto
        {
            public int WarningId { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string FormattedTimestamp { get; set; }
            public bool IsActive { get; set; }
            public int? GpsId { get; set; }
        }
    }