using Microsoft.AspNetCore.Identity;
using System;
using SharedHost.Models.Device;
using System.Collections.Generic;
using SharedHost.Models.Cluster;

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
        public virtual DeviceCap? DefaultSetting {get;set;}
        public virtual WorkerCluster? ManagedCluster {get;set;}
    }
}
