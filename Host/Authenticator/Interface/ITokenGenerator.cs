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
        Task<string> GenerateJwt(UserAccount user);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<UserAccount?> ValidateToken(string token);
    }
}
