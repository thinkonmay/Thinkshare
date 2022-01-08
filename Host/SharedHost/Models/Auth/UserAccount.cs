using Microsoft.AspNetCore.Identity;
using System;
using SharedHost.Models.Cluster;
using System.Collections.Generic;

namespace SharedHost.Models.User
{
    public class UserAccount : IdentityUser<int>
    {
        public string FullName { get; set; }
        public string? Jobs { get; set; }
        public string? Gender {get;set;}
        public DateTime? DateOfBirth { get; set; }
        public DateTime? Created { get; set; }
        public string? Avatar {get; set; }
    }
}
