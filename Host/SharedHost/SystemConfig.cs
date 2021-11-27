﻿using SharedHost.Models.Auth;
using System.Collections.Generic;

namespace SharedHost
{
    public class SystemConfig
    {
        public string BaseUrl { get; set; }
        public string Signalling { get; set; }
        public string SignallingWs { get; set; }
        public string Conductor { get; set; }
        public string Authenticator { get; set; }
        public string MetricCollector {get;set;}
        public string SystemHub {get;set;}
        public string GoogleOauthID {get;set;}
        public LoginModel AdminLogin { get; set; }
        public List<string> STUNlist { get; set; }
    }

    public class TurnServer
    {
        public string urls {get;set;}

        public string username {get;set;}

        public string credential {get;set;}
    } 

}
