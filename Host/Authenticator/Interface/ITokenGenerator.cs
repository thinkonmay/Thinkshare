using SharedHost.Models.Session;
using SharedHost.Models.User;
using System.Threading.Tasks;

namespace Authenticator.Interfaces
{
    public interface ITokenGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<string> GenerateUserJwt(UserAccount user);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<UserAccount?> ValidateUserToken(string token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        Task<string> GenerateSessionJwt(SessionAccession accession);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<SessionAccession?> ValidateSessionToken(string user);
    }
}
