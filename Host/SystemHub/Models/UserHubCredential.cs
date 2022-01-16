using SharedHost.Auth;
using MersenneTwister;
using System.Net.WebSockets;

namespace SystemHub.Models
{
    public class UserHubCredential : AuthenticationResponse
    {
        public UserHubCredential(AuthenticationResponse response,WebSocket ws)
        {
            this.UserID = response.UserID;
            this.IsAdmin = response.IsAdmin;
            this.IsManager = response.IsManager;
            this.IsUser = response.IsUser;
            this.ValidatedBy = response.ValidatedBy;
            rand = Randoms.Next();
        }
        public int rand {get;set;}
    }
}