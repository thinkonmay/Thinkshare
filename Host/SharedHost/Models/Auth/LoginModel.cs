using System;
using System.ComponentModel.DataAnnotations;

namespace SharedHost.Models.Auth
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterModel : LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Jobs { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
    }
}