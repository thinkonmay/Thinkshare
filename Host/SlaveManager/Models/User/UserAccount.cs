using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace SlaveManager.Models.User
{
    public class UserAccount : IdentityUser<int>
    {
        /// <summary>
        /// each client can have a specific number of cloud storage at the same time
        /// </summary>
        //public virtual ICollection<StorageBlock> currentStorage { get; set; }

        public string FullName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Created { get; set; }
    }
}
