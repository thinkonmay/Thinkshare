using System;
namespace SharedHost
{
    public class ClusterConfig
    {
        public string ClusterHub {get;set;}
        public string ClusterInforUrl {get;set;}
        public string OwnerAuthorizeUrl {get;set;}
        public string OwnerAccountUrl {get;set;}
        public string ScriptModelUrl {get;set;}
        public string ClusterRegisterUrl{get;set;}
        public string WorkerRegisterUrl{get;set;}
    }
}
