using SharedHost.Models.Auth;

namespace SharedHost
{
    public class SystemConfig
    {
        public string BaseUrl { get; set; }
        public string Signalling { get; set; }
        public string SignallingWs { get; set; }
        public string SlaveManager { get; set; }
        public string SlaveManagerWs { get; set; }
        public string StunServer { get; set; }
        public string Flutter { get; set; }
        public string Conductor { get; set; }
        public string StunServerLibsoup { get; set; }
        public LoginModel AdminLogin { get; set; }
    }

}
