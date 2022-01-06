using System;
using System.Collections.Generic;
using System.Text;

namespace DbSchema.DbSeeding
{
    public class RoleSeeding
    {

        public const string ADMIN = "Administrator";
        public const string MOD = "Moderator";
        public const string USER = "User";
        public const string TEST_CLUSTER = "TestCluster";

        public static readonly string[] DEFAULT_ROLES = { ADMIN, MOD, USER };
    }
}
