using System.ComponentModel.DataAnnotations;

namespace DATN.Dto
{
    public class LoginDto
    {
        [Required]
        public string Credential { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
