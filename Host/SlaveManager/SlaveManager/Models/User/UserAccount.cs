using Microsoft.AspNetCore.Identity;
using System;

namespace SlaveManager.Models.User
{
    public class UserAccount : IdentityUser<int>
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime Created { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
