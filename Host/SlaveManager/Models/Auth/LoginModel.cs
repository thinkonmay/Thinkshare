using System;
using System.ComponentModel.DataAnnotations;

namespace SlaveManager.Models.Auth
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RegisterModel : LoginModel
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string DateOfBirth { get; set; }
    }
}