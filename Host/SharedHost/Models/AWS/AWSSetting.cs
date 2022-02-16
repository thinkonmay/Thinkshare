namespace SharedHost.Models.AWS 
{
    public class AWSSetting
    {
        public string Keyname { get; set; }

        public string InstanceType {get;set;}

        public string AMI{get;set;}

        public string SSHkeyPath {get;set;}

        public string CredentialPath{get;set;}

        public string ConfigPath{get;set;}



    }

    public class Region
    {
        public const string US_West = "US-West";

        public const string US_East = "US-East";

        public const string Canada = "Canada";

        public const string Singapore = "Singapore";

        public const string India = "India";

        public const string South_Korea = "South-Korea";

        public const string Australia = "Australia";

        public const string Tokyo = "Tokyo";

        public static bool CorrectTypo(string region)
        {
            switch (region)
            {
                case "US-West":
                    return true;

                case "US-East":
                    return true;

                case "Canada":
                    return true;

                case "Singapore":
                    return true;

                case "India":
                    return true;

                case "South-Korea":
                    return true;

                case "Australia":
                    return true;

                case "Tokyo":
                    return true;
                
                default:
                    return false;
            }
        }
    }
}
