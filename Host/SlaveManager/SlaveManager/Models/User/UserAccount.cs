using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models.User
{
    public class UserAccount : IdentityUser<int>
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime Created { get; set; }
    }
}
