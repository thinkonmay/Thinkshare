using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Models.Auth
{
    public class JwtOptions
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string Key { get; set; }
    }
}
