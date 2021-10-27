using SharedHost.Models.User;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Conductor.Interfaces
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
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        int GetUserFromHttpRequest(ClaimsPrincipal User);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        bool IsAdmin(ClaimsPrincipal User);
    }
}
