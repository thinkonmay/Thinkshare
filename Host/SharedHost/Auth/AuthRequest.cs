using System.ComponentModel.DataAnnotations;

namespace SharedHost.Auth
{
    public class AuthenticationRequest
    {
        [Required]
        public string token {get;set;}

        [Required]
        public string Validator {get;set;}
    }

    public class AuthenticationResponse 
    {
        [Required]
        public string  UserID {get;set;}

        [Required]
        public bool IsAdmin {get;set;}

        [Required]
        public bool IsManager {get;set;}

        [Required]
        public bool IsUser {get;set;}

        [Required]
        public string ValidatedBy {get;set;}
    }
}