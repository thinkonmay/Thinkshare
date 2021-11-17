using Microsoft.AspNetCore.Identity;
using System;
using SharedHost.Models.Session;
using System.Collections.Generic;
using SharedHost.Models.Device;

namespace SharedHost.Models.User
{
    public class UserInforModel
    {
        public UserInforModel(){}

        public UserInforModel(UserAccount account)
        {
            UserName = account.UserName;
            FullName = account.FullName;
            Jobs = account.Jobs;
            PhoneNumber = account.PhoneNumber;
            Gender = account.Gender;
            DateOfBirth = account.DateOfBirth;
            Avatar = account.Avatar;
            DefaultSetting = account.DefaultSetting;
        }


        public string? UserName {get;set;}

        public string? FullName {get;set;}

        public string? Jobs {get;set;}

        public string? PhoneNumber {get;set;}

        public string? Gender {get;set;}

        public DateTime? DateOfBirth {get;set;}

        public string? Avatar {get; set; }

        public DeviceCap?  DefaultSetting {get;set;}


    }
}
