using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Role { get; set; } 

    
    public PatientDto Patient { get; set; }
}

public class PatientDto
{
    public string PatientName { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public bool Gender { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}