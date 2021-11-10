using Microsoft.AspNetCore.Identity;
using System;
using SharedHost.Models.Device;
using System.Collections.Generic;

namespace SharedHost.Models.User
{
    public class UserAccount : IdentityUser<int>
    {
        /// <summary>
        /// each client can have a specific number of cloud storage at the same time
        /// </summary>
        //public virtual ICollection<StorageBlock> currentStorage { get; set; }


        public string FullName { get; set; }
        public string? Jobs { get; set; }
        public string? Gender {get;set;}
        public DateTime? DateOfBirth { get; set; }
        public DateTime? Created { get; set; }
        public string Avatar {get; set; }
        public virtual DeviceCap? DefaultSetting {get;set;}
    }
}
