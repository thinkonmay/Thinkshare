using SlaveManager.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Interfaces
{
    public interface ITokenGenerator
    {
        Task<string> GenerateJwt(UserAccount user);
    }
}
