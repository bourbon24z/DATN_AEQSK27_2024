﻿using System.ComponentModel.DataAnnotations;

public class RegisterUserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string PatientName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool Gender { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}