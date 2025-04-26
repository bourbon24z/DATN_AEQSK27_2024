using System;

namespace DATN.Dto
{
    public class MedicalImageDto
    {
        public string ImageUrl { get; set; }
        public DateTime CapturedAt { get; set; }
        public string Metadata { get; set; }
    }
}