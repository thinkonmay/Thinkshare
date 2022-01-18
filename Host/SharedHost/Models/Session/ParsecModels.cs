namespace SharedHost.Models.Session
{
    public class ParsecLoginModel
    {
        public string email {get;set;}
        public string password {get;set;}
        public string tfa{get;set;}
    }

    public class ParsecLoginResponse
    {
        public string user_id{get;set;}
        public string session_id{get;set;}
        public string instance_id{get;set;}
        public string host_peer_id{get;set;}
    }

    public class ParsecAPI
    {
        public const string authAPI = "https://kessel-api.parsecgaming.com/v1/auth/";
    }
}